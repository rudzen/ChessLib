namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System;
    using System.Diagnostics;
    using Types;

    public class MoveList2
    {
        private readonly Memory<ExtMove> _moves;
        public int cur, last;

        public MoveList2()
        {
            _moves = new ExtMove[256].AsMemory();
        }

        public ulong Count => (ulong)last;

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
            _moves.Span[last] = Move.EmptyMove;
        }

        public ReadOnlySpan<ExtMove> GetMoves()
        {
            return last == 0 || _moves.Span[0].Move.IsNullMove()
                ? ReadOnlySpan<ExtMove>.Empty
                : _moves.Span.Slice(0, last);
        }

        private static int GeneratePawnMoves(IPosition pos, Span<ExtMove> moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            // Compute our parametrized parameters at compile time, named according to the point of
            // view of white side.

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

            var ksq = pos.GetKingSquare(them);

            BitBoard pawnOne;
            BitBoard pawnTwo;
            var emptySquares = BitBoard.Empty;

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

                            var dcCandidates = pos.BlockersForKing(them);

                            // Add pawn pushes which give discovered check. This is possible only if
                            // the pawn is not on the same file as the enemy king, because we don't
                            // generate captures. Note that a possible discovery check promotion has
                            // been already generated among captures.
                            if (!(pawnsNotOn7 & dcCandidates).IsEmpty)
                            {
                                var dc1 = (pawnsNotOn7 & dcCandidates).Shift(up) & emptySquares & ~ksq.BitBoardSquare();
                                var dc2 = (dc1 & rank3Bb).Shift(up) & emptySquares;
                                pawnOne |= dc1;
                                pawnTwo |= dc2;
                            }

                            break;
                        }
                }

                while (!pawnOne.IsEmpty)
                {
                    var to = BitBoards.PopLsb(ref pawnOne);
                    moves[index++].Move = Move.MakeMove(to - up, to);
                }

                while (!pawnTwo.IsEmpty)
                {
                    var to = BitBoards.PopLsb(ref pawnTwo);
                    moves[index++].Move = Move.MakeMove(to - up - up, to);
                }
            }

            // Promotions and underpromotions
            if (!pawnsOn7.IsEmpty)// && (type != MoveGenerationType.Evasions || !(target & rank8Bb).IsEmpty))
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

                var b1 = pawnsOn7.Shift(right) & enemies;
                var b2 = pawnsOn7.Shift(left) & enemies;
                var b3 = pawnsOn7.Shift(up) & emptySquares;

                while (!b1.IsEmpty)
                    index = MakePromotions(moves, index, BitBoards.PopLsb(ref b1), ksq, right, type);

                while (!b2.IsEmpty)
                    index = MakePromotions(moves, index, BitBoards.PopLsb(ref b2), ksq, left, type);

                while (!b3.IsEmpty)
                    index = MakePromotions(moves, index, BitBoards.PopLsb(ref b3), ksq, up, type);
            }

            // Standard and en-passant captures
            if (type != MoveGenerationType.Captures && type != MoveGenerationType.Evasions && type != MoveGenerationType.NonEvasions)
                return index;

            pawnOne = pawnsNotOn7.Shift(right) & enemies;
            pawnTwo = pawnsNotOn7.Shift(left) & enemies;

            while (!pawnOne.IsEmpty)
            {
                var to = BitBoards.PopLsb(ref pawnOne);
                moves[index++].Move = Move.MakeMove(to - right, to);
            }

            while (!pawnTwo.IsEmpty)
            {
                var to = BitBoards.PopLsb(ref pawnTwo);
                moves[index++].Move = Move.MakeMove(to - left, to);
            }

            if (pos.State.EnPassantSquare == Square.None)
                return index;

            Debug.Assert(pos.EnPassantSquare.Rank() == Ranks.Rank6.RelativeRank(us));
            // An en passant capture can be an evasion only if the checking piece is the double
            // pushed pawn and so is in the target. Otherwise this is a discovery check and we are
            // forced to do otherwise.
            if (type == MoveGenerationType.Evasions && (target & (pos.EnPassantSquare - up)) == 0)
                return index;

            pawnOne = pawnsNotOn7 & pos.State.EnPassantSquare.PawnAttack(them);
            Debug.Assert(!pawnOne.IsEmpty);
            while (!pawnOne.IsEmpty)
                moves[index++].Move = Move.MakeMove(BitBoards.PopLsb(ref pawnOne), pos.EnPassantSquare, MoveTypes.Enpassant);

            return index;
        }

        private static int GenerateMoves(IPosition pos, Span<ExtMove> moves, int index, Player us, BitBoard target, PieceTypes pt, bool checks)
        {
            Debug.Assert(pt != PieceTypes.King && pt != PieceTypes.Pawn);

            var squares = pos.Squares(pt, us);
            
            if (squares.IsEmpty)
                return index;

            foreach (var from in squares)
            {
                if (from == Square.None)
                    return index;

                if (checks)
                {
                    if ((pt == PieceTypes.Bishop || pt == PieceTypes.Rook || pt == PieceTypes.Queen)//  .AsInt().InBetween(3, 5)
                        && (pt.PseudoAttacks(from) & target & pos.CheckedSquares(pt)).IsEmpty)
                        continue;

                    if (pos.BlockersForKing(~us) & from)
                        continue;
                }

                var b = pos.GetAttacks(from, pt) & target;

                var ksq = pos.GetKingSquare(~us);
                if (b & ksq)
                {
                    var f = pos.GenerateFen().ToString();
                }

                if (checks)
                    b &= pos.CheckedSquares(pt);

                while (!b.IsEmpty)
                {
                    var to = BitBoards.PopLsb(ref b);
                    moves[index++].Move = Move.MakeMove(from, to);
                }
            }

            return index;
        }

        private static int GenerateAll(IPosition pos, Span<ExtMove> moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            var checks = type == MoveGenerationType.QuietChecks;
            index = GeneratePawnMoves(pos, moves, index, target, us, type);

            for (var pt = PieceTypes.Knight; pt <= PieceTypes.Queen; ++pt)
                index = GenerateMoves(pos, moves, index, us, target, pt, checks);

            if (checks || type == MoveGenerationType.Evasions)
                return index;

            var ksq = pos.GetPieceSquare(PieceTypes.King, us);
            var b = pos.GetAttacks(ksq, PieceTypes.King) & target;
            while (!b.IsEmpty)
                moves[index++].Move = Move.MakeMove(ksq, BitBoards.PopLsb(ref b));

            if (type == MoveGenerationType.Captures)
                return index;

            var (kingSide, queenSide) = us.IsWhite
                ? (CastlelingRights.WhiteOo, CastlelingRights.WhiteOoo)
                : (CastlelingRights.BlackOo, CastlelingRights.BlackOoo);

            if (!pos.CanCastle(kingSide | queenSide))
                return index;

            if (!pos.CastlingImpeded(kingSide) && pos.CanCastle(kingSide))
                moves[index++].Move = Move.MakeMove(ksq, pos.CastlingRookSquare(kingSide), MoveTypes.Castling);

            if (!pos.CastlingImpeded(queenSide) && pos.CanCastle(queenSide))
                moves[index++].Move = Move.MakeMove(ksq, pos.CastlingRookSquare(queenSide), MoveTypes.Castling);

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
            Debug.Assert(!pos.State.InCheck);
            var us = pos.SideToMove;
            var target = type switch
            {
                MoveGenerationType.Captures => pos.Pieces(~us),
                MoveGenerationType.Quiets => ~pos.Pieces(),
                MoveGenerationType.NonEvasions => ~pos.Pieces(us),
                _ => BitBoard.Empty
            };

            return GenerateAll(pos, moves, index, target, us, type);
        }

        /// generate<EVASIONS> generates all pseudo-legal check evasions when the side to move is in
        /// check. Returns a pointer to the end of the move list.
        private static int GenerateEvasions(IPosition pos, Span<ExtMove> moves, int index)
        {
            Debug.Assert(pos.State.InCheck);
            var us = pos.SideToMove;
            var ksq = pos.GetKingSquare(us);
            var sliderAttacks = BitBoard.Empty;
            var sliders = pos.Checkers & ~pos.Pieces(PieceTypes.Pawn, PieceTypes.Knight);
            Square checksq;

            // Find all the squares attacked by slider checkers. We will remove them from the king
            // evasions in order to skip known illegal moves, which avoids any useless legality
            // checks later on.
            while (!sliders.IsEmpty)
            {
                checksq = BitBoards.PopLsb(ref sliders);
                sliderAttacks |= checksq.Line(ksq) ^ checksq.BitBoardSquare();
            }

            // Generate evasions for king, capture and non capture moves
            var b = pos.GetAttacks(ksq, PieceTypes.King) & ~pos.Pieces(us) & ~sliderAttacks;
            while (!b.IsEmpty)
                moves[index++].Move = Move.MakeMove(ksq, BitBoards.PopLsb(ref b));

            if (pos.Checkers.MoreThanOne())
                return index; // Double check, only a king move can save the day

            // Generate blocking evasions or captures of the checking piece
            checksq = pos.Checkers.Lsb();
            var target = checksq.BitboardBetween(ksq) | checksq.BitBoardSquare();

            return GenerateAll(pos, moves, index, target, us, MoveGenerationType.Evasions);
        }

        /// generate<LEGAL> generates all the legal moves in the given position
        private static int GenerateLegal(IPosition pos, Span<ExtMove> moves, int index)
        {
            var cur = index;
            var us = pos.SideToMove;
            var pinned = pos.BlockersForKing(us) & pos.Pieces(us);// pos.PinnedPieces(pos.SideToMove);
            var ksq = pos.GetKingSquare(us);

            var end = pos.State.InCheck
                ? GenerateEvasions(pos, moves, index)
                : Generate(pos, moves, index, MoveGenerationType.NonEvasions);

            while (cur != end)
                if ((!pinned.IsEmpty || moves[cur].Move.GetFromSquare() == ksq || moves[cur].Move.IsEnPassantMove())
                    && !pos.IsLegal(moves[cur].Move))
                    moves[cur].Move = moves[--end].Move;
                else
                    ++cur;
            return end;
        }

        /// generate<QUIET_CHECKS> generates all pseudo-legal non-captures and knight
        /// underpromotions that give check. Returns a pointer to the end of the move list.
        private static int GenerateQuietChecks(IPosition pos, Span<ExtMove> moves, int index)
        {
            Debug.Assert(!pos.State.InCheck);
            var us = pos.SideToMove;
            var dc = pos.BlockersForKing(~us) & pos.Pieces(us);

            while (!dc.IsEmpty)
            {
                var from = BitBoards.PopLsb(ref dc);
                var pt = pos.GetPiece(from).Type();

                if (pt == PieceTypes.Pawn)
                    continue; // Will be generated together with direct checks

                var b = pos.GetAttacks(from, pt) & ~pos.Pieces();

                if (pt == PieceTypes.King)
                    b &= ~PieceTypes.Queen.PseudoAttacks(pos.GetKingSquare(~us));

                while (!b.IsEmpty)
                    moves[index++].Move = Move.MakeMove(from, BitBoards.PopLsb(ref b));
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
                case MoveGenerationType.Quiets:
                case MoveGenerationType.NonEvasions:
                    result = GenerateCapturesQuietsNonEvasions(pos, moves, index, type);
                    break;

                case MoveGenerationType.Evasions:
                    result = GenerateEvasions(pos, moves, index);
                    break;

                case MoveGenerationType.QuietChecks:
                    result = GenerateQuietChecks(pos, moves, index);
                    break;

                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }

        private static int MakePromotions(Span<ExtMove> moves, int index, Square to, Square ksq, Direction direction, MoveGenerationType Type)
        {
            if (Type == MoveGenerationType.Captures || Type == MoveGenerationType.Evasions || Type == MoveGenerationType.NonEvasions)
                moves[index++].Move = Move.MakeMove(to - direction, to, MoveTypes.Promotion, PieceTypes.Queen);

            if (Type == MoveGenerationType.Quiets || Type == MoveGenerationType.Evasions || Type == MoveGenerationType.NonEvasions)
            {
                moves[index++].Move = Move.MakeMove(to - direction, to, MoveTypes.Promotion, PieceTypes.Rook);
                moves[index++].Move = Move.MakeMove(to - direction, to, MoveTypes.Promotion, PieceTypes.Bishop);
                moves[index++].Move = Move.MakeMove(to - direction, to, MoveTypes.Promotion);
            }

            // Knight promotion is the only promotion that can give a direct check that's not
            // already included in the queen promotion.
            if (Type == MoveGenerationType.QuietChecks && !(PieceTypes.Knight.PseudoAttacks(to) & ksq).IsEmpty)
                moves[index++].Move = Move.MakeMove(to - direction, to, MoveTypes.Promotion);

            return index;
        }
    }
}