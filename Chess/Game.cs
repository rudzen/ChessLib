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
    using System.Runtime.CompilerServices;
    using System.Text;
    using Transposition;
    using Types;
    using Move = Types.Move;
    using Piece = Types.Piece;
    using Square = Types.Square;

    public sealed class Game : IGame
    {
        private const int MaxPositions = 256;

        private readonly CastlelingRights[] _castleRightsMask;

        private readonly State[] _stateList;

        private readonly StringBuilder _output;

        private bool _xfen;

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
            _xfen = false;
        }

        static Game()
        {
            Table = new TranspositionTable(256);
        }

        public State State => Pos.State;

        public Action<Piece, Square> PieceUpdated => Pos.PieceUpdated;

        public int PositionIndex { get; private set; }

        public int PositionStart { get; private set; }

        public int MoveNumber => (PositionIndex - 1) / 2 + 1;

        public BitBoard Occupied => Pos.Pieces();

        public IPosition Pos { get; }

        public GameEndTypes GameEndType { get; set; }

        public static TranspositionTable Table { get; set; }

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
            var state = _stateList[PositionIndex];
            Pos.State = state;
            state.Previous = previous;
            state.SideToMove = ~previous.SideToMove;
            state.Material.CopyFrom(previous.Material);
            state.HalfMoveCount = PositionIndex;
            state.LastMove = m;

            var to = m.GetToSquare();
            var from = m.GetFromSquare();
            var us = state.SideToMove;
            var them = ~us;

            var ksq = Pos.GetPieceSquare(PieceTypes.King, us);

            // compute in-check
            state.InCheck = Pos.IsAttacked(ksq, them);

            // compute checkers
            state.Checkers = Pos.AttacksTo(ksq);
            ksq = Pos.GetPieceSquare(PieceTypes.King, them);
            // Discover
            state.DicoveredCheckers = Pos.AttacksTo(ksq);
            
            state.CastlelingRights = previous.CastlelingRights & _castleRightsMask[from.AsInt()] & _castleRightsMask[to.AsInt()];
            state.NullMovesInRow = 0;

            // compute reversible half move count
            state.ReversibleHalfMoveCount = m.IsCaptureMove() || m.GetMovingPieceType() == PieceTypes.Pawn
                ? 0
                : previous.ReversibleHalfMoveCount + 1;

            // compute en-passant if present
            state.EnPassantSquare = m.IsDoublePush()
                ? from + m.GetMovingSide().PawnPushDistance()
                : Squares.none;

            state.Key = previous.Key;
            state.PawnStructureKey = previous.PawnStructureKey;
            state.Material.MakeMove(m);

            UpdateKey(m);

            return true;
        }

        public void TakeMove()
        {
            Pos.TakeMove(State.LastMove);
            Pos.State = Pos.State.Previous;
            if (PositionIndex == 0 || State.Previous == null)
                throw new Exception("fail");
            --PositionIndex;
            Pos.State = State.Previous;
            // Pos.State = _stateList[PositionIndex];
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
            Pos.Clear();

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

                    var pc = ((PieceTypes) pieceIndex).MakePiece(player);
                    Pos.AddPiece(pc, square);

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

            var (castlelingOk, castlelingRights) = SetupCastleling(chunk);
            if (!castlelingOk)
                return new FenError(-5, fen.Index);

            // en-passant
            chunk = fen.Chunk();

            Square enPessantSquare = chunk.Length == 1 || chunk[0] == '-' || !chunk[0].InBetween('a', 'h')
                ? Squares.none
                : chunk[1].InBetween('3', '6') ? Squares.none : new Square(chunk[1] - '1', chunk[0] - 'a').Value;

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

            PositionStart = moveNum;

            var key = Pos.GetPiecesKey();
            var pawnKey = Pos.GetPawnKey();

            if (player.IsBlack())
            {
                var k = Zobrist.GetZobristSide();
                key ^= k;
                pawnKey ^= k;
            }

            key ^= castlelingRights.GetZobristCastleling();

            if (enPessantSquare != Squares.none)
                key ^= enPessantSquare.File().GetZobristEnPessant();

            var ksq = Pos.GetPieceSquare(PieceTypes.King, player);

            Pos.State = _stateList[PositionIndex];
            var state = Pos.State;
            state.EnPassantSquare = enPessantSquare;
            state.ReversibleHalfMoveCount = halfMoveNum;
            state.SideToMove = player;
            state.CastlelingRights = castlelingRights;
            state.Checkers = Pos.AttacksTo(ksq);
            state.InCheck = Pos.IsAttacked(ksq, ~player);
            state.Key = key;
            state.PawnStructureKey = pawnKey;

            // Set hidden checkers
            ksq = Pos.GetPieceSquare(PieceTypes.King, ~player);
            // TODO : Discover
            state.DicoveredCheckers = Pos.AttacksTo(ksq);

            Pos.State = state;

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData GetFen() => Pos.GenerateFen();

        public void UpdateDrawTypes()
        {
            var gameEndType = GameEndTypes.None;
            if (IsRepetition())
                gameEndType |= GameEndTypes.Repetition;
            if (State.Material[PlayerExtensions.White.Side] <= 300 && State.Material[PlayerExtensions.Black.Side] <= 300 && Pos.BoardPieces[0].Empty() && Pos.BoardPieces[8].Empty())
                gameEndType |= GameEndTypes.MaterialDrawn;
            if (State.ReversibleHalfMoveCount >= 100)
                gameEndType |= GameEndTypes.FiftyMove;

            var moveList = Pos.GenerateMoves().GetMoves();
            foreach (var move in moveList)
            {
                if (Pos.IsLegal(move))
                    continue;
                gameEndType |= GameEndTypes.Pat;
                break;
            }

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
            _output.AppendLine($"Zobrist : 0x{State.Key.Key:X}");
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
                return Pos.GenerateMoves().Count;

            // var posKey = Pos.State.Key;
            //
            // var (found, entry) = Table.Probe(posKey);
            // if (found && entry.Depth == depth && entry.Key32 == posKey.UpperKey)
            //     return (ulong)entry.Value;

            var tot = 0ul;
            var move = MoveExtensions.EmptyMove;

            var moves = Pos.GenerateMoves().GetMoves();
            foreach (var m in moves)
            {
                MakeMove(m);
                move = m;
                tot += Perft(depth - 1);
                TakeMove();
            }

            // if (!move.IsNullMove() && tot <= int.MaxValue)
            //     Table.Store(posKey, (int)tot, Bound.Exact, (sbyte)depth, move, 0);

            return tot;
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
            var from = m.GetFromSquare();

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
                pawnKey ^= pc.GetZobristPst(from);
                if (m.IsEnPassantMove())
                    pawnKey ^= m.GetCapturedPiece().GetZobristPst(to + State.SideToMove.PawnPushDistance());
                else if (m.IsCaptureMove())
                    pawnKey ^= m.GetCapturedPiece().GetZobristPst(to);
            }
            else
            {
                key ^= pc.GetZobristPst(from);
                if (m.IsCaptureMove())
                    key ^= m.GetCapturedPiece().GetZobristPst(to);
                else if (m.IsCastlelingMove())
                {
                    var rook = PieceTypes.Rook.MakePiece(Pos.State.SideToMove);
                    var rookFrom = Pos.CastlingRookSquare(Pos.State.CastlelingRights);

                    key ^= rook.GetZobristPst(from) ^ rook.GetZobristPst(to.GetRookCastleTo());
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

        private (bool, CastlelingRights) SetupCastleling(ReadOnlySpan<char> castleling)
        {
            var castlelingRights = CastlelingRights.None;
            
            // 3. Castling availability. Compatible with 3 standards: Normal FEN standard,
            // Shredder-FEN that uses the letters of the columns on which the rooks began the game
            // instead of KQkq and also X-FEN standard that, in case of Chess960, if an inner rook
            // is associated with the castling right, the castling tag is replaced by the file
            // letter of the involved rook, as for the Shredder-FEN.
            foreach (var ca in castleling)
            {
                Square rsq;
                Player c = char.IsLower(ca) ? 1 : 0;
                var token = char.ToUpper(ca);

                switch (token)
                {
                    case 'K':
                    {
                        for (rsq = Squares.h1.RelativeSquare(c); Pos.GetPieceType(rsq) != PieceTypes.Rook; --rsq) { }

                        break;
                    }
                    case 'Q':
                    {
                        for (rsq = Squares.a1.RelativeSquare(c); Pos.GetPieceType(rsq) != PieceTypes.Rook; --rsq) { }

                        break;
                    }
                    default:
                    {
                        if (token.InBetween('A', 'H'))
                            rsq = new Square(Ranks.Rank1.RelativeRank(c), new File(token - 'A'));
                        else
                            continue;
                        break;
                    }
                }

                castlelingRights |= Pos.SetCastlingRight(c, rsq);
            }

            return (true, castlelingRights);
            
            // reset castleling rights to defaults
            // _castleRightsMask.Fill(CastlelingRights.Any);
            //
            // if (castleling.Length == 1 && castleling[0] == '-')
            //     return (true, CastlelingRights.None);
            //
            // var castlelingRights = CastlelingRights.None;
            //
            // foreach (var token in castleling)
            // {
            //     var player = (!char.IsUpper(token)).ToInt();
            //     var c = char.ToUpper(token);
            //     var rookFile = -1;
            //     CastlelingSides side;
            //
            //     switch (c)
            //     {
            //         case 'K':
            //             side = CastlelingSides.King;
            //             break;
            //
            //         case 'Q':
            //             side = CastlelingSides.Queen;
            //             break;
            //
            //         default:
            //             if (c.InBetween('A', 'H'))
            //             {
            //                 _chess960 = true;
            //                 _xfen = false;
            //
            //                 rookFile = c - (player == PlayerExtensions.Black ? 'A' : 'a');
            //
            //                 side = rookFile > Pos.GetPieceSquare(PieceTypes.King, player).File()
            //                     ? CastlelingSides.King
            //                     : CastlelingSides.Queen;
            //             }
            //             else
            //                 side = CastlelingSides.None;
            //             break;
            //     }
            //
            //     if (side != CastlelingSides.None)
            //         castlelingRights |= AddCastleRights(rookFile, player, side);
            // }
            //
            // return (true, castlelingRights);
        }

        //private CastlelingRights AddCastleRights(int rookFile, Player us, CastlelingSides castlelingSide)
        //{
        //    var isKingSideCastleling = castlelingSide == CastlelingSides.King;
        //    var index = (!isKingSideCastleling).ToInt();

        //    if (rookFile == -1)
        //    {
        //        // begin temporary code

        //        var doStatement = new Func<Files, Files>[]
        //        {
        //            files => --files,
        //            files => ++files
        //        };

        //        var predicate = new Func<Files, Files, bool>[]
        //        {
        //            (current, target) => current >= target,
        //            (current, target) => current <= target
        //        };

        //        var (first, last) = isKingSideCastleling ? (Files.FileH, Files.FileA) : (Files.FileA, Files.FileH);

        //        // end temporary code

        //        for (var file = first; predicate[index](file, last); file = doStatement[index](file))
        //        {
        //            if (Pos.IsPieceTypeOnSquare((Squares)((int)file + us.Side * 56), PieceTypes.Rook))
        //            {
        //                rookFile = (int)file; // right outermost rook for side
        //                break;
        //            }
        //        }

        //        _xfen = true;
        //    }

        //    // TODO : Replace with validation guarding
        //    if (rookFile < 0)
        //    {
        //        _xfen = false;
        //        return CastlelingRights.None;
        //    }

        //    var result = us.GetCastlePositionalOr(index);
        //    var castlelingMask = castlelingSide.GetCastleAllowedMask(us);
        //    var ksq = Pos.GetPieceSquare(PieceTypes.King, us);
        //    var them = ~us;
        //    _castleRightsMask[SquareExtensions.GetFlip(rookFile, them).AsInt()] ^= castlelingMask;
        //    _castleRightsMask[SquareExtensions.GetFlip(ksq.File().AsInt(), them).AsInt()] ^= castlelingMask;
        //    var sq = isKingSideCastleling ? Squares.g1 : Squares.c1;
        //    Pos.SetRookCastleFrom(SquareExtensions.GetFlip((int)sq, them), SquareExtensions.GetFlip(rookFile, them));
        //    Pos.SetKingCastleFrom(us, ksq, castlelingSide);

        //    if (ksq.File() != 4 || rookFile != (isKingSideCastleling ? 7 : 0))
        //        _chess960 = true;

        //    return result;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int HalfMoveCount()
        {
            // TODO : This is WRONG!? :)
            return State.HalfMoveCount;
        }
    }
}