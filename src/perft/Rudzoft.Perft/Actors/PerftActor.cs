using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rudzoft.ChessLib.Polyglot;
using Rudzoft.Perft.Domain;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Services;

namespace Rudzoft.Perft.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PerftActor : ReceiveActor
{
    private readonly IPerftRunner _perftRunner;
    private readonly IActorRef _outputActor;

    public PerftActor(IServiceProvider sp)
    {
        _perftRunner = sp.GetRequiredService<IPerftRunner>();
        var props = Props.Create<OutputActor>();

        var polyConfig = sp.GetRequiredService<PolyglotBookConfiguration>();

        var optionsFactory = sp.GetRequiredService<IOptionsFactory>();
        var options = optionsFactory.Parse();

        foreach (var option in options)
        {
            if (option.Type == OptionType.TTOptions)
                _perftRunner.TranspositionTableOptions = option.PerftOptions;
            else
                _perftRunner.Options = option.PerftOptions;
        }

        _outputActor = Context.ActorOf(props, "output-actor");
        ReceiveAsync<RunPerft>(_ => _perftRunner.Run());
    }
}