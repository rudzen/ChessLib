using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class KeyBenchmarks
{
    private sealed record Affe(string Cvr, string Se, DateOnly Ksl);

    private sealed record Affe2(Guid Key, DateOnly Ksl);

    private const string CvrN = "12345678";
    private const string SeN = "87654321";

    private DateOnly _ksl;
    
    [GlobalSetup]
    public void Setup()
    {
        _ksl = DateOnly.FromDateTime(DateTime.Now);
        var doSize = Unsafe.SizeOf<DateOnly>();
        var dtSize = Unsafe.SizeOf<DateTime>();

        var a = 0;
    }
    
    [Benchmark]
    public int StringToGuid()
    {
        return A(CvrN, SeN, in _ksl).GetHashCode();
    }

    [Benchmark]
    public int StringToGuid2()
    {
        return A2(CvrN, SeN, in _ksl).GetHashCode();
    }
    
    [Benchmark(Baseline = true)]
    public int BaseRecord()
    {
        return B(CvrN, SeN, in _ksl).GetHashCode();
    }
    
    [SkipLocalsInit]
    private static Affe2 A(string s1, string s2, in DateOnly dateOnly)
    {
        var s1Span = s1.AsSpan();
        var s2Span = s2.AsSpan();
        
        var s1Bytes = MemoryMarshal.AsBytes(s1Span);
        var s2Bytes = MemoryMarshal.AsBytes(s2Span);

        Span<byte> finalBytes = stackalloc byte[16];
        
        s1Bytes.CopyTo(finalBytes);
        finalBytes[8] = s2Bytes[0];
        finalBytes[9] = s2Bytes[1];
        finalBytes[10] = s2Bytes[2];
        finalBytes[11] = s2Bytes[3];
        finalBytes[12] = s2Bytes[4];
        finalBytes[13] = s2Bytes[5];
        finalBytes[14] = s2Bytes[6];
        finalBytes[15] = s2Bytes[7];

        return new(new(finalBytes), dateOnly);
        
        //var h = MemoryMarshal.TryRead(finalBytes, out ulong v);
    }

    [SkipLocalsInit]
    private static Affe2 A2(string s1, string s2, in DateOnly dateOnly)
    {
        var s1Span = s1.AsSpan();
        var s2Span = s2.AsSpan();
        
        var s1Bytes = MemoryMarshal.AsBytes(s1Span);
        var s2Bytes = MemoryMarshal.AsBytes(s2Span);

        Span<byte> finalBytes = stackalloc byte[s1Bytes.Length + s2Bytes.Length];
        
        if (Sse2.IsSupported)
        {
            var s1Vector = MemoryMarshal.Cast<byte, Vector128<byte>>(s1Bytes)[0];
            var s2Vector = MemoryMarshal.Cast<byte, Vector128<byte>>(s2Bytes)[0];

            MemoryMarshal.Cast<byte, Vector128<byte>>(finalBytes)[0] = s1Vector;
            MemoryMarshal.Cast<byte, Vector128<byte>>(finalBytes[16..])[0] = s2Vector;
        }
        else
        {
            // Fall back to non-SIMD code if not supported.
            s1Bytes.CopyTo(finalBytes);
            s2Bytes.CopyTo(finalBytes[16..]);
        }

        return new(new Guid(finalBytes), dateOnly);
        
        //var h = MemoryMarshal.TryRead(finalBytes, out ulong v);
    }

    // private static Affe2 AVector(string cvr, string se, in DateOnly now)
    // {
    //     var cvrSpan = cvr.AsSpan();
    //     var seSpan = se.AsSpan();
    //     
    //     var cvrBytes = MemoryMarshal.AsBytes(cvrSpan);
    //     var seBytes = MemoryMarshal.AsBytes(seSpan);
    //
    //     Span<byte> finalBytes = stackalloc byte[16];
    //     
    //     cvrBytes.CopyTo(finalBytes);
    //     finalBytes[8] = seBytes[0];
    //     finalBytes[9] = seBytes[1];
    //     finalBytes[10] = seBytes[2];
    //     finalBytes[11] = seBytes[3];
    //     finalBytes[12] = seBytes[4];
    //     finalBytes[13] = seBytes[5];
    //     finalBytes[14] = seBytes[6];
    //     finalBytes[15] = seBytes[7];
    //
    //     
    //             
    //
    // }

    private static Affe B(string cvr, string se, in DateOnly now)
    {
        return new(cvr, se, now);
    }
}