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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.Perft;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.Perft.Models;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Parsers;
using Serilog;

namespace Rudzoft.Perft.Services;

public sealed class PerftRunner : IPerftRunner
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PerftRunner>();

    private static readonly string Line = new('-', 65);

    private static string CurrentDirectory => Environment.CurrentDirectory;

    private readonly Func<CancellationToken, IAsyncEnumerable<PerftPosition>>[] _runners;

    private readonly IEpdParser _epdParser;

    private readonly IPerft _perft;

    private readonly ITranspositionTable _transpositionTable;

    private readonly ObjectPool<PerftResult> _resultPool;

    private readonly IUci _uci;

    private readonly Cpu _cpu;

    private bool _usingEpd;

    public PerftRunner(
        IEpdParser parser,
        IPerft perft,
        IConfiguration configuration,
        ITranspositionTable transpositionTable,
        ObjectPool<PerftResult> resultPool,
        IUci uci)
    {
        _epdParser                =   parser;
        _perft                    =   perft;
        _perft.BoardPrintCallback ??= s => Log.Information("Board:\n{Board}", s);
        _transpositionTable       =   transpositionTable;
        _resultPool               =   resultPool;
        _uci                      =   uci;
        _runners                  =   [ParseEpd, ParseFen];

        configuration.Bind("TranspositionTable", TranspositionTableOptions);

        _cpu = new();
    }

    public bool SaveResults { get; set; }

    public IPerftOptions Options { get; set; }

    public IPerftOptions TranspositionTableOptions { get; set; }

    public Task<int> Run(CancellationToken cancellationToken = default) => InternalRun(cancellationToken);

    private async Task<int> InternalRun(CancellationToken cancellationToken = default)
    {
        InternalRunArgumentCheck(Options);

        if (TranspositionTableOptions is TTOptions { Use: true } ttOptions)
            _transpositionTable.SetSize(ttOptions.Size);

        var errors      = 0;
        var runnerIndex = (Options is FenOptions).AsByte();
        _usingEpd = runnerIndex == 0;
        var positions = _runners[runnerIndex].Invoke(cancellationToken);

        _perft.Positions = [];

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        await foreach (var position in positions.WithCancellation(cancellationToken).ConfigureAwait(false))
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

        GCSettings.LatencyMode = GCLatencyMode.Interactive;

        return errors;
    }

    private static void InternalRunArgumentCheck(IPerftOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options), "Cannot be null");
    }

    private IAsyncEnumerable<PerftPosition> ParseEpd(CancellationToken cancellationToken)
    {
        var options = Options as EpdOptions;
        return ParseEpd(options, cancellationToken);
    }

    private async IAsyncEnumerable<PerftPosition> ParseEpd(
        EpdOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var epd in options.Epds)
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

    private IAsyncEnumerable<PerftPosition> ParseFen(CancellationToken cancellationToken)
    {
        var options = Options as FenOptions;
        return ParseFen(options, cancellationToken);
    }

#pragma warning disable 1998
    private static async IAsyncEnumerable<PerftPosition> ParseFen(
        FenOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
#pragma warning restore 1998
    {
        const ulong zero = ulong.MinValue;

        var depths = options.Depths.Select(static d => new PerftPositionValue(d, zero)).ToList();

        var perftPositions =
            options.Fens.Select(f => PerftPositionFactory.Create(Guid.NewGuid().ToString(), f, depths));

        foreach (var perftPosition in perftPositions)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;
            yield return perftPosition;
        }
    }

    private async Task<PerftResult> ComputePerft(CancellationToken cancellationToken)
    {
        var result = _resultPool.Get();
        result.Clear();

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

            var perftResult = await _perft.DoPerftAsync(depth).ConfigureAwait(false);
            var elapsedMs   = Stopwatch.GetElapsedTime(start);

            ComputeResults(in perftResult, depth, in expected, in elapsedMs, result);

            errors += LogResults(result);

            if (baseFileName.IsNullOrEmpty())
                continue;

            await WriteOutput(result, baseFileName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        Log.Information("{Info} parsing complete. Encountered {Errors} errors", _usingEpd ? "EPD" : "FEN", errors);

        result.Errors = errors;

        return result;
    }

    private static async Task WriteOutput(
        IPerftResult result,
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
        IPerftResult results)
    {
        // compute results
        results.Result = result;
        results.Depth  = depth;
        // add 1 to avoid potential dbz
        results.Elapsed       = elapsedMs.Add(TimeSpan.FromMicroseconds(1));
        results.Nps           = _uci.Nps(in result, results.Elapsed);
        results.CorrectResult = expected;
        results.Passed        = expected == result;
        results.TableHits     = _transpositionTable.Hits;
    }

    private int LogResults(IPerftResult result)
    {
        Log.Information("Time passed : {Elapsed}", result.Elapsed);
        Log.Information("Nps         : {Nps}", result.Nps);
        if (_usingEpd)
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

        if (!_usingEpd)
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