using System.Diagnostics;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Rudzoft.ChessLib.Fen;
using Rudzoft.Perft.Messages;
using Rudzoft.Perft.Models;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Services;
using Rudzoft.Perft.Settings.Settings;
using Serilog;

namespace Rudzoft.Perft.Actors;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PerftActor : ReceiveActor
{
    private readonly IEpdParser _epdParser;

    private readonly IPerftRunner _perftRunner;
    private readonly IActorRef _outputActor;
    private readonly IActorRef _peftRunnerActor;

    private readonly EpdSettings _epdSettings;
    private readonly FenSettings _fenSettings;

    public PerftActor(IServiceProvider sp)
    {
        _perftRunner = sp.GetRequiredService<IPerftRunner>();
        _epdParser = sp.GetRequiredService<IEpdParser>();
        _epdSettings = sp.GetRequiredService<EpdSettings>();
        _fenSettings = sp.GetRequiredService<FenSettings>();

        var outputProps = Props.Create<OutputActor>();
        _outputActor = Context.ActorOf(outputProps, "output-actor");

        var runnerProps = Props.Create<PerftRunnerActor>(sp);
        _peftRunnerActor = Context.ActorOf(runnerProps, "perft-runner-actor");

        ReceiveAsync<RunPerft>(Run);
        Receive<PerftResult>(Result);
    }

    private async Task Run(RunPerft runPerft)
    {
        var fenPositions = ParseFen(_fenSettings.Entries);

        foreach (var fenPosition in fenPositions)
        {
            _peftRunnerActor.Tell(fenPosition);
        }

        fenPositions.Clear();

        foreach (var epdFile in _epdSettings.Files)
        {
            fenPositions.AddRange(await ParseEpdFile(epdFile));
            foreach (var perftPosition in fenPositions)
            {
                _peftRunnerActor.Tell(perftPosition);
            }
        }

        _perftRunner.Run();
    }

    private void Result(PerftResult perftResult)
    {

    }

    private async Task<List<PerftPosition>> ParseEpdFile(string epdFile)
    {
        var start = Stopwatch.GetTimestamp();

        var result = new List<PerftPosition>();

        await foreach (var epdPos in _epdParser.Parse(epdFile))
        {
            var perftPosition = PerftPositionFactory.Create(epdPos.Id, epdPos.Epd, epdPos.Perft);
            result.Add(perftPosition);
        }

        var elapsed = Stopwatch.GetElapsedTime(start);

        Log.Information("EPD processing completed. parsed={Parsed},time={Elapsed}", result.Count, elapsed);

        return result;
    }

    private List<PerftPosition> ParseFen(FenEntry[] fenEntries)
    {
        return fenEntries.Select(ParseFen).ToList();
    }

    private static PerftPosition ParseFen(FenEntry fenEntry)
    {
        var fen = string.Equals("startpos", fenEntry.Fen, StringComparison.OrdinalIgnoreCase)
            ? Fen.StartPositionFen
            : fenEntry.Fen;
        var ppValues = fenEntry.Depths.Select(x => new PerftPositionValue(x.Depth, x.ExpectedMoveCount)).ToList();
        return PerftPositionFactory.Create(Guid.NewGuid().ToString(), fen, ppValues);
    }


    /*
     * PerftActor ->
     *              PositionParserActor
     *              PerftRunnerActor
     *
     *
     *
     */
}