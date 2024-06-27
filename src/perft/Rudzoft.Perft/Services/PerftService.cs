using System.Diagnostics;
using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Hosting;
using Rudzoft.Perft.Actors;
using Rudzoft.Perft.Domain;
using Serilog;

namespace Rudzoft.Perft.Services;

public sealed class PerftService : IHostedService
{
    private const string Version = "v0.2.0";

    private static readonly ILogger Log = Serilog.Log.ForContext<PerftService>();

    private readonly ActorSystem _actorSystem;
    private readonly IRequiredActor<PerftActor> _perftActor;

    public PerftService(
        IHostApplicationLifetime applicationLifetime,
        ActorSystem actorSystem,
        IRequiredActor<PerftActor> perftActor)
    {
        _actorSystem = actorSystem;
        _perftActor = perftActor;

        actorSystem.WhenTerminated.ContinueWith(_ => { applicationLifetime.StopApplication(); });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("ChessLib Perft test program {Version}", Version);
        Log.Information("High timer resolution : {HighRes}", Stopwatch.IsHighResolution);
        Log.Information("Initializing..");

        var perftActor = await _perftActor.GetAsync(cancellationToken);

        perftActor.Tell(new RunPerft());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // strictly speaking this may not be necessary - terminating the ActorSystem would also work
        // but this call guarantees that the shutdown of the cluster is graceful regardless
        await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
    }
}