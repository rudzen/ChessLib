using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Rudz.Chess.Enums;
using Rudz.Chess.Extensions;
using Rudz.Chess.Types;

namespace Rudz.Chess
{
    public class MoveGen : IEnumerable<Move>
    {
        private readonly ExtMove[] _moveList;
        public int cur, last;

        public MoveGen(IPosition pos)
        {
            _moveList = new ExtMove[256];
        }

        public static MoveGen operator ++(MoveGen moveList)
        {
            ++moveList.cur;
            return moveList;
        }

        public IEnumerator<Move> GetEnumerator()
            => _moveList.TakeWhile(move => !move.Move.IsNullMove()).Select(move => move.Move).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static void GenerateCastling(IPosition pos, IMoveList moveList, Player us, CastlelingRights cr, bool checks, MoveGenerationFlags flags)
        {
            if (pos.CastlingImpeded(cr) || !pos.CanCastle(cr))
                return;

            var kingSide = cr.HasFlagFast(CastlelingRights.WhiteOo | CastlelingRights.BlackOo);

            // After castling, the rook and king final positions are the same in Chess960 as they
            // would be in standard chess.
            var kfrom = pos.GetPieceSquare(PieceTypes.King, us);
            var rfrom = pos.CastlingRookSquare(cr);
            var kto = (kingSide ? Squares.g1 : Squares.c1).RelativeSquare(us);
            var enemies = pos.Pieces(~us);

            //Debug.Assert(0 == pos.checkers());

            var k = pos.Chess960
                ? kto > kfrom
                    ? Directions.West
                    : Directions.East
                : kingSide
                    ? Directions.West
                    : Directions.East;

            for (var s = kto; s != kfrom; s += k)
                if ((pos.AttacksTo(s) & enemies) != 0)
                    return;

            // Because we generate only legal castling moves we need to verify that when moving the
            // castling rook we do not discover some hidden checker. For instance an enemy queen in
            // SQ_A1 when castling rook is in SQ_B1.
            if (pos.Chess960 && (kto.GetAttacks(PieceTypes.Rook, pos.Pieces() ^ rfrom) & pos.Pieces(PieceTypes.Rook, PieceTypes.Queen, ~us)) != 0)
                return;

            var m = new Move(PieceTypes.King.MakePiece(us), kfrom, rfrom, MoveTypes.Castle, PieceExtensions.EmptyPiece);
            if (!pos.State.InCheck && !pos.GivesCheck(m))
                return;

            moveList.Add(m);

            //var m = new Move();
            //var m = Types.make(kfrom, rfrom, MoveTypeS.CASTLING);

            //if (Checks && !pos.gives_check(m, ci))
            //    return mPos;

            //mlist[mPos++].move = m;

            //return mPos;
        }

