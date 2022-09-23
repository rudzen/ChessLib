/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

using System;
using System.Diagnostics;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.MoveGeneration;

public static class MoveGenerator
{
    private static readonly (CastleRight, CastleRight)[] CastleSideRights =
    {
        (CastleRight.WhiteKing, CastleRight.WhiteQueen),
        (CastleRight.BlackKing, CastleRight.BlackQueen)
    };

    public static int Generate(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        Player us,
        MoveGenerationType type)
    {
        switch (type)
        {
            case MoveGenerationType.Legal:
                return GenerateLegal(in pos, moves, index, us);

            case MoveGenerationType.Captures:
            case MoveGenerationType.Quiets:
            case MoveGenerationType.NonEvasions:
                return GenerateCapturesQuietsNonEvasions(in pos, moves, index, us, type);

            case MoveGenerationType.Evasions:
                return GenerateEvasions(in pos, moves, index, us);

            case MoveGenerationType.QuietChecks:
                return GenerateQuietChecks(in pos, moves, index, us);

            default:
                Debug.Assert(false);
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static int GenerateAll(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        in BitBoard target,
        Player us,
        MoveGenerationType type)
    {
        index = GeneratePawnMoves(in pos, moves, index, in target, us, type);

        var checks = type == MoveGenerationType.QuietChecks;

        for (var pt = PieceTypes.Knight; pt <= PieceTypes.Queen; ++pt)
            index = GenerateMoves(in pos, moves, index, us, in target, pt, checks);

        if (checks || type == MoveGenerationType.Evasions)
            return index;

        var ksq = pos.GetKingSquare(us);
        var b = pos.GetAttacks(ksq, PieceTypes.King) & target;
        index = Move.Create(moves, index, ksq, ref b);

        if (type == MoveGenerationType.Captures || !pos.CanCastle(pos.SideToMove))
            return index;

        var (kingSide, queenSide) = CastleSideRights[us.Side];

        if (pos.CanCastle(kingSide) && !pos.CastlingImpeded(kingSide))
            moves[index++].Move = Move.Create(ksq, pos.CastlingRookSquare(kingSide), MoveTypes.Castling);

        if (pos.CanCastle(queenSide) && !pos.CastlingImpeded(queenSide))
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
    private static int GenerateCapturesQuietsNonEvasions(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        Player us,
        MoveGenerationType type)
    {
        Debug.Assert(type is MoveGenerationType.Captures or MoveGenerationType.Quiets
            or MoveGenerationType.NonEvasions);
        Debug.Assert(!pos.InCheck);
        var target = type switch
        {
            MoveGenerationType.Captures => pos.Pieces(~us),
            MoveGenerationType.Quiets => ~pos.Pieces(),
            MoveGenerationType.NonEvasions => ~pos.Pieces(us),
            _ => BitBoard.Empty
        };

        return GenerateAll(in pos, moves, index, in target, us, type);
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
    private static int GenerateEvasions(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        Player us)
    {
        Debug.Assert(pos.InCheck);
        var ksq = pos.GetKingSquare(us);
        var sliderAttacks = BitBoard.Empty;
        var sliders = pos.Checkers & ~pos.Pieces(PieceTypes.Pawn, PieceTypes.Knight);
        Square checkSquare;

        // Find all the squares attacked by slider checkers. We will remove them from the king
        // evasions in order to skip known illegal moves, which avoids any useless legality
        // checks later on.
        while (sliders)
        {
            checkSquare = BitBoards.PopLsb(ref sliders);
            var fullLine = checkSquare.Line(ksq);
            fullLine ^= checkSquare;
            fullLine ^= ksq;
            sliderAttacks |= fullLine;
        }

        // Generate evasions for king, capture and non capture moves
        var b = pos.GetAttacks(ksq, PieceTypes.King) & ~pos.Pieces(us) & ~sliderAttacks;
        index = Move.Create(moves, index, ksq, ref b);

        if (pos.Checkers.MoreThanOne())
            return index; // Double check, only a king move can save the day

        // Generate blocking evasions or captures of the checking piece
        checkSquare = pos.Checkers.Lsb();
        var target = checkSquare.BitboardBetween(ksq) | checkSquare;

        return GenerateAll(in pos, moves, index, in target, us, MoveGenerationType.Evasions);
    }

    /// <summary>
    /// Generates all the legal moves in the given position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="moves"></param>
    /// <param name="index"></param>
    /// <param name="us"></param>
    /// <returns></returns>
    private static int GenerateLegal(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        Player us)
    {
        var end = pos.InCheck
            ? GenerateEvasions(in pos, moves, index, us)
            : Generate(in pos, moves, index, us, MoveGenerationType.NonEvasions);

        var pinned = pos.KingBlockers(us) & pos.Pieces(us);
        var ksq = pos.GetKingSquare(us);

        // In case there exists pinned pieces, the move is a king move (including castles) or
        // it's an en-passant move the move is checked, otherwise we assume the move is legal.
        var pinnedIsEmpty = pinned.IsEmpty;
        while (index != end)
            if ((!pinnedIsEmpty || moves[index].Move.FromSquare() == ksq || moves[index].Move.IsEnPassantMove())
                && !pos.IsLegal(moves[index].Move))
                moves[index].Move = moves[--end].Move;
            else
                ++index;

        return end;
    }

    /// <summary>
    /// Generate moves for knight, bishop, rook and queen
    /// </summary>
    /// <param name="pos">The current position</param>
    /// <param name="moves">The current moves so far</param>
    /// <param name="index">The current move index</param>
    /// <param name="us">The current player</param>
    /// <param name="target">The target of where to move to</param>
    /// <param name="pt">The piece type performing the moves</param>
    /// <param name="checks">Position is in check</param>
    /// <returns>The new move index</returns>
    private static int GenerateMoves(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        Player us,
        in BitBoard target,
        PieceTypes pt,
        bool checks)
    {
        Debug.Assert(pt != PieceTypes.King && pt != PieceTypes.Pawn);

        var squares = pos.Squares(pt, us);

        foreach (var from in squares)
        {
            if (checks
                && (!(pos.KingBlockers(~us) & from).IsEmpty ||
                    !(pt.PseudoAttacks(from) & target & pos.CheckedSquares(pt))))
                continue;

            var b = checks
                ? pos.GetAttacks(from, pt) & target & pos.CheckedSquares(pt)
                : pos.GetAttacks(from, pt) & target;

            index = Move.Create(moves, index, from, ref b);
        }

        return index;
    }

    /// <summary>
    /// Generate all pawn moves. The type of moves are determined by the <see cref="MoveGenerationType"/>
    /// </summary>
    /// <param name="pos">The current position</param>
    /// <param name="moves">The current moves so far</param>
    /// <param name="index">The index of the current moves</param>
    /// <param name="target">The target squares</param>
    /// <param name="us">The current player</param>
    /// <param name="type">The type of moves to generate</param>
    /// <returns>The new move index</returns>
    private static int GeneratePawnMoves(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        in BitBoard target,
        Player us,
        MoveGenerationType type)
    {
        // Compute our parametrized parameters named according to the point of view of white side.

        var them = ~us;
        var rank7Bb = us.Rank7();

        var pawns = pos.Pieces(PieceTypes.Pawn, us);

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
        BitBoard pawnOne;
        BitBoard pawnTwo;

        // Single and double pawn pushes, no promotions
        if (type != MoveGenerationType.Captures)
        {
            emptySquares = type is MoveGenerationType.Quiets or MoveGenerationType.QuietChecks
                ? target
                : ~pos.Pieces();

            pawnOne = pawnsNotOn7.Shift(up) & emptySquares;
            pawnTwo = (pawnOne & us.Rank3()).Shift(up) & emptySquares;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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

                    var dcCandidates = pawnsNotOn7 & pos.KingBlockers(them);

                    // Add pawn pushes which give discovered check. This is possible only if
                    // the pawn is not on the same file as the enemy king, because we don't
                    // generate captures. Note that a possible discovery check promotion has
                    // been already generated among captures.
                    if (dcCandidates)
                    {
                        var dc1 = dcCandidates.Shift(up) & emptySquares & ~ksq;
                        var dc2 = (dc1 & us.Rank3()).Shift(up) & emptySquares;
                        pawnOne |= dc1;
                        pawnTwo |= dc2;
                    }

                    break;
                }
            }

            while (pawnOne)
            {
                var to = BitBoards.PopLsb(ref pawnOne);
                moves[index++].Move = Move.Create(to - up, to);
            }

            while (pawnTwo)
            {
                var to = BitBoards.PopLsb(ref pawnTwo);
                moves[index++].Move = Move.Create(to - up - up, to);
            }
        }

        var pawnsOn7 = pawns & rank7Bb;
        var (right, left) = (us.PawnEastAttackDistance(), us.PawnWestAttackDistance());

        // Promotions and under-promotions
        if (pawnsOn7)
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

            var pawnPromoRight = pawnsOn7.Shift(right) & enemies;
            var pawnPromoLeft = pawnsOn7.Shift(left) & enemies;
            var pawnPromoUp = pawnsOn7.Shift(up) & emptySquares;

            while (pawnPromoRight)
                index = MakePromotions(moves, index, BitBoards.PopLsb(ref pawnPromoRight), ksq, right, type);

            while (pawnPromoLeft)
                index = MakePromotions(moves, index, BitBoards.PopLsb(ref pawnPromoLeft), ksq, left, type);

            while (pawnPromoUp)
                index = MakePromotions(moves, index, BitBoards.PopLsb(ref pawnPromoUp), ksq, up, type);
        }

        // Standard and en-passant captures
        if (type != MoveGenerationType.Captures
            && type != MoveGenerationType.Evasions
            && type != MoveGenerationType.NonEvasions)
            return index;

        pawnOne = pawnsNotOn7.Shift(right) & enemies;
        pawnTwo = pawnsNotOn7.Shift(left) & enemies;

        while (pawnOne)
        {
            var to = BitBoards.PopLsb(ref pawnOne);
            moves[index++].Move = Move.Create(to - right, to);
        }

        while (pawnTwo)
        {
            var to = BitBoards.PopLsb(ref pawnTwo);
            moves[index++].Move = Move.Create(to - left, to);
        }

        if (pos.EnPassantSquare == Square.None)
            return index;

        Debug.Assert(pos.EnPassantSquare.Rank == Rank.Rank6.Relative(us));

        // An en passant capture can be an evasion only if the checking piece is the double
        // pushed pawn and so is in the target. Otherwise this is a discovery check and we are
        // forced to do otherwise.
        if (type == MoveGenerationType.Evasions && (target & (pos.EnPassantSquare - up)).IsEmpty)
            return index;

        pawnOne = pawnsNotOn7 & pos.EnPassantSquare.PawnAttack(them);
        Debug.Assert(!pawnOne.IsEmpty);
        while (pawnOne)
            moves[index++].Move = Move.Create(BitBoards.PopLsb(ref pawnOne), pos.EnPassantSquare, MoveTypes.Enpassant);

        return index;
    }

    /// <summary>
    /// GenerateQuietChecks generates all pseudo-legal non-captures and knight underpromotions
    /// that give check. Returns a pointer to the end of the move list.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="moves"></param>
    /// <param name="index"></param>
    /// <param name="us"></param>
    /// <returns></returns>
    private static int GenerateQuietChecks(
        in IPosition pos,
        Span<ExtMove> moves,
        int index,
        Player us)
    {
        Debug.Assert(!pos.InCheck);
        var dc = pos.KingBlockers(~us) & pos.Pieces(us);
        var emptySquares = ~pos.Pieces();

        while (dc)
        {
            var from = BitBoards.PopLsb(ref dc);
            var pt = pos.GetPiece(from).Type();

            // No need to generate pawn quiet checks,
            // as these will be generated together with direct checks
            if (pt == PieceTypes.Pawn)
                continue;

            var b = pos.GetAttacks(from, pt) & emptySquares;

            if (pt == PieceTypes.King)
                b &= ~PieceTypes.Queen.PseudoAttacks(pos.GetKingSquare(~us));

            index = Move.Create(moves, index, from, ref b);
        }

        return GenerateAll(in pos, moves, index, in emptySquares, us, MoveGenerationType.QuietChecks);
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
    private static int MakePromotions(
        Span<ExtMove> moves,
        int index,
        Square to,
        Square ksq,
        Direction direction,
        MoveGenerationType type)
    {
        var from = to - direction;

        switch (type)
        {
            case MoveGenerationType.Captures:
            case MoveGenerationType.Evasions:
            case MoveGenerationType.NonEvasions:
                moves[index++].Move = Move.Create(from, to, MoveTypes.Promotion, PieceTypes.Queen);
                if (PieceTypes.Knight.PseudoAttacks(to) & ksq)
                    moves[index++].Move = Move.Create(from, to, MoveTypes.Promotion);
                break;
        }

        if (type != MoveGenerationType.Quiets
            && type != MoveGenerationType.Evasions
            && type != MoveGenerationType.NonEvasions)
            return index;

        moves[index++].Move = Move.Create(from, to, MoveTypes.Promotion, PieceTypes.Rook);
        moves[index++].Move = Move.Create(from, to, MoveTypes.Promotion, PieceTypes.Bishop);

        if (!(PieceTypes.Knight.PseudoAttacks(to) & ksq))
            moves[index++].Move = Move.Create(from, to, MoveTypes.Promotion);

        return index;
    }
}