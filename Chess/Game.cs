/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using Fen;
    using Hash;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Transposition;
    using Types;
    using Move = Types.Move;
    using Piece = Types.Piece;
    using Square = Types.Square;

    public sealed class Game : IGame
    {
        private const int MaxPositions = 1 << 11;

        /// <summary>
        /// [short/long, side] castle positional | array when altering castleling rights.
        /// </summary>
        private static readonly CastlelingRights[][] CastlePositionalOr;

        private readonly CastlelingRights[] _castleRightsMask;

        private readonly State[] _stateList;

        private readonly StringBuilder _output;

        private bool _chess960;

        private bool _xfen;

        static Game()
        {
            CastlePositionalOr = new CastlelingRights[2][];
            CastlePositionalOr[0] = new[] { CastlelingRights.WhiteOo, CastlelingRights.BlackOo };
            CastlePositionalOr[1] = new[] { CastlelingRights.WhiteOoo, CastlelingRights.BlackOoo };
        }

        public Game(IPosition pos)
        {
            _castleRightsMask = new CastlelingRights[64];
            Pos = pos;
            _stateList = new State[MaxPositions];
            _output = new StringBuilder(256);

            for (var i = 0; i < _stateList.Length; i++)
                _stateList[i] = new State();

            PositionIndex = 0;
            Pos.State = _stateList[PositionIndex];
            _chess960 = false;
            _xfen = false;
        }

        public State State => Pos.State;

        public Action<Piece, Square> PieceUpdated => Pos.PieceUpdated;

        public int PositionIndex { get; private set; }

        public int PositionStart { get; private set; }

        public int MoveNumber => (PositionIndex - 1) / 2 + 1;

        public BitBoard Occupied => Pos.Pieces();

        public IPosition Pos { get; }

        public GameEndTypes GameEndType { get; set; }

        public static TranspositionTable Table { get; set; } = new TranspositionTable(256);

        /// <summary>
        /// Makes a chess move in the data structure
        /// </summary>
        /// <param name="m">The move to make</param>
        /// <returns>true if everything was fine, false if unable to progress - fx castleling position under attack</returns>
        public bool MakeMove(Move m)
        {
            if (!Pos.MakeMove(m))
                return false;

            // advances the position
            var previous = _stateList[PositionIndex++];
            Pos.State = _stateList[PositionIndex];
            State.SideToMove = ~previous.SideToMove;
            State.Material = previous.Material;
            State.HalfMoveCount = PositionIndex;
            State.LastMove = m;

            var ksq = Pos.GetPieceSquare(PieceTypes.King, State.SideToMove);

            // compute in-check
            State.InCheck = Pos.IsAttacked(ksq, ~State.SideToMove);

            State.CastlelingRights = _stateList[PositionIndex - 1].CastlelingRights & _castleRightsMask[m.GetFromSquare().AsInt()] & _castleRightsMask[m.GetToSquare().AsInt()];
            State.NullMovesInRow = 0;

            // compute reversible half move count
            State.ReversibleHalfMoveCount = m.IsCaptureMove() || m.GetMovingPieceType() == PieceTypes.Pawn
                ? 0
                : previous.ReversibleHalfMoveCount + 1;

            // compute en-passant if present
            State.EnPassantSquare = m.IsDoublePush()
                ? m.GetFromSquare() + m.GetMovingSide().PawnPushDistance()
                : Squares.none;

            State.Key = previous.Key;
            State.PawnStructureKey = previous.PawnStructureKey;
            State.Material.MakeMove(m);
            //State.Pinned = Pos.GetPinnedPieces(ksq, Pos.State.SideToMove);

            UpdateKey(m);

            return true;
        }

        public void TakeMove()
        {
            // careful.. NO check for invalid PositionIndex.. make sure it's always counted correctly
            Pos.TakeMove(State.LastMove);
            if (PositionIndex == 0)
                throw new Exception("fail");
            --PositionIndex;
            Pos.State = _stateList[PositionIndex];
            State.HalfMoveCount = PositionIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenError NewGame(string fen = Fen.Fen.StartPositionFen) => SetFen(new FenData(fen), true);

        /// <summary>
        /// Apply a FEN string board setup to the board structure.
        /// </summary>
        /// <param name="fen">The fen data to set</param>
        /// <param name="validate">If true, the fen string is validated, otherwise not</param>
        /// <returns>
        /// 0 = all ok.
        /// -1 = Error in piece file layout parsing
        /// -2 = Error in piece rank layout parsing
        /// -3 = Unknown piece detected
        /// -4 = Error while parsing moving side
        /// -5 = Error while parsing castleling
        /// -6 = Error while parsing en-passant square
        /// -9 = FEN length exceeding maximum
        /// </returns>
        public FenError SetFen(FenData fen, bool validate = false)
        {
            if (validate)
                Fen.Fen.Validate(fen.Fen.ToString());

            // correctly clear all pieces and invoke possible notification(s)
            var bb = Occupied;
            while (bb)
            {
                var square = bb.Lsb();
                Pos.RemovePiece(square);
                BitBoards.ResetLsb(ref bb);
            }

            var chunk = fen.Chunk();

            if (chunk.IsEmpty)
                return new FenError();

            var f = 1; // file (column)
            var r = 8; // rank (row)
            Player player;

            foreach (var c in chunk)
            {
                if (char.IsDigit(c))
                {
                    f += c - '0';
                    if (f > 9)
                        return new FenError(-1, fen.Index);
                }
                else if (c == '/')
                {
                    if (f != 9)
                        return new FenError(-2, fen.Index);

                    r--;
                    f = 1;
                }
                else
                {
                    var pieceIndex = PieceExtensions.PieceChars.IndexOf(c);

                    if (pieceIndex == -1)
                        return new FenError(-3, fen.Index);

                    player = char.IsLower(PieceExtensions.PieceChars[pieceIndex]);

                    var square = new Square(r - 1, f - 1);

                    AddPiece(square, player, (PieceTypes)pieceIndex);

                    f++;
                }
            }

            // player
            chunk = fen.Chunk();

            if (chunk.IsEmpty || chunk.Length != 1)
                return new FenError(-3, fen.Index);

            player = (chunk[0] != 'w').ToInt();

            // castleling
            chunk = fen.Chunk();

            if (!SetupCastleling(chunk))
                return new FenError(-5, fen.Index);

            // en-passant
            chunk = fen.Chunk();

            if (chunk.Length == 1 || chunk[0] == '-' || !chunk[0].InBetween('a', 'h'))
                State.EnPassantSquare = Squares.none;
            else
                State.EnPassantSquare = chunk[1].InBetween('3', '6') ? Squares.none : new Square(chunk[1] - '1', chunk[0] - 'a').Value;

            // move number
            chunk = fen.Chunk();

            var moveNum = 0;
            var halfMoveNum = 0;

            if (!chunk.IsEmpty)
            {
                chunk.ToIntegral(out halfMoveNum);

                // half move number
                chunk = fen.Chunk();

                chunk.ToIntegral(out moveNum);

                if (moveNum > 0)
                    moveNum--;
            }

            PositionIndex = PositionStart = moveNum;

            Pos.State = _stateList[PositionIndex];

            State.ReversibleHalfMoveCount = halfMoveNum;

            State.SideToMove = player;

            if (player.IsBlack())
            {
                State.Key ^= Zobrist.GetZobristSide();
                State.PawnStructureKey ^= Zobrist.GetZobristSide();
            }

            State.Key ^= State.CastlelingRights.GetZobristCastleling();

            if (State.EnPassantSquare != Squares.none)
                State.Key ^= State.EnPassantSquare.File().GetZobristEnPessant();

            var ksq = Pos.GetPieceSquare(PieceTypes.King, player);

            State.InCheck = Pos.IsAttacked(ksq, ~player);
            State.Checkers = Pos.AttacksTo(ksq);

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData GetFen() => Pos.GenerateFen();

        /// <summary>
        /// Converts a move data type to move notation string format which chess engines understand.
        /// e.g. "a2a4", "a7a8q"
        /// </summary>
        /// <param name="m">The move to convert</param>
        /// <param name="output">The string builder used to generate the string with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToString(Move m, StringBuilder output)
        {
            if (!_chess960 && !m.IsCastlelingMove())
                output.Append(m.ToString());
            else
            {
                if (_xfen && m.GetToSquare() == CastlelingSides.Queen.GetKingCastleTo(m.GetMovingSide()))
                    output.Append(CastlelingSides.Queen.GetCastlelingString());
                else if (_xfen)
                    output.Append(CastlelingSides.King.GetCastlelingString());
                else
                {
                    output.Append(m.GetFromSquare().ToString());
                    output.Append(Pos.GetRookCastleFrom(m.GetToSquare()).ToString());
                }
            }
        }

        public void UpdateDrawTypes()
        {
            var gameEndType = GameEndTypes.None;
            if (IsRepetition())
                gameEndType |= GameEndTypes.Repetition;
            if (State.Material[PlayerExtensions.White.Side] <= 300 && State.Material[PlayerExtensions.Black.Side] <= 300 && Pos.BoardPieces[0].Empty() && Pos.BoardPieces[8].Empty())
                gameEndType |= GameEndTypes.MaterialDrawn;
            if (State.ReversibleHalfMoveCount >= 100)
                gameEndType |= GameEndTypes.FiftyMove;

            var moveList = Pos.GenerateMoves();
            if (!moveList.Any(move => Pos.IsLegal(move)))
                gameEndType |= GameEndTypes.Pat;

            GameEndType = gameEndType;
        }

        public override string ToString()
        {
            const string separator = "\n  +---+---+---+---+---+---+---+---+\n";
            const char splitter = '|';
            const char space = ' ';
            _output.Clear();
            _output.Append(separator);
            for (var rank = Ranks.Rank8; rank >= Ranks.Rank1; rank--)
            {
                _output.Append((int)rank + 1);
                _output.Append(space);
                for (var file = Files.FileA; file <= Files.FileH; file++)
                {
                    var piece = Pos.GetPiece(new Square(rank, file));
                    _output.AppendFormat("{0}{1}{2}{1}", splitter, space, piece.GetPieceChar());
                }

                _output.Append(splitter);
                _output.Append(separator);
            }

            _output.AppendLine("    a   b   c   d   e   f   g   h");
            _output.AppendLine($"Zobrist : 0x{State.Key:X}");
            return _output.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<Piece> GetEnumerator() => Pos.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard OccupiedBySide(Player c) => Pos.OccupiedBySide[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player CurrentPlayer() => State.SideToMove;

        public ulong Perft(int depth)
        {
            if (depth == 1)
                return (ulong)Pos.GenerateMoves().Count;

            var (found, entry) = Table.Probe(Pos.State.Key);
            if (found && entry.Depth == depth)
            {
                var stateKey = (uint) (Pos.State.Key >> 32);
                if (entry.Key32 == stateKey)
                    return (ulong)entry.Value;
            }

            var tot = 0ul;
            var move = MoveExtensions.EmptyMove;

            foreach (var m in Pos.GenerateMoves())
            {
                if (MakeMove(m))
                {
                    move = m;
                    tot += Perft(depth - 1);
                    TakeMove();
                }
                else
                {
                    move = MoveExtensions.EmptyMove;
                    break;
                }
            }

            if (move != MoveExtensions.EmptyMove && tot <= int.MaxValue)
                Table.Store(Pos.State.Key, (int)tot, Bound.Exact, (sbyte)depth, move, 0);

            return tot;
        }

        /// <summary>
        /// Adds a piece to the board, and updates the hash keys if needed
        /// </summary>
        /// <param name="sq">The square for the piece</param>
        /// <param name="c">For which side the piece is to be added</param>
        /// <param name="pt">The type of piece to add</param>
        private void AddPiece(Square sq, Player c, PieceTypes pt)
        {
            Piece piece = (int)pt | (c << 3);

            Pos.AddPiece(piece, sq);

            State.Key ^= piece.GetZobristPst(sq);

            if (pt == PieceTypes.Pawn)
                State.PawnStructureKey ^= piece.GetZobristPst(sq);

            State.Material.Add(piece);
        }

        /// <summary>
        /// Updates the hash key depending on a move
        /// </summary>
        /// <param name="m">The move the hash key is depending on</param>
        private void UpdateKey(Move m)
        {
            // TODO : Merge with MakeMove to avoid duplicate ifs

            var pawnKey = State.PawnStructureKey;
            var key = State.Key ^ pawnKey;

            pawnKey ^= Zobrist.GetZobristSide();
            var previousState = _stateList[PositionIndex - 1];

            if (previousState.EnPassantSquare != Squares.none)
                key ^= previousState.EnPassantSquare.File().GetZobristEnPessant();

            if (State.EnPassantSquare != Squares.none)
                key ^= State.EnPassantSquare.File().GetZobristEnPessant();

            if (m.IsNullMove())
            {
                State.Key = key ^ pawnKey;
                State.PawnStructureKey = pawnKey;
                return;
            }

            var pc = m.GetMovingPiece();
            var isPawn = pc.Type() == PieceTypes.Pawn;
            var to = m.GetToSquare();

            if (m.IsPromotionMove())
                key ^= m.GetPromotedPiece().GetZobristPst(to);
            else
            {
                if (isPawn)
                    pawnKey ^= pc.GetZobristPst(to);
                else
                    key ^= pc.GetZobristPst(to);
            }

            if (isPawn)
            {
                pawnKey ^= pc.GetZobristPst(m.GetFromSquare());
                if (m.IsEnPassantMove())
                    pawnKey ^= m.GetCapturedPiece().GetZobristPst(to + State.SideToMove.PawnPushDistance());
                else if (m.IsCaptureMove())
                    pawnKey ^= m.GetCapturedPiece().GetZobristPst(to);
            }
            else
            {
                key ^= pc.GetZobristPst(m.GetFromSquare());
                if (m.IsCaptureMove())
                    key ^= m.GetCapturedPiece().GetZobristPst(to);
                else if (m.IsCastlelingMove())
                {
                    var rook = PieceTypes.Rook.MakePiece(Pos.State.SideToMove);
                    key ^= rook.GetZobristPst(Pos.GetRookCastleFrom(to));
                    key ^= rook.GetZobristPst(to.GetRookCastleTo());
                }
            }

            // castling rights
            if (State.CastlelingRights != previousState.CastlelingRights)
            {
                key ^= previousState.CastlelingRights.GetZobristCastleling();
                key ^= State.CastlelingRights.GetZobristCastleling();
            }

            State.Key = key ^ pawnKey;
            State.PawnStructureKey = pawnKey;
        }

        private bool IsRepetition()
        {
            var repetitionCounter = 0;
            var backPosition = PositionIndex;
            while ((backPosition -= 2) >= 0)
                if (_stateList[backPosition].Key == State.Key && ++repetitionCounter == 3)
                    return true;

            return false;
        }

        private bool SetupCastleling(ReadOnlySpan<char> castleling)
        {
            // reset castleling rights to defaults
            _castleRightsMask.Fill(CastlelingRights.Any);

            if (castleling.Length == 1 && castleling[0] == '-')
                return true;

            foreach (var token in castleling)
            {
                var player = (!char.IsUpper(token)).ToInt();
                var c = char.ToUpper(token);
                var rookFile = -1;
                CastlelingSides side;

                switch (c)
                {
                    case 'K':
                        side = CastlelingSides.King;
                        break;

                    case 'Q':
                        side = CastlelingSides.Queen;
                        break;

                    default:
                        if (c.InBetween('A', 'H'))
                        {
                            _chess960 = true;
                            _xfen = false;

                            rookFile = c - (player == PlayerExtensions.Black ? 'A' : 'a');

                            side = rookFile > Pos.GetPieceSquare(PieceTypes.King, player).File()
                                ? CastlelingSides.King
                                : CastlelingSides.Queen;
                        }
                        else
                            side = CastlelingSides.None;
                        break;
                }

                if (side != CastlelingSides.None)
                    AddCastleRights(rookFile, player, side);
            }

            return true;
        }

        private void AddCastleRights(int rookFile, Player us, CastlelingSides castlelingSide)
        {
            var isKingSideCastleling = castlelingSide == CastlelingSides.King;
            var index = (!isKingSideCastleling).ToInt();

            if (rookFile == -1)
            {
                // begin temporary code

                var doStatement = new Func<Files, Files>[]
                {
                    files => --files,
                    files => ++files
                };

                var predicate = new Func<Files, Files, bool>[]
                {
                    (current, target) => current >= target,
                    (current, target) => current <= target
                };

                var (first, last) = isKingSideCastleling ? (Files.FileH, Files.FileA) : (Files.FileA, Files.FileH);

                // end temporary code

                for (var file = first; predicate[index](file, last); file = doStatement[index](file))
                {
                    if (Pos.IsPieceTypeOnSquare((Squares)((int)file + us.Side * 56), PieceTypes.Rook))
                    {
                        rookFile = (int)file; // right outermost rook for side
                        break;
                    }
                }

                _xfen = true;
            }

            // TODO : Replace with validation guarding
            if (rookFile < 0)
            {
                _xfen = false;
                return;
            }

            State.CastlelingRights |= CastlePositionalOr[index][us.Side];
            var them = ~us;
            var castlelingMask = castlelingSide.GetCastleAllowedMask(us);
            var ksq = Pos.GetPieceSquare(PieceTypes.King, us);
            _castleRightsMask[SquareExtensions.GetFlip(rookFile, them).AsInt()] -= castlelingMask;
            _castleRightsMask[SquareExtensions.GetFlip(ksq.File().AsInt(), them).AsInt()] -= castlelingMask;
            var sq = isKingSideCastleling ? Squares.g1 : Squares.c1;
            Pos.SetRookCastleFrom(SquareExtensions.GetFlip((int)sq, them), SquareExtensions.GetFlip(rookFile, them));
            Pos.SetKingCastleFrom(us, ksq, castlelingSide);

            if (ksq.File() != 4 || rookFile != (isKingSideCastleling ? 7 : 0))
                _chess960 = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int HalfMoveCount()
        {
            // TODO : This is WRONG!? :)
            return State.HalfMoveCount;
        }
    }
}