        // private static int generate_castling(Position pos, ExtMove[] mlist, int mPos, Player us, CheckInfo ci, CastlelingRights Cr, bool Checks, bool Chess960)
        // {
        //     var KingSide = Cr == CastlelingRights.WhiteOo || Cr == CastlelingRights.BlackOo;
        //     if (pos.CastlingImpeded(Cr) || !pos.CanCastle(Cr))
        //         return mPos;
        //     // After castling, the rook and king final positions are the same in Chess960 as they
        //     // would be in standard chess.
        //     var kfrom = pos.GetPieceSquare(PieceTypes.King, us);
        //     var rfrom = pos.CastlingRookSquare(Cr);
        //     var kto = (KingSide ? Squares.g1 : Squares.c1).RelativeSquare(us);
        //     var enemies = pos.pieces_color(Types.notColor(us));
        //     Debug.Assert(0 == pos.checkers());
        //     var K = Chess960 ? kto > kfrom ? SquareS.DELTA_W : SquareS.DELTA_E
        //         : KingSide ? SquareS.DELTA_W : SquareS.DELTA_E;
        //     for (var s = kto; s != kfrom; s += K)
        //         if ((pos.attackers_to(s) & enemies) != 0)
        //             return mPos;
        //     // Because we generate only legal castling moves we need to verify that when moving the
        //     // castling rook we do not discover some hidden checker. For instance an enemy queen in
        //     // SQ_A1 when castling rook is in SQ_B1.
        //     if (Chess960 && (kto.attacks_bb_SBBPT(pos.pieces() ^ BitBoard.SquareBB[rfrom], PieceTypeS.ROOK) & pos.pieces_color_piecetype(Types.notColor(us), PieceTypeS.ROOK, PieceTypeS.QUEEN)) != 0)
        //         return mPos;
        //     var m = Types.make(kfrom, rfrom, MoveTypeS.CASTLING);
        //     if (Checks && !pos.gives_check(m, ci))
        //         return mPos;
        //     mlist[mPos++].move = m;
        //     return mPos;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GeneratePromotions(ExtMove[] moveList, int mPos, BitBoard pawnsOn7, BitBoard target, CheckInfo ci, MoveGenerationFlags type, Direction delta)
        {
            var b = pawnsOn7.Shift(delta) & target;
            while (b != 0)
            {
                var to = BitBoards.PopLsb(ref b);
                if (type == MoveGenerationFlags.CAPTURES || type == MoveGenerationFlags.EVASIONS || type == MoveGenerationFlags.NON_EVASIONS)
                    moveList[mPos++].move = Types.make(to - delta, to, MoveTypeS.PROMOTION, PieceTypeS.QUEEN);
                if (type == MoveGenerationFlags.QUIETS || type == MoveGenerationFlags.EVASIONS || type == MoveGenerationFlags.NON_EVASIONS)
                {
                    moveList[mPos++].move = Types.make(to - delta, to, MoveTypeS.PROMOTION, PieceTypeS.ROOK);
                    moveList[mPos++].move = Types.make(to - delta, to, MoveTypeS.PROMOTION, PieceTypeS.BISHOP);
                    moveList[mPos++].move = Types.make(to - delta, to, MoveTypeS.PROMOTION, PieceTypeS.KNIGHT);
                }

                // Knight promotion is the only promotion that can give a direct check that's not
                // already included in the queen promotion.
                if (type == MoveGenerationFlags.QUIET_CHECKS && (BitBoard.StepAttacksBB[PieceS.W_KNIGHT][to] & ci.ksq) != 0)
                    moveList[mPos++].move = Types.make(to - delta, to, MoveTypeS.PROMOTION, PieceTypeS.KNIGHT);
            }

            return mPos;
        }

