using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Perft;
using Rudzoft.ChessLib.Perft.Interfaces;
using Rudzoft.Perft.Models;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Services;
using Serilog;

var host = new HostBuilder()
    .ConfigureServices((_, services) =>
    {
        var configuration = ConfigurationBuilder().Build();
        services.AddSingleton(configuration);
        services.AddSingleton(new CommandLineArgs(args));

        services.AddChessLib(configuration);

        services.AddSingleton(ConfigureLogger(configuration));

        services.AddTransient<IPerft, Perft>();
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

        services.AddHostedService<PerftService>();
    })
    .Build();

host.Run();
return;

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
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false);
}
