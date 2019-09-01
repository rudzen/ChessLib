namespace Perft.Factories
{
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationFactory
    {
        public static IConfiguration CreateConfiguration(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Build();
        }
    }
}