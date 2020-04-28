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
    public class MoveList2 : IReadOnlyCollection<ExtMove>
    {
        private readonly ExtMove[] _moves;
        public int cur, last;

        public MoveList2()
        {
            _moves = new ExtMove[256];
        }

        public int Count => last;

        public Move Move => _moves[cur].Move;
        
        public IEnumerator<ExtMove> GetEnumerator()
            => _moves.TakeWhile(move => !move.Move.IsNullMove()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static MoveList2 operator ++(MoveList2 moveList)
        {
            ++moveList.cur;
            return moveList;
        }

        /// <summary>
        /// Clears the move generated data
        /// </summary>
        public void Clear()
        {
            cur = last = 0;
            _moves.Fill(ExtMove.Empty);
        }

        public void Generate(IPosition pos, MoveGenerationType type)
        {
            cur = 0;
            last = Generate(pos, _moves, 0, type);
            _moves[last].Move = MoveExtensions.EmptyMove;
        }
        
        private int GenerateCastling(IPosition pos, ExtMove[] moves, int index, Player us, CastlelingRights cr, bool checks, MoveGenerationType type)
        {
            if (pos.CastlingImpeded(cr) || !pos.CanCastle(cr))
                return index;

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
                    return index;

            // Because we generate only legal castling moves we need to verify that when moving the
            // castling rook we do not discover some hidden checker. For instance an enemy queen in
            // SQ_A1 when castling rook is in SQ_B1.
            if (pos.Chess960 && (pos.GetAttacks(kto,PieceTypes.Rook, pos.Pieces() ^ rfrom) & pos.Pieces(PieceTypes.Rook, PieceTypes.Queen, ~us)) != 0)
                return index;

            var m = Move.MakeMove(kfrom, rfrom, MoveTypes.Castling);
            if (!pos.State.InCheck && !pos.GivesCheck(m))
                return index;

            // moves[index++].Move = 
            // moves.Add(m);

            //var m = new Move();
            //var m = make(kfrom, rfrom, MoveTypeS.CASTLING);

            //if (Checks && !pos.gives_check(m, ci))
            //    return mPos;

            //mlist[mPos++].Move = m;

            //return mPos;

            return index;
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
        //     var enemies = pos.pieces_color(notColor(us));
        //     Debug.Assert(0 == pos.checkers());
        //     var K = Chess960 ? kto > kfrom ? SquareS.DELTA_W : SquareS.DELTA_E
        //         : KingSide ? SquareS.DELTA_W : SquareS.DELTA_E;
        //     for (var s = kto; s != kfrom; s += K)
        //         if ((pos.attackers_to(s) & enemies) != 0)
        //             return mPos;
        //     // Because we generate only legal castling moves we need to verify that when moving the
        //     // castling rook we do not discover some hidden checker. For instance an enemy queen in
        //     // SQ_A1 when castling rook is in SQ_B1.
        //     if (Chess960 && (kto.attacks_bb_SBBPT(pos.pieces() ^ BitBoard.SquareBB[rfrom], PieceTypeS.ROOK) & pos.pieces_color_piecetype(notColor(us), PieceTypeS.ROOK, PieceTypeS.QUEEN)) != 0)
        //         return mPos;
        //     var m = make(kfrom, rfrom, MoveTypeS.CASTLING);
        //     if (Checks && !pos.gives_check(m, ci))
        //         return mPos;
        //     mlist[mPos++].Move = m;
        //     return mPos;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GeneratePromotions(ExtMove[] moveList, int index, BitBoard pawnsOn7, BitBoard target, MoveGenerationType type, Direction delta, Square ksq)
        {
            var b = pawnsOn7.Shift(delta) & target;
            while (b != 0)
            {
                var to = BitBoards.PopLsb(ref b);
                if (type == MoveGenerationType.CAPTURES || type == MoveGenerationType.EVASIONS || type == MoveGenerationType.NON_EVASIONS)
                    moveList[index++].Move = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Queen);
                if (type == MoveGenerationType.QUIETS || type == MoveGenerationType.EVASIONS || type == MoveGenerationType.NON_EVASIONS)
                {
                    moveList[index++].Move = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Rook);
                    moveList[index++].Move = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Bishop);
                    moveList[index++].Move = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Knight);
                }

                // Knight promotion is the only promotion that can give a direct check that's not
                // already included in the queen promotion.
                if (type == MoveGenerationType.QUIET_CHECKS && (BitBoard.StepAttacksBB[PieceS.W_KNIGHT][to] & ksq) != 0)
                    moveList[index++].Move = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Knight);
            }

            return index;
        }

        private static int GeneratePawnMoves(IPosition pos, ExtMove[] mlist, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            // Compute our parametrized parameters at compile time, named according to the point of view of white side.
            
            var them = ~us;
            var rank8Bb = us.PromotionRank();
            var rank7Bb = us.Rank7();
            var rank3Bb = us.Rank3();
            
            var up = us.PawnPushDistance();
            var right = us.IsWhite() ? Directions.NorthEast : Directions.SouthWest;
            var left = us.IsWhite() ? Directions.NorthWest : Directions.SouthEast;
            
            var pawns = pos.Pieces(PieceTypes.Pawn, us);
            var pawnsOn7 = pawns & rank7Bb;
            var pawnsNotOn7 = pawns & ~rank7Bb;

            BitBoard b1, b2;
            BitBoard emptySquares = 0;

            var enemies = type switch
            {
                MoveGenerationType.EVASIONS => pos.Pieces(them) & target,
                MoveGenerationType.CAPTURES => target,
                _ => pos.Pieces(them)
            };
            
            // Single and double pawn pushes, no promotions
            if (type != MoveGenerationType.CAPTURES)
            {
                emptySquares = type == MoveGenerationType.QUIETS || type == MoveGenerationType.QUIET_CHECKS
                    ? target
                    : ~pos.Pieces();
                
                b1 = pawnsNotOn7.Shift(up) & emptySquares;
                b2 = (b1 & rank3Bb).Shift(up) & emptySquares;
                
                switch (type)
                {
                    // Consider only blocking squares
                    case MoveGenerationType.EVASIONS:
                        b1 &= target;
                        b2 &= target;
                        break;
                    case MoveGenerationType.QUIET_CHECKS:
                    {
                        var ksq = pos.GetPieceSquare(PieceTypes.King, us);
                        b1 &= ksq.PawnAttack(them);
                        b2 &= ksq.PawnAttack(them);
                        
                        // Add pawn pushes which give discovered check. This is possible only if the
                        // pawn is not on the same file as the enemy king, because we don't generate
                        // captures. Note that a possible discovery check promotion has been already
                        // generated among captures.
                        if ((pawnsNotOn7 & ci.dcCandidates) != 0)
                        {
                            var dc1 = (pawnsNotOn7 & ci.dcCandidates).Shift(up) & emptySquares & ~ksq.BitBoardSquare();
                            var dc2 = (dc1 & rank3Bb).Shift(up) & emptySquares;
                            b1 |= dc1;
                            b2 |= dc2;
                        }

                        break;
                    }
                }

                while (b1 != 0)
                {
                    var to = BitBoards.PopLsb(ref b1);
                    mlist[index++].Move = Move.MakeMove(to - up, to);
                }

                while (b2 != 0)
                {
                    var to = BitBoards.PopLsb(ref b2);
                    mlist[index++].Move = Move.MakeMove(to - up - up, to);
                }
            }

            // Promotions and underpromotions
            if (pawnsOn7 != 0 && (type != MoveGenerationType.EVASIONS || (target & rank8Bb) != 0))
            {
                switch (type)
                {
                    case MoveGenerationType.CAPTURES:
                        emptySquares = ~pos.Pieces();
                        break;
                    case MoveGenerationType.EVASIONS:
                        emptySquares &= target;
                        break;
                }
                
                index = GeneratePromotions(mlist, index, pawnsOn7, enemies, type, right);
                index = GeneratePromotions(mlist, index, pawnsOn7, enemies, type, left);
                index = GeneratePromotions(mlist, index, pawnsOn7, emptySquares, type, up);
            }

            // Standard and en-passant captures
            if (type != MoveGenerationType.CAPTURES && type != MoveGenerationType.EVASIONS && type != MoveGenerationType.NON_EVASIONS)
                return index;
            
            b1 = pawnsNotOn7.Shift(right) & enemies;
            b2 = pawnsNotOn7.Shift(left) & enemies;
            
            while (b1 != 0)
            {
                var to = BitBoards.PopLsb(ref b1);
                mlist[index++].Move = Move.MakeMove(to - right, to);
            }

            while (b2 != 0)
            {
                var to = BitBoards.PopLsb(ref b2);
                mlist[index++].Move = Move.MakeMove(to - left, to);
            }

            if (pos.State.EnPassantSquare == Squares.none)
                return index;
            
            Debug.Assert(pos.EnPassantSquare.Rank() == Ranks.Rank6.RelativeRank(us));
            // An en passant capture can be an evasion only if the checking piece is the
            // double pushed pawn and so is in the target. Otherwise this is a discovery
            // check and we are forced to do otherwise.
            if (type == MoveGenerationType.EVASIONS && (target & (pos.EnPassantSquare - up)) == 0)
                return index;
                
            b1 = pawnsNotOn7 & pos.State.EnPassantSquare.PawnAttack(them);
            Debug.Assert(b1 != 0);
            while (b1 != 0)
                mlist[index++].Move = Move.MakeMove(BitBoards.PopLsb(ref b1), pos.EnPassantSquare, MoveTypes.Enpassant);

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static int GenerateMoves(IPosition pos, ExtMove[] moves, int index, Player us, BitBoard target, PieceTypes pt, bool checks)
        {
            Debug.Assert(pt != PieceTypes.King && pt != PieceTypes.Pawn);
            var pieces = pos.Pieces(pt, us);
            while (!pieces.Empty())
            {
                var from = BitBoards.PopLsb(ref pieces);

                if (checks)
                {
                    if ((pt.AsInt().InBetween(3, 5))
                        && 0 == (pt.PseudoAttacks(from) & target & pos.CheckedSquares(pt)))
                        continue;
                    if (ci.dcCandidates != 0 && (ci.dcCandidates & from) != 0)
                        continue;
                }

                var b = pos.PieceAttacks(from, pt) & target;
                if (checks)
                    b &= pos.CheckedSquares(pt);
                while (b != 0)
                    moves[index++].Move = Move.MakeMove(from, BitBoards.PopLsb(ref b));
            }

            // var pieceList = pos.list(us, pt);
            // var pl = 0;
            // for (var from = pieceList[pl]; from != SquareS.SQ_NONE; from = pieceList[++pl])
            // {
            //
            // }
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static int GenerateAll(IPosition pos, ExtMove[] moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            var checks = type == MoveGenerationType.QUIET_CHECKS;
            index = GeneratePawnMoves(pos, moves, index, target, us, type);

            for (var pt = PieceTypes.Knight; pt <= PieceTypes.Queen; ++pt)
                index = GenerateMoves(pos, moves, index, us, target, pt, checks);

            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Knight, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Bishop, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Rook, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Queen, checks);

            if (!checks && type != MoveGenerationType.EVASIONS)
            {
                var ksq = pos.GetPieceSquare(PieceTypes.King, us);
                var b = pos.PieceAttacks(ksq, PieceTypes.King) & target;
                while (b != 0)
                    moves[index++].Move = Move.MakeMove(ksq, BitBoards.PopLsb(ref b));
            }

            if (type == MoveGenerationType.CAPTURES || type == MoveGenerationType.EVASIONS || !pos.CanCastle(us))
                return index;
            
            index = GenerateCastling(pos, moves, index, us, ci, new MakeCastlingS(us, CastlingSideS.KING_SIDE).right, checks, pos.Chess960);
            index = GenerateCastling(pos, moves, index, us, ci, new MakeCastlingS(us, CastlingSideS.QUEEN_SIDE).right, checks, pos.Chess960);

            return index;
        }

        /// generate<CAPTURES> generates all pseudo-legal captures and queen promotions. Returns a
        /// pointer to the end of the move list.
        ///
        /// generate<QUIETS> generates all pseudo-legal non-captures and underpromotions. Returns a
        /// pointer to the end of the move list.
        ///
        /// generate<NON_EVASIONS> generates all pseudo-legal captures and non-captures. Returns a
        /// pointer to the end of the move list.
        private static int GenerateCapturesQuietsNonEvasions(IPosition pos, ExtMove[] moves, int index, MoveGenerationType type)
        {
            Debug.Assert(type == MoveGenerationType.CAPTURES || type == MoveGenerationType.QUIETS || type == MoveGenerationType.NON_EVASIONS);
            Debug.Assert(pos.checkers() == 0);
            var us = pos.SideToMove;
            var target = type switch
            {
                MoveGenerationType.CAPTURES => pos.Pieces(~us),
                MoveGenerationType.QUIETS => ~pos.Pieces(),
                MoveGenerationType.NON_EVASIONS => ~pos.Pieces(us),
                _ => 0
            };
            
            return GenerateAll(pos, moves, index, target, us, type);
        }

        /// generate<EVASIONS> generates all pseudo-legal check evasions when the side to move is in
        /// check. Returns a pointer to the end of the move list.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static int GenerateEvasions(IPosition pos, ExtMove[] moves, int index)
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
                moves[index++].Move = Move.MakeMove(ksq, BitBoards.PopLsb(ref b));
            
            // Double check, only a king move can save the day
            if (pos.checkers().more_than_one())
                return index;
            
            // Generate blocking evasions or captures of the checking piece
            Square checksq2 = pos.checkers().lsb();
            var target = checksq2.BitboardBetween(ksq) | checksq2;
            return GenerateAll(pos, moves, index, target, us, MoveGenerationType.EVASIONS);
        }

        /// generate<LEGAL> generates all the legal moves in the given position
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static int GenerateLegal(IPosition pos, ExtMove[] moves, int index)
        {
            var cur = index;
            var pinned = pos.pinned_pieces(pos.side_to_move());
            var ksq = pos.GetPieceSquare(PieceTypes.King, pos.SideToMove);
            var end = pos.checkers() != 0
                ? GenerateEvasions(pos, moves, index)
                : Generate(pos, moves, index, MoveGenerationType.NON_EVASIONS);
            while (cur != end)
                if ((pinned != 0 || moves[cur].Move.GetFromSquare() == ksq || moves[cur].Move.IsEnPassantMove())
                    && !pos.legal(moves[cur].Move, pinned))
                    moves[cur].Move = moves[--end].Move;
                else
                    ++cur;
            return end;
        }

        /// generate<QUIET_CHECKS> generates all pseudo-legal non-captures and knight
        /// underpromotions that give check. Returns a pointer to the end of the move list.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static int GenerateQuietChecks(IPosition pos, ExtMove[] moves, int index)
        {
            Debug.Assert(0 == pos.checkers());
            var us = pos.SideToMove;
            var dc = ci.dcCandidates;
            var emptySquares = ~pos.Pieces();
            while (dc != 0)
            {
                var from = BitBoards.PopLsb(ref dc);
                var pt = pos.GetPieceType(from);
                if (pt == PieceTypes.Pawn)
                    continue; // Will be generated togheter with direct checks
                var b = pos.attacks_from_piece_square(pt, from) & emptySquares;
                if (pt == PieceTypes.King)
                    b &= ~PieceTypes.Queen.PseudoAttacks(ci.ksq);
                while (b != 0)
                    moves[index++].Move = Move.MakeMove(from, BitBoards.PopLsb(ref b));
            }

            return GenerateAll(pos, moves, index, ~pos.Pieces(), us, MoveGenerationType.QUIET_CHECKS, ci);
        }

        private static int Generate(IPosition pos, ExtMove[] mlist, int mPos, MoveGenerationType type)
        {
            int result;
            switch (type)
            {
                case MoveGenerationType.LEGAL:
                    result = GenerateLegal(pos, mlist, mPos);
                    break;
                case MoveGenerationType.CAPTURES:
                    result = GenerateCapturesQuietsNonEvasions(pos, mlist, mPos, type);
                    break;
                case MoveGenerationType.QUIETS:
                    result = GenerateCapturesQuietsNonEvasions(pos, mlist, mPos, type);
                    break;
                case MoveGenerationType.NON_EVASIONS:
                    result = GenerateCapturesQuietsNonEvasions(pos, mlist, mPos, type);
                    break;
                case MoveGenerationType.EVASIONS:
                    result = GenerateEvasions(pos, mlist, mPos);
                    break;
                case MoveGenerationType.QUIET_CHECKS:
                    result = GenerateQuietChecks(pos, mlist, mPos);
                    break;
                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }
    }
}