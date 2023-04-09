using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.ObjectPoolPolicies;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Benchmark;

[MemoryDiagnoser]
public class MoveGeneratorBenchmark
{
    [Params
        (
            Fen.Fen.StartPositionFen,
            // "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",
            // "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
            // "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
            // "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10",
            "rnbqk2r/pppppppp/8/8/8/8/PPPPPPPP/RNBQK2R w KQkq - 5 25",
            "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1"
        )
    ]
    public string _fen;
    
    private IPosition _pos;

    private ObjectPool<IMoveList> _objectPool;

    [GlobalSetup]
    public void Setup()
    {
        var board = new Board();
        var pieceValue = new Values();
        var state = new State();
        var validator = new PositionValidator();
        _objectPool = new DefaultObjectPool<IMoveList>(new MoveListPolicy());
        _pos = new Position(board, pieceValue, validator, _objectPool);
        _pos.Set(_fen, ChessMode.Normal, state);
    }

    [Benchmark(Baseline = true)]
    public int GenerateMovesNoPool()
    {
        var moves = _pos.GenerateMoves();
        return moves.Length;
    }
    
    [Benchmark]
    public int GenerateMovesWithPool()
    {
        var moves = _objectPool.Get();
        moves.Generate(_pos);
        var length = moves.Length;
        _objectPool.Return(moves);
        return length;
    }
}