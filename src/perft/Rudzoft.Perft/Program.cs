using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.Perft.Actors;
using Rudzoft.Perft.Models;
using Rudzoft.Perft.Parsers;
using Rudzoft.Perft.Services;
using Rudzoft.Perft.Settings.Extensions;
using Serilog;

var host = new HostBuilder()
           .ConfigureAppConfiguration(Configure)
           .ConfigureServices((context, services) =>
           {
               services.RegisterEpdSettings();
               services.RegisterFenSettings();
               services.RegisterTranspositionTableSettings();
               services.RegisterPolyglotBookSettings();

               services.AddChessLib(context.Configuration);

               services.AddSingleton(ConfigureLogger(context.Configuration));

               services.AddTransient<IPerft, Perft>();
               services.AddTransient<IPerftRunner, PerftRunner>();

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

               services.AddAkka("perft-system", (builder, sp) =>
               {
                   // const string postgresql
                   //     = "Host=localhost; Database=chesslib-perft; Username=postgres; Password=rudz; Include Error Detail=true";
                   // builder.WithPostgreSqlPersistence(postgresql, autoInitialize: true);

                   builder.WithActors((system, registry) =>
                   {
                       var props = Props.Create<PerftActor>(sp);
                       var perftActor = system.ActorOf(props, "perft-actor");
                       registry.Register<PerftActor>(perftActor);
                   });
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

static void Configure(IConfigurationBuilder builder)
{
#if RELEASE
    const string envName = "Production";
#else
    const string envName = "Development";
#endif

    // Add environment variables
    builder.AddEnvironmentVariables()
           // Set base path for Json files as the startup location of the application
           .SetBasePath(Directory.GetCurrentDirectory())
           // Add application settings json files
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
           .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false);
}