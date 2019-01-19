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

// ReSharper disable RedundantCheckBeforeAssignment

using System.Threading.Tasks;
using Rudz.Chess.Fen;

namespace Rudz.Chess
{
    using Data;
    using Enums;
    using Extensions;
    using Properties;
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
        [NotNull]
        private static readonly ECastlelingRights[,] CastlePositionalOr;

        [NotNull]
        private readonly ECastlelingRights[] _castleRightsMask = new ECastlelingRights[64];

        [NotNull]
        private readonly State[] _stateList;

        private readonly StringBuilder _output = new StringBuilder(256);

        private bool _chess960;

        private bool _xfen;

        private int _repetitionCounter;

        static Game() => CastlePositionalOr = new[,] { { ECastlelingRights.WhiteKing, ECastlelingRights.BlackKing }, { ECastlelingRights.WhiteQueen, ECastlelingRights.BlackQueen } };

        public Game()
            : this(null) { }

        public Game(Action<Piece, Square> pieceUpdateCallback)
        {
            Position = new Position(pieceUpdateCallback);
            _stateList = new State[MaxPositions];

            for (int i = 0; i < _stateList.Length; i++)
                _stateList[i] = new State(Position);

            PositionIndex = 0;
            State = _stateList[PositionIndex];
            _chess960 = false;
            _xfen = false;
        }

        [NotNull]
        public State State { get; private set; }

        public int PositionIndex { get; private set; }

        public int PositionStart { get; private set; }

        public int MoveNumber => (PositionIndex - 1) / 2 + 1;

        public BitBoard Occupied => Position.Occupied;

        [NotNull]
        public Position Position { get; }

        public EGameEndType GameEndType { get; set; }

        public Task<bool> MakeMoveAsync(Move move) => Task.FromResult(MakeMove(move));

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
            if (!move.IsCastlelingMove() && Position.IsAttacked(Position.KingSquares[State.SideToMove.Side], ~State.SideToMove))
            {
                Position.TakeMove(move);
                return false;
            }

            // advances the position
            State previous = _stateList[PositionIndex++];
            State = _stateList[PositionIndex];
            State.SideToMove = ~previous.SideToMove;
            State.Material = previous.Material;
            State.LastMove = move;

            // compute in-check
            State.InCheck = Position.IsAttacked(Position.KingSquares[State.SideToMove.Side], ~State.SideToMove);
            State.CastlelingRights = _stateList[PositionIndex - 1].CastlelingRights & _castleRightsMask[move.GetFromSquare().ToInt()] & _castleRightsMask[move.GetToSquare().ToInt()];
            State.NullMovesInRow = 0;

            // compute reversible half move count
            State.ReversibleHalfMoveCount = move.IsCaptureMove() || move.GetMovingPieceType() == EPieceType.Pawn
                ? 0
                : previous.ReversibleHalfMoveCount + 1;

