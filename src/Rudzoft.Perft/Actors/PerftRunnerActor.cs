using System.Collections.Immutable;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Services;

namespace Rudzoft.Perft.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PerftRunnerActor : ReceiveActor
{
    public sealed record RunOptions(ImmutableArray<PerftOption> Options);

    public sealed record Run;

    private readonly IPerftRunner _perftRunner;

    public PerftRunnerActor(IServiceProvider sp)
    {
        var scope = sp.CreateScope();
        _perftRunner = scope.ServiceProvider.GetRequiredService<IPerftRunner>();
        Become(OptionsBehaviour);
    }

    public static Props Prop(IServiceProvider sp)
    {
        return Props.Create<PerftRunnerActor>(sp);
    }

    private void RunBehaviour()
    {
        ReceiveAsync<Run>(_ => _perftRunner.Run());
    }

    private void OptionsBehaviour()
    {
        Receive<RunOptions>(options =>
        {
            foreach (var option in options.Options)
            {
                if (option.Type == OptionType.TTOptions)
                    _perftRunner.TranspositionTableOptions = option.PerftOptions;
                else
                    _perftRunner.Options = option.PerftOptions;
            }

            Become(RunBehaviour);
        });
    }
}