/*
 * Adapted from PortFish.
 * Minor modernizations by Rudy Alex Kohn
 */

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Hash;

namespace Rudzoft.ChessLib.Types;

public static class MagicBB
{
    private static readonly BitBoard[] RMasks = new BitBoard[64];
    private static readonly BitBoard[] RMagics = new BitBoard[64];
    private static readonly BitBoard[][] RAttacks = new BitBoard[64][];
    private static readonly int[] RShifts = new int[64];

    private static readonly BitBoard[] BMasks = new BitBoard[64];
    private static readonly BitBoard[] BMagics = new BitBoard[64];
    private static readonly BitBoard[][] BAttacks = new BitBoard[64][];
    private static readonly int[] BShifts = new int[64];

    private static readonly int[][] MagicBoosters =
    [
        [3191, 2184, 1310, 3618, 2091, 1308, 2452, 3996],
        [1059, 3608, 605, 3234, 3326, 38, 2029, 3043]
    ];

    private static readonly byte[] BitCount8Bit = new byte[256];

    [SkipLocalsInit]
    static MagicBB()
    {
        var start = Stopwatch.GetTimestamp();

        for (ulong b = 0; b <= byte.MaxValue; b++)
            BitCount8Bit[b] = (byte)PopCountMax15(b);

        Span<Direction> rookDeltas = stackalloc Direction[]
            { Direction.North, Direction.East, Direction.South, Direction.West };

        Span<Direction> bishopDeltas = stackalloc Direction[]
            { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };

        var occupancy = new BitBoard[4096];
        var reference = new BitBoard[4096];
        var rk = new RKiss();

        ref var occupancyRef = ref MemoryMarshal.GetArrayDataReference(occupancy);
        ref var referenceRef = ref MemoryMarshal.GetArrayDataReference(reference);

        InitMagics(
            pt: PieceTypes.Rook,
            attacks: RAttacks,
            magics: RMagics,
            masks: RMasks,
            shifts: RShifts,
            deltas: rookDeltas,
            occupancyRef: ref occupancyRef,
            referenceRef: ref referenceRef,
            rk: rk
        );

        InitMagics(
            pt: PieceTypes.Bishop,
            attacks: BAttacks,
            magics: BMagics,
            masks: BMasks,
            shifts: BShifts,
            deltas: bishopDeltas,
            occupancyRef: ref occupancyRef,
            referenceRef: ref referenceRef,
            rk: rk
        );

        var end = Stopwatch.GetElapsedTime(start);
        Console.WriteLine($"MagicBB init took: {end}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PopCountMax15(ulong b)
    {
        b -= (b >> 1) & 0x5555555555555555UL;
        b = ((b >> 2) & 0x3333333333333333UL) + (b & 0x3333333333333333UL);
        return (int)((b * 0x1111111111111111UL) >> 60);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard RookAttacks(this Square s, in BitBoard occ)
    {
        return RAttacks[s.AsInt()][((occ & RMasks[s.AsInt()]).Value * RMagics[s.AsInt()].Value) >> RShifts[s.AsInt()]];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BishopAttacks(this Square s, in BitBoard occ)
    {
        return BAttacks[s.AsInt()][((occ & BMasks[s.AsInt()]).Value * BMagics[s.AsInt()].Value) >> BShifts[s.AsInt()]];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard QueenAttacks(this Square s, in BitBoard occ)
    {
        return RookAttacks(s, in occ) | BishopAttacks(s, in occ);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MagicIndex(PieceTypes pt, Square s, in BitBoard occ)
    {
        return pt == PieceTypes.Rook
            ? (uint)(((occ & RMasks[s.AsInt()]).Value * RMagics[s.AsInt()].Value) >> RShifts[s.AsInt()])
            : (uint)(((occ & BMasks[s.AsInt()]).Value * BMagics[s.AsInt()].Value) >> BShifts[s.AsInt()]);
    }

    // init_magics() computes all rook and bishop attacks at startup. Magic
    // bitboards are used to look up attacks of sliding pieces. As a reference see
    // chessprogramming.wikispaces.com/Magic+Bitboards. In particular, here we
    // use the so called "fancy" approach.

    private static void InitMagics(
        PieceTypes pt,
        BitBoard[][] attacks,
        BitBoard[] magics,
        BitBoard[] masks,
        int[] shifts,
        ReadOnlySpan<Direction> deltas,
        ref BitBoard occupancyRef,
        ref BitBoard referenceRef,
        IRKiss rk)
    {
        var rankMask = Rank.Rank1.RankBB() | Rank.Rank8.RankBB();
        var fileMask = File.FileA.FileBB() | File.FileH.FileBB();

        for (var s = Squares.a1; s <= Squares.h8; s++)
        {
            var sq = new Square(s);
            // Board edges are not considered in the relevant occupancies
            var edges = (rankMask & ~sq.Rank) | (fileMask & ~sq.File);

            // Given a square 's', the mask is the bitboard of sliding attacks from
            // 's' computed on an empty board. The index must be big enough to contain
            // all the attacks for each possible subset of the mask and so is 2 power
            // the number of 1s of the mask. Hence we deduce the size of the shift to
            // apply to the 64 or 32 bits word to get the index.
            masks[s.AsInt()] = SlidingAttack(deltas, s, 0) & ~edges;
            shifts[s.AsInt()] = 64 - PopCountMax15(masks[s.AsInt()].Value);

            // Use Carry-Rippler trick to enumerate all subsets of masks[s] and
            // store the corresponding sliding attack bitboard in reference[].
            var b = BitBoard.Empty;
            var size = 0;
            do
            {
                Unsafe.Add(ref occupancyRef, size) = b;
                Unsafe.Add(ref referenceRef, size) = SlidingAttack(deltas, sq, b);
                size++;
                b = (b.Value - masks[s.AsInt()].Value) & masks[s.AsInt()];
            } while (b.IsNotEmpty);

            // Set the offset for the table of the next square. We have individual
            // table sizes for each square with "Fancy Magic Bitboards".
            var booster = MagicBoosters[1][sq.Rank.AsInt()];

            attacks[s.AsInt()] = new BitBoard[size];

            // Find a magic for square 's' picking up an (almost) random number
            // until we find the one that passes the verification test.
            int i;
            do
            {
                magics[s.AsInt()] = PickRandom(masks[s.AsInt()], rk, booster);
                Array.Clear(attacks[s.AsInt()], 0, size);

                // A good magic must map every possible occupancy to an index that
                // looks up the correct sliding attack in the attacks[s] database.
                // Note that we build up the database for square 's' as a side
                // effect of verifying the magic.
                for (i = 0; i < size; i++)
                {
                    var idx = MagicIndex(pt, s, in Unsafe.Add(ref occupancyRef, i));
                    var attack = attacks[s.AsInt()][idx];
                    ref var reference = ref Unsafe.Add(ref referenceRef, i);

                    if (attack.IsNotEmpty && attack != reference)
                        break;

                    attacks[s.AsInt()][idx] = reference;
                }
            } while (i != size);
        }
    }

    private static BitBoard SlidingAttack(ReadOnlySpan<Direction> deltas, Square sq, BitBoard occupied)
    {
        var attack = BitBoard.Empty;

        foreach (var delta in deltas)
        {
            for (var s = sq + delta; s.IsOk && s.Distance(s - delta) == 1; s += delta)
            {
                attack |= s;

                if (occupied.Contains(s))
                    break;
            }
        }

        return attack;
    }

    private static BitBoard PickRandom(BitBoard mask, IRKiss rk, int booster)
    {
        // Values s1 and s2 are used to rotate the candidate magic of a
        // quantity known to be the optimal to quickly find the magics.
        var s1 = booster & 63;
        var s2 = (booster >> 6) & 63;

        while (true)
        {
            ulong magic = rk.Rand();
            magic = (magic >> s1) | (magic << (64 - s1));
            magic &= rk.Rand();
            magic = (magic >> s2) | (magic << (64 - s2));
            magic &= rk.Rand();

            if (BitCount8Bit[((mask.Value * magic) >> 56)] >= 6)
                return magic;
        }
    }
}