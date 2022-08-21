using BenchmarkDotNet.Attributes;
using Rudz.Chess;
using Rudz.Chess.Fen;
using Rudz.Chess.Types;

namespace Chess.Benchmark;

[MemoryDiagnoser]
public class FenBenchmark
{
    private const string F = "rnkq1bnr/p3ppp1/1ppp3p/3B4/6b1/2PQ3P/PP1PPP2/RNB1K1NR w KQ 4 10";

    private IGame _game;

    [Params(10000, 50000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var board = new Board();
        var pieceValue = new PieceValue();
        var pos = new Position(board, pieceValue);
        _game = new Game(pos);
        var fp = new FenData(F);
        var state = new State();
        _game.Pos.Set(in fp, Rudz.Chess.Enums.ChessMode.Normal, state);
    }

    [Benchmark(Description = "StringBuilder - NOT PRESENT")]
    public void GetFen()
    {
        for (var i = 0; i < N; ++i)
        {
            var fd = _game.Pos.GenerateFen();
        }
    }

    [Benchmark(Baseline = true, Description = "StackAlloc")]
    public void GetFenOp()
    {
        for (var i = 0; i < N; ++i)
        {
            var fd = _game.Pos.GenerateFen();
        }
    }
}
