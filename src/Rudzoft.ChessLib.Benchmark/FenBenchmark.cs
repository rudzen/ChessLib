using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class FenBenchmark
{
    private const string F = "rnkq1bnr/p3ppp1/1ppp3p/3B4/6b1/2PQ3P/PP1PPP2/RNB1K1NR w KQ 4 10";

    private IPosition _pos;

    [Params(10000, 50000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var board = new Board();
        var pieceValue = new Values();
        var fp = new FenData(F);
        var state = new State();
        var rKiss = new RKiss();
        var zobrist = new Zobrist(rKiss);
        var cuckoo = new Cuckoo(zobrist);
        var validator = new PositionValidator(zobrist);
        var moveListObjectPool = new DefaultObjectPool<IMoveList>(new MoveListPolicy());
        _pos = new Position(board, pieceValue, zobrist, cuckoo, validator, moveListObjectPool);
        _pos.Set(in fp, ChessMode.Normal, in state);
    }

    [Benchmark(Description = "StringBuilder - NOT PRESENT")]
    public void GetFen()
    {
        for (var i = 0; i < N; ++i)
        {
            var fd = _pos.GenerateFen();
        }
    }

    [Benchmark(Baseline = true, Description = "StackAlloc")]
    public void GetFenOp()
    {
        for (var i = 0; i < N; ++i)
        {
            var fd = _pos.GenerateFen();
        }
    }
}
