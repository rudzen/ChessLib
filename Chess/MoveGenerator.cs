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
    using Enums;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using Types;

    public class MoveGenerator
    {
        private static readonly ConcurrentDictionary<ulong, List<Move>> Table;

        // to be replaced
        private readonly Func<BitBoard, BitBoard>[] _pawnAttacksWest = { BitBoards.NorthEastOne, BitBoards.SouthEastOne };

        // to be replaced
        private readonly Func<BitBoard, BitBoard>[] _pawnAttacksEast = { BitBoards.NorthWestOne, BitBoards.SouthWestOne };

        private BitBoard[] _bitboardPieces;

        private BitBoard[] _occupiedBySide;

        private BitBoard _occupied;

        static MoveGenerator()
        {
            Table = new ConcurrentDictionary<ulong, List<Move>>();

            // Force cleaning when process exists
            AppDomain.CurrentDomain.ProcessExit += Clean;
        }

        protected internal MoveGenerator(Position position)
        {
            Position = position;
            _bitboardPieces = position.BoardPieces;
            _occupiedBySide = position.OccupiedBySide;
        }

        public int CastlelingRights { get; set; }

        public bool InCheck { get; set; }

        public Square EnPassantSquare { get; set; }

        public Player SideToMove { get; set; }

        public BitBoard Pinned { get; set; }

        public ulong Key { get; internal set; }

        public Emgf Flags { get; set; } = Emgf.Legalmoves;

        public List<Move> Moves { get; private set; }

        public Position Position { get; set; }

        public static void ClearMoveCache()
        {
            Table.Clear();
        }

        public void GenerateMoves(bool force = false)
        {
            // Align current structure to position.
            Reset(MoveExtensions.EmptyMove);

            // this is only preparation for true engine integration (not used yet)
            if (Flags.HasFlagFast(Emgf.Stages))
                return;

            if (Table.ContainsKey(Key) && !force)
                Moves = Table[Key];
            else
            {
                var moves = new List<Move>(256);

                // relax the gc while generating moves.
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                GenerateCapturesAndPromotions(moves);
                GenerateQuietMoves(moves);
                Moves = moves;
                Table.TryAdd(Key, moves);
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
            }
        }

        /// <summary>
        /// Determine if a move is legal or not, by performing the move and checking if the king is under attack afterwards.
        /// </summary>
        /// <param name="move">The move to check</param>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="type">The move type</param>
        /// <returns>true if legal, otherwise false</returns>
        public bool IsLegal(Move move, Piece piece, Square from, EMoveType type)
        {
            if (!InCheck && piece.Type() != EPieceType.King && (Pinned & from).Empty() && (type & EMoveType.Epcapture) == 0)
                return true;

            Position.IsProbing = true;
            Position.MakeMove(move);
            var opponentAttacking = Position.IsAttacked(Position.KingSquares[SideToMove.Side], ~SideToMove);
            Position.TakeMove(move);
            Position.IsProbing = false;
            return !opponentAttacking;
        }

        public bool IsLegal(Move move) => IsLegal(move, move.GetMovingPiece(), move.GetFromSquare(), move.GetMoveType());

        /// <summary>
        /// <para>"Validates" a move using simple logic. For example that the piece actually being moved exists etc.</para>
        /// <para>This is basically only useful while developing and/or debugging</para>
        /// </summary>
        /// <param name="move">The move to check for logical errors</param>
        /// <returns>True if move "appears" to be legal, otherwise false</returns>
        public bool IsPseudoLegal(Move move)
        {
            // Verify that the piece actually exists on the board at the location defined by the move struct
            if ((_bitboardPieces[move.GetMovingPiece().ToInt()] & move.GetFromSquare()).Empty())
                return false;

            var to = move.GetToSquare();

            if (move.IsCastlelingMove())
            {
                // TODO : Basic castleling verification
                if (CanCastle(move.GetFromSquare() < to ? ECastleling.Short : ECastleling.Long))
                    return true;

                var mg = new MoveGenerator(Position);
                mg.GenerateMoves();
                return mg.Moves.Contains(move);
            }
            else if (move.IsEnPassantMove())
            {
                // TODO : En-passant here

                // TODO : Test with unit test
                var opponent = ~move.GetMovingSide();
                if (EnPassantSquare.PawnAttack(opponent) & Position.Pieces(EPieceType.Pawn, opponent))
                    return true;
            }
            else if (move.IsCaptureMove())
            {
                var opponent = ~move.GetMovingSide();
                if ((_occupiedBySide[opponent.Side] & to).Empty())
                    return false;

                if ((_bitboardPieces[move.GetCapturedPiece().ToInt()] & to).Empty())
                    return false;
            }
            else if ((_occupied & to) != 0)
            {
                return false;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (move.GetMovingPiece().Type())
            {
                case EPieceType.Bishop:
                case EPieceType.Rook:
                case EPieceType.Queen:
                    if (move.GetFromSquare().BitboardBetween(to) & _occupied)
                        return false;

                    break;
            }

            return true;
        }

        /// <summary>
        /// Class clean up method, to be expanded at some point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private static void Clean(object sender, EventArgs ea)
        {
            ClearMoveCache();
        }

        /// <summary>
        /// Aligns the current positional data to the position data.
        /// </summary>
        /// <param name="move">Not used atm, but for future library completion</param>
        private void Reset(Move move)
        {
            var flags = Flags;

            // not being used in the current version
            if (!move.IsNullMove() && (move.IsCastlelingMove() || move.IsEnPassantMove()))
                flags &= ~Emgf.Stages;

            if (flags.HasFlagFast(Emgf.Legalmoves))
                Pinned = Position.GetPinnedPieces(Position.KingSquares[SideToMove.Side], SideToMove);

            _occupied = Position.Occupied;
            _occupiedBySide = Position.OccupiedBySide;
            _bitboardPieces = Position.BoardPieces;

            Flags = flags;
        }

        private void GenerateCapturesAndPromotions(ICollection<Move> moves)
        {
            var currentSide = SideToMove;
            var them = ~currentSide;
            var occupiedByThem = _occupiedBySide[them.Side];

            AddMoves(moves, occupiedByThem);

            var pawns = Position.Pieces(EPieceType.Pawn, currentSide);

            AddPawnMoves(moves, currentSide.PawnPush(pawns & currentSide.Rank7()) & ~_occupied, currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(moves, _pawnAttacksWest[currentSide.Side](pawns) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture);
            AddPawnMoves(moves, _pawnAttacksEast[currentSide.Side](pawns) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture);
            if (EnPassantSquare != ESquare.none)
            {
                AddPawnMoves(moves, _pawnAttacksWest[currentSide.Side](pawns) & EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture);
                AddPawnMoves(moves, _pawnAttacksEast[currentSide.Side](pawns) & EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture);
            }
        }

        private void GenerateQuietMoves(ICollection<Move> moves)
        {
            var currentSide = SideToMove;
            if (!InCheck)
                for (var castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++)
                {
                    if (CanCastle(castleType))
                        AddCastleMove(moves, Position.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide));
                }

            var notOccupied = ~_occupied;
            var pushed = currentSide.PawnPush(Position.Pieces(EPieceType.Pawn, currentSide).Value & ~currentSide.Rank7()) & notOccupied;
            AddPawnMoves(moves, pushed.Value, currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(moves,  currentSide.PawnPush(pushed.Value & currentSide.Rank3()) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush);
            AddMoves(moves, notOccupied);
        }

        /// <summary>
        /// Move generation leaf method.
        /// Constructs the actual move based on the arguments.
        /// </summary>
        /// <param name="moves">The move list to add the generated (if any) moves into</param>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="promoted">The promotion piece (if any, defaults to NoPiece type)</param>
        /// <param name="type">The move type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMove(ICollection<Move> moves, Piece piece, Square from, Square to, Piece promoted, EMoveType type = EMoveType.Quiet)
        {
            Move move;

            if ((type & EMoveType.Capture) != 0)
                move = new Move(piece, Position.GetPiece(to), from, to, type, promoted);
            else if ((type & EMoveType.Epcapture) != 0)
                move = new Move(piece, EPieceType.Pawn.MakePiece(~SideToMove), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actual a legal move if the flag is enabled
            if (Flags.HasFlagFast(Emgf.Legalmoves) && !IsLegal(move, piece, from, type))
                return;

            moves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMoves(ICollection<Move> moves, Piece piece, Square from, BitBoard attacks)
        {
            var target = Position.Occupied & attacks;
            foreach (var to in target)
                AddMove(moves, piece, from, to, EPieces.NoPiece, EMoveType.Capture);

            target = ~Position.Occupied & attacks;
            foreach (var to in target)
                AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece);
        }

        /// <summary>
        /// Iterates through the piece types and generates moves based on their attacks.
        /// It does not contain any checks for moves that are invalid, as the leaf methods
        /// contains implicit denial of move generation if the target bitboard is empty.
        /// </summary>
        /// <param name="moves">The move list to add potential moves to.</param>
        /// <param name="targetSquares">The target squares to move to</param>
        private void AddMoves(ICollection<Move> moves, BitBoard targetSquares)
        {
            var c = SideToMove;
            var occupied = Position.Occupied;

            for (var pt = EPieceType.King; pt >= EPieceType.Knight; --pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = _bitboardPieces[pc.ToInt()];
                foreach (var from in pieces)
                    AddMoves(moves, pc, from, from.GetAttacks(pt, occupied) & targetSquares);
            }
        }

        private void AddPawnMoves(ICollection<Move> moves, BitBoard targetSquares, Direction direction, EMoveType type)
        {
            if (targetSquares.Empty())
                return;

            var piece = EPieceType.Pawn.MakePiece(SideToMove);

            foreach (var squareTo in targetSquares)
            {
                var squareFrom = squareTo - direction;
                if (!squareTo.IsPromotionRank())
                {
                    AddMove(moves, piece, squareFrom, squareTo, PieceExtensions.EmptyPiece, type);
                    continue;
                }

                if (Flags.HasFlagFast(Emgf.Queenpromotion))
                    AddMove(moves, piece, squareFrom, squareTo, EPieceType.Queen.MakePiece(SideToMove), type | EMoveType.Promotion);
                else
                    for (var promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                        AddMove(moves, piece, squareFrom, squareTo, promotedPiece.MakePiece(SideToMove), type | EMoveType.Promotion);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCastleMove(ICollection<Move> moves, Square from, Square to) => AddMove(moves, EPieceType.King.MakePiece(SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanCastle(ECastleling type)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case ECastleling.Short:
                case ECastleling.Long:
                    return (CastlelingRights & type.GetCastleAllowedMask(SideToMove)) != 0 && IsCastleAllowed(type.GetKingCastleTo(SideToMove));

                default:
                    throw new ArgumentException("Illegal castleling type.");
            }
        }

        private bool IsCastleAllowed(Square square)
        {
            var c = SideToMove;
            // The complexity of this function is mainly due to the support for Chess960 variant.
            var rookTo = square.GetRookCastleTo();
            var rookFrom = Position.GetRookCastleFrom(square);
            var kingSquare = Position.KingSquares[c.Side];

            // The pieces in question.. rook and king
            var castlePieces = rookFrom | kingSquare;

            // The span between the rook and the king
            var castleSpan = castlePieces | rookTo;
            castleSpan |= square;
            castleSpan |= kingSquare.BitboardBetween(rookFrom) | rookFrom.BitboardBetween(rookTo);

            // check that the span AND current occupied pieces are no different that the piece themselves.
            if ((castleSpan & _occupied) != castlePieces)
                return false;

            // Check that no square between the king's initial and final squares (including the initial and final squares)
            // may be under attack by an enemy piece. Initial square was already checked a this point.

            c = ~c;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var s in kingSquare.BitboardBetween(square) | square)
            {
                if (Position.IsAttacked(s, c))
                    return false;
            }

            return true;
        }
    }
}