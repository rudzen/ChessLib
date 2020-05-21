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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Types;

    public sealed class MoveList : IMoveList
    {
        private readonly ExtMove[] _moves;
        private int _cur, _last;

        public MoveList()
        {
            _moves = new ExtMove[218];
        }

        int IReadOnlyCollection<ExtMove>.Count => _last;

        public ulong Length => (ulong)_last;

        public Move CurrentMove => _moves[_cur].Move;

        public static MoveList operator ++(MoveList moveList)
        {
            ++moveList._cur;
            return moveList;
        }

        public void Add(ExtMove item) => _moves[_last++] = item;

        public void Add(Move item) => _moves[_last++] = item;

        /// <summary>
        /// Reset the moves
        /// </summary>
        public void Clear()
        {
            _cur = _last = 0;
            _moves[0].Move = ExtMove.Empty;
        }

        public bool Contains(ExtMove item)
            => Contains(item.Move);

        public bool Contains(Move item)
        {
            for (var i = 0; i < _last; ++i)
                if (_moves[i].Move == item)
                    return true;

            return false;
        }

        public bool Contains(Square from, Square to)
        {
            for (var i = 0; i < _last; ++i)
            {
                var move = _moves[i].Move;
                if (move.GetFromSquare() == from && move.GetToSquare() == to)
                    return true;
            }

            return false;
        }

        public void Generate(IPosition pos, MoveGenerationType type = MoveGenerationType.Legal)
        {
            _cur = 0;
            _last = Generate(pos, _moves, 0, pos.SideToMove, type);
            _moves[_last] = Move.EmptyMove;
        }

        public ReadOnlySpan<ExtMove> Get() =>
            _last == 0
                ? ReadOnlySpan<ExtMove>.Empty
                : _moves.AsSpan().Slice(0, _last);

        public IEnumerator<ExtMove> GetEnumerator()
            => _moves.Take(_last).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <param name="us"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static int GeneratePawnMoves(IPosition pos, ExtMove[] moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            // Compute our parametrized parameters at compile time, named according to the point of
            // view of white side.

            var them = ~us;
            var rank7Bb = us.Rank7();
            var rank3Bb = us.Rank3();

            var pawns = pos.Pieces(PieceTypes.Pawn, us);

            BitBoard pawnOne;
            BitBoard pawnTwo;

            var enemies = type switch
            {
                MoveGenerationType.Evasions => pos.Pieces(them) & target,
                MoveGenerationType.Captures => target,
                _ => pos.Pieces(them)
            };

            var ksq = pos.GetKingSquare(them);
            var emptySquares = BitBoard.Empty;
            var pawnsNotOn7 = pawns & ~rank7Bb;
            var up = us.PawnPushDistance();

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
                                var dc1 = (pawnsNotOn7 & dcCandidates).Shift(up) & emptySquares & ~ksq;
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
                    moves[index++].Move = Move.Create(to - up, to);
                }

                while (!pawnTwo.IsEmpty)
                {
                    var to = BitBoards.PopLsb(ref pawnTwo);
                    moves[index++].Move = Move.Create(to - up - up, to);
                }
            }

            var pawnsOn7 = pawns & rank7Bb;
            var (right, left) = us.IsWhite
                ? (Directions.NorthEast, Directions.NorthWest)
                : (Directions.SouthWest, Directions.SouthEast);

            // Promotions and underpromotions
            if (!pawnsOn7.IsEmpty)
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
                moves[index++].Move = Move.Create(to - right, to);
            }

            while (!pawnTwo.IsEmpty)
            {
                var to = BitBoards.PopLsb(ref pawnTwo);
                moves[index++].Move = Move.Create(to - left, to);
            }

            if (pos.EnPassantSquare == Square.None)
                return index;

            Debug.Assert(pos.EnPassantSquare.Rank == Ranks.Rank6.RelativeRank(us));

            // An en passant capture can be an evasion only if the checking piece is the double
            // pushed pawn and so is in the target. Otherwise this is a discovery check and we are
            // forced to do otherwise.
            if (type == MoveGenerationType.Evasions && (target & (pos.EnPassantSquare - up)) == 0)
                return index;

            pawnOne = pawnsNotOn7 & pos.EnPassantSquare.PawnAttack(them);
            Debug.Assert(!pawnOne.IsEmpty);
            while (!pawnOne.IsEmpty)
                moves[index++].Move = Move.Create(BitBoards.PopLsb(ref pawnOne), pos.EnPassantSquare, MoveTypes.Enpassant);

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="us"></param>
        /// <param name="target"></param>
        /// <param name="pt"></param>
        /// <param name="checks"></param>
        /// <returns></returns>
        private static int GenerateMoves(IPosition pos, ExtMove[] moves, int index, Player us, BitBoard target, PieceTypes pt, bool checks)
        {
            Debug.Assert(pt != PieceTypes.King && pt != PieceTypes.Pawn);

            var squares = pos.Squares(pt, us);

            if (squares.IsEmpty)
                return index;

            foreach (var from in squares)
            {
                if (checks)
                {
                    if ((pt == PieceTypes.Bishop || pt == PieceTypes.Rook || pt == PieceTypes.Queen)
                        && (pt.PseudoAttacks(from) & target & pos.CheckedSquares(pt)).IsEmpty)
                        continue;

                    if (pos.BlockersForKing(~us) & from)
                        continue;
                }

                var b = pos.GetAttacks(from, pt) & target;

                if (checks)
                    b &= pos.CheckedSquares(pt);

                while (!b.IsEmpty)
                {
                    var to = BitBoards.PopLsb(ref b);
                    moves[index++].Move = Move.Create(from, to);
                }
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <param name="us"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static int GenerateAll(IPosition pos, ExtMove[] moves, int index, BitBoard target, Player us, MoveGenerationType type)
        {
            var checks = type == MoveGenerationType.QuietChecks;
            index = GeneratePawnMoves(pos, moves, index, target, us, type);

            for (var pt = PieceTypes.Knight; pt <= PieceTypes.Queen; ++pt)
                index = GenerateMoves(pos, moves, index, us, target, pt, checks);

            if (checks || type == MoveGenerationType.Evasions)
                return index;

            var ksq = pos.GetKingSquare(us);
            var b = pos.GetAttacks(ksq, PieceTypes.King) & target;
            while (!b.IsEmpty)
                moves[index++].Move = Move.Create(ksq, BitBoards.PopLsb(ref b));

            if (type == MoveGenerationType.Captures)
                return index;

            var (kingSide, queenSide) = us.IsWhite
                ? (CastlelingRights.WhiteOo, CastlelingRights.WhiteOoo)
                : (CastlelingRights.BlackOo, CastlelingRights.BlackOoo);

            if (!pos.CanCastle(kingSide | queenSide))
                return index;

            if (!pos.CastlingImpeded(kingSide) && pos.CanCastle(kingSide))
                moves[index++].Move = Move.Create(ksq, pos.CastlingRookSquare(kingSide), MoveTypes.Castling);

            if (!pos.CastlingImpeded(queenSide) && pos.CanCastle(queenSide))
                moves[index++].Move = Move.Create(ksq, pos.CastlingRookSquare(queenSide), MoveTypes.Castling);

            return index;
        }

        /// <summary>
        /// Generates (pseudo-legal) Captures, Quiets and NonEvasions depending on the type
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="us"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static int GenerateCapturesQuietsNonEvasions(IPosition pos, ExtMove[] moves, int index, Player us, MoveGenerationType type)
        {
            Debug.Assert(type == MoveGenerationType.Captures || type == MoveGenerationType.Quiets || type == MoveGenerationType.NonEvasions);
            Debug.Assert(!pos.InCheck);
            var target = type switch
            {
                MoveGenerationType.Captures => pos.Pieces(~us),
                MoveGenerationType.Quiets => ~pos.Pieces(),
                MoveGenerationType.NonEvasions => ~pos.Pieces(us),
                _ => BitBoard.Empty
            };

            return GenerateAll(pos, moves, index, target, us, type);
        }

        /// <summary>
        /// GenerateEvasions generates all pseudo-legal check evasions when the side to move is in
        /// check. Returns a pointer to the end of the move list.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="us"></param>
        /// <returns></returns>
        private static int GenerateEvasions(IPosition pos, ExtMove[] moves, int index, Player us)
        {
            Debug.Assert(pos.InCheck);
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
                var fullLine = checksq.Line(ksq);
                fullLine ^= checksq;
                fullLine ^= ksq;
                sliderAttacks |= fullLine;
            }

            // Generate evasions for king, capture and non capture moves
            var b = pos.GetAttacks(ksq, PieceTypes.King) & ~pos.Pieces(us) & ~sliderAttacks;
            while (!b.IsEmpty)
                moves[index++].Move = Move.Create(ksq, BitBoards.PopLsb(ref b));

            if (pos.Checkers.MoreThanOne())
                return index; // Double check, only a king move can save the day

            // Generate blocking evasions or captures of the checking piece
            checksq = pos.Checkers.Lsb();
            var target = checksq.BitboardBetween(ksq) | checksq;

            return GenerateAll(pos, moves, index, target, us, MoveGenerationType.Evasions);
        }

        /// <summary>
        /// Generates all the legal moves in the given position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="us"></param>
        /// <returns></returns>
        private static int GenerateLegal(IPosition pos, ExtMove[] moves, int index, Player us)
        {
            var pinned = pos.BlockersForKing(us) & pos.Pieces(us);
            var ksq = pos.GetKingSquare(us);

            var end = pos.InCheck
                ? GenerateEvasions(pos, moves, index, us)
                : Generate(pos, moves, index, us, MoveGenerationType.NonEvasions);

            // In case there exists pinned pieces, the move is a king move (including castleling) or
            // it's an en-pessant move the move is checked, otherwise we assume the move is legal.
            while (index != end)
            {
                var pinnedIsEmpty = pinned.IsEmpty;
                if ((!pinnedIsEmpty || moves[index].Move.GetFromSquare() == ksq || moves[index].Move.IsEnPassantMove())
                    && !pos.IsLegal(moves[index].Move))
                    moves[index].Move = moves[--end].Move;
                else
                    ++index;
            }

            return end;
        }

        /// <summary>
        /// GenerateQuietChecks generates all pseudo-legal non-captures and knight underpromotions
        /// that give check. Returns a pointer to the end of the move list. ///
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="us"></param>
        /// <returns></returns>
        private static int GenerateQuietChecks(IPosition pos, ExtMove[] moves, int index, Player us)
        {
            Debug.Assert(!pos.InCheck);
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
                    moves[index++].Move = Move.Create(from, BitBoards.PopLsb(ref b));
            }

            return GenerateAll(pos, moves, index, ~pos.Pieces(), us, MoveGenerationType.QuietChecks);
        }

        private static int Generate(IPosition pos, ExtMove[] moves, int index, Player us, MoveGenerationType type)
        {
            int result;
            switch (type)
            {
                case MoveGenerationType.Legal:
                    result = GenerateLegal(pos, moves, index, us);
                    break;

                case MoveGenerationType.Captures:
                case MoveGenerationType.Quiets:
                case MoveGenerationType.NonEvasions:
                    result = GenerateCapturesQuietsNonEvasions(pos, moves, index, us, type);
                    break;

                case MoveGenerationType.Evasions:
                    result = GenerateEvasions(pos, moves, index, us);
                    break;

                case MoveGenerationType.QuietChecks:
                    result = GenerateQuietChecks(pos, moves, index, us);
                    break;

                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }

        /// <summary>
        /// Generate promotion moves and adds them to the list depending on the generation type
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="index"></param>
        /// <param name="to"></param>
        /// <param name="ksq"></param>
        /// <param name="direction"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static int MakePromotions(ExtMove[] moves, int index, Square to, Square ksq, Direction direction, MoveGenerationType type)
        {
            if (type == MoveGenerationType.Captures || type == MoveGenerationType.Evasions || type == MoveGenerationType.NonEvasions)
                moves[index++].Move = Move.Create(to - direction, to, MoveTypes.Promotion, PieceTypes.Queen);

            if (type == MoveGenerationType.Quiets || type == MoveGenerationType.Evasions || type == MoveGenerationType.NonEvasions)
            {
                moves[index++].Move = Move.Create(to - direction, to, MoveTypes.Promotion, PieceTypes.Rook);
                moves[index++].Move = Move.Create(to - direction, to, MoveTypes.Promotion, PieceTypes.Bishop);
                moves[index++].Move = Move.Create(to - direction, to, MoveTypes.Promotion);
            }

            // Knight promotion is the only promotion that can give a direct check that's not
            // already included in the queen promotion.
            if (type == MoveGenerationType.QuietChecks && !(PieceTypes.Knight.PseudoAttacks(to) & ksq).IsEmpty)
                moves[index++].Move = Move.Create(to - direction, to, MoveTypes.Promotion);

            return index;
        }
    }
}