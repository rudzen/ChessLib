using System.Collections.Immutable;
using Akka.Actor;
using Rudzoft.ChessLib.Types;
using Serilog;

namespace Rudzoft.Perft.Actors;

public sealed class FenTargetActor : ReceiveActor, IWithTimers
{
    public sealed record Inititalize(string Fen, ImmutableArray<Move> Moves, int DepthTarget);

    public sealed record Update();

    private static readonly ILogger Logger = Log.ForContext<FenTargetActor>();
    
    private string _fen;
    private ImmutableArray<Move> _moves;
    private int _depthTarget;

    public FenTargetActor()
    {
        Receive<Inititalize>(initialize =>
        {
            _fen = initialize.Fen;
            _moves = initialize.Moves;
            _depthTarget = initialize.DepthTarget;
        });
    }
    
    public ITimerScheduler Timers { get; set; }

}