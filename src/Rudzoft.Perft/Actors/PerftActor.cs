using System.Collections.Immutable;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Rudzoft.Perft.Options;

namespace Rudzoft.Perft.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PerftActor : ReceiveActor
{
    public sealed record StartPerft;
    
    private readonly IServiceProvider _sp;
    
    public PerftActor(IServiceProvider sp)
    {
        _sp = sp;
        var optionsFactory = _sp.GetRequiredService<IOptionsFactory>();
        var options = optionsFactory.Parse().ToImmutableArray();
        
        Receive<StartPerft>(_ =>
        {
            var runner = Context.ActorOf(PerftRunnerActor.Prop(_sp), "runner-actor");
            runner.Tell(new PerftRunnerActor.RunOptions(options));
            runner.Tell(new PerftRunnerActor.Run());
        });
    }

    public static Props Prop(IServiceProvider sp)
    {
        return Props.Create<PerftActor>(sp);
    }
}