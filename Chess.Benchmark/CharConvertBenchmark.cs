using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Rudz.Chess.Types;

namespace Chess.Benchmark;

[MemoryDiagnoser]
public sealed class CharConvertBenchmark
{

    private static readonly char[] FileChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

    private static readonly string[] FileStrings = FileChars.Select(static x => x.ToString()).ToArray();

    public static IEnumerable<object> FileParams()
    {
        yield return File.FILE_A;
        yield return File.FILE_B;
        yield return File.FILE_C;
        yield return File.FILE_D;
        yield return File.FILE_E;
        yield return File.FILE_F;
        yield return File.FILE_G;
        yield return File.FILE_H;
    }

    [Benchmark]
    [ArgumentsSource(nameof(FileParams))]
    public char CharFromArray(File f)
    {
        return FileChars[f.AsInt()];
    }

    [Benchmark]
    [ArgumentsSource(nameof(FileParams))]
    public char CharFromConvert(File f)
    {
        return (char)('a' + f.AsInt());
    }

    [Benchmark]
    [ArgumentsSource(nameof(FileParams))]
    public string StringFromArray(File f)
    {
        return FileStrings[f.AsInt()];
    }

    [Benchmark]
    [ArgumentsSource(nameof(FileParams))]
    public string StringFromCreate(File f)
    {
        return string.Create(1, f.Value, static (span, v) => span[0] = (char)('a' + v));
    }

    [Benchmark]
    [ArgumentsSource(nameof(FileParams))]
    public string StringFromCharConvert(File f)
    {
        return new string((char)('a' + f.AsInt()), 1);
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(FileParams))]
    public string StringFromChar(File f)
    {
        return ((char)('a' + f.AsInt())).ToString();
    }
}