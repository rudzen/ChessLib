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
        private const int MaxPositions = 2 << 11;

        /// <summary>
        /// [short/long, side] castle positional | array when altering castleling rights.
        /// </summary>
        private static readonly CastlelingRights[,] CastlePositionalOr = {
            {CastlelingRights.WhiteOo, CastlelingRights.BlackOo},
            {CastlelingRights.WhiteOoo, CastlelingRights.BlackOoo}
        };

        private readonly CastlelingRights[] _castleRightsMask;

        private readonly State[] _stateList;

        private readonly StringBuilder _output;

        private bool _chess960;

        private bool _xfen;

        public Game(IPosition position)
        {
            _castleRightsMask = new CastlelingRights[64];
            Position = position;
            _stateList = new State[MaxPositions];
            _output = new StringBuilder(256);

            for (var i = 0; i < _stateList.Length; i++)
                _stateList[i] = new State();

            PositionIndex = 0;
            Position.State = _stateList[PositionIndex];
            _chess960 = false;
            _xfen = false;
        }

        public State State => Position.State;

        public Action<Piece, Square> PieceUpdated => Position.PieceUpdated;

        public int PositionIndex { get; private set; }

        public int PositionStart { get; private set; }

        public int MoveNumber => (PositionIndex - 1) / 2 + 1;

        public BitBoard Occupied => Position.Pieces();

        public IPosition Position { get; }

        public GameEndTypes GameEndType { get; set; }

        public static TranspositionTable Table { get; set; } = new TranspositionTable(256);

        /// <summary>
        /// Makes a chess move in the data structure
        /// </summary>
        /// <param name="move">The move to make</param>
        /// <returns>true if everything was fine, false if unable to progress - fx castleling position under attack</returns>
        public bool MakeMove(Move move)
        {
            if (!Position.MakeMove(move))
                return false;

            // advances the position
            var previous = _stateList[PositionIndex++];
            Position.State = _stateList[PositionIndex];
            State.SideToMove = ~previous.SideToMove;
            State.Material = previous.Material;
            State.HalfMoveCount = PositionIndex;
            State.LastMove = move;

            // compute in-check
            Position.InCheck = Position.IsAttacked(Position.GetPieceSquare(EPieceType.King, State.SideToMove), ~State.SideToMove);
            State.CastlelingRights = _stateList[PositionIndex - 1].CastlelingRights & _castleRightsMask[move.GetFromSquare().AsInt()] & _castleRightsMask[move.GetToSquare().AsInt()];
            State.NullMovesInRow = 0;

            // compute reversible half move count
            State.ReversibleHalfMoveCount = move.IsCaptureMove() || move.GetMovingPieceType() == EPieceType.Pawn
                ? 0
                : previous.ReversibleHalfMoveCount + 1;

            // compute en-passant if present
            State.EnPassantSquare = move.IsDoublePush()
                ? move.GetFromSquare() + move.GetMovingSide().PawnPushDistance()
                : ESquare.none;

            State.Key = previous.Key;
            State.PawnStructureKey = previous.PawnStructureKey;
            State.Material.MakeMove(move);

            UpdateKey(move);

            return true;
        }

        public void TakeMove()
        {
            // careful.. NO check for invalid PositionIndex.. make sure it's always counted correctly
            Position.TakeMove(State.LastMove);
            if (PositionIndex == 0)
                throw new Exception("fail");
            --PositionIndex;
            Position.State = _stateList[PositionIndex];
            State.HalfMoveCount = PositionIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenError NewGame(string fen = Fen.Fen.StartPositionFen) => SetFen(new FenData(fen));

        /// <summary>
        /// Apply a FEN string board setup to the board structure.
        /// </summary>
        /// <param name="fenString">The string to set</param>
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
                Position.RemovePiece(square, Position.BoardLayout[square.AsInt()]);
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

                    AddPiece(square, player, (EPieceType)pieceIndex);

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
                State.EnPassantSquare = ESquare.none;
            else
                State.EnPassantSquare = chunk[1].InBetween('3', '6') ? ESquare.none : new Square(chunk[1] - '1', chunk[0] - 'a').Value;

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

            Position.State = _stateList[PositionIndex];

            State.ReversibleHalfMoveCount = halfMoveNum;

            State.SideToMove = player;

            if (player.IsBlack())
            {
                State.Key ^= Zobrist.GetZobristSide();
                State.PawnStructureKey ^= Zobrist.GetZobristSide();
            }

            State.Key ^= State.CastlelingRights.GetZobristCastleling();

            if (State.EnPassantSquare != ESquare.none)
                State.Key ^= State.EnPassantSquare.File().GetZobristEnPessant();

            Position.InCheck = Position.IsAttacked(Position.GetPieceSquare(EPieceType.King, player), ~player);

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData GetFen() => Position.GenerateFen();

        /// <summary>
        /// Converts a move data type to move notation string format which chess engines understand.
        /// e.g. "a2a4", "a7a8q"
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="output">The string builder used to generate the string with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToString(Move move, StringBuilder output)
        {
            if (!_chess960 && !move.IsCastlelingMove())
                output.Append(move.ToString());
            else
            {
                if (_xfen && move.GetToSquare() == CastlelingSides.Queen.GetKingCastleTo(move.GetMovingSide()))
                    output.Append(CastlelingSides.Queen.GetCastlelingString());
                else if (_xfen)
                    output.Append(CastlelingSides.King.GetCastlelingString());
                else
                {
                    output.Append(move.GetFromSquare().ToString());
                    output.Append(Position.GetRookCastleFrom(move.GetToSquare()).ToString());
                }
            }
        }

        public void UpdateDrawTypes()
        {
            var gameEndType = GameEndTypes.None;
            if (IsRepetition())
                gameEndType |= GameEndTypes.Repetition;
            if (State.Material[PlayerExtensions.White.Side] <= 300 && State.Material[PlayerExtensions.Black.Side] <= 300 && Position.BoardPieces[0].Empty() && Position.BoardPieces[8].Empty())
                gameEndType |= GameEndTypes.MaterialDrawn;
            if (State.ReversibleHalfMoveCount >= 100)
                gameEndType |= GameEndTypes.FiftyMove;

            var moveList = Position.GenerateMoves();
            if (!moveList.Any(move => Position.IsLegal(move)))
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
            for (var rank = ERank.Rank8; rank >= ERank.Rank1; rank--)
            {
                _output.Append((int)rank + 1);
                _output.Append(space);
                for (var file = Files.FileA; file <= Files.FileH; file++)
                {
                    var piece = Position.GetPiece(new Square(rank, file));
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

        public IEnumerator<Piece> GetEnumerator() => Position.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard OccupiedBySide(Player side) => Position.OccupiedBySide[side.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player CurrentPlayer() => State.SideToMove;

        public ulong Perft(int depth)
        {
            var moveList = Position.GenerateMoves();
            if (depth == 1)
                return (ulong)moveList.Count;

            //var (found, entry) = Table.Probe(Position.State.Key);

            //if (found && entry.Key32 == (uint)(Position.State.Key >> 32) && entry.Depth == depth && entry.StaticValue == int.MinValue)
            //    return (ulong)entry.Value;

            var tot = 0ul;

            foreach (var move in moveList)
            {
                if (MakeMove(move))
                {
                    tot += Perft(depth - 1);
                    TakeMove();
                }
            }

            //if (move != MoveExtensions.EmptyMove)
            //    Table.Store(Position.State.Key, (int)tot, Bound.Exact, (sbyte)depth, move, int.MinValue);

            return tot;
        }

        /// <summary>
        /// Adds a piece to the board, and updates the hash keys if needed
        /// </summary>
        /// <param name="square">The square for the piece</param>
        /// <param name="side">For which side the piece is to be added</param>
        /// <param name="pieceType">The type of piece to add</param>
        private void AddPiece(Square square, Player side, EPieceType pieceType)
        {
            Piece piece = (int)pieceType | (side << 3);

            Position.AddPiece(piece, square);

            State.Key ^= piece.GetZobristPst(square);

            if (pieceType == EPieceType.Pawn)
                State.PawnStructureKey ^= piece.GetZobristPst(square);

            State.Material.Add(piece);
        }

        /// <summary>
        /// Updates the hash key depending on a move
        /// </summary>
        /// <param name="move">The move the hash key is depending on</param>
        private void UpdateKey(Move move)
        {
            // TODO : Merge with MakeMove to avoid duplicate ifs

            var pawnKey = State.PawnStructureKey;
            var key = State.Key ^ pawnKey;
            pawnKey ^= Zobrist.GetZobristSide();

            if (_stateList[PositionIndex - 1].EnPassantSquare != ESquare.none)
                key ^= _stateList[PositionIndex - 1].EnPassantSquare.File().GetZobristEnPessant();

            if (State.EnPassantSquare != ESquare.none)
                key ^= State.EnPassantSquare.File().GetZobristEnPessant();

            if (move.IsNullMove())
            {
                State.Key = key ^ pawnKey;
                State.PawnStructureKey = pawnKey;
                return;
            }

            var pawnPiece = move.GetMovingPieceType() == EPieceType.Pawn;
            var squareTo = move.GetToSquare();

            if (move.IsPromotionMove())
                key ^= move.GetPromotedPiece().GetZobristPst(squareTo);
            else
            {
                if (pawnPiece)
                    pawnKey ^= move.GetMovingPiece().GetZobristPst(squareTo);
                else
                    key ^= move.GetMovingPiece().GetZobristPst(squareTo);
            }

            if (pawnPiece)
            {
                pawnKey ^= move.GetMovingPiece().GetZobristPst(move.GetFromSquare());
                if (move.IsEnPassantMove())
                    pawnKey ^= move.GetCapturedPiece().GetZobristPst(squareTo + State.SideToMove.PawnPushDistance());
                else if (move.IsCaptureMove())
                    pawnKey ^= move.GetCapturedPiece().GetZobristPst(squareTo);
            }
            else
            {
                key ^= move.GetMovingPiece().GetZobristPst(move.GetFromSquare());
                if (move.IsCaptureMove())
                    key ^= move.GetCapturedPiece().GetZobristPst(squareTo);
                else if (move.IsCastlelingMove())
                {
                    var piece = EPieceType.Rook.MakePiece(Position.State.SideToMove);
                    key ^= piece.GetZobristPst(Position.GetRookCastleFrom(squareTo));
                    key ^= piece.GetZobristPst(squareTo.GetRookCastleTo());
                }
            }

            // castling rights
            if (State.CastlelingRights != _stateList[PositionIndex - 1].CastlelingRights)
            {
                key ^= _stateList[PositionIndex - 1].CastlelingRights.GetZobristCastleling();
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

        private bool SetupCastleling(ReadOnlySpan<char> castlelingSpan)
        {
            // reset castleling rights to defaults
            _castleRightsMask.Fill(CastlelingRights.Any);

            if (castlelingSpan.Length == 1 && castlelingSpan[0] == '-')
                return true;

            foreach (var c in castlelingSpan)
            {
                if (c.InBetween('A', 'H'))
                {
                    _chess960 = true;
                    _xfen = false;

                    var rookFile = c - 'A';

                    if (rookFile > Position.GetPieceSquare(EPieceType.King, PlayerExtensions.White).File())
                        AddShortCastleRights(rookFile, PlayerExtensions.White);
                    else
                        AddLongCastleRights(rookFile, PlayerExtensions.White);
                }
                else if (c.InBetween('a', 'h'))
                {
                    _chess960 = true;
                    _xfen = false;

                    var rookFile = c - 'a';

                    if (rookFile > Position.GetPieceSquare(EPieceType.King, PlayerExtensions.Black).File())
                        AddShortCastleRights(rookFile, PlayerExtensions.Black);
                    else
                        AddLongCastleRights(rookFile, PlayerExtensions.Black);
                }
                else
                {
                    switch (c)
                    {
                        case 'K':
                            AddShortCastleRights(-1, PlayerExtensions.White);
                            break;

                        case 'Q':
                            AddLongCastleRights(-1, PlayerExtensions.White);
                            break;

                        case 'k':
                            AddShortCastleRights(-1, PlayerExtensions.Black);
                            break;

                        case 'q':
                            AddLongCastleRights(-1, PlayerExtensions.Black);
                            break;

                        case '-':
                            break;

                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        private void AddShortCastleRights(int rookFile, Player side)
        {
            if (rookFile == -1)
            {
                for (var file = Files.FileH; file >= Files.FileA; file--)
                {
                    if (Position.IsPieceTypeOnSquare((ESquare)((int)file + side.Side * 56), EPieceType.Rook))
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

            State.CastlelingRights |= CastlePositionalOr[0, side.Side];
            var them = ~side;
            var castlelingMask = CastlelingSides.King.GetCastleAllowedMask(side);
            var ksq = Position.GetPieceSquare(EPieceType.King, side);
            _castleRightsMask[SquareExtensions.GetFlip(rookFile, them).AsInt()] -= castlelingMask;
            _castleRightsMask[SquareExtensions.GetFlip(ksq.File().AsInt(), them).AsInt()] -= castlelingMask;
            Position.SetRookCastleFrom(SquareExtensions.GetFlip((int)ESquare.g1, them), SquareExtensions.GetFlip(rookFile, them));
            Position.SetKingCastleFrom(side, ksq, CastlelingSides.King);

            if (ksq.File() != 4 || rookFile != 7)
                _chess960 = true;
        }

        private void AddLongCastleRights(int rookFile, Player side)
        {
            if (rookFile == -1)
            {
                for (var file = Files.FileA; file <= Files.FileH; file++)
                {
                    if (Position.IsPieceTypeOnSquare((ESquare)(int)(file + side.Side * 56), EPieceType.Rook))
                    {
                        rookFile = (int)file; // left outermost rook for side
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

            State.CastlelingRights |= CastlePositionalOr[1, side.Side];
            var them = ~side;
            var castlelingMask = CastlelingSides.Queen.GetCastleAllowedMask(side);
            var ksq = Position.GetPieceSquare(EPieceType.King, side);
            _castleRightsMask[SquareExtensions.GetFlip(rookFile, them).AsInt()] -= castlelingMask;
            _castleRightsMask[SquareExtensions.GetFlip(ksq.File().AsInt(), them).AsInt()] -= castlelingMask;
            Position.SetRookCastleFrom(SquareExtensions.GetFlip((int)ESquare.c1, them), SquareExtensions.GetFlip(rookFile, them));
            Position.SetKingCastleFrom(side, ksq, CastlelingSides.Queen);

            if (ksq.File() != 4 || rookFile != 0)
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