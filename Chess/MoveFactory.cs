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
#1#
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
        public static MoveList2 GenerateMoves(this IPosition pos, MoveGenerationType type = MoveGenerationType.Legal)
        {
            var ml = new MoveList2();
            if (type == MoveGenerationType.Legal && pos.InCheck)
                type = MoveGenerationType.Evasions;
            ml.Generate(pos, type);
            return ml;
        }

        // private static void GenerateCapturesAndPromotions(this IPosition pos, IMoveList moves, MoveGenerationFlags flags, Player stm)
        // {
        //     var them = ~stm;
        //     var occupiedByThem = pos.Pieces(them);
        //     var (northEast, northWest) = stm.GetPawnAttackDirections();
        //
        //     var pawns = pos.Pieces(PieceTypes.Pawn, stm);
        //
        //     pos.AddPawnMoves(moves, stm.PawnPush(pawns & stm.Rank7()) & ~pos.Pieces(), stm.PawnPushDistance(), MoveTypes.Quiet, flags, stm);
        //     pos.AddPawnMoves(moves, pawns.Shift(northEast) & occupiedByThem, stm.PawnWestAttackDistance(), MoveTypes.Capture, flags, stm);
        //     pos.AddPawnMoves(moves, pawns.Shift(northWest) & occupiedByThem, stm.PawnEastAttackDistance(), MoveTypes.Capture, flags, stm);
        //
        //     if (pos.State.EnPassantSquare != Squares.none)
        //     {
        //         pos.AddPawnMoves(moves, pawns.Shift(northEast) & pos.State.EnPassantSquare, stm.PawnWestAttackDistance(), MoveTypes.Epcapture, flags, stm);
        //         pos.AddPawnMoves(moves, pawns.Shift(northWest) & pos.State.EnPassantSquare, stm.PawnEastAttackDistance(), MoveTypes.Epcapture, flags, stm);
        //     }
        //
        //     pos.AddMoves(moves, occupiedByThem, flags, stm);
        // }
        //
        // private static void GenerateQuietMoves(this IPosition pos, IMoveList moves, MoveGenerationFlags flags, Player stm)
        // {
        //     var up = stm.PawnPushDistance();
        //     var notOccupied = ~pos.Pieces();
        //     var pushed = (pos.Pieces(PieceTypes.Pawn, stm) & ~stm.Rank7()).Shift(up) & notOccupied;
        //     pos.AddPawnMoves(moves, pushed, stm.PawnPushDistance(), MoveTypes.Quiet, flags, stm);
        //
        //     pushed &= stm.Rank3();
        //     pos.AddPawnMoves(moves, pushed.Shift(up) & notOccupied, stm.PawnDoublePushDistance(), MoveTypes.Doublepush, flags, stm);
        //
        //     pos.AddMoves(moves, notOccupied, flags, stm);
        //
        //     if (pos.State.InCheck)
        //         return;
        //
        //     if (!pos.CanCastle(stm))
        //         return;
        //
        //     for (var castleType = CastlelingSides.King; castleType < CastlelingSides.CastleNb; castleType++)
        //         GenerateCastling(pos, moves, stm, castleType.MakeCastlelingRights(stm), false, flags);
        //     
        //     // GenerateCastling(pos, moves, stm, CastlelingSides.King.MakeCastlelingRights(stm), false, flags);
        //     // GenerateCastling(pos, moves, stm, CastlelingSides.Queen.MakeCastlelingRights(stm), false, flags);
        //
        //     //for (var castleType = CastlelingSides.King; castleType < CastlelingSides.CastleNb; castleType++)
        //     //    if (pos.CanCastle(castleType))
        //     //        pos.AddCastleMove(moves, pos.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide), flags);
        // }
        //
        // /// <summary>
        // /// Iterates through the piece types and generates moves based on their attacks.
        // /// It does not contain any checks for moves that are invalid, as the leaf methods
        // /// contains implicit denial of move generation if the target bitboard is empty.
        // /// </summary>
        // /// <param name="pos"></param>
        // /// <param name="moves">The move list to add potential moves to.</param>
        // /// <param name="targetSquares">The target squares to move to</param>
        // /// <param name="flags"></param>
        // private static void AddMoves(this IPosition pos, IMoveList moves, BitBoard targetSquares, MoveGenerationFlags flags, Player stm)
        // {
        //     var occupied = pos.Pieces();
        //     var ourPieces = pos.Pieces(stm);
        //     // Console.WriteLine(BitBoards.PrintBitBoard(occupied, "occupied"));
        //     // Console.WriteLine(BitBoards.PrintBitBoard(targetSquares, "targetSquares"));
        //
        //     var wat = false;
        //     ulong c = 0;
        //     
        //     for (var pt = PieceTypes.Knight; pt <= PieceTypes.King; ++pt)
        //     {
        //         if (pt == PieceTypes.Rook && stm.IsBlack())
        //         {
        //             wat = true;
        //             Console.WriteLine("hdjksahkj");
        //         }
        //         var pc = pt.MakePiece(stm);
        //         var pieces = pos.Pieces(pc);
        //         c = moves.Count;
        //         while (pieces)
        //         {
        //             var from = pieces.Lsb();
        //             var attacks = from.GetAttacks(pt, occupied) & ~ourPieces;
        //             // if (wat && from == Squares.h8)
        //             // {
        //             //     Console.WriteLine(BitBoards.PrintBitBoard(attacks, "rook attacks"));
        //             // }
        //             pos.AddMoves(moves, pc, from, attacks & targetSquares, flags, stm);
        //             // if (wat && c != moves.Count)
        //             // {
        //             //     Console.WriteLine(BitBoards.PrintBitBoard(attacks, "rook attacks"));
        //             // }
        //             BitBoards.ResetLsb(ref pieces);
        //         }
        //
        //         
        //     }
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static void AddMoves(this IPosition pos, IMoveList moves, Piece piece, Square from, BitBoard attacks, MoveGenerationFlags flags, Player stm)
        // {
        //     var target = pos.Pieces(~stm) & attacks;
        //     while (target)
        //     {
        //         var to = target.Lsb();
        //         pos.AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece, flags, MoveTypes.Capture);
        //         BitBoards.ResetLsb(ref target);
        //     }
        //
        //     target = ~pos.Pieces() & attacks;
        //     while (target)
        //     {
        //         var to = target.Lsb();
        //         pos.AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece, flags);
        //         BitBoards.ResetLsb(ref target);
        //     }
        // }
        //
        // private static void AddPawnMoves(this IPosition pos, IMoveList moves, BitBoard targetSquares, Direction direction, MoveTypes type, MoveGenerationFlags flags, Player stm)
        // {
        //     if (targetSquares.Empty())
        //         return;
        //
        //     var piece = PieceTypes.Pawn.MakePiece(stm);
        //
        //     var promotionRank = stm.PromotionRank();
        //     var promotionSquares = targetSquares & promotionRank;
        //     var nonPromotionSquares = targetSquares & ~promotionRank;
        //
        //     while (nonPromotionSquares)
        //     {
        //         var to = nonPromotionSquares.Lsb();
        //         var from = to - direction;
        //         pos.AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece, flags, type);
        //         BitBoards.ResetLsb(ref nonPromotionSquares);
        //     }
        //
        //     type |= MoveTypes.Promotion;
        //
        //     if (flags.HasFlagFast(MoveGenerationFlags.Queenpromotion))
        //     {
        //         var sqTo = promotionSquares.Lsb();
        //         var sqFrom = sqTo - direction;
        //         pos.AddMove(moves, piece, sqFrom, sqTo, PieceTypes.Queen.MakePiece(stm), flags, type);
        //         BitBoards.ResetLsb(ref promotionSquares);
        //     }
        //     else
        //     {
        //         while (promotionSquares)
        //         {
        //             var sqTo = promotionSquares.Lsb();
        //             var sqFrom = sqTo - direction;
        //             for (var promotedPiece = PieceTypes.Queen; promotedPiece >= PieceTypes.Knight; promotedPiece--)
        //                 pos.AddMove(moves, piece, sqFrom, sqTo, promotedPiece.MakePiece(stm), flags, type);
        //
        //             BitBoards.ResetLsb(ref promotionSquares);
        //         }
        //     }
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static void AddCastleMove(this IPosition pos, IMoveList moves, Square from, Square to, MoveGenerationFlags flags)
        //     => pos.AddMove(moves, PieceTypes.King.MakePiece(pos.SideToMove), from, to, PieceExtensions.EmptyPiece, flags, MoveTypes.Castle);
        //
        // /// <summary>
        // /// Move generation leaf method.
        // /// Constructs the actual move based on the arguments.
        // /// </summary>
        // /// <param name="pos"></param>
        // /// <param name="moves">The move list to add the generated (if any) moves into</param>
        // /// <param name="piece">The moving piece</param>
        // /// <param name="from">The from square</param>
        // /// <param name="to">The to square</param>
        // /// <param name="promoted">The promotion piece (if any, defaults to NoPiece type)</param>
        // /// <param name="flags"></param>
        // /// <param name="type">The move type</param>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static void AddMove(this IPosition pos, IMoveList moves, Piece piece, Square from, Square to, Piece promoted, MoveGenerationFlags flags, MoveTypes type = MoveTypes.Quiet)
        // {
        //     Move move;
        //
        //     if (type.HasFlagFast(MoveTypes.Capture))
        //         move = new Move(piece, pos.GetPiece(to), from, to, type, promoted);
        //     else if (type.HasFlagFast(MoveTypes.Epcapture))
        //         move = new Move(piece, PieceTypes.Pawn.MakePiece(~pos.SideToMove), from, to, type, promoted);
        //     else
        //         move = new Move(piece, from, to, type, promoted);
        //
        //     // check if move is actual a legal move if the flag is enabled
        //     if (flags.HasFlagFast(MoveGenerationFlags.Legalmoves) && !pos.IsLegal(move, piece, from, type))
        //         return;
        //
        //     moves.Add(move);
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private static (Direction, Direction) GetPawnAttackDirections(this Player us)
        // {
        //     Span<(Directions, Directions)> directions = stackalloc[] {(Directions.NorthEast, Directions.NorthWest), (Directions.SouthEast, Directions.SouthWest)};
        //     return directions[us.Side];
        // }
        //
        // private static void GenerateCastling(IPosition pos, IMoveList moveList, Player us, CastlelingRights cr, bool checks, MoveGenerationFlags flags)
        // {
        //     if (pos.CastlingImpeded(cr) || !pos.CanCastle(cr))
        //         return;
        //
        //     var kingSide = cr.HasFlagFast(CastlelingRights.WhiteOo | CastlelingRights.BlackOo);
        //
        //     // After castling, the rook and king final positions are the same in Chess960 as they
        //     // would be in standard chess.
        //     var kfrom = pos.GetPieceSquare(PieceTypes.King, us);
        //     var rfrom = pos.CastlingRookSquare(cr);
        //     var kto = (kingSide ? Squares.g1 : Squares.c1).RelativeSquare(us);
        //     var enemies = pos.Pieces(~us);
        //
        //     //Debug.Assert(0 == pos.checkers());
        //
        //     var k = pos.Chess960
        //         ? kto > kfrom
        //             ? Directions.West
        //             : Directions.East
        //         : kingSide
        //             ? Directions.West
        //             : Directions.East;
        //
        //     for (var s = kto; s != kfrom; s += k)
        //         if ((pos.AttacksTo(s) & enemies) != 0)
        //             return;
        //
        //     // Because we generate only legal castling moves we need to verify that when moving the
        //     // castling rook we do not discover some hidden checker. For instance an enemy queen in
        //     // SQ_A1 when castling rook is in SQ_B1.
        //     if (pos.Chess960 && (kto.GetAttacks(PieceTypes.Rook, pos.Pieces() ^ rfrom) & pos.Pieces(PieceTypes.Rook, PieceTypes.Queen, ~us)) != 0)
        //         return;
        //
        //     var m = new Move(PieceTypes.King.MakePiece(us), kfrom, rfrom, MoveTypes.Castle, PieceExtensions.EmptyPiece);
        //     if (!pos.State.InCheck && !pos.GivesCheck(m))
        //         return;
        //
        //     moveList.Add(m);
        //
        //     //var m = new Move();
        //     //var m = Types.make(kfrom, rfrom, MoveTypeS.CASTLING);
        //
        //     //if (Checks && !pos.gives_check(m, ci))
        //     //    return mPos;
        //
        //     //mlist[mPos++].move = m;
        //
        //     //return mPos;
        // }
    }
}