        private static int GeneratePawnMoves(IPosition pos, ExtMove[] mlist, int mPos, BitBoard target, CheckInfo ci, Player us, MoveGenerationFlags type)
        {
            // Compute our parametrized parameters at compile time, named according to the point of
            // view of white side.
            var them = ~us;
            var rank8Bb = us.PromotionRank();
            var rank7Bb = us.Rank7();
            var rank3Bb = us.Rank3();
            var up = us.PawnPushDistance();
            var right = us.IsWhite() ? Directions.NorthEast : Directions.SouthWest;
            var left = us.IsWhite() ? Directions.NorthWest : Directions.SouthEast;
            BitBoard b1, b2;
            BitBoard emptySquares = 0;
            var pawns = pos.Pieces(PieceTypes.Pawn, us);
            var pawnsOn7 = pawns & rank7Bb;
            var pawnsNotOn7 = pawns & ~rank7Bb;

            var enemies = type switch
            {
                MoveGenerationFlags.EVASIONS => pos.Pieces(them) & target,
                MoveGenerationFlags.CAPTURES => target,
                _ => pos.Pieces(them)
            };
            
            // Single and double pawn pushes, no promotions
            if (type != MoveGenerationFlags.CAPTURES)
            {
                emptySquares = type == MoveGenerationFlags.QUIETS || type == MoveGenerationFlags.QUIET_CHECKS
                    ? target
                    : ~pos.Pieces();
                b1 = pawnsNotOn7.Shift(up) & emptySquares;
                b2 = (b1 & rank3Bb).Shift(up) & emptySquares;
                if (type == MoveGenerationFlags.EVASIONS) // Consider only blocking squares
                {
                    b1 &= target;
                    b2 &= target;
                }
                else if (type == MoveGenerationFlags.QUIET_CHECKS)
                {
                    b1 &= ci.ksq.Pawn pos.PieceAttacks(ci.ksq,) pos.attacks_from_pawn(ci.ksq, them);
                    b2 &= pos.attacks_from_pawn(ci.ksq, them);
                    // Add pawn pushes which give discovered check. This is possible only if the
                    // pawn is not on the same file as the enemy king, because we don't generate
                    // captures. Note that a possible discovery check promotion has been already
                    // generated among captures.
                    if ((pawnsNotOn7 & ci.dcCandidates) != 0)
                    {
                        var dc1 = (pawnsNotOn7 & ci.dcCandidates).Shift(up) & emptySquares & ~ci.ksq.file_bb_square();
                        var dc2 = (dc1 & rank3Bb).Shift(up) & emptySquares;
                        b1 |= dc1;
                        b2 |= dc2;
                    }
                }

                while (b1 != 0)
                {
                    var to = BitBoards.PopLsb(ref b1);
                    mlist[mPos++].move = Types.make_move(to - up, to);
                }

                while (b2 != 0)
                {
                    var to = BitBoards.PopLsb(ref b2);
                    mlist[mPos++].move = Types.make_move(to - up - up, to);
                }
            }

            // Promotions and underpromotions
            if (pawnsOn7 != 0 && (type != MoveGenerationFlags.EVASIONS || (target & rank8Bb) != 0))
            {
                if (type == MoveGenerationFlags.CAPTURES)
                    emptySquares = ~pos.Pieces();
                else if (type == MoveGenerationFlags.EVASIONS)
                    emptySquares &= target;
                
                mPos = GeneratePromotions(mlist, mPos, pawnsOn7, enemies, ci, type, right);
                mPos = GeneratePromotions(mlist, mPos, pawnsOn7, enemies, ci, type, left);
                mPos = GeneratePromotions(mlist, mPos, pawnsOn7, emptySquares, ci, type, up);
            }

            // Standard and en-passant captures
            if (type == MoveGenerationFlags.CAPTURES || type == MoveGenerationFlags.EVASIONS || type == MoveGenerationFlags.NON_EVASIONS)
            {
                b1 = pawnsNotOn7.Shift(right) & enemies;
                b2 = pawnsNotOn7.Shift(left) & enemies;
                while (b1 != 0)
                {
                    var to = BitBoards.PopLsb(ref b1);
                    mlist[mPos++].move = Types.make_move(to - right, to);
                }

                while (b2 != 0)
                {
                    var to = BitBoards.PopLsb(ref b2);
                    mlist[mPos++].move = Types.make_move(to - left, to);
                }

                if (pos.State.EnPassantSquare != Squares.none)
                {
                    Debug.Assert(pos.EnPassantSquare.Rank() == Ranks.Rank6.RelativeRank(us));
                    // An en passant capture can be an evasion only if the checking piece is the
                    // double pushed pawn and so is in the target. Otherwise this is a discovery
                    // check and we are forced to do otherwise.
                    if (type == MoveGenerationFlags.EVASIONS && (target & (pos.EnPassantSquare - up)) == 0)
                        return mPos;
                    
                    b1 = pawnsNotOn7 & pos.State.EnPassantSquare.PawnAttack(them);
                    Debug.Assert(b1 != 0);
                    while (b1 != 0)
                        mlist[mPos++].move = Types.make(BitBoards.PopLsb(ref b1), pos.EnPassantSquare, MoveTypeS.ENPASSANT);
                }
            }

            return mPos;
        }

        private static int GenerateMoves(IPosition pos, ExtMove[] mlist, int mPos, Player us, BitBoard target, CheckInfo ci, PieceTypes pt, bool Checks)
        {
            Debug.Assert(pt != PieceTypes.King && pt != PieceTypes.Pawn);
            var pieces = pos.Pieces(pt, us);
            while (!pieces.Empty())
            {
                var from = BitBoards.PopLsb(ref pieces);

                if (Checks)
                {
                    if ((pt.AsInt().InBetween(3, 5))
                        && 0 == (from.PseudoAttack(pt) & target & ci.checkSq[pt]))
                        continue;
                    if (ci.dcCandidates != 0 && (ci.dcCandidates & from) != 0)
                        continue;
                }

                var b = pos.PieceAttacks(from, pt) & target;
                if (Checks)
                    b &= ci.checkSq[pt];
                while (b != 0)
                    mlist[mPos++].Move = Types.make_move(from, BitBoards.PopLsb(ref b));
            }

            // var pieceList = pos.list(us, pt);
            // var pl = 0;
            // for (var from = pieceList[pl]; from != SquareS.SQ_NONE; from = pieceList[++pl])
            // {
            //
            // }
            return mPos;
        }

