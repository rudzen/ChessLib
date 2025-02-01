using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rudzoft.Perft.Settings.Settings;

namespace Rudzoft.Perft.Settings.Extensions;

public static class SettingsServiceCollectionExtensions
{
    public static IServiceCollection RegisterEpdSettings(this IServiceCollection sc)
    {
        ArgumentNullException.ThrowIfNull(sc);

        sc.AddOptions<EpdSettings>().BindConfiguration(EpdSettings.RootName);
        sc.AddSingleton(sp => sp.GetRequiredService<IOptions<EpdSettings>>().Value);

        return sc;
    }

    public static IServiceCollection RegisterFenSettings(this IServiceCollection sc)
    {
        ArgumentNullException.ThrowIfNull(sc);

        sc.AddOptions<FenSettings>().BindConfiguration(FenSettings.RootName);
        sc.AddSingleton(sp => sp.GetRequiredService<IOptions<FenSettings>>().Value);

        return sc;
    }

    public static IServiceCollection RegisterTranspositionTableSettings(this IServiceCollection sc)
    {
        ArgumentNullException.ThrowIfNull(sc);

        sc.AddOptions<TranspositionTableSettings>().BindConfiguration(TranspositionTableSettings.RootName);
        sc.AddSingleton(sp => sp.GetRequiredService<IOptions<TranspositionTableSettings>>().Value);

        return sc;
    }

    public static IServiceCollection RegisterPolyglotBookSettings(this IServiceCollection sc)
    {
        ArgumentNullException.ThrowIfNull(sc);

        sc.AddOptions<PolyglotBookSettings>().BindConfiguration(PolyglotBookSettings.RootName);
        sc.AddSingleton(sp => sp.GetRequiredService<IOptions<PolyglotBookSettings>>().Value);

        return sc;
    }
}