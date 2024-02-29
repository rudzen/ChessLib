namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class NumberCompactingBenchmark
{
    private const int numMask = 0b0000_0000_0000_0000_0000_0000_0000_1111;

    private const string strNum = "12345678";
    private const string strNum2 = "45678945";

    [Benchmark(Baseline = true)]
    public (string, string) Base()
    {
        var first = ulong.Parse(strNum);
        var second = ulong.Parse(strNum2);
        var compacted = (first << 32) | second;

        var reverseNum = ((int)(compacted >> 32)).ToString();
        var reverseNum2 = ((int)(compacted & 0xFFFFFFFF)).ToString();

        return (reverseNum, reverseNum2);
    }

    [Benchmark]
    public (string, string) InPlace()
    {
        var numSpan = strNum.AsSpan();

        var result = Compact(numSpan, 0);
        result |= Compact(strNum2.AsSpan(), 32);

        Span<char> resultSpan = stackalloc char[8];
        for (var i = 0; i < 8; i++)
        {
            var shift = i * 4;
            var b = (byte)((result >> shift) & numMask);
            resultSpan[i] = (char)(b + 48);
        }

        Span<char> resultSpan2 = stackalloc char[8];
        for (var i = 0; i < 8; i++)
        {
            var shift = (i * 4) + 32;
            var b = (byte)((result >> shift) & numMask);
            resultSpan2[i] = (char)(b + 48);
        }

        return (resultSpan.ToString(), resultSpan2.ToString());
    }

    private static ulong Compact(ReadOnlySpan<char> numSpan, int delta)
    {
        var result = ulong.MinValue;

        for (var index = 0; index < numSpan.Length; index++)
        {
            var shift = (index * 4) + delta;
            var c = numSpan[index];
            var b = (byte)c;
            result |= (ulong)(b & numMask) << shift;
        }

        return result;
    }

}