            // compute en-passant if present
            State.EnPassantSquare = move.IsDoublePush()
                ? (BitBoard)(move.GetToSquare().BitBoardSquare().First() + State.SideToMove.PawnPushDistance())
                : 0ul;

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
            State = _stateList[PositionIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenError NewGame([CanBeNull] string fen = Fen.Fen.StartPositionFen) => SetFen(fen);

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
        /// -9 = FEN lenght exceeding maximum
        /// </returns>
        public FenError SetFen([CanBeNull] string fenString, bool validate = false)
        {
            // TODO : Replace with stream at some point

            // basic validation, catches format errors
            if (validate && !Fen.Fen.Validate(fenString))
                return new FenError(-9, 0);

            foreach (Square square in Occupied)
                Position.RemovePiece(square, Position.BoardLayout[square.ToInt()]);

            for (int i = 0; i <= PositionIndex; i++)
                _stateList[i].Clear();

            Position.Clear();

            // ReSharper disable once AssignNullToNotNullAttribute
            FenData fen = new FenData(fenString);

            Player player;
            char c = fen.GetAdvance();

            int f = 1; // file (column)
            int r = 8; // rank (row)

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

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (c)
                {
                    case '/' when f != 9:
                        return new FenError(-2, fen.GetIndex());

                    case '/':
                        r--;
                        f = 1;
                        c = fen.GetAdvance();
                        continue;
                }

                int pieceIndex = PieceExtensions.PieceChars.IndexOf(c);

                if (pieceIndex == -1)
                    return new FenError(-3, fen.GetIndex());

                player = char.IsLower(PieceExtensions.PieceChars[pieceIndex]);

                Square square = new Square(r - 1, f - 1);

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

            Square epSq = fen.GetEpSquare();

            if (epSq == ESquare.fail)
                return new FenError(-6, fen.GetIndex());

            // temporary.. the whole method should be using this, but this will do for now.
            string first = string.Empty;
            string second = string.Empty;

            var moveCounters = fen.Fen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            first = moveCounters[moveCounters.Length - 2];
            second = moveCounters[moveCounters.Length - 1];

            second.ToIntegral(out int number);

            if (number > 0)
                number -= 1;

            PositionIndex = number;
            PositionStart = number;

            first.ToIntegral(out number);

            State = _stateList[PositionIndex];

            State.ReversibleHalfMoveCount = number;

            State.SideToMove = player;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (epSq.Value)
            {
                case ESquare.none:
                    State.EnPassantSquare = 0;
                    break;

                default:
                    State.EnPassantSquare = epSq;
                    break;
            }

            if (!player.IsWhite())
            {
                /* black */
                State.Key ^= Zobrist.GetZobristSide();
                State.PawnStructureKey ^= Zobrist.GetZobristSide();
            }

            State.Key ^= Zobrist.GetZobristCastleling(State.CastlelingRights);

            if (State.EnPassantSquare)
                State.Key ^= Zobrist.GetZobristEnPessant(BitBoards.First(State.EnPassantSquare).File());

            State.InCheck = Position.IsAttacked(Position.KingSquares[player.Side], ~State.SideToMove);

            State.GenerateMoves(true);
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData GetFen() => State.GenerateFen(Position.BoardLayout, HalfMoveCount());

        /// <summary>
        /// Converts a move data type to move notation string format which chess engines understand.
        /// e.g. "a2a4", "a7a8q"
        /// </summary>
        /// <param name="move">
        /// The move to convert
        /// </param>
        /// <param name="output">
        /// The stringbuilder used to generate the string with
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToString(Move move, [NotNull] StringBuilder output)
        {
            if (_chess960 || move.IsCastlelingMove())
            {
                if (_xfen && move.GetToSquare() == ECastleling.Long.GetKingCastleTo(move.GetMovingSide()))
                {
                    output.Append(ECastleling.Long.GetCastlelingString());
                }
                else if (_xfen)
                {
                    output.Append(ECastleling.Short.GetCastlelingString());
                }
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
            EGameEndType gameEndType = EGameEndType.None;
            if (!State.Moves.Any(move => State.IsLegal(move)))
                gameEndType |= EGameEndType.Pat;
            if (IsRepetition())
                gameEndType |= EGameEndType.Repetition;
            if (State.Material.MaterialValueBlack <= 300 && State.Material.MaterialValueWhite <= 300 && Position.BoardPieces[0].Empty() && Position.BoardPieces[8].Empty())
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
            for (ERank rank = ERank.Rank8; rank >= ERank.Rank1; rank--)
            {
                _output.Append((int)rank + 1);
                _output.Append(space);
                for (EFile file = EFile.FileA; file <= EFile.FileH; file++)
                {
                    _output.Append(splitter);
                    Piece piece = Position.GetPiece(new Square((int)file, (int)rank));
                    _output.Append(space);
                    _output.Append(piece.GetPieceChar());
                    _output.Append(space);
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

        //public Task<ulong> PerftAsync(int depth)
        //{
        //    return Task.FromResult(Perft(depth));
        //}

        public async Task<ulong> Perft(int depth)
        {
            State.GenerateMoves();
            if (depth == 1)
                return (ulong)State.Moves.Count;

            ulong tot = 0;

            foreach (Move move in State.Moves)
            {
                bool ok = await MakeMoveAsync(move).ConfigureAwait(false);
                if (ok)
                {
                    tot += await Perft(depth - 1).ConfigureAwait(false);
                    TakeMove();
                }
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
        /// Updates the hashkey depending on a move
        /// </summary>
        /// <param name="move">The move the hashkey is depending on</param>
        private void UpdateKey(Move move)
        {
            // TODO : Merge with MakeMove to avoid duplicate ifs

            ulong pawnKey = State.PawnStructureKey;
            ulong key = State.Key ^ pawnKey;
            pawnKey ^= Zobrist.GetZobristSide();

            if (_stateList[PositionIndex - 1].EnPassantSquare)
                key ^= Zobrist.GetZobristEnPessant(_stateList[PositionIndex - 1].EnPassantSquare.First().File());

            if (State.EnPassantSquare)
                key ^= Zobrist.GetZobristEnPessant(State.EnPassantSquare.First().File());

            if (move.IsNullMove())
            {
                key ^= pawnKey;
                State.Key = key;
                State.PawnStructureKey = pawnKey;
                return;
            }

            bool pawnPiece = move.GetMovingPieceType() == EPieceType.Pawn;

            if (pawnPiece)
                pawnKey ^= Zobrist.GetZobristPst(move.GetMovingPiece(), move.GetFromSquare());
            else
                key ^= Zobrist.GetZobristPst(move.GetMovingPiece(), move.GetFromSquare());

            Square squareTo = move.GetToSquare();

            if (move.IsPromotionMove())
            {
                key ^= Zobrist.GetZobristPst(move.GetPromotedPiece(), squareTo);
            }
            else
            {
                if (pawnPiece)
                    pawnKey ^= Zobrist.GetZobristPst(move.GetMovingPiece(), squareTo);
                else
                    key ^= Zobrist.GetZobristPst(move.GetMovingPiece(), squareTo);
            }

            if (pawnPiece && move.IsEnPassantMove())
            {
                pawnKey ^= Zobrist.GetZobristPst(move.GetCapturedPiece().Type() + (State.SideToMove << 3), squareTo + (State.SideToMove.Side == 0 ? 8 : -8));
            }
            else if (move.IsCaptureMove())
            {
                if (pawnPiece)
                    pawnKey ^= Zobrist.GetZobristPst(move.GetCapturedPiece(), squareTo);
                else
                    key ^= Zobrist.GetZobristPst(move.GetCapturedPiece(), squareTo);
            }
            else if (move.IsCastlelingMove())
            {
                EPieces piece = (EPieces)(EPieceType.Rook + move.GetSideMask());
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
            int backPosition = PositionIndex;
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
            _castleRightsMask.Fill(ECastlelingRights.All);

            if (fen.Get() == '-')
            {
                fen.Advance();
                return 0;
            }

            // List to gather functions for castleling rights addition.
            List<Action> castleFunctions = new List<Action>(4);

            while (fen.Get() != 0 && fen.Get() != ' ')
            {
                char c = fen.Get();

                if (c.InBetween('A', 'H'))
                {
                    _chess960 = true;
                    _xfen = false;

                    // ReSharper disable once HeapView.ClosureAllocation
                    int rookFile = c - 'A';

                    if (rookFile > (int)Position.KingSquares[0].File())
                        castleFunctions.Add(() => AddShortCastleRights(rookFile, PlayerExtensions.White));
                    else
                        castleFunctions.Add(() => AddLongCastleRights(rookFile, PlayerExtensions.White));
                }
                else if (c.InBetween('a', 'h'))
                {
                    _chess960 = true;
                    _xfen = false;

                    // ReSharper disable once HeapView.ClosureAllocation
                    int rookFile = c - 'a';

                    if (rookFile > (int)Position.KingSquares[1].File())
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
                foreach (Action castleFunction in castleFunctions)
                    castleFunction();
            }

            return 0;
        }

        private void AddShortCastleRights(int rookFile, Player side)
        {
            if (rookFile == -1)
            {
                for (EFile file = EFile.FileH; file >= EFile.FileA; file--)
                {
                    if (!Position.IsPieceTypeOnSquare((ESquare)((int)file + side.Side * 56), EPieceType.Rook))
                        continue;
                    rookFile = (int)file; // right outermost rook for side
                    break;
                }

                _xfen = true;
            }

            // TODO : Replace with validation guarding
            if (rookFile < 0)
            {
                _xfen = false;
                return;
            }

            UpdateCastlelingData((EFile)rookFile, ECastleling.Short, side, EFile.FileE, EFile.FileH);

            //State.CastlelingRights |= CastlePositionalOr[0, side.Side];
            //Player them = ~side;
            //int castlelingMask = ECastleling.Short.GetCastleAllowedMask(side);
            //_castleRightsMask[SquareExtensions.GetFlip(rookFile, them).ToInt()] -= castlelingMask;
            //_castleRightsMask[SquareExtensions.GetFlip(Position.KingSquares[side.Side].File(), them).ToInt()] -= castlelingMask;
            //Position.SetRookCastleFrom(SquareExtensions.GetFlip((int)ESquare.g1, them), SquareExtensions.GetFlip(rookFile, them));
            //Position.SetKingCastleFrom(side, Position.KingSquares[side.Side], ECastleling.Short);

            //if (Position.KingSquares[side.Side].File() != 4 || rookFile != 7)
            //    _chess960 = true;
        }

        private void AddLongCastleRights(int rookFile, Player side)
        {
            if (rookFile == -1)
            {
                for (EFile file = EFile.FileA; file <= EFile.FileH; file++)
                {
                    if (!Position.IsPieceTypeOnSquare((ESquare)(int)(file + (byte) (side.Side * 56)), EPieceType.Rook))
                        continue;
                    rookFile = (int)file; // left outermost rook for side
                    break;
                }

                _xfen = true;
            }

            // TODO : Replace with validation guarding
            if (rookFile < 0)
            {
                _xfen = false;
                return;
            }

            UpdateCastlelingData((EFile)rookFile, ECastleling.Long, side, EFile.FileE, EFile.FileA);

            //State.CastlelingRights |= CastlePositionalOr[1, side.Side];
            //Player them = ~side;
            //int castlelingMask = ECastleling.Long.GetCastleAllowedMask(side);
            //_castleRightsMask[SquareExtensions.GetFlip(rookFile, them).ToInt()] -= castlelingMask;
            //_castleRightsMask[SquareExtensions.GetFlip(Position.KingSquares[side.Side].File(), them).ToInt()] -= castlelingMask;
            //Position.SetRookCastleFrom(SquareExtensions.GetFlip((int)ESquare.c1, them), SquareExtensions.GetFlip(rookFile, them));
            //Position.SetKingCastleFrom(side, Position.KingSquares[side.Side], ECastleling.Long);

            //if (Position.KingSquares[side.Side].File() != 4 || rookFile != 0)
            //    _chess960 = true;
        }

        private void UpdateCastlelingData(EFile rookFile, ECastleling castleling, Player side, EFile kingChess960File, EFile rookChess960File)
        {
            int castlelingIndex = (int) castleling - 1;
            State.CastlelingRights |= CastlePositionalOr[castlelingIndex, side.Side];

            ECastlelingRights castlelingMask = ~castleling.GetCastleAllowedMask(side);
            Player them = ~side;

            Square ksq = Position.KingSquares[side.Side];
            EFile ksqFile = ksq.File();
            int flippedRookSquareIndex = SquareExtensions.GetFlip(rookFile, them).ToInt();

            _castleRightsMask[flippedRookSquareIndex] &= castlelingMask;
            _castleRightsMask[SquareExtensions.GetFlip(ksqFile, them).ToInt()] &= castlelingMask;

            EFile rookFromFile = new Square(castleling == ECastleling.Short ? ESquare.g1 : ESquare.c1).File();

            Position.SetRookCastleFrom(SquareExtensions.GetFlip(rookFromFile, them), flippedRookSquareIndex);
            Position.SetKingCastleFrom(side, ksq, castleling);

            if (ksqFile != EFile.FileE || rookFile != EFile.FileH)
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