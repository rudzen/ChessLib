/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Rudzoft.ChessLib.Evaluation;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Polyglot;
using Rudzoft.ChessLib.Protocol.UCI;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib.Extensions;

public static class ChessLibServiceCollectionExtensions
{
    public static IServiceCollection AddChessLib(
        this IServiceCollection serviceCollection,
        IConfiguration configuration = null,
        string configurationFile = null)
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);

        if (configuration == null)
        {
            configuration = LoadConfiguration(configurationFile);
            serviceCollection.AddSingleton(configuration);
        }

        serviceCollection.BindConfigurations(configuration);

        serviceCollection.TryAddSingleton<ITranspositionTable, TranspositionTable>();
        serviceCollection.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        serviceCollection.TryAddSingleton(static serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new DefaultPooledObjectPolicy<MoveList>();
            return provider.Create(policy);
        });

        return serviceCollection
               .AddSingleton(static sp =>
               {
                   var  pool = sp.GetRequiredService<ObjectPool<MoveList>>();
                   IUci uci  = new Uci(pool);
                   uci.Initialize();
                   return uci;
               })
               .AddSingleton<ICuckoo, Cuckoo>()
               .AddSingleton<IRKiss, RKiss>()
               .AddSingleton<IZobrist, Zobrist>()
               .AddTransient<IKillerMovesFactory, KillerMovesFactory>()
               .AddSingleton<ISearchParameters, SearchParameters>()
               .AddSingleton<IValues, Values>()
               .AddSingleton<IKpkBitBase, KpkBitBase>()
               .AddTransient<IBoard, Board>()
               .AddSingleton<IPositionValidator, PositionValidator>()
               .AddTransient<IPosition, Position>()
               .AddTransient<IGame, Game>()
               .AddSingleton<IPolyglotBookFactory, PolyglotBookFactory>()
               .AddSingleton<ICpu, Cpu>()
               .AddNotationServices();
    }

    private static void BindConfigurations(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var transpositionConfigurationSection = configuration.GetSection(TranspositionTableConfiguration.Section);
        serviceCollection.Configure<TranspositionTableConfiguration>(transpositionConfigurationSection);

        var polyglotBookConfiguration = configuration.GetSection(PolyglotBookConfiguration.Section);
        serviceCollection.Configure<PolyglotBookConfiguration>(polyglotBookConfiguration);

        serviceCollection.AddSingleton(provider =>
        {
            var ttOptions = provider.GetRequiredService<IOptions<TranspositionTableConfiguration>>();
            return ttOptions.Value;
        });

        serviceCollection.AddSingleton(provider =>
        {
            var polyOptions = provider.GetRequiredService<IOptions<PolyglotBookConfiguration>>();
            return polyOptions.Value;
        });

    }

    private static IConfigurationRoot LoadConfiguration(string file)
    {
        var configurationFile     = string.IsNullOrWhiteSpace(file) ? "appsettings.json" : file;
        var configurationFilePath = Path.Combine(AppContext.BaseDirectory, configurationFile);
        return new ConfigurationBuilder()
               .AddJsonFile(configurationFilePath, optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();
    }

    public static void AddFactory<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddSingleton<Func<TService>>(static x => () => x.GetService<TService>()!);
        services.AddSingleton<IServiceFactory<TService>, ServiceFactory<TService>>();
    }

    private static IServiceCollection AddNotationServices(this IServiceCollection services)
    {
        return services
               .AddSingleton<INotationToMove, NotationToMove>()
               .AddSingleton<IMoveNotation, MoveNotation>()
               .AddKeyedSingleton<INotation, CoordinateNotation>(MoveNotations.Coordinate)
               .AddKeyedSingleton<INotation, FanNotation>(MoveNotations.Fan)
               .AddKeyedSingleton<INotation, IccfNotation>(MoveNotations.ICCF)
               .AddKeyedSingleton<INotation, LanNotation>(MoveNotations.Lan)
               .AddKeyedSingleton<INotation, RanNotation>(MoveNotations.Ran)
               .AddKeyedSingleton<INotation, SanNotation>(MoveNotations.San)
               .AddKeyedSingleton<INotation, SmithNotation>(MoveNotations.Smith)
               .AddKeyedSingleton<INotation, UciNotation>(MoveNotations.Uci);
    }
}