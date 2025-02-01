using Akka.Actor;
using Akka.Event;
using Rudzoft.Perft.Models;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Services;

namespace Rudzoft.Perft.Actors;

/// <summary>
/// Keeps track of the EPD sets to be processed
/// If we are working, we don't do anything until we are told
/// </summary>
public sealed class PerftRunnerActor : ReceiveActor
{
    public sealed record ParseEpdEntry(IEpdSet EpdSet);

    public sealed record PerftFen(string Fen, int Depth);

    public sealed record RequestEpdSet;

    private readonly ILoggingAdapter _log = Context.GetLogger();

    private readonly Queue<PerftPosition> _positionSets = new();

    private bool _working;

    public PerftRunnerActor(IServiceProvider sp)
    {
        //Receive<PerftPosition>();
        Receive<PerftFen>(Fen);
        Receive<ParseEpdEntry>(AddEpdEntry);
        Receive<RequestEpdSet>(_ => ProcessRequestEpdSet());
    }

    private void RunPosition(PerftPosition perftPosition)
    {

    }

    private void Fen(PerftFen fen)
    {
        var perftPositionValue = new PerftPositionValue(fen.Depth, ulong.MinValue);
        var perftPosition = new PerftPosition(Guid.NewGuid().ToString(), fen.Fen, [perftPositionValue]);

        _positionSets.Enqueue(perftPosition);

        _log.Info("Fen: {Fen} Depth: {Depth}", fen.Fen, fen.Depth);

        if (_working)
            return;

        SendNext();
    }

    private void AddEpdEntry(ParseEpdEntry parseEpdEntry)
    {
        var id = Guid.NewGuid().ToString();
        foreach (var values in parseEpdEntry.EpdSet.Perft)
        {
            _positionSets.Enqueue(new(id, parseEpdEntry.EpdSet.Epd, [values]));
        }

        if (_working)
            return;
    }

    private void ProcessRequestEpdSet()
    {
        SendNext();
    }

    private void SendNext()
    {
        if (_positionSets.Count == 0)
        {
            _working = false;
            return;
        }

        _working = true;
        var epdSet = _positionSets.Dequeue();
        Sender.Tell(epdSet);
    }

    private PerftResult ComputePerft(PerftPosition perftPosition)
    {
        return new();
        // var result = _resultPool.Get();
        //
        // var pp = _perft.Positions[^1];
        // var baseFileName = SaveResults
        //     ? Path.Combine(CurrentDirectory, $"{FixFileName(pp.Fen)}[")
        //     : string.Empty;
        //
        // var s = Stopwatch.GetTimestamp();
        // var ss = Environment.TickCount64;
        //
        // var errors = 0;
        //
        // result.Fen = perftPosition.Fen;
        // _perft.SetGamePosition(perftPosition);
        // _perft.BoardPrintCallback(_perft.GetBoard());
        // Log.Information("Fen         : {Fen}", perftPosition.Fen);
        // Log.Information(Line);
        //
        // foreach (var (depth, expected) in perftPosition.Value)
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //
        //     Log.Information("Depth       : {Depth}", depth);
        //
        //     var start = Stopwatch.GetTimestamp();
        //
        //     var perftResult = _perft.DoPerftSimple(depth);
        //     var elapsedMs = Stopwatch.GetElapsedTime(start);
        //
        //     ComputeResults(in perftResult, depth, in expected, in elapsedMs, result);
        //
        //     errors += LogResults(result);
        //
        //     if (baseFileName.IsNullOrEmpty())
        //         continue;
        //
        //     await WriteOutput(result, baseFileName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        // }
        //
        // Log.Information("{Info} parsing complete. Encountered {Errors} errors", _parsers.ToString(), errors);
        //
        // result.Errors = errors;
        //
        // return result;
        // }
    }
}