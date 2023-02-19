/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Types;

[assembly: InternalsVisibleTo("Chess.Test")]

namespace Rudzoft.ChessLib;

/// <summary>
/// Computes pawn blockage (fences) See
/// https://elidavid.com/pubs/blockage2.pdf
/// https://pdfs.semanticscholar.org/31c2/d37c80ea1aef0676ba30393bc46c0ccc70e9.pdf
/// </summary>
public sealed class Blockage : IBlockage
{
    private static readonly BitBoard PawnFileASquares =
        File.FileA.FileBB() & ~(Rank.Rank1.RankBB() | Rank.Rank8.RankBB());

    /// <inheritdoc />
    public bool IsBlocked(in IPosition pos)
    {
        // Quick check if there is only pawns and kings on the board
        // It might be possible to have a minor piece and exchange it into a passing pawn
        if (pos.PieceCount(PieceTypes.AllPieces) > pos.PieceCount(PieceTypes.Pawn) + 2)
            return false;

        // Contains the rank for each file which has a fence
        Span<Rank> fenceRank = stackalloc Rank[File.Count];

        var us = pos.SideToMove;
        var them = ~us;

        var ourPawns = pos.Pieces(PieceTypes.Pawn, us);
        var theirPawns = pos.Pieces(PieceTypes.Pawn, them);
        var ourPawn = PieceTypes.Pawn.MakePiece(us);

        var up = us.PawnPushDistance();

        var fixedPawn = BitBoards.EmptyBitBoard;
        var marked = BitBoards.EmptyBitBoard;
        var dynamicPawns = BitBoards.EmptyBitBoard;
        var processed = BitBoards.EmptyBitBoard;

        var fence = BitBoards.EmptyBitBoard;

        MarkOurPawns(in pos, ref fixedPawn, ref marked, ref dynamicPawns);
        MarkTheirPawns(ref marked, in theirPawns, us);

        var isFenceFormed = FormFence(in marked, ref fence, ref processed, us);

        if (!isFenceFormed)
            return false;

        ComputeFenceRanks(fenceRank, in fence);

        var ourKsq = pos.GetKingSquare(us);

        if (ourKsq.RelativeRank(us) > fenceRank[ourKsq.File.AsInt()].Relative(us))
            return false;

        var theirKsq = pos.GetKingSquare(them);
        dynamicPawns |= ComputeDynamicFencedPawns(in pos, fenceRank, in theirPawns, us);

        while (dynamicPawns)
        {
            var sq = BitBoards.PopLsb(ref dynamicPawns);
            var (r, f) = sq;
            var rr = r.Relative(us);

            if (r > fenceRank[f.AsInt()])
            {
                if ((theirPawns & sq.PassedPawnFrontAttackSpan(us)).IsEmpty &&
                    (theirKsq.File != f || theirKsq.Rank.Relative(us) < rr))
                    return false;
            }
            else if (fence & sq)
            {
                if (rr >= Ranks.Rank6)
                    return false;

                if (pos.GetPiece(sq + us.PawnDoublePushDistance()) != ~ourPawn)
                {
                    if (theirKsq.File != f || theirKsq.RelativeRank(us) < rr)
                        return false;

                    if (f != File.FileA)
                    {
                        if (pos.GetPiece(sq + Direction.West) != ourPawn)
                            return false;

                        if (BitBoards.PopCount(ourPawns & PreviousFile(f)) > 1)
                            return false;

                        if ((fixedPawn & (sq + Direction.West)).IsEmpty)
                            return false;

                        if ((fence & (sq + Direction.West)).IsEmpty)
                            return false;
                    }

                    if (f != File.FileH)
                    {
                        if (pos.GetPiece(sq + Direction.East) != ourPawn)
                            return false;

                        if (BitBoards.PopCount(ourPawns & NextFile(f)) > 1)
                            return false;

                        if ((fixedPawn & (sq + Direction.East)).IsEmpty)
                            return false;

                        if ((fence & (sq + Direction.East)).IsEmpty)
                            return false;
                    }
                }

                if ((sq + us.PawnDoublePushDistance()).PawnAttack(us) & theirPawns)
                    return false;

                if (BitBoards.MoreThanOne(ourPawns & f))
                    return false;
            }
            else if (r < fenceRank[f.AsInt()])
            {
                sq += up;
                r = sq.Rank;
                rr = r.Relative(us);

                while ((fence & Square.Create(r, f)).IsEmpty)
                {
                    var pawnAttacks = sq.PawnAttack(us);
                    if (theirPawns & pawnAttacks)
                        return false;

                    if (ourPawns & sq)
                        break;

                    sq += up;
                    r = sq.Rank;
                }

                if (pos.IsOccupied(sq) || (fence & Square.Create(r, f)).IsEmpty)
                    continue;

                if (rr >= Ranks.Rank6)
                    return false;

                if ((theirPawns & (sq + us.PawnDoublePushDistance())).IsEmpty)
                {
                    if (theirKsq.File != f || theirKsq.RelativeRank(us) < rr)
                        return false;

                    if (f != File.FileA)
                    {
                        if (pos.GetPiece(sq + Direction.West) != ourPawn)
                            return false;

                        if (BitBoards.PopCount(ourPawns & (f - 1)) > 1)
                            return false;

                        if ((fixedPawn & Square.Create(r, PreviousFile(f))).IsEmpty)
                            return false;

                        if ((fence & Square.Create(r, PreviousFile(f))).IsEmpty)
                            return false;
                    }

                    if (f != File.FileH)
                    {
                        if (pos.GetPiece(sq + Direction.East) != ourPawn)
                            return false;

                        if (BitBoards.PopCount(ourPawns & (f + 1)) > 1)
                            return false;

                        if ((fixedPawn & Square.Create(r, NextFile(f))).IsEmpty)
                            return false;

                        if ((fence & Square.Create(r, NextFile(f))).IsEmpty)
                            return false;
                    }
                }

                if ((sq + up).PawnAttack(us) & theirPawns)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Computes the fence ranks
    /// </summary>
    private void ComputeFenceRanks(Span<Rank> fenceRank, in BitBoard fence)
    {
        var covered = fence;

        while (covered)
        {
            var sq = BitBoards.PopLsb(ref covered);
            var (r, f) = sq;
            fenceRank[f.AsInt()] = r;
        }
    }

    /// <summary>
    /// Marks the current players pawns as either fixed and marked or dynamic
    /// </summary>
    private static void MarkOurPawns(
        in IPosition pos,
        ref BitBoard fixedPawn,
        ref BitBoard marked,
        ref BitBoard dynamicPawns)
    {
        var us = pos.SideToMove;
        var them = ~us;
        var up = us.PawnPushDistance();
        var ourPawns = pos.Pieces(PieceTypes.Pawn, us);
        var theirPawns = pos.Pieces(PieceTypes.Pawn, them);
        var theirPawn = PieceTypes.Pawn.MakePiece(them);

        while (ourPawns)
        {
            var psq = BitBoards.PopLsb(ref ourPawns);
            var rr = psq.RelativeRank(us);
            if (rr < Rank.Rank7
                && (pos.GetPiece(psq + up) == theirPawn || !(fixedPawn & (psq + up)).IsEmpty)
                && (psq.PawnAttack(us) & theirPawns).IsEmpty)
            {
                fixedPawn |= psq;
                marked |= psq;
            }
            else
                dynamicPawns |= psq;
        }
    }

    /// <summary>
    /// Marks the opponent pawn attacks
    /// </summary>
    private static void MarkTheirPawns(ref BitBoard marked, in BitBoard theirPawns, Player us)
    {
        var (southEast, southWest) = us.IsWhite
            ? (Direction.SouthEast, Direction.SouthWest)
            : (Direction.NorthEast, Direction.NorthWest);

        marked |= theirPawns.Shift(southEast) | theirPawns.Shift(southWest);
    }

    /// <summary>
    /// Determines which squares forms a fence. First square is always on file A - and will
    /// perform a depth first verification of its surrounding squares.
    /// </summary>
    /// <param name="sq">The square which is currently being looked at</param>
    /// <param name="processed"></param>
    /// <param name="fence"></param>
    /// <param name="marked"></param>
    /// <param name="us"></param>
    /// <returns>true if the square is in the fence</returns>
    private static bool FormsFence(Square sq, ref BitBoard processed, ref BitBoard fence, in BitBoard marked, Player us)
    {
        processed |= sq;

        // File H is marked as fence if it is reached.
        if (sq.File == File.FileH)
        {
            fence |= sq;
            return true;
        }

        var them = ~us;

        Span<Direction> directions = stackalloc Direction[]
            { us.PawnPushDistance(), Directions.East, them.PawnPushDistance() };

        ref var directionSpace = ref MemoryMarshal.GetReference(directions);
        for (var i = 0; i < directions.Length; ++i)
        {
            var direction = Unsafe.Add(ref directionSpace, i);
            var s = sq + direction;
            if ((marked & s).IsEmpty || !(processed & s).IsEmpty ||
                !FormsFence(s, ref processed, ref fence, in marked, us))
                continue;
            fence |= s;
            return true;
        }

        return false;
    }

    private static Square NextFenceRankSquare(Span<Rank> fenceRank, File f, Player them)
        => new Square(fenceRank[f.AsInt()].AsInt() * 8 + f.AsInt()) + them.PawnPushDistance();

    private static bool FormFence(in BitBoard marked, ref BitBoard fence, ref BitBoard processed, Player us)
    {
        var bb = PawnFileASquares;
        while (bb)
        {
            var startSquare = BitBoards.PopLsb(ref bb);
            if ((marked & startSquare).IsEmpty || !FormsFence(startSquare, ref processed, ref fence, in marked, us))
                continue;
            fence |= startSquare;
            return true;
        }

        return false;
    }

    private static BitBoard ComputeDynamicFencedPawns(
        in IPosition pos,
        Span<Rank> fenceRank,
        in BitBoard theirPawns,
        Player us)
    {
        // reverse order of Down
        var down = us.PawnPushDistance();

        var them = ~us;
        var ourPawn = PieceTypes.Pawn.MakePiece(us);
        var result = BitBoard.Empty;

        ref var fileSpace = ref MemoryMarshal.GetArrayDataReference(File.AllFiles);
        for (var i = 0; i < File.AllFiles.Length; ++i)
        {
            var f = Unsafe.Add(ref fileSpace, i);
            var sq = NextFenceRankSquare(fenceRank, f, them);
            var b = sq.ForwardFile(them) & theirPawns;
            while (b)
            {
                sq = BitBoards.PopLsb(ref b) + down;
                if (pos.GetPiece(sq) == ourPawn)
                    result |= sq;
            }
        }

        return result;
    }

    private static File NextFile(File f)
        => f + 1;

    private static File PreviousFile(File f)
        => f - 1;
}