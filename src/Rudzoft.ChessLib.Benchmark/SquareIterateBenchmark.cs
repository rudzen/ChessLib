using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class SquareIterateBenchmark
{

    [Benchmark(Baseline = true)]
    public int SimpleLoopBaseType()
    {
        var sum = 0;
        for (var i = Squares.a1; i <= Squares.h8; i++)
            sum += i.AsInt();
        return sum;
    }

    [Benchmark]
    public int SimpleLoop()
    {
        var sum = 0;
        for (var i = Square.A1; i <= Square.H8; i++)
            sum += i.AsInt();
        return sum;
    }

    [Benchmark]
    public int ForEachLoop()
    {
        var sum = 0;
        foreach (var sq in Square.All)
            sum += sq.AsInt();
        return sum;
    }

    [Benchmark]
    public int ForEachLoop_AsSpan()
    {
        var sum = 0;
        var span = Square.All.AsSpan();
        foreach (var sq in span)
            sum += sq.AsInt();
        return sum;
    }

    [Benchmark]
    public int ForLoop_AsSpan_Marshal()
    {
        var sum = 0;
        ref var space = ref MemoryMarshal.GetArrayDataReference(Square.All);
        for (var i = 0; i < Square.All.Length; i++)
            sum += Unsafe.Add(ref space, i).AsInt();
        return sum;
    }
    
    [Benchmark]
    public int BitBoard_WhileLoop()
    {
        var sum = 0;
        var all = BitBoards.AllSquares;
        while (all)
        {
            var sq = BitBoards.PopLsb(ref all);
            sum += sq.AsInt();
        }

        return sum;
    }
}