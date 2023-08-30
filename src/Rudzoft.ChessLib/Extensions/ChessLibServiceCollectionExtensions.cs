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
using Rudzoft.ChessLib.Evaluation;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Hash.Tables.Transposition;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.ObjectPoolPolicies;
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
        if (serviceCollection == null)
            throw new ArgumentNullException(nameof(serviceCollection));

        if (configuration == null)
        {
            configuration = LoadConfiguration(configurationFile);
            serviceCollection.AddSingleton(configuration);
        }

        // TODO : Add configuration "manually" to avoid IL2026 warning
        serviceCollection.AddOptions<TranspositionTableConfiguration>().Configure<IConfiguration>(
            static (settings, configuration)
                => configuration
                    .GetSection(TranspositionTableConfiguration.Section)
                    .Bind(settings));

        // TODO : Add configuration "manually" to avoid IL2026 warning
        serviceCollection.AddOptions<PolyglotBookConfiguration>().Configure<IConfiguration>(
            static (settings, configuration)
                => configuration
                    .GetSection(PolyglotBookConfiguration.Section)
                    .Bind(settings));

        serviceCollection.TryAddSingleton<ITranspositionTable, TranspositionTable>();
        serviceCollection.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        serviceCollection.TryAddSingleton(static serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            var policy = new MoveListPolicy();
            return provider.Create(policy);
        });

        return serviceCollection.AddSingleton(static sp =>
            {
                var pool = sp.GetRequiredService<ObjectPool<IMoveList>>();
                IUci uci = new Uci(pool);
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
            .AddSingleton<ICpu, Cpu>();
    }

    private static IConfigurationRoot LoadConfiguration(string file)
    {
        var configurationFile = string.IsNullOrWhiteSpace(file) ? "chesslib.json" : file;
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
}