        public static int GenerateAll(IPosition pos, ExtMove[] mlist, int moveIndex, BitBoard target, Player us, MoveGenerationFlags type, CheckInfo ci = null)
        {
            var checks = type == MoveGenerationFlags.QUIET_CHECKS;
            moveIndex = GeneratePawnMoves(pos, mlist, moveIndex, target, ci, us, type);

            for (var pt = PieceTypes.Knight; pt <= PieceTypes.Queen; ++pt)
                moveIndex = GenerateMoves(pos, mlist, moveIndex, us, target, ci, pt, checks);

            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Knight, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Bishop, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Rook, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Queen, checks);

            if (!checks && type != MoveGenerationFlags.EVASIONS)
            {
                var ksq = pos.GetPieceSquare(PieceTypes.King, us);
                var b = pos.PieceAttacks(ksq, PieceTypes.King) & target;
                while (b != 0)
                    mlist[moveIndex++].move = Types.make_move(ksq, BitBoard.pop_lsb(ref b));
            }

            if (type == MoveGenerationFlags.CAPTURES || type == MoveGenerationFlags.EVASIONS || !pos.CanCastle(us))
                return moveIndex;
            
            moveIndex = GenerateCastling(pos, mlist, moveIndex, us, ci, new MakeCastlingS(us, CastlingSideS.KING_SIDE).right, checks, pos.Chess960);
            moveIndex = GenerateCastling(pos, mlist, moveIndex, us, ci, new MakeCastlingS(us, CastlingSideS.QUEEN_SIDE).right, checks, pos.Chess960);

            return moveIndex;
        }

        /// generate<CAPTURES> generates all pseudo-legal captures and queen promotions. Returns a
        /// pointer to the end of the move list.
        ///
        /// generate<QUIETS> generates all pseudo-legal non-captures and underpromotions. Returns a
        /// pointer to the end of the move list.
        ///
        /// generate<NON_EVASIONS> generates all pseudo-legal captures and non-captures. Returns a
        /// pointer to the end of the move list.
        public static int generate_captures_quiets_non_evasions(IPosition pos, ExtMove[] mlist, int mPos, MoveGenerationFlags type)
        {
            Debug.Assert(type == MoveGenerationFlags.CAPTURES || type == MoveGenerationFlags.QUIETS || type == MoveGenerationFlags.NON_EVASIONS);
            Debug.Assert(pos.checkers() == 0);
            var us = pos.SideToMove;
            var target = type switch
            {
                MoveGenerationFlags.CAPTURES => pos.Pieces(~us),
                MoveGenerationFlags.QUIETS => ~pos.Pieces(),
                MoveGenerationFlags.NON_EVASIONS => ~pos.Pieces(us),
                _ => 0
            };
            
            return GenerateAll(pos, mlist, mPos, target, us, type);
        }

