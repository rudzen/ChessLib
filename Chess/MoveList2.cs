namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Types;

    public class MoveList2
    {
        private readonly Memory<ExtMove> _moves;
        public int cur, last;

        public MoveList2()
        {
            _moves = new ExtMove[256].AsMemory();
        }

        public ulong Count => (ulong) last;

        public bool IsReadOnly => true;

        public Move Move => _moves.Span[cur].Move;
        
        public static MoveList2 operator ++(MoveList2 moveList)
        {
            ++moveList.cur;
            return moveList;
        }

        public void Add(ExtMove item) => _moves.Span[last++] = item;

        public void Add(Move item) => _moves.Span[last++] = item;

        /// <summary>
        /// Clears the move generated data
        /// </summary>
        public void Clear()
        {
            cur = last = 0;
            _moves.Span.Clear();
            // _moves.Fill(ExtMove.Empty);
        }

        public bool Contains(ExtMove item)
            => Contains(item.Move);

        public bool Contains(Move item)
        {
            var s = GetMoves();
            
            foreach (var em in s)
            {
                if (em.Move == item)
                    return true;
            }

            return false;
        }
        
        public void Generate(IPosition pos, MoveGenerationType type)
        {
            cur = 0;
            last = Generate(pos, _moves.Span, 0, type);
            _moves.Span[last] = MoveExtensions.EmptyMove;
        }

        public ReadOnlySpan<ExtMove> GetMoves()
        {
            return last == 0 || _moves.Span[0].Move.IsNullMove()
                ? ReadOnlySpan<ExtMove>.Empty
                : _moves.Span.Slice(0, last);
        }

        private static int GenerateCastling(IPosition pos, Span<ExtMove> moves, int index, Player us, CastlelingRights cr)
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

            moves[index++] = m;

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
        private static int GeneratePromotions(Span<ExtMove> moveList, int index, BitBoard pawnsOn7, BitBoard target, MoveGenerationType type, Direction delta, Square ksq)
        {
            var b = pawnsOn7.Shift(delta) & target;
            while (b != 0)
            {
                var to = BitBoards.PopLsb(ref b);
                if (type == MoveGenerationType.Captures || type == MoveGenerationType.Evasions || type == MoveGenerationType.NonEvasions)
                    moveList[index++] = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Queen);
                if (type == MoveGenerationType.Quiets || type == MoveGenerationType.Evasions || type == MoveGenerationType.NonEvasions)
                {
                    moveList[index++] = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Rook);
                    moveList[index++] = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Bishop);
                    moveList[index++] = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Knight);
                }

                // Knight promotion is the only promotion that can give a direct check that's not
                // already included in the queen promotion.
                if (type == MoveGenerationType.QuietChecks && (PieceTypes.Knight.PseudoAttacks(to) & ksq) != 0)
                    moveList[index++] = Move.MakeMove(to - delta, to, MoveTypes.Promotion, PieceTypes.Knight);
            }

            return index;
        }

        private static int GeneratePawnMoves(IPosition pos, Span<ExtMove> moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            // Compute our parametrized parameters at compile time, named according to the point of view of white side.
            
            var them = ~us;
            var rank8Bb = us.PromotionRank();
            var rank7Bb = us.Rank7();
            var rank3Bb = us.Rank3();
            var up = us.PawnPushDistance();
            var (right, left) = us.IsWhite
                ? (Directions.NorthEast, Directions.NorthWest)
                : (Directions.SouthWest, Directions.SouthEast);
            
            var pawns = pos.Pieces(PieceTypes.Pawn, us);
            var pawnsOn7 = pawns & rank7Bb;
            var pawnsNotOn7 = pawns & ~rank7Bb;

            var ksq = pos.GetPieceSquare(PieceTypes.King, us);
            
            BitBoard pawnOne;
            BitBoard pawnTwo;
            var emptySquares = BitBoards.EmptyBitBoard;

            var enemies = type switch
            {
                MoveGenerationType.Evasions => pos.Pieces(them) & target,
                MoveGenerationType.Captures => target,
                _ => pos.Pieces(them)
            };
            
            // Single and double pawn pushes, no promotions
            if (type != MoveGenerationType.Captures)
            {
                emptySquares = type == MoveGenerationType.Quiets || type == MoveGenerationType.QuietChecks
                    ? target
                    : ~pos.Pieces();
                
                pawnOne = pawnsNotOn7.Shift(up) & emptySquares;
                pawnTwo = (pawnOne & rank3Bb).Shift(up) & emptySquares;
                
                switch (type)
                {
                    // Consider only blocking squares
                    case MoveGenerationType.Evasions:
                        pawnOne &= target;
                        pawnTwo &= target;
                        break;
                    case MoveGenerationType.QuietChecks:
                    {
                        pawnOne &= ksq.PawnAttack(them);
                        pawnTwo &= ksq.PawnAttack(them);

                        var dcCandidates = pos.BlockersForKing(us);
                        
                        // Add pawn pushes which give discovered check. This is possible only if the
                        // pawn is not on the same file as the enemy king, because we don't generate
                        // captures. Note that a possible discovery check promotion has been already
                        // generated among captures.
                        if ((pawnsNotOn7 & dcCandidates) != 0)
                        {
                            var dc1 = (pawnsNotOn7 & dcCandidates).Shift(up) & emptySquares & ~ksq.BitBoardSquare();
                            var dc2 = (dc1 & rank3Bb).Shift(up) & emptySquares;
                            pawnOne |= dc1;
                            pawnTwo |= dc2;
                        }

                        break;
                    }
                }

                while (pawnOne != 0)
                {
                    var to = BitBoards.PopLsb(ref pawnOne);
                    moves[index++] = Move.MakeMove(to - up, to);
                }

                while (pawnTwo != 0)
                {
                    var to = BitBoards.PopLsb(ref pawnTwo);
                    moves[index++] = Move.MakeMove(to - up - up, to);
                }
            }

            // Promotions and underpromotions
            if (pawnsOn7 != 0 && (type != MoveGenerationType.Evasions || (target & rank8Bb) != 0))
            {
                switch (type)
                {
                    case MoveGenerationType.Captures:
                        emptySquares = ~pos.Pieces();
                        break;
                    case MoveGenerationType.Evasions:
                        emptySquares &= target;
                        break;
                }
                
                index = GeneratePromotions(moves, index, pawnsOn7, enemies, type, right, ksq);
                index = GeneratePromotions(moves, index, pawnsOn7, enemies, type, left, ksq);
                index = GeneratePromotions(moves, index, pawnsOn7, emptySquares, type, up, ksq);
            }

            // Standard and en-passant captures
            if (type != MoveGenerationType.Captures && type != MoveGenerationType.Evasions && type != MoveGenerationType.NonEvasions)
                return index;
            
            pawnOne = pawnsNotOn7.Shift(right) & enemies;
            pawnTwo = pawnsNotOn7.Shift(left) & enemies;
            
            while (pawnOne != 0)
            {
                var to = BitBoards.PopLsb(ref pawnOne);
                moves[index++] = Move.MakeMove(to - right, to);
            }

            while (pawnTwo != 0)
            {
                var to = BitBoards.PopLsb(ref pawnTwo);
                moves[index++] = Move.MakeMove(to - left, to);
            }

            if (pos.State.EnPassantSquare == Squares.none)
                return index;
            
            Debug.Assert(pos.EnPassantSquare.Rank() == Ranks.Rank6.RelativeRank(us));
            // An en passant capture can be an evasion only if the checking piece is the
            // double pushed pawn and so is in the target. Otherwise this is a discovery
            // check and we are forced to do otherwise.
            if (type == MoveGenerationType.Evasions && (target & (pos.EnPassantSquare - up)) == 0)
                return index;
                
            pawnOne = pawnsNotOn7 & pos.State.EnPassantSquare.PawnAttack(them);
            Debug.Assert(pawnOne != 0);
            while (pawnOne != 0)
                moves[index++] = Move.MakeMove(BitBoards.PopLsb(ref pawnOne), pos.EnPassantSquare, MoveTypes.Enpassant);

            return index;
        }

        private static int GenerateMoves(IPosition pos, Span<ExtMove> moves, int index, Player us, BitBoard target, PieceTypes pt, bool checks)
        {
            Debug.Assert(pt != PieceTypes.King && pt != PieceTypes.Pawn);
            
            var pieces = pos.Pieces(pt, us);
            var dcCandidates = pos.BlockersForKing(us);
            while (!pieces.Empty)
            {
                var from = BitBoards.PopLsb(ref pieces);

                if (checks)
                {
                    if ((pt.AsInt().InBetween(3, 5))
                        && (pt.PseudoAttacks(from) & target & pos.CheckedSquares(pt)).Empty)
                        continue;
                    if (!dcCandidates.Empty && !(dcCandidates & from).Empty)
                        continue;
                }

                var b = pos.GetAttacks(from, pt) & target;
                if (checks)
                    b &= pos.CheckedSquares(pt);
                while (b != 0)
                    moves[index++] = Move.MakeMove(from, BitBoards.PopLsb(ref b));
            }

            // var pieceList = pos.list(us, pt);
            // var pl = 0;
            // for (var from = pieceList[pl]; from != SquareS.SQ_NONE; from = pieceList[++pl])
            // {
            //
            // }
            return index;
        }

        private static int GenerateAll(IPosition pos, Span<ExtMove> moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            var checks = type == MoveGenerationType.QuietChecks;
            index = GeneratePawnMoves(pos, moves, index, target, us, type);

            for (var pt = PieceTypes.Knight; pt <= PieceTypes.Queen; ++pt)
                index = GenerateMoves(pos, moves, index, us, target, pt, checks);

            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Knight, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Bishop, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Rook, checks);
            // mPos = generate_moves(pos, mlist, mPos, us, target, ci, PieceTypes.Queen, checks);

            if (!checks && type != MoveGenerationType.Evasions)
            {
                var ksq = pos.GetPieceSquare(PieceTypes.King, us);
                var b = pos.GetAttacks(ksq, PieceTypes.King) & target;
                while (b != 0)
                    moves[index++] = Move.MakeMove(ksq, BitBoards.PopLsb(ref b));
            }

            if (type == MoveGenerationType.Captures || type == MoveGenerationType.Evasions || !pos.CanCastle(us))
                return index;
            
            index = GenerateCastling(pos, moves, index, us, CastlelingSides.King.MakeCastlelingRights(us));
            index = GenerateCastling(pos, moves, index, us, CastlelingSides.Queen.MakeCastlelingRights(us));

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
        private static int GenerateCapturesQuietsNonEvasions(IPosition pos, Span<ExtMove> moves, int index, MoveGenerationType type)
        {
            Debug.Assert(type == MoveGenerationType.Captures || type == MoveGenerationType.Quiets || type == MoveGenerationType.NonEvasions);
            Debug.Assert(pos.Checkers().Empty);
            var us = pos.SideToMove;
            var target = type switch
            {
                MoveGenerationType.Captures => pos.Pieces(~us),
                MoveGenerationType.Quiets => ~pos.Pieces(),
                MoveGenerationType.NonEvasions => ~pos.Pieces(us),
                _ => 0
            };
            
            return GenerateAll(pos, moves, index, target, us, type);
        }

        /// generate<EVASIONS> generates all pseudo-legal check evasions when the side to move is in
        /// check. Returns a pointer to the end of the move list.
        private static int GenerateEvasions(IPosition pos, Span<ExtMove> moves, int index)
        {
            Debug.Assert(!pos.Checkers().Empty);
            var us = pos.SideToMove;
            var ksq = pos.GetPieceSquare(PieceTypes.King, us);
            BitBoard sliderAttacks = 0;
            var sliders = pos.Checkers() & ~pos.Pieces(PieceTypes.Pawn, PieceTypes.Knight);
            // Find all the squares attacked by slider checkers. We will remove them from the king
            // evasions in order to skip known illegal moves, which avoids any useless legality
            // checks later on.
            while (!sliders.Empty)
            {
                var checksq = BitBoards.PopLsb(ref sliders);
                sliderAttacks |= checksq.Line(ksq) ^ checksq;
            }

            // Generate evasions for king, capture and non capture moves
            var b = pos.GetAttacks(ksq, PieceTypes.King) & ~pos.Pieces(us) & ~sliderAttacks;
            while (!b.Empty)
                moves[index++] = Move.MakeMove(ksq, BitBoards.PopLsb(ref b));
            
            // Double check, only a king move can save the day
            if (pos.Checkers().MoreThanOne())
                return index;
            
            // Generate blocking evasions or captures of the checking piece
            var checksq2 = pos.Checkers().Lsb();
            var target = checksq2.BitboardBetween(ksq) | checksq2;
            return GenerateAll(pos, moves, index, target, us, MoveGenerationType.Evasions);
        }

        /// generate<LEGAL> generates all the legal moves in the given position
        private static int GenerateLegal(IPosition pos, Span<ExtMove> moves, int index)
        {
            var cur = index;
            var pinned = pos.PinnedPieces(pos.SideToMove);
            var ksq = pos.GetPieceSquare(PieceTypes.King, pos.SideToMove);
            var end = !pos.Checkers().Empty
                ? GenerateEvasions(pos, moves, index)
                : Generate(pos, moves, index, MoveGenerationType.NonEvasions);
            while (cur != end)
                if ((!pinned.Empty || moves[cur].Move.GetFromSquare() == ksq || moves[cur].Move.IsEnPassantMove())
                    && !pos.IsLegal(moves[cur].Move))
                    moves[cur].Move = moves[--end].Move;
                else
                    ++cur;
            return end;
        }

        /// generate<QUIET_CHECKS> generates all pseudo-legal non-captures and knight
        /// underpromotions that give check. Returns a pointer to the end of the move list.
        private static int GenerateQuietChecks(IPosition pos, Span<ExtMove> moves, int index, Square ksq)
        {
            Debug.Assert(pos.Checkers().Empty);
            var us = pos.SideToMove;
            var dc = pos.BlockersForKing(us);
            var emptySquares = ~pos.Pieces();
            
            while (!dc.Empty)
            {
                var from = BitBoards.PopLsb(ref dc);
                var pt = pos.GetPieceType(from);
                if (pt == PieceTypes.Pawn)
                    continue; // Will be generated togheter with direct checks
                var b = pos.GetAttacks(from, pt) & emptySquares;
                if (pt == PieceTypes.King)
                    b &= ~PieceTypes.Queen.PseudoAttacks(ksq);
                while (b != 0)
                    moves[index++] = Move.MakeMove(from, BitBoards.PopLsb(ref b));
            }

            return GenerateAll(pos, moves, index, ~pos.Pieces(), us, MoveGenerationType.QuietChecks);
        }

        private static int Generate(IPosition pos, Span<ExtMove> moves, int index, MoveGenerationType type)
        {
            int result;
            switch (type)
            {
                case MoveGenerationType.Legal:
                    result = GenerateLegal(pos, moves, index);
                    break;
                case MoveGenerationType.Captures:
                    result = GenerateCapturesQuietsNonEvasions(pos, moves, index, type);
                    break;
                case MoveGenerationType.Quiets:
                    result = GenerateCapturesQuietsNonEvasions(pos, moves, index, type);
                    break;
                case MoveGenerationType.NonEvasions:
                    result = GenerateCapturesQuietsNonEvasions(pos, moves, index, type);
                    break;
                case MoveGenerationType.Evasions:
                    result = GenerateEvasions(pos, moves, index);
                    break;
                case MoveGenerationType.QuietChecks:
                {
                    var ksq = pos.GetPieceSquare(PieceTypes.King, pos.SideToMove);
                    result = GenerateQuietChecks(pos, moves, index, ksq);
                }
                    break;
                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }
    }
}