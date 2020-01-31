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
    using System;
    using System.Runtime.CompilerServices;
    using Types;

    public static class MoveFactory
    {
        public static MoveList GenerateMoves(this IPosition pos, Emgf flags = Emgf.Legalmoves, bool useCache = true, bool force = false)
        {
            pos.State.Pinned = flags.HasFlagFast(Emgf.Legalmoves)
                ? pos.GetPinnedPieces(pos.GetPieceSquare(EPieceType.King, pos.State.SideToMove), pos.State.SideToMove)
                : BitBoards.EmptyBitBoard;

            var moves = new MoveList();

            pos.GenerateCapturesAndPromotions(moves, flags);
            pos.GenerateQuietMoves(moves, flags);

            return moves;
        }

        private static void GenerateCapturesAndPromotions(this IPosition pos, MoveList moves, Emgf flags)
        {
            var currentSide = pos.State.SideToMove;
            var them = ~currentSide;
            var occupiedByThem = pos.OccupiedBySide[them.Side];
            var (northEast, northWest) = currentSide.GetPawnAttackDirections();

            var pawns = pos.Pieces(EPieceType.Pawn, currentSide);

            pos.AddPawnMoves(moves, currentSide.PawnPush(pawns & currentSide.Rank7()) & ~pos.Pieces(), currentSide.PawnPushDistance(), EMoveType.Quiet, flags);
            pos.AddPawnMoves(moves, pawns.Shift(northEast) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture, flags);
            pos.AddPawnMoves(moves, pawns.Shift(northWest) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture, flags);

            if (pos.State.EnPassantSquare != ESquare.none)
            {
                pos.AddPawnMoves(moves, pawns.Shift(northEast) & pos.State.EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture, flags);
                pos.AddPawnMoves(moves, pawns.Shift(northWest) & pos.State.EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture, flags);
            }

            pos.AddMoves(moves, occupiedByThem, flags);
        }

        private static void GenerateQuietMoves(this IPosition pos, MoveList moves, Emgf flags)
        {
            var currentSide = pos.State.SideToMove;
            var up = currentSide == PlayerExtensions.White ? EDirection.North : EDirection.South;
            var notOccupied = ~pos.Pieces();
            var pushed = (pos.Pieces(EPieceType.Pawn, currentSide) & ~currentSide.Rank7()).Shift(up) & notOccupied;
            pos.AddPawnMoves(moves, pushed, currentSide.PawnPushDistance(), EMoveType.Quiet, flags);

            pushed &= currentSide.Rank3();
            pos.AddPawnMoves(moves, pushed.Shift(up) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush, flags);

            pos.AddMoves(moves, notOccupied, flags);

            if (pos.InCheck)
                return;

            for (var castleType = CastlelingSides.King; castleType < CastlelingSides.CastleNb; castleType++)
                if (pos.CanCastle(castleType))
                    pos.AddCastleMove(moves, pos.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide), flags);
        }

        /// <summary>
        /// Iterates through the piece types and generates moves based on their attacks.
        /// It does not contain any checks for moves that are invalid, as the leaf methods
        /// contains implicit denial of move generation if the target bitboard is empty.
        /// </summary>
        /// <param name="moves">The move list to add potential moves to.</param>
        /// <param name="targetSquares">The target squares to move to</param>
        private static void AddMoves(this IPosition pos, MoveList moves, BitBoard targetSquares, Emgf flags)
        {
            var c = pos.State.SideToMove;
            var occupied = pos.Pieces();

            for (var pt = EPieceType.Knight; pt <= EPieceType.King; ++pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = pos.Pieces(pc);
                while (pieces)
                {
                    var from = pieces.Lsb();
                    pos.AddMoves(moves, pc, from, from.GetAttacks(pt, occupied) & targetSquares, flags);
                    BitBoards.ResetLsb(ref pieces);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddMoves(this IPosition pos, MoveList moves, Piece piece, Square from, BitBoard attacks, Emgf flags)
        {
            var target = pos.Pieces(~pos.State.SideToMove) & attacks;
            while (target)
            {
                var to = target.Lsb();
                pos.AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece, flags, EMoveType.Capture);
                BitBoards.ResetLsb(ref target);
            }

            target = ~pos.Pieces() & attacks;
            while (target)
            {
                var to = target.Lsb();
                pos.AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece, flags);
                BitBoards.ResetLsb(ref target);
            }
        }

        private static void AddPawnMoves(this IPosition pos, MoveList moves, BitBoard targetSquares, Direction direction, EMoveType type, Emgf flags)
        {
            if (targetSquares.Empty())
                return;

            var stm = pos.State.SideToMove;
            var piece = EPieceType.Pawn.MakePiece(stm);

            var promotionRank = stm.PromotionRank();
            var promotionSquares = targetSquares & promotionRank;
            var nonPromotionSquares = targetSquares & ~promotionRank;

            while (nonPromotionSquares)
            {
                var to = nonPromotionSquares.Lsb();
                var from = to - direction;
                pos.AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece, flags, type);
                BitBoards.ResetLsb(ref nonPromotionSquares);
            }

            type |= EMoveType.Promotion;

            if (flags.HasFlagFast(Emgf.Queenpromotion))
            {
                var sqTo = promotionSquares.Lsb();
                var sqFrom = sqTo - direction;
                pos.AddMove(moves, piece, sqFrom, sqTo, EPieceType.Queen.MakePiece(stm), flags, type);
                BitBoards.ResetLsb(ref promotionSquares);
            }
            else
            {
                while (promotionSquares)
                {
                    var sqTo = promotionSquares.Lsb();
                    var sqFrom = sqTo - direction;
                    for (var promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                        pos.AddMove(moves, piece, sqFrom, sqTo, promotedPiece.MakePiece(stm), flags, type);

                    BitBoards.ResetLsb(ref promotionSquares);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddCastleMove(this IPosition pos, MoveList moves, Square from, Square to, Emgf flags)
            => pos.AddMove(moves, EPieceType.King.MakePiece(pos.State.SideToMove), from, to, PieceExtensions.EmptyPiece, flags, EMoveType.Castle);

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
        private static void AddMove(this IPosition pos, MoveList moves, Piece piece, Square from, Square to, Piece promoted, Emgf flags, EMoveType type = EMoveType.Quiet)
        {
            Move move;

            if (type.HasFlagFast(EMoveType.Capture))
                move = new Move(piece, pos.GetPiece(to), from, to, type, promoted);
            else if (type.HasFlagFast(EMoveType.Epcapture))
                move = new Move(piece, EPieceType.Pawn.MakePiece(~pos.State.SideToMove), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actual a legal move if the flag is enabled
            if (flags.HasFlagFast(Emgf.Legalmoves) && !pos.IsLegal(move, piece, from, type))
                return;

            moves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (Direction, Direction) GetPawnAttackDirections(this Player us)
        {
            Span<(EDirection, EDirection)> directions = stackalloc[] { (EDirection.NorthEast, EDirection.NorthWest), (EDirection.SouthEast, EDirection.SouthWest) };
            return directions[us.Side];
        }
    }
}