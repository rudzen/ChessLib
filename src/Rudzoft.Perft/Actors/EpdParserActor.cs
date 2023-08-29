using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Rudzoft.Perft.Parsers;

namespace Rudzoft.Perft.Actors;

public sealed class EpdParserActor : ReceiveActor
{
    public EpdParserActor(IServiceProvider sp)
    {
        var epdParser = sp.CreateScope().ServiceProvider.GetRequiredService<IEpdParser>();
        
        ReceiveAsync<string>(async filename =>
        {
            await foreach(var parsed in epdParser.Parse(filename).ConfigureAwait(false))
                Sender.Tell(parsed);
        });
    }

}