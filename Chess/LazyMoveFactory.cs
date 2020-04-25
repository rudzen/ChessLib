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

using System.Collections.Generic;
using System.Linq;

namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System;
    using System.Runtime.CompilerServices;
    using Types;

    public static class LazyMoveFactory
    {
        public static IEnumerable<Move> GenerateMovesLazy(this IPosition pos, MoveGenerationFlags flags = MoveGenerationFlags.Legalmoves, bool useCache = true, bool force = false)
        {
            pos.State.Pinned = flags.HasFlagFast(MoveGenerationFlags.Legalmoves)
                ? pos.GetPinnedPieces(pos.GetPieceSquare(PieceTypes.King, pos.State.SideToMove), pos.State.SideToMove)
                : BitBoards.EmptyBitBoard;

            return pos.GenerateCapturesAndPromotions(flags)
                .Concat(pos.GenerateQuietMoves(flags));
        }

        private static IEnumerable<Move> GenerateCapturesAndPromotions(this IPosition pos, MoveGenerationFlags flags)
        {
            var currentSide = pos.State.SideToMove;
            var them = ~currentSide;
            var occupiedByThem = pos.OccupiedBySide[them.Side];
            var (northEast, northWest) = currentSide.GetPawnAttackDirections();

            var pawns = pos.Pieces(PieceTypes.Pawn, currentSide);

            var moves = pos.AddPawnMoves(currentSide.PawnPush(pawns & currentSide.Rank7()) & ~pos.Pieces(), currentSide.PawnPushDistance(), MoveTypes.Quiet, flags)
                .Concat(pos.AddPawnMoves(pawns.Shift(northEast) & occupiedByThem, currentSide.PawnWestAttackDistance(), MoveTypes.Capture, flags))
                .Concat(pos.AddPawnMoves(pawns.Shift(northWest) & occupiedByThem, currentSide.PawnEastAttackDistance(), MoveTypes.Capture, flags));

            if (pos.State.EnPassantSquare != Squares.none)
            {
                moves = moves.Concat(pos.AddPawnMoves(pawns.Shift(northEast) & pos.State.EnPassantSquare, currentSide.PawnWestAttackDistance(), MoveTypes.Epcapture, flags))
                    .Concat(pos.AddPawnMoves(pawns.Shift(northWest) & pos.State.EnPassantSquare, currentSide.PawnEastAttackDistance(), MoveTypes.Epcapture, flags));
            }
            
            return moves.Concat(pos.AddMoves(occupiedByThem, flags));
        }

        private static IEnumerable<Move> GenerateQuietMoves(this IPosition pos, MoveGenerationFlags flags)
        {
            var currentSide = pos.State.SideToMove;
            var up = currentSide == PlayerExtensions.White ? Directions.North : Directions.South;
            var notOccupied = ~pos.Pieces();
            var pushed = (pos.Pieces(PieceTypes.Pawn, currentSide) & ~currentSide.Rank7()).Shift(up) & notOccupied;
            
            var moves = pos.AddPawnMoves(pushed, currentSide.PawnPushDistance(), MoveTypes.Quiet, flags);
            foreach (var move in moves)
                yield return move;

            pushed &= currentSide.Rank3();
            moves = pos.AddPawnMoves(pushed.Shift(up) & notOccupied, currentSide.PawnDoublePushDistance(), MoveTypes.Doublepush, flags)
                .Concat(pos.AddMoves(notOccupied, flags));

            foreach (var move in moves)
                yield return move;

            if (pos.State.InCheck)
                yield break;

            for (var castleType = CastlelingSides.King; castleType < CastlelingSides.CastleNb; castleType++)
                if (pos.CanCastle(castleType))
                {
                    var (valid, move) = pos.AddCastleMove(pos.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide), flags);
                    if (valid)
                        yield return move;
                }
        }

        /// <summary>
        /// Iterates through the piece types and generates moves based on their attacks.
        /// It does not contain any checks for moves that are invalid, as the leaf methods
        /// contains implicit denial of move generation if the target bitboard is empty.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves">The move list to add potential moves to.</param>
        /// <param name="targetSquares">The target squares to move to</param>
        /// <param name="flags"></param>
        private static IEnumerable<Move> AddMoves(this IPosition pos, BitBoard targetSquares, MoveGenerationFlags flags)
        {
            var c = pos.State.SideToMove;
            var occupied = pos.Pieces();

            var result = Enumerable.Empty<Move>();
            
            for (var pt = PieceTypes.Knight; pt <= PieceTypes.King; ++pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = pos.Pieces(pc);
                while (pieces)
                {
                    var from = pieces.Lsb();
                    var moves = pos.AddMoves(pc, from, from.GetAttacks(pt, occupied) & targetSquares, flags);
                    foreach (var move in moves)
                        yield return move;
                    BitBoards.ResetLsb(ref pieces);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Move> AddMoves(this IPosition pos, Piece piece, Square from, BitBoard attacks, MoveGenerationFlags flags)
        {
            var target = pos.Pieces(~pos.State.SideToMove) & attacks;
            while (target)
            {
                var to = target.Lsb();
                var (valid, move) = pos.MakeMove(piece, from, to, PieceExtensions.EmptyPiece, flags, MoveTypes.Capture);
                if (valid)
                    yield return move;
                BitBoards.ResetLsb(ref target);
            }

            target = ~pos.Pieces() & attacks;
            while (target)
            {
                var to = target.Lsb();
                var (valid, move) = pos.MakeMove(piece, from, to, PieceExtensions.EmptyPiece, flags);
                if (valid)
                    yield return move;
                BitBoards.ResetLsb(ref target);
            }
        }

        private static IEnumerable<Move> AddPawnMoves(this IPosition pos, BitBoard targetSquares, Direction direction, MoveTypes type, MoveGenerationFlags flags)
        {
            if (targetSquares.Empty())
                yield break;

            var stm = pos.State.SideToMove;
            var piece = PieceTypes.Pawn.MakePiece(stm);

            var promotionRank = stm.PromotionRank();
            var promotionSquares = targetSquares & promotionRank;
            var nonPromotionSquares = targetSquares & ~promotionRank;

            while (nonPromotionSquares)
            {
                var to = nonPromotionSquares.Lsb();
                var from = to - direction;
                var (valid, move) = pos.MakeMove(piece, from, to, PieceExtensions.EmptyPiece, flags, type);
                if (valid)
                    yield return move;
                BitBoards.ResetLsb(ref nonPromotionSquares);
            }

            type |= MoveTypes.Promotion;

            if (flags.HasFlagFast(MoveGenerationFlags.Queenpromotion))
            {
                var sqTo = promotionSquares.Lsb();
                var sqFrom = sqTo - direction;
                var (valid, move) = pos.MakeMove(piece, sqFrom, sqTo, PieceTypes.Queen.MakePiece(stm), flags, type);
                if (valid)
                    yield return move;
                BitBoards.ResetLsb(ref promotionSquares);
            }
            else
            {
                while (promotionSquares)
                {
                    var sqTo = promotionSquares.Lsb();
                    var sqFrom = sqTo - direction;
                    for (var promotedPiece = PieceTypes.Queen; promotedPiece >= PieceTypes.Knight; promotedPiece--)
                    {
                        var (validMove, move) = pos.MakeMove(piece, sqFrom, sqTo, promotedPiece.MakePiece(stm), flags, type);
                        if (validMove)
                            yield return move;
                    }

                    BitBoards.ResetLsb(ref promotionSquares);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (bool, Move) AddCastleMove(this IPosition pos, Square from, Square to, MoveGenerationFlags flags)
            => pos.MakeMove(PieceTypes.King.MakePiece(pos.State.SideToMove), from, to, PieceExtensions.EmptyPiece, flags, MoveTypes.Castle);

        /// <summary>
        /// Move generation leaf method.
        /// Constructs the actual move based on the arguments.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="promoted">The promotion piece (if any, defaults to NoPiece type)</param>
        /// <param name="flags"></param>
        /// <param name="type">The move type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (bool, Move) MakeMove(this IPosition pos, Piece piece, Square from, Square to, Piece promoted, MoveGenerationFlags flags, MoveTypes type = MoveTypes.Quiet)
        {
            Move move;

            if (type.HasFlagFast(MoveTypes.Capture))
                move = new Move(piece, pos.GetPiece(to), from, to, type, promoted);
            else if (type.HasFlagFast(MoveTypes.Epcapture))
                move = new Move(piece, PieceTypes.Pawn.MakePiece(~pos.State.SideToMove), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actual a legal move if the flag is enabled
            if (flags.HasFlagFast(MoveGenerationFlags.Legalmoves) && !pos.IsLegal(move, piece, from, type))
                return (false, MoveExtensions.EmptyMove);

            return (true, move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (Direction, Direction) GetPawnAttackDirections(this Player us)
        {
            Span<(Directions, Directions)> directions = stackalloc[] { (Directions.NorthEast, Directions.NorthWest), (Directions.SouthEast, Directions.SouthWest) };
            return directions[us.Side];
        }
    }
}