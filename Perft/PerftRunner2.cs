using Chess.Perft;
using Chess.Perft.Interfaces;
using DryIoc;
using Perft.Options;
using Perft.Parsers;
using Rudz.Chess;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Perft
{
    public sealed class PerftRunner2
    {
        private static readonly string Line = new string('-', 65);

        private static readonly ILogger log = Framework.Logger;

        private static Action<string> _boardPrint;

        private TransformManyBlock<IEpdParser, IPerftPosition> _startBlock;

        private readonly object _blockLock = new object();

        public async Task Run(EpdOptions options, Action<string> boardPrint = null)
        {
            lock (_blockLock)
            {
                _boardPrint = boardPrint;
                if (_startBlock == null)
                    _startBlock = GenerateBlocks();
            }

            var parser = Framework.IoC.Resolve<IEpdParser>();
            parser.Settings.Filename = options.Epds.First();
            var sw = Stopwatch.StartNew();

            var parsedCount = await parser.ParseAsync().ConfigureAwait(false);

            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;
            log.Information("Parsed {0} epd entries in {1} ms", parsedCount, elapsedMs);

            _startBlock.Post(parser);
            _startBlock.Complete();
            _startBlock.Completion.Wait();

            return;


            var perftPositions = parser.Sets.Select(set => PerftPositionFactory.Create(set.Epd, set.Perft)).ToList();

            var errors = 0;

            var perft = Framework.IoC.Resolve<IPerft>();

            perft.Positions = perftPositions;

            for (var i = 0; i < parser.Sets.Count; ++i)
            {
                var pp = perftPositions[i];
                perft.SetGamePosition(pp);
                boardPrint?.Invoke(perft.GetBoard());
                log.Information("Fen         : {0}", pp.Fen);
                log.Information(Line);

                foreach (var (depth, expected) in parser.Sets[i].Perft)
                {
                    log.Information("Depth       : {0}", depth);
                    sw.Restart();
                    var result = await perft.DoPerftAsync(depth).ConfigureAwait(false);
                    sw.Stop();

                    // add 1 to avoid potential dbz
                    elapsedMs = sw.ElapsedMilliseconds + 1;
                    var nps = 1000 * result / (ulong)elapsedMs;
                    log.Information("Time passed : {0}", elapsedMs);
                    log.Information("Nps         : {0}", nps);
                    log.Information("Result      : {0} - should be {1}", result, expected);
                    log.Information("TT hits     : {0}", Game.Table.Hits);
                    if (expected == result)
                        log.Information("Move count matches!");
                    else
                    {
                        log.Error("Move count failed!");
                        errors++;
                    }
                    Console.WriteLine();
                }
            }
        }

        private static TransformManyBlock<IEpdParser, IPerftPosition> GenerateBlocks()
        {
            var init = new TransformManyBlock<IEpdParser, IPerftPosition>(
                _ => _.Sets.Select(set => PerftPositionFactory.Create(set.Epd, set.Perft)));

            var makePerft = new TransformBlock<IPerftPosition, IPerft>(_ =>
            {
                var perft = Framework.IoC.Resolve<IPerft>();
                perft.Positions.Add(_);
                return perft;
            });

            var doSet = new ActionBlock<IPerft>(perft =>
            {
                var errors = 0;
                PerftPrintData pd;
                foreach (var pp in perft.Positions)
                {
                    perft.SetGamePosition(pp);
                    _boardPrint?.Invoke(perft.GetBoard());
                    log.Information("Fen         : {0}", pp.Fen);
                    log.Information(Line);

                    foreach (var (depth, expected) in pp.Value)
                    {
                        log.Information("Depth       : {0}", depth);
                        var sw = Stopwatch.StartNew();
                        pd.result = perft.DoPerftAsync(depth).GetAwaiter().GetResult();
                        sw.Stop();

                        // add 1 to avoid potential dbz
                        pd.elapsed = sw.ElapsedMilliseconds + 1;
                        pd.nps = 1000 * pd.result / (ulong)pd.elapsed;
                        pd.expected = expected;

                        if (expected == pd.result)
                            log.Information("Move count matches!");
                        else
                        {
                            log.Error("Move count failed!");
                            errors++;
                        }

                        log.Information("Time passed : {0}", pd.elapsed);
                        log.Information("Nps         : {0}", pd.nps);
                        log.Information("Result      : {0} - should be {1}", pd.result, expected);
                        log.Information("TT hits     : {0}", Game.Table.Hits);

                        Console.WriteLine();
                    }

                    if (errors > 0)
                        log.Error("Error count {0}", errors);

                }
            });

            init.LinkTo(makePerft);
            makePerft.LinkTo(doSet);

            return init;
        }
    }
}