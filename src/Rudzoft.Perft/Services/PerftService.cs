using System.Diagnostics;
using Akka.Actor;
using Akka.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rudzoft.Perft.Actors;
using Serilog;

namespace Rudzoft.Perft.Services;

public sealed class PerftService : IHostedService
{
    private const string Version = "v0.2.0";

    private static readonly ILogger Log = Serilog.Log.ForContext<PerftService>();

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IBuildTimeStamp _buildTimeStamp;
    
    private ActorSystem _actorSystem;
    private IActorRef _perftActor;

    public PerftService(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime applicationLifetime,
        IBuildTimeStamp buildTimeStamp)
    {
        _serviceProvider = serviceProvider;
        _applicationLifetime = applicationLifetime;
        _buildTimeStamp = buildTimeStamp;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("ChessLib Perft test program {Version} ({Time})", Version, _buildTimeStamp.TimeStamp);
        Log.Information("High timer resolution : {HighRes}", Stopwatch.IsHighResolution);
        Log.Information("Initializing..");

        var bootstrap = BootstrapSetup.Create();

        // enable DI support inside this ActorSystem, if needed
        var diSetup = DependencyResolverSetup.Create(_serviceProvider);

        // merge this setup (and any others) together into ActorSystemSetup
        var actorSystemSetup = bootstrap.And(diSetup);

        // start ActorSystem
        _actorSystem = ActorSystem.Create("perft", actorSystemSetup);
        _perftActor = _actorSystem.ActorOf(PerftActor.Prop(_serviceProvider));
        
        _perftActor.Tell(new PerftActor.StartPerft());
        
        // add a continuation task that will guarantee shutdown of application if ActorSystem terminates
        _actorSystem.WhenTerminated.ContinueWith(_ => {
            _applicationLifetime.StopApplication();
        }, cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // strictly speaking this may not be necessary - terminating the ActorSystem would also work
        // but this call guarantees that the shutdown of the cluster is graceful regardless
        await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
    }
}