/*
Perft, a chess perft testing application

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

using System.Collections.Generic;

namespace Perft
{
    using Chess.Perft;
    using Chess.Perft.Interfaces;
    using CommandLine;
    using DryIoc;
    using Microsoft.Extensions.Configuration;
    using Options;
    using Parsers;
    using Rudz.Chess;
    using Rudz.Chess.Fen;
    using Serilog;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using TimeStamp;

    internal static class Program
    {
        private static readonly string Line = new string('-', 65);

        private static readonly ILogger log;

        private const string ConfigFileName = "appsettings.json";

        static Program()
        {
            Framework.Startup(BuildConfiguration, AddServices);
            log = Framework.Logger;
        }

        public static async Task<int> Main(string[] args)
        {
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
            log.Information("ChessLib Perft test program {0} ({1})", "v0.1.1", Framework.IoC.Resolve<IBuildTimeStamp>().TimeStamp);
            log.Information("High timer resolution : {0}", Stopwatch.IsHighResolution);
            log.Information("Initializing..");

            var useEpd = epdOptions != null;

            if (!useEpd && fenOptions == null)
                fenOptions = new FenOptions { Depths = new[] { 6 }, Fens = new[] { Fen.StartPositionFen } };

            if (ttOptions == null)
                ttOptions = new TTOptions { Use = true, Size = 32 };

            Game.Table.SetSize(ttOptions.Size);

            void BoardPrint(string s) => Log.Information("Board:\n{0}", s);

            // test parse
            if (useEpd)
            {
                var epdResult = await RunEpd(epdOptions, BoardPrint).ConfigureAwait(false);
                return epdResult;
            }
            else if (fenOptions != null)
            {
                var fenResult = await RunFen(fenOptions, BoardPrint).ConfigureAwait(false);
                return fenResult;
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

        private static async Task<int> RunFen(FenOptions fenOptions, Action<string> boardPrint = null)
        {
            var depths = fenOptions.Depths.Select(d => (d, 0ul)).ToList();

            var perftPositions = new List<IPerftPosition>(fenOptions.Fens.Select(f => PerftPositionFactory.Create(f, depths)));

            var p = Framework.IoC.Resolve<IPerft>();
            var errors = 0;

            foreach (var pp in perftPositions)
            {
                p.SetGamePosition(pp);
                boardPrint?.Invoke(p.GetBoard());
                log.Information("Fen         : {0}", pp.Fen);
                log.Information(Line);

                foreach (var (d, e) in pp.Value)
                {
                    log.Information("Depth       : {0}", d);
                    var sw = Stopwatch.StartNew();
                    var result = await p.DoPerftAsync(d).ConfigureAwait(false);
                    sw.Stop();

                    // add 1 to avoid potential dbz
                    var elapsedMs = sw.ElapsedMilliseconds + 1;
                    var nps = 1000 * result / (ulong)elapsedMs;
                    log.Information("Time passed : {0}", elapsedMs);
                    log.Information("Nps         : {0}", nps);
                    log.Information("Result      : {0} - should be {1}", result, e);
                    log.Information("TT hits     : {0}", Game.Table.Hits);
                    if (e == result)
                        log.Information("Move count matches!");
                    else
                    {
                        log.Error("Move count failed!");
                        errors++;
                    }
                    Console.WriteLine();
                }

                Game.Table.Clear();

            }

            return 0;
        }

        private static async Task<int> RunEpd(EpdOptions options, Action<string> boardPrint = null)
        {
            //var r = new PerftRunner2();
            //await r.Run(options, boardPrint).ConfigureAwait(false);

            //var r = new PerftRunner();
            //r.Initialize();
            //await r.Run(options);

            //return 0;

            var parser = Framework.IoC.Resolve<IEpdParser>();
            parser.Settings.Filename = options.Epds.First();
            var sw = Stopwatch.StartNew();

            var parsedCount = await parser.ParseAsync().ConfigureAwait(false);

            sw.Stop();
            var elapsedMs = sw.ElapsedMilliseconds;
            log.Information("Parsed {0} epd entries in {1} ms", parsedCount, elapsedMs);

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

            //Log.Information("EPD parsing complete. Encountered {0} errors.", errors);

            return 0;
            //return errors;
        }

        private static void BuildConfiguration(IConfigurationBuilder builder)
        {
            // Read the configuration file for this assembly
            builder.SetBasePath(Directory.GetCurrentDirectory())
                //.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("ScannerName", scannerName) })
                .AddJsonFile(ConfigFileName);
        }

        public static void AddServices(IContainer container, IConfiguration configuration)
        {
            // Bind logger with configuration
            container.Register(made: Made.Of(() => ConfigureLogger(configuration)), reuse: Reuse.Singleton);

            // Bind build time stamp class
            container.Register<IBuildTimeStamp, BuildTimeStamp>(reuse: Reuse.Singleton);

            // Bind chess classes
            container.Register<IGame, Game>(reuse: Reuse.Transient);
            container.Register<IMoveList, MoveList>(reuse: Reuse.Transient);
            container.Register<IMaterial, Material>(reuse: Reuse.Transient);
            container.Register<IPosition, Position>(reuse: Reuse.Transient);
            container.Register<IKillerMoves, KillerMoves>(reuse: Reuse.Transient);

            // Bind chess perft classes
            container.Register<IPerftPosition, PerftPosition>(reuse: Reuse.Transient);
            container.Register<IPerft, Perft>(reuse: Reuse.Transient);

            // Bind perft classes
            container.Register<IEpdParserSettings, EpdParserSettings>(reuse: Reuse.Singleton);
            container.Register<IEpdSet, EpdSet>(reuse: Reuse.Transient);
            container.Register<IEpdParser, EpdParser>(reuse: Reuse.Singleton);

            // Bind a connection string
            //container.Register(made: Made.Of(() => ConnectionFactory.CreateConnection(configuration)), reuse: Reuse.Singleton);

            //container.Register<IFileSystemScannerDatabase, FileSystemScannerDatabase>();

            //// Add other singletons services
            //container.Register<IExceptionFormatter, ExceptionFormatter>(reuse: Reuse.Singleton);
            //container.Register<IDisk, Disk>(reuse: Reuse.Singleton);
            //container.Register<IScanner, Scanner>(reuse: Reuse.Transient);

            //// Add other transient services
            //container.Register<ISqlScriptIterator, SqlScriptIterator>(reuse: Reuse.Transient);
            //container.Register<IStatistics, Statistics>(reuse: Reuse.Transient);
            //container.Register<IBatch, Batch>(reuse: Reuse.Transient);
            //container.Register<IPropertyType, PropertyType>(reuse: Reuse.Transient);

            //// TODO : Re-code the entirety to use a more subtle IoC
            //// Register services which requires special care (for now).
            //// Note that when resolving these, the helper method object.ToParams() can be used
            //container.Register<IFileScannerBranchConfig, FileScannerBranchConfig>
            //(
            //    made: Made.Of(() => new FileScannerBranchConfig
            //        (
            //        Arg.Of(0, IfUnresolved.Throw),
            //        Arg.Of(0, IfUnresolved.Throw),
            //        Arg.Of<string>(null, IfUnresolved.ReturnDefault),
            //        Arg.Of<string>(null, IfUnresolved.ReturnDefault),
            //        Arg.Of<string>(null, IfUnresolved.ReturnDefault)
            //        )
            //    ),
            //    reuse: Reuse.Transient
            //);

            //container.Register<IBranch, Branch>
            //(
            //    //made: Made.Of(() => new Branch
            //    //    (
            //    //    Arg.Of<IFileSystemScannerDatabase>(IfUnresolved.Throw),
            //    //    Arg.Of<IFileScannerBranchConfig>(IfUnresolved.Throw)
            //    //    )
            //    //),
            //    reuse: Reuse.Transient
            //);

            //container.Register<IPoint, Point>
            //(
            //    made: Made.Of(() => new Point
            //        (
            //            Arg.Of<int>(IfUnresolved.Throw),
            //            Arg.Of<string>(null, IfUnresolved.Throw),
            //            Arg.Of<string>(null, IfUnresolved.Throw),
            //            Arg.Of(false, IfUnresolved.Throw)
            //        )
            //    ),
            //    reuse: Reuse.Transient
            //);

            //// Register PropertyReader services
            //container.Register<IPropertyReader, AccessControlPropertyReader>(serviceKey: "Access Control", reuse: Reuse.Transient);
            //container.Register<IPropertyReader, ExtendedPropertyReader>(serviceKey: "Extended", reuse: Reuse.Transient);
            //container.Register<IPropertyReader, OpenXmlPropertyReader>(serviceKey: "OpenXML", reuse: Reuse.Transient);

            //// Register calculated properties
            ////int propertyTypeID, string type, string value
            //container.Register<ICalculatedProperty, ConstantCalculatedProperty>
            //(
            //    made: Made.Of(() => new ConstantCalculatedProperty
            //        (
            //            Arg.Of<int>(IfUnresolved.Throw),
            //            Arg.Of<string>(null, IfUnresolved.Throw)
            //        )
            //    ),
            //    reuse: Reuse.Transient,
            //    serviceKey: CalculatedPropertyType.Constant
            //);

            //container.Register<ICalculatedProperty, PropertyCalculatedProperty>
            //(
            //    made: Made.Of(() => new PropertyCalculatedProperty
            //        (
            //            Arg.Of<int>(IfUnresolved.Throw),
            //            Arg.Of<string>(null, IfUnresolved.Throw)
            //        )
            //    ),
            //    reuse: Reuse.Transient,
            //    serviceKey: CalculatedPropertyType.Property
            //);

            //container.Register<ICalculatedProperty, RootDirectoryCalculatedProperty>
            //(
            //    made: Made.Of(() => new RootDirectoryCalculatedProperty
            //        (
            //            Arg.Of<int>(IfUnresolved.Throw),
            //            Arg.Of<string>(null, IfUnresolved.Throw)
            //        )
            //    ),
            //    reuse: Reuse.Transient,
            //    serviceKey: CalculatedPropertyType.RootDirectoryName
            //);

            //// single instance
            //container.Register<ISingleApplicationSemaphore, SingleApplicationSemaphore>(reuse: Reuse.Singleton);

            // property
            //container.Register<IProperty, Property>
            //(
            //    made:Made.Of(() => PropertyFactory.Create(
            //        Arg.Of<ICalculatedProperty>(IfUnresolved.Throw),
            //        Arg.Of<IFileSystemInfoExtended>(IfUnresolved.ReturnDefault),
            //        Arg.Of<IDictionary<int, string>>(IfUnresolved.ReturnDefault)
            //    )),
            //    reuse: Reuse.Transient
            //);

            // propertiesconfiguration
            //container.Register<IPropertiesConfiguration, PropertiesConfiguration>
            //(
            //    made: Made.Of(() => PropertiesConfigurationFactory.Create(
            //        Arg.Of<IFileSystemInfoExtended>(IfUnresolved.Throw),
            //        Arg.Of<List<IPropertyReader>>(IfUnresolved.ReturnDefault)
            //    ))
            //);
        }

        private static ILogger ConfigureLogger(IConfiguration configuration)
        {
            // Apply the config to the logger
            Log.Logger = new LoggerConfiguration()
                //.WriteTo.EventLog("File System Scanner", manageEventSource: true, restrictedToMinimumLevel: LogEventLevel.Error)
                .ReadFrom.Configuration(configuration)
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
            return Log.Logger;
        }

        private static void PrintData(PerftPrintData data)
        {
        }
    }
}