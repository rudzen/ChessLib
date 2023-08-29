using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Services;

namespace Rudzoft.Perft.Actors;

public sealed class PerftActor : ReceiveActor
{
    public sealed record StartPerft;
    
    private readonly IServiceProvider _sp;
    private readonly IPerftRunner _perftRunner;
    
    public PerftActor(IServiceProvider sp)
    {
        _sp = sp;
        _perftRunner = _sp.GetRequiredService<IPerftRunner>();
        var optionsFactory = _sp.GetRequiredService<IOptionsFactory>();
        foreach (var option in optionsFactory.Parse())
        {
            if (option.Type == OptionType.TTOptions)
                _perftRunner.TranspositionTableOptions = option.PerftOptions;
            else
                _perftRunner.Options = option.PerftOptions;
        }
        
        ReceiveAsync<StartPerft>(_ => _perftRunner.Run());
    }

    public static Props Prop(IServiceProvider sp)
    {
        return Props.Create<PerftActor>(sp);
    }
}