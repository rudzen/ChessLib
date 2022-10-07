/*
Perft, a chess perft testing application

MIT License

Copyright (c) 2019-2022 Rudy Alex Kohn

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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Perft;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Perft;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.TimeStamp;
using Serilog;

namespace Rudzoft.Perft;

public sealed class PerftRunner : IPerftRunner
{
    private const string Version = "v0.1.4";

    private static readonly string Line = new('-', 65);

    private static readonly Lazy<string> CurrentDirectory = new(static () => Environment.CurrentDirectory);

    private readonly Func<CancellationToken, IAsyncEnumerable<PerftPosition>>[] _runners;

    private readonly IEpdParser _epdParser;

    private readonly ILogger _log;

    private readonly IBuildTimeStamp _buildTimeStamp;

    private readonly IPerft _perft;

    private readonly ObjectPool<PerftResult> _resultPool;

    private readonly IUci _uci;

    private readonly CPU _cpu;

    private bool _usingEpd;

    public PerftRunner(
        IEpdParser parser,
        ILogger log,
        IBuildTimeStamp buildTimeStamp,
        IPerft perft,
        IConfiguration configuration,
        ObjectPool<PerftResult> resultPool,
        IUci uci)
    {
        _epdParser = parser;
        _log = log;
        _buildTimeStamp = buildTimeStamp;
        _perft = perft;
        _perft.BoardPrintCallback ??= s => _log.Information("Board:\n{0}", s);
        _resultPool = resultPool;

        _uci = uci;
        _uci.Initialize();

        _runners = new Func<CancellationToken, IAsyncEnumerable<PerftPosition>>[] { ParseEpd, ParseFen };

        TranspositionTableOptions = Framework.IoC.Resolve<IOptions>(OptionType.TTOptions) as TTOptions;
        configuration.Bind("TranspositionTable", TranspositionTableOptions);

        _cpu = new CPU();
    }

    public bool SaveResults { get; set; }

    public IOptions Options { get; set; }

    public TTOptions TranspositionTableOptions { get; set; }

    public Task<int> Run(CancellationToken cancellationToken = default) => InternalRun(cancellationToken);

    private async Task<int> InternalRun(CancellationToken cancellationToken = default)
    {
        LogInfoHeader();

        InternalRunArgumentCheck(Options);

        if (TranspositionTableOptions.Use)
            Game.Table.SetSize(TranspositionTableOptions.Size);

        var errors = 0;
        var runnerIndex = (Options is FenOptions).AsByte();
        _usingEpd = runnerIndex == 0;
        var positions = _runners[runnerIndex].Invoke(cancellationToken);

        _perft.Positions = new List<PerftPosition>();

        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        await foreach (var position in positions.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            _perft.AddPosition(position);
            try
            {
                var result = await ComputePerft(cancellationToken).ConfigureAwait(false);
                errors = result.Errors;
                if (errors != 0)
                    _log.Error("Parsing failed for Id={0}", position.Id);
                _resultPool.Return(result);
            }
            catch (AggregateException e)
            {
                _log.Error(e.GetBaseException(), "Cancel requested.");
                errors = 1;
                break;
            }
        }

        GCSettings.LatencyMode = GCLatencyMode.Interactive;

        return errors;
    }

    private static void InternalRunArgumentCheck(IOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options), "Cannot be null");
    }

    private IAsyncEnumerable<PerftPosition> ParseEpd(CancellationToken cancellationToken)
    {
        var options = Options as EpdOptions;
        return ParseEpd(options);
    }

    private async IAsyncEnumerable<PerftPosition> ParseEpd(EpdOptions options)
    {
        _epdParser.Settings.Filename = options.Epds.First();
        var sw = Stopwatch.StartNew();

        var parsedCount = await _epdParser.ParseAsync().ConfigureAwait(false);

        sw.Stop();
        var elapsedMs = sw.ElapsedMilliseconds;
        _log.Information("Parsed {0} epd entries in {1} ms", parsedCount, elapsedMs);

        var perftPositions = _epdParser.Sets.Select(static set => PerftPositionFactory.Create(set.Id, set.Epd, set.Perft));

        foreach (var perftPosition in perftPositions)
            yield return perftPosition;
    }

    private IAsyncEnumerable<PerftPosition> ParseFen(CancellationToken cancellationToken)
    {
        var options = Options as FenOptions;
        return ParseFen(options);
    }

#pragma warning disable 1998
    private static async IAsyncEnumerable<PerftPosition> ParseFen(FenOptions options)
#pragma warning restore 1998
    {
        const ulong zero = ulong.MinValue;

        var depths = options.Depths.Select(static d => new PerftPositionValue(d, zero)).ToList();

        var perftPositions =
            options.Fens.Select(f => PerftPositionFactory.Create(Guid.NewGuid().ToString(), f, depths));

        foreach (var perftPosition in perftPositions)
            yield return perftPosition;
    }

    private async Task<PerftResult> ComputePerft(CancellationToken cancellationToken)
    {
        var result = _resultPool.Get();
        result.Clear();

        var pp = _perft.Positions.Last();
        var baseFileName = SaveResults
            ? Path.Combine(CurrentDirectory.Value, $"{FixFileName(pp.Fen)}[")
            : string.Empty;

        var errors = 0;

        result.Fen = pp.Fen;
        _perft.SetGamePosition(pp);
        _perft.BoardPrintCallback(_perft.GetBoard());
        _log.Information("Fen         : {0}", pp.Fen);
        _log.Information(Line);

        var sw = new Stopwatch();

        foreach (var (depth, expected) in pp.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _log.Information("Depth       : {0}", depth);
            sw.Restart();

            var perftResult = await _perft.DoPerftAsync(depth).ConfigureAwait(false);
            sw.Stop();

            var elapsedMs = sw.ElapsedMilliseconds;

            ComputeResults(perftResult, depth, expected, elapsedMs, result);

            errors += await LogResults(result).ConfigureAwait(false);

            if (baseFileName.IsNullOrEmpty())
                continue;

            await WriteOutput(result, baseFileName, cancellationToken);
        }

        _log.Information("{0} parsing complete. Encountered {1} errors.", _usingEpd ? "EPD" : "FEN", errors);

        result.Errors = errors;

        return result;
    }

    private static async ValueTask WriteOutput(IPerftResult result, string baseFileName, CancellationToken cancellationToken)
    {
        // ReSharper disable once MethodHasAsyncOverload
        var contents = JsonSerializer.Serialize(result);
        var outputFileName = $"{baseFileName}{result.Depth}].json";
        await File.WriteAllTextAsync(
                outputFileName,
                contents,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private void ComputeResults(ulong result, int depth, ulong expected, long elapsedMs, IPerftResult results)
    {
        // compute results
        results.Result = result;
        results.Depth = depth;
        // add 1 to avoid potential dbz
        results.Elapsed = TimeSpan.FromMilliseconds(elapsedMs + 1);
        results.Nps = _uci.Nps(in result, results.Elapsed);
        results.CorrectResult = expected;
        results.Passed = expected == result;
        results.TableHits = Game.Table.Hits;
    }

    private void LogInfoHeader()
    {
        _log.Information("ChessLib Perft test program {0} ({1})", Version, _buildTimeStamp.TimeStamp);
        _log.Information("High timer resolution : {0}", Stopwatch.IsHighResolution);
        _log.Information("Initializing..");
    }

    private Task<int> LogResults(IPerftResult result)
    {
        return Task.Run(() =>
        {
            _log.Information("Time passed : {0}", result.Elapsed);
            _log.Information("Nps         : {0}", result.Nps);
            if (_usingEpd)
            {
                _log.Information("Result      : {0} - should be {1}", result.Result, result.CorrectResult);
                if (result.Result != result.CorrectResult)
                {
                    var difference = (long)(result.CorrectResult - result.Result);
                    _log.Information("Difference  : {0}", Math.Abs(difference));
                }
            }
            else
                _log.Information("Result      : {0}", result.Result);

            _log.Information("TT hits     : {0}", Game.Table.Hits);

            var error = 0;

            if (!_usingEpd)
                return error;

            if (result.CorrectResult == result.Result)
                _log.Information("Move count matches!");
            else
            {
                _log.Error("Failed for position: {0}", _perft.CurrentGame.Pos.GenerateFen());
                error = 1;
            }

            return error;
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FixFileName(string input)
        => input.Replace('/', '_');
}