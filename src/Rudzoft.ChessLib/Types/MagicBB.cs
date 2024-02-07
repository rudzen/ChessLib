using System.Numerics;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Hash;

namespace Rudzoft.ChessLib.Types;

public static class MagicBB
{
    private static readonly BitBoard[]   RMasks   = new BitBoard[64];
    private static readonly BitBoard[]   RMagics  = new BitBoard[64];
    private static readonly BitBoard[][] RAttacks = new BitBoard[64][];
    private static readonly int[]        RShifts  = new int[64];

    private static readonly BitBoard[]   BMasks   = new BitBoard[64];
    private static readonly BitBoard[]   BMagics  = new BitBoard[64];
    private static readonly BitBoard[][] BAttacks = new BitBoard[64][];
    private static readonly int[]        BShifts  = new int[64];

    private static readonly int[][] MagicBoosters =
    [
        [3191, 2184, 1310, 3618, 2091, 1308, 2452, 3996],
        [1059, 3608, 605, 3234, 3326, 38, 2029, 3043]
    ];

    private static readonly  byte[]  BitCount8Bit   = new byte[256];

    static MagicBB()
    {
        for (ulong b = 0; b < 256; b++)
            BitCount8Bit[b] = (byte)popcount_1s_Max15(b);

        Direction[] RDeltas = [Direction.North, Direction.East, Direction.South, Direction.West];
        Direction[] BDeltas = [Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest];

        var occupancy = new BitBoard[4096];
        var reference = new BitBoard[4096];
        var rk = new RKiss();

        init_magics(PieceTypes.Rook, RAttacks, RMagics, RMasks, RShifts, RDeltas, magic_index, occupancy, reference, rk);
        init_magics(PieceTypes.Bishop, BAttacks, BMagics, BMasks, BShifts, BDeltas, magic_index, occupancy, reference, rk);

    }

#if AGGR_INLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static int popcount_1s_Max15(in ulong b)
    {
#if X64
        b -= (b >> 1) & 0x5555555555555555UL;
        b = ((b >> 2) & 0x3333333333333333UL) + (b & 0x3333333333333333UL);
        return (int)((b * 0x1111111111111111UL) >> 60);
#else
        uint w = (uint)(b >> 32), v = (uint)(b);
        v -= (v >> 1) & 0x55555555; // 0-2 in 2 bits
        w -= (w >> 1) & 0x55555555;
        v = ((v >> 2) & 0x33333333) + (v & 0x33333333); // 0-4 in 4 bits
        w = ((w >> 2) & 0x33333333) + (w & 0x33333333);
        v += w; // 0-8 in 4 bits
        v *= 0x11111111;
        return (int)(v >> 28);
#endif
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

    private static uint magic_index(PieceTypes pt, Square s, BitBoard occ)
    {
        if (pt == PieceTypes.Rook)
            return (uint)(((occ & RMasks[s.AsInt()]).Value * RMagics[s.AsInt()].Value) >> RShifts[s.AsInt()]);

        return (uint)(((occ & BMasks[s.AsInt()]).Value * BMagics[s.AsInt()].Value) >> BShifts[s.AsInt()]);
    }


    // init_magics() computes all rook and bishop attacks at startup. Magic
    // bitboards are used to look up attacks of sliding pieces. As a reference see
    // chessprogramming.wikispaces.com/Magic+Bitboards. In particular, here we
    // use the so called "fancy" approach.

    private static void init_magics(
        PieceTypes pt,
        BitBoard[][] attacks,
        BitBoard[] magics,
        BitBoard[] masks,
        int[] shifts,
        Direction[] deltas,
        Func<PieceTypes, Square, BitBoard, uint> index,
        BitBoard[] occupancy,
        BitBoard[] reference,
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
            masks[s.AsInt()] = sliding_attack(deltas, s, 0) & ~edges;
            shifts[s.AsInt()] = 64 - popcount_1s_Max15(masks[s.AsInt()].Value);

            // Use Carry-Rippler trick to enumerate all subsets of masks[s] and
            // store the corresponding sliding attack bitboard in reference[].
            var b = BitBoard.Empty;
            var size = 0;
            do
            {
                occupancy[size] = b;
                reference[size++] = sliding_attack(deltas, sq, b);
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
                magics[s.AsInt()] = pick_random(masks[s.AsInt()], rk, booster);
                Array.Clear(attacks[s.AsInt()], 0, size);

                // A good magic must map every possible occupancy to an index that
                // looks up the correct sliding attack in the attacks[s] database.
                // Note that we build up the database for square 's' as a side
                // effect of verifying the magic.
                for (i = 0; i < size; i++)
                {
                    var idx = index(pt, s, occupancy[i]);

                    var attack = attacks[s.AsInt()][idx];

                    if (attack.IsNotEmpty && attack != reference[i])
                        break;

                    attacks[s.AsInt()][idx] = reference[i];
                }
            } while (i != size);
        }
    }

    private static BitBoard sliding_attack(IReadOnlyList<Direction> deltas, Square sq, BitBoard occupied)
    {
        var attack = BitBoard.Empty;

        for (var i = 0; i < 4; i++)
        {
            for (var s = sq + deltas[i]; s.IsOk && s.Distance(s - deltas[i]) == 1; s += deltas[i])
            {
                attack |= s;

                if (occupied.Contains(s))
                    break;
            }
        }

        return attack;
    }

    private static BitBoard pick_random(BitBoard mask, IRKiss rk, int booster)
    {
        // Values s1 and s2 are used to rotate the candidate magic of a
        // quantity known to be the optimal to quickly find the magics.
        int s1 = booster        & 63;
        var s2 = (booster >> 6) & 63;

        while (true)
        {
            var magic = rk.Rand();
            magic =  (magic >> s1) | (magic << (64 - s1));
            magic &= rk.Rand();
            magic =  (magic >> s2) | (magic << (64 - s2));
            magic &= rk.Rand();

            if (BitCount8Bit[((mask.Value * magic) >> 56)] >= 6)
                return magic;
        }
    }

}