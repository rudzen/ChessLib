/*
Perft, a chess perft testing application

MIT License

Copyright (c) 2019-2023 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.Perft.Models;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Settings.Settings;
using Serilog;

namespace Rudzoft.Perft.Services;

public sealed class PerftRunner : IPerftRunner
{
    [Flags]
    private enum PerftTypes
    {
        None = 0,
        Epd = 1,
        Fen = 2
    }

    private const string Line = "-----------------------------------------------------------------";

    private static readonly ILogger Log = Serilog.Log.ForContext<PerftRunner>();

    private static string CurrentDirectory => Environment.CurrentDirectory;

    private readonly Func<CancellationToken, IAsyncEnumerable<PerftPosition>>[] _runners;

    private readonly IEpdParser _epdParser;

    private readonly IPerft _perft;

    private readonly ITranspositionTable _transpositionTable;

    private readonly ObjectPool<PerftResult> _resultPool;

    private readonly IUci _uci;

    private readonly EpdSettings _epdSettings;

    private readonly FenSettings _fenSettings;

    private readonly TranspositionTableSettings _ttSettings;

    private readonly Cpu _cpu;

    private PerftTypes _parsers;

    public PerftRunner(
        IEpdParser parser,
        IPerft perft,
        ITranspositionTable transpositionTable,
        ObjectPool<PerftResult> resultPool,
        IUci uci,
        EpdSettings epdSettings,
        FenSettings fenSettings,
        TranspositionTableSettings ttSettings
    )
    {
        _epdParser = parser;
        _perft = perft;
        _perft.BoardPrintCallback ??= s => Log.Information("Board:\n{Board}", s);
        _transpositionTable = transpositionTable;
        _resultPool = resultPool;
        _uci = uci;
        _epdSettings = epdSettings;
        _fenSettings = fenSettings;
        _ttSettings = ttSettings;

        _runners = [ParseEpd, ParseFen];

        _cpu = new();
    }

    public bool SaveResults { get; set; }

    public Task<int> Run(CancellationToken cancellationToken = default) => InternalRun(cancellationToken);

    private async Task<int> InternalRun(CancellationToken cancellationToken = default)
    {
        if (_ttSettings.Use)
            _transpositionTable.SetSize(_ttSettings.Size);

        var errors = 0;

        _perft.Positions = [];

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        _parsers = PerftTypes.None;

        if (_epdSettings.Files.Length > 0)
            _parsers |= PerftTypes.Epd;

        if (_fenSettings.Entries.Length > 0)
            _parsers |= PerftTypes.Fen;

        if ((_parsers & PerftTypes.Epd) != 0)
        {
            await foreach (var position in ParseEpd(cancellationToken).ConfigureAwait(false))
            {
                _perft.AddPosition(position);
                try
                {
                    var result = await ComputePerft(cancellationToken).ConfigureAwait(false);
                    Interlocked.Add(ref errors, result.Errors);
                    if (errors != 0)
                        Log.Error("Parsing failed for Id={Id}", position.Id);
                    _resultPool.Return(result);
                }
                catch (AggregateException e)
                {
                    Log.Error(e.GetBaseException(), "Cancel requested");
                    Interlocked.Increment(ref errors);
                    break;
                }
            }
        }

        if ((_parsers & PerftTypes.Fen) != 0)
        {
            await foreach (var position in ParseFen(cancellationToken).ConfigureAwait(false))
            {
                _perft.AddPosition(position);
                try
                {
                    var result = await ComputePerft(cancellationToken).ConfigureAwait(false);
                    Interlocked.Add(ref errors, result.Errors);
                    if (errors != 0)
                        Log.Error("Parsing failed for Id={Id}", position.Id);
                    _resultPool.Return(result);
                }
                catch (AggregateException e)
                {
                    Log.Error(e.GetBaseException(), "Cancel requested");
                    Interlocked.Increment(ref errors);
                    break;
                }
            }
        }

        GCSettings.LatencyMode = GCLatencyMode.Interactive;

        return errors;
    }

    private async IAsyncEnumerable<PerftPosition> ParseEpd(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var epd in _epdSettings.Files)
        {
            var start = Stopwatch.GetTimestamp();

            var parsedCount = 0L;

            await foreach (var epdPos in _epdParser.Parse(epd).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                parsedCount++;

                var perftPosition = PerftPositionFactory.Create(epdPos.Id, epdPos.Epd, epdPos.Perft);

                yield return perftPosition;
            }

            var elapsed = Stopwatch.GetElapsedTime(start);
            Log.Information("EPD processing completed. parsed={Parsed},time={Elapsed}", parsedCount, elapsed);
        }
    }

    private async IAsyncEnumerable<PerftPosition> ParseFen([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const ulong zero = ulong.MinValue;

        foreach (var fenEntry in _fenSettings.Entries)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            var fen = string.Equals("startpos", fenEntry.Fen, StringComparison.OrdinalIgnoreCase) ? Fen.StartPositionFen : fenEntry.Fen;
            var ppValues = fenEntry.Depths.Select(x => new PerftPositionValue(x.Depth, x.ExpectedMoveCount)).ToList();
            var perftPosition = PerftPositionFactory.Create(Guid.NewGuid().ToString(), fen, ppValues);
            yield return perftPosition;
        }
    }

    private async Task<PerftResult> ComputePerft(CancellationToken cancellationToken)
    {
        var result = _resultPool.Get();

        var pp = _perft.Positions[^1];
        var baseFileName = SaveResults
            ? Path.Combine(CurrentDirectory, $"{FixFileName(pp.Fen)}[")
            : string.Empty;

        var errors = 0;

        result.Fen = pp.Fen;
        _perft.SetGamePosition(pp);
        _perft.BoardPrintCallback(_perft.GetBoard());
        Log.Information("Fen         : {Fen}", pp.Fen);
        Log.Information(Line);

        foreach (var (depth, expected) in pp.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Log.Information("Depth       : {Depth}", depth);

            var start = Stopwatch.GetTimestamp();

            var perftResult = _perft.DoPerftSimple(depth);
            var elapsedMs = Stopwatch.GetElapsedTime(start);

            ComputeResults(in perftResult, depth, in expected, in elapsedMs, result);

            errors += LogResults(result);

            if (baseFileName.IsNullOrEmpty())
                continue;

            await WriteOutput(result, baseFileName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        Log.Information("{Info} parsing complete. Encountered {Errors} errors", _parsers.ToString(), errors);

        result.Errors = errors;

        return result;
    }

    private static async Task WriteOutput(
        PerftResult result,
        string baseFileName,
        CancellationToken cancellationToken)
    {
        var outputFileName = Path.Combine(Environment.CurrentDirectory, $"{baseFileName}{result.Depth}].json");
        await using var outStream = File.OpenWrite(outputFileName);
        await JsonSerializer.SerializeAsync(outStream, result, cancellationToken: cancellationToken);
    }

    private void ComputeResults(
        in UInt128 result,
        int depth,
        in ulong expected,
        in TimeSpan elapsedMs,
        PerftResult results)
    {
        // compute results
        results.Result = result;
        results.Depth = depth;
        // add 1 to avoid potential dbz
        results.Elapsed = elapsedMs.Add(TimeSpan.FromMicroseconds(1));
        results.Nps = _uci.Nps(in result, results.Elapsed);
        results.CorrectResult = expected;
        results.Passed = expected == result;
        results.TableHits = _transpositionTable.Hits;
    }

    private int LogResults(PerftResult result)
    {
        Log.Information("Time passed : {Elapsed}", result.Elapsed);
        Log.Information("Nps         : {Nps}", result.Nps);
        if ((_parsers & PerftTypes.Epd) != 0)
        {
            Log.Information("Result      : {Result} - should be {Expected}", result.Result, result.CorrectResult);
            if (result.Result != result.CorrectResult)
            {
                var difference = (long)(result.CorrectResult - result.Result);
                Log.Information("Difference  : {Diff}", Math.Abs(difference));
            }
        }
        else
            Log.Information("Result      : {Result}", result.Result);

        Log.Information("TT hits     : {Hits}", _transpositionTable.Hits);

        var error = 0;

        if ((_parsers & PerftTypes.Fen) == 0)
            return error;

        if (result.CorrectResult == result.Result)
            Log.Information("Move count matches!");
        else
        {
            Log.Error("Failed for position: {Fen}", _perft.Game.Pos.GenerateFen());
            error = 1;
        }

        return error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FixFileName(string input)
        => input.Replace('/', '_');
}