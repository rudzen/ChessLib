/*
 * Adapted to c# from StockFish.
 */

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Rudzoft.ChessLib.Hash;

namespace Rudzoft.ChessLib.Types;

public sealed class Magics
{
    public BitBoard Mask { get; init; } = BitBoard.Empty;

    public BitBoard Magic { get; set; } = BitBoard.Empty;

    public BitBoard[] Attacks { get; set; } = new BitBoard[64];

    public uint Shift { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Index(in BitBoard occupied)
    {
        return Bmi2.X64.IsSupported switch
        {
            true  => (uint)Bmi2.X64.ParallelBitExtract(occupied.Value, Mask.Value),
            var _ => (uint)((occupied & Mask).Value * Magic >> (int)Shift).Value
        };
    }
}

public static class MagicBB
{
    private static readonly Magics[] RookMagics = new Magics[64];
    private static readonly Magics[] BishopMagics = new Magics[64];

    // Optimal PRNG seeds to pick the correct magics in the shortest time
    private static readonly ulong[] Seeds =
    [
        728UL, 10316UL, 55013UL, 32803UL, 12281UL, 15100UL, 16645UL, 255UL
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

        var rookTable = new BitBoard[0x19000]; // To store rook attacks
        var bishopTable = new BitBoard[0x1480]; // To store bishop attacks

        var epochs = new int[0x1000];
        var occupancy = new BitBoard[0x1000];
        var reference = new BitBoard[0x1000];
        var rk = new RKiss();

        ref var occupancyRef = ref MemoryMarshal.GetArrayDataReference(occupancy);
        ref var referenceRef = ref MemoryMarshal.GetArrayDataReference(reference);
        ref var epochsRef = ref MemoryMarshal.GetArrayDataReference(epochs);

        InitMagics(
            rookTable,
            RookMagics,
            rookDeltas,
            ref occupancyRef,
            ref referenceRef,
            ref epochsRef,
            rk
        );

        InitMagics(
            bishopTable,
            BishopMagics,
            bishopDeltas,
            ref occupancyRef,
            ref referenceRef,
            ref epochsRef,
            rk
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
        return RookMagics[s.AsInt()].Attacks[RookMagics[s.AsInt()].Index(in occ)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BishopAttacks(this Square s, in BitBoard occ)
    {
        return BishopMagics[s.AsInt()].Attacks[BishopMagics[s.AsInt()].Index(in occ)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard QueenAttacks(this Square s, in BitBoard occ)
    {
        return RookAttacks(s, in occ) | BishopAttacks(s, in occ);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard SafeDistance(Square sq, Direction step)
    {
        var to = sq + step;
        return to.IsOk && sq.Distance(to) <= 2 ? to.AsBb() : BitBoard.Empty;
    }

    // InitMagics() computes all rook and bishop attacks at startup. Magic
    // bitboards are used to look up attacks of sliding pieces. As a reference see
    // chessprogramming.wikispaces.com/Magic+Bitboards. In particular, here we
    // use the so called "fancy" approach.

    private static void InitMagics(
        BitBoard[] table,
        Magics[] magics,
        ReadOnlySpan<Direction> deltas,
        ref BitBoard occupancyRef,
        ref BitBoard referenceRef,
        ref int epochRef,
        IRKiss rk
    )
    {
        var cnt = 0;
        var size = 0;
        var rankMask = Rank.Rank1.RankBB() | Rank.Rank8.RankBB();
        var fileMask = File.FileA.FileBB() | File.FileH.FileBB();
        ref var magicsRef = ref MemoryMarshal.GetArrayDataReference(magics);

        for (var s = 0; s < 64; s++)
        {
            var sq = new Square(s);
            var rank = sq.Rank;
            var edges = (rankMask & ~rank) | (fileMask & ~sq.File);

            // Given a square 's', the mask is the bitboard of sliding attacks from
            // 's' computed on an empty board. The index must be big enough to contain
            // all the attacks for each possible subset of the mask and so is 2 power
            // the number of 1s of the mask. Hence we deduce the size of the shift to
            // apply to the 64 or 32 bits word to get the index.
            ref var m = ref Unsafe.Add(ref magicsRef, s);
            m = new()
            {
                Mask = SlidingAttack(deltas, sq, 0) & ~edges,
            };
            m.Shift = (uint)(64 - m.Mask.Count);

            // Set the offset for the attacks table of the square. We have individual
            // table sizes for each square with "Fancy Magic Bitboards".
            m.Attacks = sq == Square.A1 ? table : Unsafe.Add(ref magicsRef, s - 1).Attacks[size..];


            // Use Carry-Rippler trick to enumerate all subsets of masks[s] and
            // store the corresponding sliding attack bitboard in reference[].
            var b = BitBoard.Empty;
            size = 0;
            do
            {
                Unsafe.Add(ref occupancyRef, size) = b;
                ref var reference = ref Unsafe.Add(ref referenceRef, size);
                reference = SlidingAttack(deltas, sq, in b);

                if (Bmi2.X64.IsSupported)
                    m.Attacks[Bmi2.X64.ParallelBitExtract(b.Value, m.Mask.Value)] = reference;

                size++;
                b = (b.Value - m.Mask.Value) & m.Mask;
            } while (b.IsNotEmpty);

            if (Bmi2.X64.IsSupported)
                continue;

            rk.Seed = Seeds[rank.AsInt()];

            // Find a magic for square 's' picking up an (almost) random number
            // until we find the one that passes the verification test.
            for (var i = 0; i < size;)
            {
                for (m.Magic = BitBoard.Empty; (BitBoard.Create(m.Magic.Value * m.Mask.Value) >> 56).Value < 6;)
                    m.Magic = rk.Sparse();

                // A good magic must map every possible occupancy to an index that
                // looks up the correct sliding attack in the attacks[s] database.
                // Note that we build up the database for square 's' as a side
                // effect of verifying the magic. Keep track of the attempt count
                // and save it in epoch[], little speed-up trick to avoid resetting
                // m.attacks[] after every failed attempt.
                for (++cnt, i = 0; i < size; ++i)
                {
                    var idx = m.Index(Unsafe.Add(ref occupancyRef, i));

                    ref var epoch = ref Unsafe.Add(ref epochRef, idx);

                    if (epoch < cnt)
                    {
                        epoch = cnt;
                        m.Attacks[idx] = Unsafe.Add(ref referenceRef, i);
                    }
                    else if (m.Attacks[idx] != Unsafe.Add(ref referenceRef, i))
                        break;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard SlidingAttack(ReadOnlySpan<Direction> deltas, Square sq, in BitBoard occupied)
    {
        var attack = BitBoard.Empty;

        foreach (var delta in deltas)
        {
            var s = sq;
            while (SafeDistance(s, delta) && !occupied.Contains(s))
                attack |= (s += delta).AsBb();
        }

        return attack;
    }
}