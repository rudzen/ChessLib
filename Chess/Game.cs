/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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
    using Data;
    using Enums;
    using Extensions;
    using Fen;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Types;
    using Move = Types.Move;
    using Piece = Types.Piece;
    using Square = Types.Square;

    public sealed class Game : IEnumerable<Piece>
    {
        private const int MaxPositions = 2048;

        /// <summary>
        /// [short/long, side] castle positional | array when altering castleling rights.
        /// </summary>
        private static readonly ECastlelingRights[,] CastlePositionalOr;

        private readonly ECastlelingRights[] _castleRightsMask;

        private readonly State[] _stateList;

        private readonly StringBuilder _output;

        private bool _chess960;

        private bool _xfen;

        private int _repetitionCounter;

        static Game() => CastlePositionalOr = new[,] { { ECastlelingRights.WhiteOO, ECastlelingRights.BlackOO }, { ECastlelingRights.WhiteOOO, ECastlelingRights.BlackOOO } };

        public Game()
            : this(null) { }

        public Game(Action<Piece, Square> pieceUpdateCallback)
        {
            _castleRightsMask = new ECastlelingRights[64];
            Position = new Position(pieceUpdateCallback);
            _stateList = new State[MaxPositions];
            _output = new StringBuilder(256);

            for (var i = 0; i < _stateList.Length; i++)
                _stateList[i] = new State(Position);

            PositionIndex = 0;
            State = Position.State = _stateList[PositionIndex];
            _chess960 = false;
            _xfen = false;
        }

        public State State { get; set; }

        public int PositionIndex { get; private set; }

        public int PositionStart { get; private set; }

        public int MoveNumber => (PositionIndex - 1) / 2 + 1;

        public BitBoard Occupied => Position.Occupied;

        public IPosition Position { get; }

        public EGameEndType GameEndType { get; set; }

        /// <summary>
        /// Makes a chess move in the data structure
        /// </summary>
        /// <param name="move">The move to make</param>
        /// <returns>true if everything was fine, false if unable to progress - fx castleling position under attack</returns>
        public bool MakeMove(Move move)
        {
            if (move.IsNullMove())
                return false;

            Position.MakeMove(move);

            // commented because of missing implementations
            if (!move.IsCastlelingMove() && Position.IsAttacked(Position.GetPieceSquare(EPieceType.King, State.SideToMove), ~State.SideToMove))
            {
                Position.TakeMove(move);
                return false;
            }

            // advances the position
            var previous = _stateList[PositionIndex++];
            State = Position.State = _stateList[PositionIndex];
            State.SideToMove = ~previous.SideToMove;
            State.Material = previous.Material;
            State.LastMove = move;

            // compute in-check
            Position.InCheck = Position.IsAttacked(Position.GetPieceSquare(EPieceType.King, State.SideToMove), ~State.SideToMove);
            State.CastlelingRights = _stateList[PositionIndex - 1].CastlelingRights & _castleRightsMask[move.GetFromSquare().ToInt()] & _castleRightsMask[move.GetToSquare().ToInt()];
            State.NullMovesInRow = 0;

            // compute reversible half move count
            State.ReversibleHalfMoveCount = move.IsCaptureMove() || move.GetMovingPieceType() == EPieceType.Pawn
                ? 0
                : previous.ReversibleHalfMoveCount + 1;

            // compute en-passant if present
            State.EnPassantSquare = move.IsDoublePush()
                ? move.GetFromSquare() + (move.GetMovingSide() == PlayerExtensions.White ? EDirection.North : EDirection.South)
                : ESquare.none;

            State.Key = previous.Key;
            State.PawnStructureKey = previous.PawnStructureKey;

            UpdateKey(move);
            State.Material.MakeMove(move);

            State.GenerateMoves();

            return true;
        }

        public void TakeMove()
        {
            // careful.. NO check for invalid PositionIndex.. make sure it's always counted correctly
            Position.TakeMove(State.LastMove);
            --PositionIndex;
            State = Position.State = _stateList[PositionIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenError NewGame(string fen = Fen.Fen.StartPositionFen) => SetFen(fen);

        public FenError SetFen(FenData fenData) => SetFen(fenData.Fen);

        /// <summary>
        /// Apply a FEN string board setup to the board structure.
        /// *EXCEPTION FREE FUNCTION*
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
        public FenError SetFen(string fenString, bool validate = false)
        {
            // TODO : Replace with stream at some point

            // basic validation, catches format errors
            if (validate)
                Fen.Fen.Validate(fenString);

            foreach (var square in Occupied)
                Position.RemovePiece(square, Position.BoardLayout[square.ToInt()]);

            for (var i = 0; i <= PositionIndex; i++)
                _stateList[i].Clear();

            Position.Clear();

            // ReSharper disable once AssignNullToNotNullAttribute
            var fen = new FenData(fenString);

            Player player;
            var c = fen.GetAdvance();

            var f = 1; // file (column)
            var r = 8; // rank (row)

            // map pieces to data structure
            while (c != 0 && !(f == 9 && r == 1))
            {
                if (char.IsDigit(c))
                {
                    f += c - '0';
                    if (f > 9)
                        return new FenError(-1, fen.GetIndex());

                    c = fen.GetAdvance();
                    continue;
                }

                if (c == '/')
                {
                    if (f != 9)
                        return new FenError(-2, fen.GetIndex());

                    r--;
                    f = 1;
                    c = fen.GetAdvance();
                    continue;
                }

                var pieceIndex = PieceExtensions.PieceChars.IndexOf(c);

                if (pieceIndex == -1)
                    return new FenError(-3, fen.GetIndex());

                player = char.IsLower(PieceExtensions.PieceChars[pieceIndex]);

                var square = new Square(r - 1, f - 1);

                AddPiece(square, player, (EPieceType)pieceIndex);

                c = fen.GetAdvance();

                f++;
            }

            if (!Fen.Fen.IsDelimiter(c))
                return new FenError(-4, fen.GetIndex());

            c = fen.GetAdvance();

            player = c == 'w' ? 0 : 1;

            if (player == -1)
                return new FenError(-4, fen.GetIndex());

            if (!Fen.Fen.IsDelimiter(fen.GetAdvance()))
                return new FenError(-5, fen.GetIndex());

            if (SetupCastleling(fen) == -1)
                return new FenError(-5, fen.GetIndex());

            if (!Fen.Fen.IsDelimiter(fen.GetAdvance()))
                return new FenError(-6, fen.GetIndex());

            State.EnPassantSquare = fen.GetEpSquare();

            // temporary.. the whole method should be using this, but this will do for now.

            var moveCounters = fen.Fen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var first = moveCounters[moveCounters.Length - 2];
            var second = moveCounters[moveCounters.Length - 1];

            second.ToIntegral(out int number);

            if (number > 0)
                number -= 1;

            PositionIndex = number;
            PositionStart = number;

            first.ToIntegral(out number);

            State = Position.State = _stateList[PositionIndex];

            State.ReversibleHalfMoveCount = number;

            State.SideToMove = player;

            if (!player.IsWhite())
            {
                /* black */
                State.Key ^= Zobrist.GetZobristSide();
                State.PawnStructureKey ^= Zobrist.GetZobristSide();
            }

            State.Key ^= Zobrist.GetZobristCastleling(State.CastlelingRights);

            if (State.EnPassantSquare != ESquare.none)
                State.Key ^= Zobrist.GetZobristEnPessant(State.EnPassantSquare.File().ToInt());

            Position.InCheck = Position.IsAttacked(Position.GetPieceSquare(EPieceType.King, State.SideToMove), ~State.SideToMove);

            State.GenerateMoves(true);

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData GetFen() => State.GenerateFen(Position.BoardLayout, HalfMoveCount());

        /// <summary>
        /// Converts a move data type to move notation string format which chess engines understand.
        /// e.g. "a2a4", "a7a8q"
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="output">The string builder used to generate the string with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToString(Move move, StringBuilder output)
        {
            if (_chess960 || move.IsCastlelingMove())
            {
                if (_xfen && move.GetToSquare() == ECastleling.Long.GetKingCastleTo(move.GetMovingSide()))
                    output.Append(ECastleling.Long.GetCastlelingString());
                else if (_xfen)
                    output.Append(ECastleling.Short.GetCastlelingString());
                else
                {
                    output.Append(move.GetFromSquare().ToString());
                    output.Append(Position.GetRookCastleFrom(move.GetToSquare()).ToString());
                }

                return;
            }

            output.Append(move.GetFromSquare().ToString());
            output.Append(move.GetToSquare().ToString());

            if (move.IsPromotionMove())
                output.Append(move.GetPromotedPiece().GetPromotionChar());
        }

        public void UpdateDrawTypes()
        {
            var gameEndType = EGameEndType.None;
            if (!State.Moves.Any(move => State.IsLegal(move)))
                gameEndType |= EGameEndType.Pat;
            if (IsRepetition())
                gameEndType |= EGameEndType.Repetition;
            if (State.Material[PlayerExtensions.White.Side] <= 300 && State.Material[PlayerExtensions.Black.Side] <= 300 && Position.BoardPieces[0].Empty() && Position.BoardPieces[8].Empty())
                gameEndType |= EGameEndType.MaterialDrawn;
            if (State.ReversibleHalfMoveCount >= 100)
                gameEndType |= EGameEndType.FiftyMove;
            GameEndType = gameEndType;
        }

        public override string ToString()
        {
            const string seperator = "\n  +---+---+---+---+---+---+---+---+\n";
            const char splitter = '|';
            const char space = ' ';
            _output.Clear();
            _output.Append(seperator);
            for (var rank = ERank.Rank8; rank >= ERank.Rank1; rank--)
            {
                _output.Append((int)rank + 1);
                _output.Append(space);
                for (var file = EFile.FileA; file <= EFile.FileH; file++)
                {
                    var piece = Position.GetPiece(new Square((int)file, (int)rank));
                    _output.AppendFormat("{0}{1}{2}{1}", splitter, space, piece.GetPieceChar());
                }

                _output.Append(splitter);
                _output.Append(seperator);
            }

            _output.Append("    a   b   c   d   e   f   g   h\n");
            _output.Append("Zobrist : ");
            _output.Append($"0x{State.Key:X}\n");
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
            State.GenerateMoves();
            if (depth == 1)
                return (ulong)State.Moves.Count;

            ulong tot = 0;

            foreach (var move in State.Moves)
            {
                MakeMove(move);
                tot += Perft(depth - 1);
                TakeMove();
            }

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

            State.Key ^= Zobrist.GetZobristPst(piece, square);

            if (pieceType == EPieceType.Pawn)
                State.PawnStructureKey ^= Zobrist.GetZobristPst(piece, square);

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
                key ^= Zobrist.GetZobristEnPessant(_stateList[PositionIndex - 1].EnPassantSquare.File().ToInt());

            if (State.EnPassantSquare != ESquare.none)
                key ^= Zobrist.GetZobristEnPessant(State.EnPassantSquare.File().ToInt());

            if (move.IsNullMove())
            {
                key ^= pawnKey;
                State.Key = key;
                State.PawnStructureKey = pawnKey;
                return;
            }

            var pawnPiece = move.GetMovingPieceType() == EPieceType.Pawn;

            if (pawnPiece)
                pawnKey ^= Zobrist.GetZobristPst(move.GetMovingPiece(), move.GetFromSquare());
            else
                key ^= Zobrist.GetZobristPst(move.GetMovingPiece(), move.GetFromSquare());

            var squareTo = move.GetToSquare();

            if (move.IsPromotionMove())
                key ^= Zobrist.GetZobristPst(move.GetPromotedPiece(), squareTo);
            else
            {
                if (pawnPiece)
                    pawnKey ^= Zobrist.GetZobristPst(move.GetMovingPiece(), squareTo);
                else
                    key ^= Zobrist.GetZobristPst(move.GetMovingPiece(), squareTo);
            }

            if (pawnPiece && move.IsEnPassantMove())
                pawnKey ^= Zobrist.GetZobristPst(move.GetCapturedPiece().Type() + (State.SideToMove << 3), squareTo + (State.SideToMove.Side == 0 ? 8 : -8));
            else if (move.IsCaptureMove())
            {
                if (pawnPiece)
                    pawnKey ^= Zobrist.GetZobristPst(move.GetCapturedPiece(), squareTo);
                else
                    key ^= Zobrist.GetZobristPst(move.GetCapturedPiece(), squareTo);
            }
            else if (move.IsCastlelingMove())
            {
                var piece = (EPieces)(EPieceType.Rook + move.GetSideMask());
                key ^= Zobrist.GetZobristPst(piece, Position.GetRookCastleFrom(squareTo));
                key ^= Zobrist.GetZobristPst(piece, squareTo.GetRookCastleTo());
            }

            // castleling
            // castling rights
            if (State.CastlelingRights != _stateList[PositionIndex - 1].CastlelingRights)
            {
                key ^= Zobrist.GetZobristCastleling(_stateList[PositionIndex - 1].CastlelingRights);
                key ^= Zobrist.GetZobristCastleling(State.CastlelingRights);
            }

            key ^= pawnKey;
            State.Key = key;
            State.PawnStructureKey = pawnKey;
        }

        private bool IsRepetition()
        {
            _repetitionCounter = 1;
            var backPosition = PositionIndex;
            while ((backPosition -= 2) >= 0)
            {
                if (_stateList[backPosition].Key != State.Key)
                    continue;
                if (++_repetitionCounter == 3)
                    return true;
            }

            return false;
        }

        private int SetupCastleling(IFenData fen)
        {
            // reset castleling rights to defaults
            _castleRightsMask.Fill(ECastlelingRights.Any);

            if (fen.Get() == '-')
            {
                fen.Advance();
                return 0;
            }

            // List to gather functions for castleling rights addition.
            var castleFunctions = new List<Action>(4);

            while (fen.Get() != 0 && fen.Get() != ' ')
            {
                var c = fen.Get();

                if (c.InBetween('A', 'H'))
                {
                    _chess960 = true;
                    _xfen = false;

                    // ReSharper disable once HeapView.ClosureAllocation
                    var rookFile = c - 'A';

                    if (rookFile > Position.GetPieceSquare(EPieceType.King, PlayerExtensions.White).File())
                        castleFunctions.Add(() => AddShortCastleRights(rookFile, PlayerExtensions.White));
                    else
                        castleFunctions.Add(() => AddLongCastleRights(rookFile, PlayerExtensions.White));
                }
                else if (c.InBetween('a', 'h'))
                {
                    _chess960 = true;
                    _xfen = false;

                    // ReSharper disable once HeapView.ClosureAllocation
                    var rookFile = c - 'a';

                    if (rookFile > Position.GetPieceSquare(EPieceType.King, PlayerExtensions.Black).File())
                        castleFunctions.Add(() => AddShortCastleRights(rookFile, PlayerExtensions.Black));
                    else
                        castleFunctions.Add(() => AddLongCastleRights(rookFile, PlayerExtensions.Black));
                }
                else
                {
                    switch (c)
                    {
                        case 'K':
                            castleFunctions.Add(() => AddShortCastleRights(-1, PlayerExtensions.White));
                            break;

                        case 'Q':
                            castleFunctions.Add(() => AddLongCastleRights(-1, PlayerExtensions.White));
                            break;

                        case 'k':
                            castleFunctions.Add(() => AddShortCastleRights(-1, PlayerExtensions.Black));
                            break;

                        case 'q':
                            castleFunctions.Add(() => AddLongCastleRights(-1, PlayerExtensions.Black));
                            break;

                        case '-':
                            break;

                        default:
                            return -1;
                    }
                }

                fen.Advance();
            }

            if (castleFunctions.Count != 0)
            {
                // invoke the gathered castleling configure functions
                foreach (var castleFunction in castleFunctions)
                    castleFunction();
            }

            return 0;
        }

        private void AddShortCastleRights(int rookFile, Player side)
        {
            if (rookFile == -1)
            {
                for (var file = EFile.FileH; file >= EFile.FileA; file--)
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
            var castlelingMask = ECastleling.Short.GetCastleAllowedMask(side);
            var ksq = Position.GetPieceSquare(EPieceType.King, side);
            _castleRightsMask[SquareExtensions.GetFlip(rookFile, them).ToInt()] -= castlelingMask;
            _castleRightsMask[SquareExtensions.GetFlip(ksq.File().ToInt(), them).ToInt()] -= castlelingMask;
            Position.SetRookCastleFrom(SquareExtensions.GetFlip((int)ESquare.g1, them), SquareExtensions.GetFlip(rookFile, them));
            Position.SetKingCastleFrom(side, ksq, ECastleling.Short);

            if (ksq.File() != 4 || rookFile != 7)
                _chess960 = true;
        }

        private void AddLongCastleRights(int rookFile, Player side)
        {
            if (rookFile == -1)
            {
                for (var file = EFile.FileA; file <= EFile.FileH; file++)
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
            var castlelingMask = ECastleling.Long.GetCastleAllowedMask(side);
            var ksq = Position.GetPieceSquare(EPieceType.King, side);
            _castleRightsMask[SquareExtensions.GetFlip(rookFile, them).ToInt()] -= castlelingMask;
            _castleRightsMask[SquareExtensions.GetFlip(ksq.File().ToInt(), them).ToInt()] -= castlelingMask;
            Position.SetRookCastleFrom(SquareExtensions.GetFlip((int)ESquare.c1, them), SquareExtensions.GetFlip(rookFile, them));
            Position.SetKingCastleFrom(side, ksq, ECastleling.Long);

            if (ksq.File() != 4 || rookFile != 0)
                _chess960 = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int HalfMoveCount()
        {
            // TODO : This is WRONG!? :)
            return PositionIndex;
        }
    }
}