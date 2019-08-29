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

using Chess.Perft;
using Chess.Perft.Interfaces;
using Perft.Options;
using Perft.Parsers;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Perft
{
    /// <summary>
    /// Main perft runner
    /// </summary>
    public sealed class PerftRunner
    {
        private TransformBlock<(string, int, ulong), IPerft> _epdDataFlow;

        private TransformBlock<string, PerftPrintData> _fenDataFlow;

        private int _runErrors;

        private object _printLocker = new object();

        public PerftRunner()
        {
        }

        public void Initialize()
        {
        }

        /// <summary>
        ///
        /// </summary>
        public async Task Run(EpdOptions epdOptions)
        {
            Log.Information("Parsing files");

            // generate settings
            var parserSettings = new List<IEpdParserSettings>(epdOptions.Epds.Select(e => new EpdParserSettings { Filename = e }));

            // generate parsers
            var parsers = new List<EpdParser>(parserSettings.Select(ps => new EpdParser(ps)));

            // reset run error counter
            _runErrors = 0;

            var swTotal = Stopwatch.StartNew();

            var totalParsed = 0ul;

            // start parsing..
            foreach (var epdParser in parsers)
            {
                var sw = Stopwatch.StartNew();
                var parsedCount = await epdParser.ParseAsync().ConfigureAwait(false);
                sw.Stop();
                totalParsed += parsedCount;
                Log.Information("Parsed {0} from {1} in {2} ms", parsedCount, epdParser.Settings.Filename, sw.ElapsedMilliseconds);
            }

            swTotal.Stop();

            Log.Information("Parsed a total of {0} entries from {1} epd files in {2} ms", totalParsed, parsers.Count, swTotal.ElapsedMilliseconds);
            Log.Information("Building data flow engine");

            if (_epdDataFlow == null)
                _epdDataFlow = CreateEpdDataFlow();

            foreach (var epdParserSet in parsers.SelectMany(epdParser => epdParser.Sets))
                foreach (var (depth, expected) in epdParserSet.Perft)
                    _epdDataFlow.Post((epdParserSet.Epd, depth, expected));

            _epdDataFlow.Complete();
            _epdDataFlow.Completion.Wait();
        }

        public void Run(FenOptions fenOptions)
        {
            _runErrors = 0;

            ShowErrors();
        }

        private TransformBlock<(string, int, ulong), IPerft> CreateEpdDataFlow()
        {
            //var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var blockOptions = new DataflowBlockOptions { EnsureOrdered = true };
            var executionDataflowBlockOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 };

            var initBlock = new TransformBlock<(string, int, ulong), IPerft>(set =>
            {
                var p = PerftFactory.Create();
                var (fen, depth, expected) = set;
                var pp = PerftPositionFactory.Create(fen, null);
                p.SetGamePosition(pp);
                p.Depth = depth;
                p.Expected = expected;

                return p;
            }, executionDataflowBlockOptions);

            var perftRun = new TransformBlock<IPerft, PerftPrintData>(async perft =>
            {
                var sw = Stopwatch.StartNew();
                var result = await perft.DoPerftAsync(perft.Depth).ConfigureAwait(false);
                sw.Stop();
                var pd = new PerftPrintData
                {
                    depth = perft.Depth,
                    expected = perft.Expected,
                    elapsed = sw.ElapsedMilliseconds + 1,
                    result = result
                };
                pd.nps = 1000 * result / (ulong)pd.elapsed;

                return pd;
            }, executionDataflowBlockOptions);

            var printBlock = new ActionBlock<PerftPrintData>(data =>
            {
                lock (_printLocker)
                {
                    Log.Information("Depth       : {0}", data.depth);
                    Log.Information("Time passed : {0}", data.elapsed);
                    Log.Information("Nps         : {0}", data.nps);
                    Log.Information("Result      : {0} - should be {1}", data.result, data.expected);
                    if (data.expected == data.result)
                        Log.Information("Move count matches!");
                    else
                    {
                        Log.Error("Move count failed!");
                        Interlocked.Increment(ref _runErrors);
                    }
                }
            }, executionDataflowBlockOptions);

            perftRun.LinkTo(printBlock);
            initBlock.LinkTo(perftRun);

            return initBlock;
        }

        private void BoardPrint(string s) => Log.Information("Board:\n{0}", s);

        private void ShowErrors()
            => Log.Information("Finished - found a total of {0} errors.", _runErrors);
    }
}