        /// generate<EVASIONS> generates all pseudo-legal check evasions when the side to move is in
        /// check. Returns a pointer to the end of the move list.
        private static int generate_evasions(IPosition pos, ExtMove[] mlist, int mPos)
        {
            Debug.Assert(pos.checkers() != 0);
            var us = pos.SideToMove;
            var ksq = pos.GetPieceSquare(PieceTypes.King, us);
            BitBoard sliderAttacks = 0;
            var sliders = pos.checkers() & ~pos.Pieces(PieceTypes.Pawn, PieceTypes.Knight);
            // Find all the squares attacked by slider checkers. We will remove them from the king
            // evasions in order to skip known illegal moves, which avoids any useless legality
            // checks later on.
            while (sliders != 0)
            {
                var checksq = BitBoards.PopLsb(ref sliders);
                sliderAttacks |= checksq.Line(ksq) ^ checksq;
            }

            // Generate evasions for king, capture and non capture moves
            var b = pos.PieceAttacks(ksq, PieceTypes.King) & ~pos.Pieces(us) & ~sliderAttacks;
            while (b != 0)
                mlist[mPos++].move = Types.make_move(ksq, BitBoards.PopLsb(ref b));
            if (pos.checkers().more_than_one())
                return mPos; // Double check, only a king move can save the day
            // Generate blocking evasions or captures of the checking piece
            Square checksq2 = pos.checkers().lsb();
            var target = checksq2.BitboardBetween(ksq) | checksq2;
            return GenerateAll(pos, mlist, mPos, target, us, MoveGenerationFlags.EVASIONS);
        }

        /// generate<LEGAL> generates all the legal moves in the given position
        public static int generate_legal(IPosition pos, ExtMove[] mlist, int mPos)
        {
            var cur = mPos;
            var pinned = pos.pinned_pieces(pos.side_to_move());
            var ksq = pos.king_square(pos.side_to_move());
            var end = pos.checkers() != 0
                ? generate_evasions(pos, mlist, mPos)
                : generate(pos, mlist, mPos, GenTypeS.NON_EVASIONS);
            while (cur != end)
                if ((pinned != 0 || Types.from_sq(mlist[cur].move) == ksq || Types.type_of_move(mlist[cur].move) == MoveTypeS.ENPASSANT)
                    && !pos.legal(mlist[cur].move, pinned))
                    mlist[cur].move = mlist[--end].move;
                else
                    ++cur;
            return end;
        }

        /// generate<QUIET_CHECKS> generates all pseudo-legal non-captures and knight
        /// underpromotions that give check. Returns a pointer to the end of the move list.
        public static int generate_quiet_checks(IPosition pos, ExtMove[] mlist, int mPos)
        {
            Debug.Assert(0 == pos.checkers());
            var us = pos.SideToMove;
            var ci = new CheckInfo(pos);
            var dc = ci.dcCandidates;
            while (dc != 0)
            {
                var from = BitBoards.PopLsb(ref dc);
                var pt = Types.type_of_piece(pos.piece_on(from));
                if (pt == PieceTypeS.PAWN)
                    continue; // Will be generated togheter with direct checks
                var b = pos.attacks_from_piece_square(pt, from) & ~pos.Pieces();
                if (pt == PieceTypeS.KING)
                    b &= ~BitBoard.PseudoAttacks[PieceTypeS.QUEEN][ci.ksq];
                while (b != 0)
                    mlist[mPos++].move = Types.make_move(from, BitBoard.pop_lsb(ref b));
            }

            return us == ColorS.WHITE
                ? GenerateAll(pos, mlist, mPos, ~pos.pieces(), ColorS.WHITE, GenTypeS.QUIET_CHECKS, ci)
                : GenerateAll(pos, mlist, mPos, ~pos.pieces(), ColorS.BLACK, GenTypeS.QUIET_CHECKS, ci);
        }

        public static int generate(IPosition pos, ExtMove[] mlist, int mPos, MoveGenerationFlags type)
        {
            int result;
            switch (type)
            {
                case MoveGenerationFlags.LEGAL:
                    result = generate_legal(pos, mlist, mPos);
                    break;
                case MoveGenerationFlags.CAPTURES:
                    result = generate_captures_quiets_non_evasions(pos, mlist, mPos, type);
                    break;
                case MoveGenerationFlags.QUIETS:
                    result = generate_captures_quiets_non_evasions(pos, mlist, mPos, type);
                    break;
                case MoveGenerationFlags.NON_EVASIONS:
                    result = generate_captures_quiets_non_evasions(pos, mlist, mPos, type);
                    break;
                case MoveGenerationFlags.EVASIONS:
                    result = generate_evasions(pos, mlist, mPos);
                    break;
                case MoveGenerationFlags.QUIET_CHECKS:
                    result = generate_quiet_checks(pos, mlist, mPos);
                    break;
                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }
    }
}