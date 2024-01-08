using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class StringBenchmarks
{
    [Benchmark]
    [ArgumentsSource(nameof(Squares))]
    public string ArrayLookup(Square sq)
    {
        return sq.ToString();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Squares))]
    public string NameOf(Square sq)
    {
        return GetNameOf(sq.Value);
    }
    
    public static IEnumerable<object> Squares()
    {
        var sqs = BitBoards.MakeBitboard(Square.A1, Square.H8);
        // var sqs = BitBoards.AllSquares;
        while (sqs)
            yield return BitBoards.PopLsb(ref sqs);
    }

    private static string GetNameOf(Squares sq)
    {
        return sq switch
        {
            Types.Squares.a1   => nameof(Types.Squares.a1),
            Types.Squares.b1   => nameof(Types.Squares.b1),
            Types.Squares.c1   => nameof(Types.Squares.c1),
            Types.Squares.d1   => nameof(Types.Squares.d1),
            Types.Squares.e1   => nameof(Types.Squares.e1),
            Types.Squares.f1   => nameof(Types.Squares.f1),
            Types.Squares.g1   => nameof(Types.Squares.g1),
            Types.Squares.h1   => nameof(Types.Squares.h1),
            Types.Squares.a2   => nameof(Types.Squares.a2),
            Types.Squares.b2   => nameof(Types.Squares.b2),
            Types.Squares.c2   => nameof(Types.Squares.c2),
            Types.Squares.d2   => nameof(Types.Squares.d2),
            Types.Squares.e2   => nameof(Types.Squares.e2),
            Types.Squares.f2   => nameof(Types.Squares.f2),
            Types.Squares.g2   => nameof(Types.Squares.g2),
            Types.Squares.h2   => nameof(Types.Squares.h2),
            Types.Squares.a3   => nameof(Types.Squares.a3),
            Types.Squares.b3   => nameof(Types.Squares.b3),
            Types.Squares.c3   => nameof(Types.Squares.c3),
            Types.Squares.d3   => nameof(Types.Squares.d3),
            Types.Squares.e3   => nameof(Types.Squares.e3),
            Types.Squares.f3   => nameof(Types.Squares.f3),
            Types.Squares.g3   => nameof(Types.Squares.g3),
            Types.Squares.h3   => nameof(Types.Squares.h3),
            Types.Squares.a4   => nameof(Types.Squares.a4),
            Types.Squares.b4   => nameof(Types.Squares.b4),
            Types.Squares.c4   => nameof(Types.Squares.c4),
            Types.Squares.d4   => nameof(Types.Squares.d4),
            Types.Squares.e4   => nameof(Types.Squares.e4),
            Types.Squares.f4   => nameof(Types.Squares.f4),
            Types.Squares.g4   => nameof(Types.Squares.g4),
            Types.Squares.h4   => nameof(Types.Squares.h4),
            Types.Squares.a5   => nameof(Types.Squares.a5),
            Types.Squares.b5   => nameof(Types.Squares.b5),
            Types.Squares.c5   => nameof(Types.Squares.c5),
            Types.Squares.d5   => nameof(Types.Squares.d5),
            Types.Squares.e5   => nameof(Types.Squares.e5),
            Types.Squares.f5   => nameof(Types.Squares.f5),
            Types.Squares.g5   => nameof(Types.Squares.g5),
            Types.Squares.h5   => nameof(Types.Squares.h5),
            Types.Squares.a6   => nameof(Types.Squares.a6),
            Types.Squares.b6   => nameof(Types.Squares.b6),
            Types.Squares.c6   => nameof(Types.Squares.c6),
            Types.Squares.d6   => nameof(Types.Squares.d6),
            Types.Squares.e6   => nameof(Types.Squares.e6),
            Types.Squares.f6   => nameof(Types.Squares.f6),
            Types.Squares.g6   => nameof(Types.Squares.g6),
            Types.Squares.h6   => nameof(Types.Squares.h6),
            Types.Squares.a7   => nameof(Types.Squares.a7),
            Types.Squares.b7   => nameof(Types.Squares.b7),
            Types.Squares.c7   => nameof(Types.Squares.c7),
            Types.Squares.d7   => nameof(Types.Squares.d7),
            Types.Squares.e7   => nameof(Types.Squares.e7),
            Types.Squares.f7   => nameof(Types.Squares.f7),
            Types.Squares.g7   => nameof(Types.Squares.g7),
            Types.Squares.h7   => nameof(Types.Squares.h7),
            Types.Squares.a8   => nameof(Types.Squares.a8),
            Types.Squares.b8   => nameof(Types.Squares.b8),
            Types.Squares.c8   => nameof(Types.Squares.c8),
            Types.Squares.d8   => nameof(Types.Squares.d8),
            Types.Squares.e8   => nameof(Types.Squares.e8),
            Types.Squares.f8   => nameof(Types.Squares.f8),
            Types.Squares.g8   => nameof(Types.Squares.g8),
            Types.Squares.h8   => nameof(Types.Squares.h8),
            Types.Squares.none => nameof(Types.Squares.none),
            _                  => throw new ArgumentOutOfRangeException()
        };
    }
}