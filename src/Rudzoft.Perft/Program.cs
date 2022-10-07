/*
Perft, a chess perft testing application

MIT License

Copyright (c) 2019-2020 Rudy Alex Kohn

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
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Perft;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Tables;
using Rudzoft.ChessLib.Types;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.TimeStamp;
using Serilog;

namespace Rudzoft.Perft;

/*
 *
 *    TODO: Fens to investigate:
 * r2n3r/1bNk2pp/6P1/pP3p2/3pPqnP/1P1P1p1R/2P3B1/Q1B1bKN1 b - e3
 *
 *
 *
 */

internal static class Program
{
    private const string ConfigFileName = "appsettings.json";

    static Program()
    {
        Framework.Startup(BuildConfiguration, AddServices);
    }

    public static async Task<int> Main(string[] args)
    {
        var optionsUsed = OptionType.None;
        IOptions options = null;
        IOptions ttOptions = null;

        var setEdp = new Func<EpdOptions, int>(o =>
        {
            optionsUsed |= OptionType.EdpOptions;
            options = o;
            return 0;
        });

        var setFen = new Func<FenOptions, int>(o =>
        {
            optionsUsed |= OptionType.FenOptions;
            options = o;
            return 0;
        });

        var setTT = new Func<TTOptions, int>(o =>
        {
            optionsUsed |= OptionType.TTOptions;
            ttOptions = o;
            return 0;
        });

        /*
         * fens -f "rnkq1bnr/p3ppp1/1ppp3p/3B4/6b1/2PQ3P/PP1PPP2/RNB1K1NR w KQ -" -d 6
         * fens -f "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1" -d 6
         * epd -f D:\perft-random.epd
         */

        var returnValue = Parser.Default.ParseArguments<EpdOptions, FenOptions, TTOptions>(args)
            .MapResult(
                (EpdOptions opts) => setEdp(opts),
                (FenOptions opts) => setFen(opts),
                (TTOptions opts) => setTT(opts),
                static errs => 1);

        if (returnValue != 0)
            return returnValue;

        var perftRunner = Framework.IoC.Resolve<IPerftRunner>();
        perftRunner.Options = options;
        perftRunner.SaveResults = true;

        return await perftRunner.Run().ConfigureAwait(false);
    }

    private static void BuildConfiguration(IConfigurationBuilder builder)
    {
        // Read the configuration file for this assembly
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigFileName);
    }

    private static void AddServices(IContainer container, IConfiguration configuration)
    {
        // Bind logger with configuration
        container.Register(Made.Of(() => ConfigureLogger(configuration)), Reuse.Singleton);

        // Bind build time stamp class
        container.Register<IBuildTimeStamp, BuildTimeStamp>(Reuse.Singleton);

        // Bind chess classes
        container.Register<IGame, Game>(Reuse.Transient);
        container.Register<IBoard, Board>(Reuse.Singleton);
        container.Register<IPosition, Position>(Reuse.Transient);
        container.Register<IKillerMoves, KillerMoves>(Reuse.Transient);
        container.Register<IUci, Uci>(Reuse.Singleton);
        container.Register<IPieceValue, PieceValue>(Reuse.Singleton);

        // Bind options
        container.Register<IOptions, EpdOptions>(Reuse.Singleton, serviceKey: OptionType.EdpOptions);
        container.Register<IOptions, FenOptions>(Reuse.Singleton, serviceKey: OptionType.FenOptions);
        container.Register<IOptions, TTOptions>(Reuse.Singleton, serviceKey: OptionType.TTOptions);

        // Bind chess perft classes
        container.Register<IPerft, ChessLib.Perft.Perft>(Reuse.Transient);
        container.Register<IPerftRunner, PerftRunner>(Reuse.Transient);

        // Bind perft classes
        container.Register<IEpdParserSettings, EpdParserSettings>(Reuse.Singleton);
        container.Register<IEpdSet, EpdSet>(Reuse.Transient);
        container.Register<IEpdParser, EpdParser>(Reuse.Singleton);

        // Bind object pool for perft result
        container.Register<ObjectPoolProvider, DefaultObjectPoolProvider>(Reuse.Singleton);
        container.RegisterDelegate(context =>
        {
            var provider = context.Resolve<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<PerftResult>();
            return provider.Create(policy);
        });
    }

    private static ILogger ConfigureLogger(IConfiguration configuration)
    {
        // Apply the config to the logger
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .CreateLogger();
        AppDomain.CurrentDomain.ProcessExit += static (s, e) => Log.CloseAndFlush();
        return Log.Logger;
    }
}