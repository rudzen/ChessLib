using System.Linq;
using BenchmarkDotNet.Attributes;
using Rudz.Chess.Types;

namespace Chess.Benchmark;

[MemoryDiagnoser]
public class CharConvertBenchmark
{
    
    private static readonly char[] FileChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

    private static readonly string[] FileStrings = FileChars.Select(static x => x.ToString()).ToArray();

    private static readonly File[] _files = File.AllFiles;

    [Benchmark]
    public void CharFromArray()
    {

        foreach (var file in _files)
        {
            char c = FileChars[file.AsInt()];
        }
    }

    [Benchmark]
    public void CharFromConvert()
    {
        foreach (var file in _files)
        {
            char c = (char)('a' + file.AsInt());
        }
    }

    [Benchmark]
    public void StringFromArray()
    {
        foreach (var file in _files)
        {
            string s = FileStrings[file.AsInt()];
        }
    }

    [Benchmark]
    public void StringFromCreate()
    {
        foreach (var file in _files)
        {
            string s = string.Create(1, file.Value, static (span, v) => span[0] = (char)('a' + v));
        }
    }

    [Benchmark]
    public void StringFromCharConvert()
    {
        foreach (var file in _files)
        {
            string s = new string((char)('a' + file.AsInt()), 1);
        }
    }
    
    [Benchmark]
    public void StringFromChar()
    {
        foreach (var file in _files)
        {
            string s = ((char)('a' + file.AsInt())).ToString();
        }
    }

}