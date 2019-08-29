/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Perft
{
    using Chess.Perft;
    using CommandLine;
    using Options;
    using Parsers;
    using Rudz.Chess;
    using Rudz.Chess.Fen;
    using Serilog;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    internal static class Program
    {
        private static readonly string Line = new string('-', 65);

        private static readonly BlockingCollection<PerftPrintData> PrintData = new BlockingCollection<PerftPrintData>();

        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            EpdOptions epdOptions = null;
            FenOptions fenOptions = null;
            TTOptions ttOptions = null;

            var setEdp = new Func<EpdOptions, int>(options =>
            {
                epdOptions = options;
                return 0;
            });

            var setFen = new Func<FenOptions, int>(options =>
            {
                fenOptions = options;
                return 0;
            });

            var setTT = new Func<TTOptions, int>(options =>
            {
                ttOptions = options;
                return 0;
            });

            /*
             * fens -f "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1" -d 6
             *
             */

            var returnValue = Parser.Default.ParseArguments<EpdOptions, FenOptions, TTOptions>(args)
                .MapResult(
                    (EpdOptions opts) => setEdp(opts),
                    (FenOptions opts) => setFen(opts),
                    (TTOptions opts) => setTT(opts),
                    errs => 1);

            if (returnValue == 0)
                returnValue = await RunAsync(epdOptions, fenOptions, ttOptions).ConfigureAwait(false);

            return returnValue;
        }

        private static async Task<int> RunAsync(EpdOptions epdOptions, FenOptions fenOptions, TTOptions ttOptions)
        {
            Log.Information("ChessLib Perft test program {0} ({1})", "v0.1.1", BuildTimeStamp.TimeStamp);
            Log.Information("High timer resolution : {0}", Stopwatch.IsHighResolution);
            Log.Information("Initializing..");

            var useEpd = epdOptions != null;

            if (!useEpd && fenOptions == null)
                fenOptions = new FenOptions { Depths = new[] { 5 }, Fens = new[] { Fen.StartPositionFen } };

            if (ttOptions == null)
                ttOptions = new TTOptions { Use = true, Size = 32 };

            void BoardPrint(string s) => Log.Information("Board:\n{0}", s);

            // test parse
            if (useEpd)
            {
                var epdResult = await RunEpd(epdOptions, BoardPrint).ConfigureAwait(false);
                return epdResult;
            }

            //if (baseOptions.Depth == 0)
            //    baseOptions.Depth = 5;

            //var p = new P(baseOptions.Depth, Callback);

            //bool defaultPos;

            //if (!baseOptions.Fen.Any())
            //{
            //    baseOptions.Fen = new []{ Fen.StartPositionFen };
            //    p.AddStartPosition();
            //    defaultPos = true;
            //    Log.Information("Using startpos");
            //}
            //else
            //{
            //    var perftPositions = baseOptions.Fen.Select(fen =>
            //    {
            //        var toAdd = string.Equals(fen, "startpos", StringComparison.OrdinalIgnoreCase)
            //            ? Fen.StartPositionFen
            //            : fen;
            //        return new PerftPosition(toAdd);
            //    });

            //    foreach (var perftPosition in perftPositions)
            //    {
            //        p.AddPosition(perftPosition);
            //        Log.Information("Found position {0}", perftPosition.Fen);
            //    }

            //    //p.AddPosition(new PerftPosition(options.Fen, new List<ulong>(1) { options.MoveCount }));
            //    defaultPos = false;
            //}

            //Log.Information("Running");
            //var watch = Stopwatch.StartNew();
            //var result = p.DoPerft();

            //watch.Stop();

            //// add 1 to avoid potential dbz
            //var elapsedMs = watch.ElapsedMilliseconds + 1;
            //var nps = 1000 * result / (ulong)elapsedMs;

            //Log.Information("Time passed : {0}", elapsedMs);
            //Log.Information("Nps         : {0}", nps);

            var returnValue = 0;

            //if (p.HasPositionCount(0, baseOptions.Depth) && defaultPos)
            //{
            //    var matches = p.GetPositionCount(0, baseOptions.Depth) == result;

            //    Console.WriteLine(matches
            //        ? "Move count matches!"
            //        : "Move count failed!");
            //}

            //Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();

            return returnValue;
        }

        private static async Task<int> RunEpd(EpdOptions options, Action<string> boardPrint = null)
        {
            //var r = new PerftRunner();
            //r.Initialize();
            //await r.Run(options);

            var parserSettings = new EpdParserSettings { Filename = options.Epds.First() };
            var parser = new EpdParser(parserSettings);

            var sw = Stopwatch.StartNew();

            var parsedCount = await parser.ParseAsync().ConfigureAwait(false);

            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;
            Log.Information("Parsed {0} epd entries in {1} ms", parsedCount, elapsedMs);

            var perftPositions = parser.Sets.Select(set => PerftPositionFactory.Create(set.Epd, set.Perft)).ToList();

            var errors = 0;

            var perft = new Perft(boardPrint);

            perft.Positions = perftPositions;

            for (var i = 0; i < parser.Sets.Count; ++i)
            {
                var pp = perftPositions[i];
                perft.SetGamePosition(pp);
                boardPrint?.Invoke(perft.GetBoard());
                Log.Information("Fen         : {0}", pp.Fen);
                Log.Information(Line);

                foreach (var (depth, expected) in parser.Sets[i].Perft)
                {
                    Log.Information("Depth       : {0}", depth);
                    sw.Restart();
                    var result = await perft.DoPerftAsync(depth).ConfigureAwait(false);
                    sw.Stop();
                    // add 1 to avoid potential dbz
                    elapsedMs = sw.ElapsedMilliseconds + 1;
                    var nps = 1000 * result / (ulong)elapsedMs;
                    Log.Information("Time passed : {0}", elapsedMs);
                    Log.Information("Nps         : {0}", nps);
                    Log.Information("Result      : {0} - should be {1}", result, expected);
                    Log.Information("TT hits     : {0}", Game.Table.Hits);
                    if (expected == result)
                        Log.Information("Move count matches!");
                    else
                    {
                        Log.Error("Move count failed!");
                        errors++;
                    }
                    Console.WriteLine();
                }
            }

            //Log.Information("EPD parsing complete. Encountered {0} errors.", errors);

            return 0;
            //return errors;
        }
    }
}