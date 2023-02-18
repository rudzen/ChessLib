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

using System.IO;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Perft.Environment;
using Perft.Factories;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Tables;
using Rudzoft.ChessLib.Types;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Perft;
using Rudzoft.Perft.TimeStamp;
using Serilog;

namespace Rudzoft.Perft.Host;

public class PerftServiceProviderFactory : IServiceProviderFactory<Container>
{
    public Container CreateBuilder(IServiceCollection services)
    {
        var configBuilder = ConfigurationBuilder();
        var configuration = ConfigurationFactory.CreateConfiguration(configBuilder);
        
        var container = new Container(Rules.MicrosoftDependencyInjectionRules);
        
        // Construct a configuration binding
        container.RegisterInstance(configuration);

        AddServices(container, configuration);

        container.Populate(services);
        
        return container;
    }

    public IServiceProvider CreateServiceProvider(Container container)
    {
        return container.BuildServiceProvider();
    }

    private static IConfigurationBuilder ConfigurationBuilder()
    {
#if RELEASE
        const string envName = "Production";
#else
        const string envName = "Development";
#endif
        // Create our configuration sources
        return new ConfigurationBuilder()
            // Add environment variables
            .AddEnvironmentVariables()
            // Set base path for Json files as the startup location of the application
            .SetBasePath(Directory.GetCurrentDirectory())
            // Add application settings json files
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);
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
        container.Register(made: Made.Of(static () => KillerMoves.Create(64)), Reuse.Transient);
        container.Register<IUci, Uci>(Reuse.Singleton);
        container.Register<IValues, Values>(Reuse.Singleton);

        // Bind chess perft classes
        container.Register<IFrameworkEnvironment, FrameworkEnvironment>(Reuse.Singleton);
        container.Register<IPerft, ChessLib.Perft.Perft>(Reuse.Transient);
        container.Register<IPerftRunner, PerftRunner>(Reuse.Transient);
        container.Register<IOptionsFactory, OptionsFactory>(Reuse.Singleton);
        
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
        AppDomain.CurrentDomain.ProcessExit += static (_, _) => Log.CloseAndFlush();
        return Log.Logger;
    }
}