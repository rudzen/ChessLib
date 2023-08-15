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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Perft.Environment;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.Perft.Host;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Perft;
using Rudzoft.Perft.TimeStamp;
using Serilog;


static IConfigurationBuilder ConfigurationBuilder()
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

static ILogger ConfigureLogger(IConfiguration configuration)
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

var host = new HostBuilder()
    .ConfigureServices((_, services) =>
    {
        var configuration = ConfigurationBuilder().Build();
        services.AddSingleton(configuration);
        services.AddSingleton(new CommandLineArgs(args));

        services.AddChessLib(configuration);

        services.AddSingleton(ConfigureLogger(configuration));

        services.AddSingleton<IBuildTimeStamp, BuildTimeStamp>();
        services.AddSingleton<IFrameworkEnvironment, FrameworkEnvironment>();
        services.AddTransient<IPerft, Rudzoft.ChessLib.Perft.Perft>();
        services.AddTransient<IPerftRunner, PerftRunner>();
        services.AddSingleton<IOptionsFactory, OptionsFactory>();

        services.AddSingleton<IEpdParserSettings, EpdParserSettings>();
        services.AddTransient<IEpdSet, EpdSet>();
        services.AddSingleton<IEpdParser, EpdParser>();

        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<PerftResult>();
            return provider.Create(policy);
        });

        services.AddHostedService<PerftHost>();
    })
    .Build();

host.Run();