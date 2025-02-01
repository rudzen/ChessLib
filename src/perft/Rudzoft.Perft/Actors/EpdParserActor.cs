using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Rudzoft.Perft.Parsers;

namespace Rudzoft.Perft.Actors;

public sealed class EpdParserActor : ReceiveActor
{
    public sealed record ParseFen(string Fen);

    // creates IEpdSets
    public sealed record ParseEpdFile(string EpdFile);

    private readonly IEpdParser _epdParser;

    public EpdParserActor(IServiceProvider sp)
    {
        _epdParser = sp.GetRequiredService<IEpdParser>();

        ReceiveAsync<ParseEpdFile>(Epd);
    }

    private async Task Epd(ParseEpdFile parseEpd)
    {
        var parent = Context.Parent;
        await foreach(var epdSet in _epdParser.Parse(parseEpd.EpdFile))
        {
            parent.Tell(epdSet);
        }
    }
}