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
        switch (sq)
        {
            case Types.Squares.a1:
                return nameof(Types.Squares.a1);
            case Types.Squares.b1:
                return nameof(Types.Squares.b1);
            case Types.Squares.c1:
                return nameof(Types.Squares.c1);
            case Types.Squares.d1:
                return nameof(Types.Squares.d1);
            case Types.Squares.e1:
                return nameof(Types.Squares.e1);
            case Types.Squares.f1:
                return nameof(Types.Squares.f1);
            case Types.Squares.g1:
                return nameof(Types.Squares.g1);
            case Types.Squares.h1:
                return nameof(Types.Squares.h1);
            case Types.Squares.a2:
                return nameof(Types.Squares.a2);
            case Types.Squares.b2:
                return nameof(Types.Squares.b2);
            case Types.Squares.c2:
                return nameof(Types.Squares.c2);
            case Types.Squares.d2:
                return nameof(Types.Squares.d2);
            case Types.Squares.e2:
                return nameof(Types.Squares.e2);
            case Types.Squares.f2:
                return nameof(Types.Squares.f2);
            case Types.Squares.g2:
                return nameof(Types.Squares.g2);
            case Types.Squares.h2:
                return nameof(Types.Squares.h2);
            case Types.Squares.a3:
                return nameof(Types.Squares.a3);
            case Types.Squares.b3:
                return nameof(Types.Squares.b3);
            case Types.Squares.c3:
                return nameof(Types.Squares.c3);
            case Types.Squares.d3:
                return nameof(Types.Squares.d3);
            case Types.Squares.e3:
                return nameof(Types.Squares.e3);
            case Types.Squares.f3:
                return nameof(Types.Squares.f3);
            case Types.Squares.g3:
                return nameof(Types.Squares.g3);
            case Types.Squares.h3:
                return nameof(Types.Squares.h3);
            case Types.Squares.a4:
                return nameof(Types.Squares.a4);
            case Types.Squares.b4:
                return nameof(Types.Squares.b4);
            case Types.Squares.c4:
                return nameof(Types.Squares.c4);
            case Types.Squares.d4:
                return nameof(Types.Squares.d4);
            case Types.Squares.e4:
                return nameof(Types.Squares.e4);
            case Types.Squares.f4:
                return nameof(Types.Squares.f4);
            case Types.Squares.g4:
                return nameof(Types.Squares.g4);
            case Types.Squares.h4:
                return nameof(Types.Squares.h4);
            case Types.Squares.a5:
                return nameof(Types.Squares.a5);
            case Types.Squares.b5:
                return nameof(Types.Squares.b5);
            case Types.Squares.c5:
                return nameof(Types.Squares.a1);
            case Types.Squares.d5:
                return nameof(Types.Squares.a1);
            case Types.Squares.e5:
                return nameof(Types.Squares.a1);
            case Types.Squares.f5:
                return nameof(Types.Squares.a1);
            case Types.Squares.g5:
                return nameof(Types.Squares.a1);
            case Types.Squares.h5:
                return nameof(Types.Squares.a1);
            case Types.Squares.a6:
                return nameof(Types.Squares.a1);
            case Types.Squares.b6:
                return nameof(Types.Squares.a1);
            case Types.Squares.c6:
                return nameof(Types.Squares.a1);

            case Types.Squares.e6:
                return nameof(Types.Squares.a1);

            case Types.Squares.f6:
                return nameof(Types.Squares.a1);

            case Types.Squares.g6:
                return nameof(Types.Squares.a1);

            case Types.Squares.h6:
                return nameof(Types.Squares.a1);

            case Types.Squares.a7:
                return nameof(Types.Squares.a1);
            case Types.Squares.b7:
                return nameof(Types.Squares.a1);
            case Types.Squares.c7:
                return nameof(Types.Squares.c7);
            case Types.Squares.d7:
                return nameof(Types.Squares.d7);
            case Types.Squares.e7:
                return nameof(Types.Squares.e7);
            case Types.Squares.f7:
                return nameof(Types.Squares.f7);
            case Types.Squares.g7:
                return nameof(Types.Squares.g7);
            case Types.Squares.h7:
                return nameof(Types.Squares.h7);
            case Types.Squares.a8:
                return nameof(Types.Squares.a8);
            case Types.Squares.b8:
                return nameof(Types.Squares.b8);
            case Types.Squares.c8:
                return nameof(Types.Squares.c8);
            case Types.Squares.d8:
                return nameof(Types.Squares.d8);
            case Types.Squares.e8:
                return nameof(Types.Squares.e8);
            case Types.Squares.f8:
                return nameof(Types.Squares.f8);
            case Types.Squares.g8:
                return nameof(Types.Squares.g8);
            case Types.Squares.h8:
                return nameof(Types.Squares.h8);
            case Types.Squares.none:
                return nameof(Types.Squares.a1);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}