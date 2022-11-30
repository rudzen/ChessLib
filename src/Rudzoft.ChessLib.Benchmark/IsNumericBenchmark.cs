using Rudzoft.ChessLib.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rudzoft.ChessLib.Benchmark;

public class IsNumericBenchmark
{
    [Params(100, 500)]
    public int N;

    private string _s;

    [GlobalSetup]
    public void Setup()
    {
        var r = new Random(0);
        _s = RandomString(r, N);
    }

    [Benchmark(Baseline = true)]
    public void MathExtInBetween()
    {
        foreach (var c in _s.AsSpan())
        {
            var b = MathExtensions.InBetween(c, '0', '9');
        }
    }

    [Benchmark]
    public void IsAsciiDigit()
    {
        foreach (var c in _s.AsSpan())
        {
            var b = char.IsAsciiDigit(c);
        }
    }

    private static string RandomString(Random random, int length)
    {
        const string chars = "ABCDE0123456789ABCDE";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
