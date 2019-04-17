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
    using EnsureThat;
    using Enums;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveGenerator
    {
        private static readonly ConcurrentDictionary<ulong, MoveList> Table;

        private readonly IPosition _position;

        static MoveGenerator()
        {
            Table = new ConcurrentDictionary<ulong, MoveList>();

            // Force cleaning when process exists
            AppDomain.CurrentDomain.ProcessExit += Clean;
        }

        public MoveGenerator(IPosition position)
        {
            EnsureArg.IsNotNull(position, nameof(position));
            _position = position;
        }

        public Emgf Flags { get; set; } = Emgf.Legalmoves;

        public MoveList Moves { get; private set; }

        public static void ClearMoveCache() => Table.Clear();

        public void GenerateMoves(bool force = false)
        {
            // this is only preparation for true engine integration (not used yet)
            //if (Flags.HasFlagFast(Emgf.Stages))
            //    return;

            if (Table.TryGetValue(_position.State.Key, out var moves) && !force)
                Moves = moves;
            else
            {
                moves = new MoveList();
                // relax the gc while generating moves.
                var old = GCSettings.LatencyMode;
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                GenerateCapturesAndPromotions(moves);
                GenerateQuietMoves(moves);
                Table.TryAdd(_position.State.Key, moves);
                Moves = moves;
                GCSettings.LatencyMode = old;
            }
        }

        /// <summary>
        /// Class clean up method, to be expanded at some point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private static void Clean(object sender, EventArgs ea) => ClearMoveCache();

        private void GenerateCapturesAndPromotions(MoveList moves)
        {
            var currentSide = _position.State.SideToMove;
            var them = ~currentSide;
            var occupiedByThem = _position.OccupiedBySide[them.Side];
            var (northEast, northWest) = currentSide == PlayerExtensions.White ? (EDirection.NorthEast, EDirection.NorthWest) : (EDirection.SouthEast, EDirection.SouthWest);

            AddMoves(moves, occupiedByThem);

            var pawns = _position.Pieces(EPieceType.Pawn, currentSide);

            AddPawnMoves(moves, currentSide.PawnPush(pawns & currentSide.Rank7()) & ~_position.Occupied, currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(moves, pawns.Shift(northEast) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture);
            AddPawnMoves(moves, pawns.Shift(northWest) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture);

            if (_position.State.EnPassantSquare == ESquare.none)
                return;

            AddPawnMoves(moves, pawns.Shift(northEast) & _position.State.EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture);
            AddPawnMoves(moves, pawns.Shift(northWest) & _position.State.EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture);
        }

        private void GenerateQuietMoves(MoveList moves)
        {
            var currentSide = _position.State.SideToMove;
            var up = currentSide == PlayerExtensions.White ? EDirection.North : EDirection.South;
            var notOccupied = ~_position.Occupied;
            var pushed = (_position.Pieces(EPieceType.Pawn, currentSide) & ~currentSide.Rank7()).Shift(up) & notOccupied;
            AddPawnMoves(moves, pushed, currentSide.PawnPushDistance(), EMoveType.Quiet);

            pushed &= currentSide.Rank3();
            AddPawnMoves(moves, pushed.Shift(up) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush);

            AddMoves(moves, notOccupied);

            if (_position.InCheck)
                return;

            for (var castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++)
                if (_position.CanCastle(castleType))
                    AddCastleMove(moves, _position.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide));
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
        private void AddMove(MoveList moves, Piece piece, Square from, Square to, Piece promoted, EMoveType type = EMoveType.Quiet)
        {
            Move move;

            if (type.HasFlagFast(EMoveType.Capture))
                move = new Move(piece, _position.GetPiece(to), from, to, type, promoted);
            else if (type.HasFlagFast(EMoveType.Epcapture))
                move = new Move(piece, EPieceType.Pawn.MakePiece(~_position.State.SideToMove), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actual a legal move if the flag is enabled
            if (Flags.HasFlagFast(Emgf.Legalmoves) && !_position.IsLegal(move, piece, from, type))
                return;

            moves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMoves(MoveList moves, Piece piece, Square from, BitBoard attacks)
        {
            var target = _position.Occupied & attacks;
            foreach (var to in target)
                AddMove(moves, piece, from, to, EPieces.NoPiece, EMoveType.Capture);

            target = ~_position.Occupied & attacks;
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
        private void AddMoves(MoveList moves, BitBoard targetSquares)
        {
            var c = _position.State.SideToMove;

            var occupied = _position.Occupied;

            for (var pt = EPieceType.King; pt >= EPieceType.Knight; --pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = _position.BoardPieces[pc.ToInt()];
                foreach (var from in pieces)
                    AddMoves(moves, pc, from, from.GetAttacks(pt, occupied) & targetSquares);
            }
        }

        private void AddPawnMoves(MoveList moves, BitBoard targetSquares, Direction direction, EMoveType type)
        {
            if (targetSquares.Empty())
                return;

            var piece = EPieceType.Pawn.MakePiece(_position.State.SideToMove);

            foreach (var squareTo in targetSquares)
            {
                var squareFrom = squareTo - direction;
                if (squareTo.IsPromotionRank())
                {
                    if (Flags.HasFlagFast(Emgf.Queenpromotion))
                        AddMove(moves, piece, squareFrom, squareTo, EPieceType.Queen.MakePiece(_position.State.SideToMove),
                            type | EMoveType.Promotion);
                    else
                        for (var promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                            AddMove(moves, piece, squareFrom, squareTo, promotedPiece.MakePiece(_position.State.SideToMove),
                                type | EMoveType.Promotion);
                }
                else
                    AddMove(moves, piece, squareFrom, squareTo, PieceExtensions.EmptyPiece, type);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCastleMove(MoveList moves, Square from, Square to) => AddMove(moves, EPieceType.King.MakePiece(_position.State.SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);
    }
}