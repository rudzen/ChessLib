/*
xorshift64star Pseudo-Random Number Generator
This class is based on original code written and dedicated
to the public domain by Sebastiano Vigna (2014).
It has the following characteristics:
-  Outputs 64-bit numbers
-  Passes Dieharder and SmallCrush test batteries
-  Does not require warm-up, no zeroland to escape
-  Internal state is a single 64-bit integer
-  Period is 2^64 - 1
-  Speed: 1.60 ns/call (Core i7 @3.40GHz) (not so for C#)

For further analysis see
<http://vigna.di.unimi.it/ftp/papers/xorshift.pdf>

 C# single type (ulong) adaptation by Rudy Alex Kohn (2017).
 C# Performance (approx) against

      Method |      Mean |     Error |    StdDev |
------------ |----------:|----------:|----------:|
    ULongRnd | 78.205 ns | 0.5307 ns | 0.4704 ns |
 RKissRandom |  3.911 ns | 0.0876 ns | 0.0777 ns |

 */

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Hash;

/// <summary>
/// ------------ |----------:|----------:|----------:|
///       Method |      Mean |     Error |    StdDev |
/// ------------ |----------:|----------:|----------:|
///     ULongRnd | 77.335 ns | 1.0416 ns | 0.9743 ns |
///  RKissRandom |  4.369 ns | 0.0665 ns | 0.0622 ns |
/// ------------ |----------:|----------:|----------:|
/// </summary>
public sealed class RKiss : IRKiss
{
    /// <summary>
    /// The default value for random seed for improved consistency
    /// </summary>
    private const ulong DefaultRandomSeed = 1070372;

    private ulong _s = DefaultRandomSeed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<ulong> Get(int count)
        => Enumerable.Repeat(Rand64(), count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Rand() => Rand64();

    /// Special generator used to fast init magic numbers.
    /// Output values only have 1/8th of their bits set on average.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Sparse()
    {
        var h = Rand64();
        h &= Rand64();
        return h & Rand64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong Rand64()
    {
        _s ^= _s >> 12;
        _s ^= _s << 25;
        _s ^= _s >> 27;
        return _s * 2685821657736338717L;
    }
}