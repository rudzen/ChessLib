/*
/// xorshift64star Pseudo-Random Number Generator
/// This class is based on original code written and dedicated
/// to the public domain by Sebastiano Vigna (2014).
/// It has the following characteristics:
///
///  -  Outputs 64-bit numbers
///  -  Passes Dieharder and SmallCrush test batteries
///  -  Does not require warm-up, no zeroland to escape
///  -  Internal state is a single 64-bit integer
///  -  Period is 2^64 - 1
///  -  Speed: 1.60 ns/call (Core i7 @3.40GHz) (not so for C#)
///
/// For further analysis see
///   <http://vigna.di.unimi.it/ftp/papers/xorshift.pdf>
///
/// C# single type (ulong) adaptation by Rudy Alex Kohn (2017).
/// C# Performance (approx) against

/// { ULongRnd.NextBytes(Buffer);
/// return BitConverter.ToUInt64(Buffer, 0); }

      Method |      Mean |     Error |    StdDev |
------------ |----------:|----------:|----------:|
    ULongRnd | 78.205 ns | 0.5307 ns | 0.4704 ns |
 RKissRandom |  3.911 ns | 0.0876 ns | 0.0777 ns |

 */

using System.Collections.Generic;

namespace Rudzoft.ChessLib.Hash;

public interface IRKiss
{
    IEnumerable<ulong> Get(int count);

    ulong Rand();

    /// Special generator used to fast init magic numbers.
    /// Output values only have 1/8th of their bits set on average.
    ulong Sparse();
}