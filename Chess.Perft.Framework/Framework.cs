namespace Perft
{
    using Chess.Perft.Environment;
    using DryIoc;
    using Environment;
    using Factories;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.IO;

    /// <summary>
    /// The main entry point into the Dna Framework library
    /// </summary>
    public static class Framework
    {
        #region Private Members

        #endregion Private Members

        #region Public Properties

        /// <summary>
        /// The main ioc container
        /// </summary>
        public static IContainer IoC { get; private set; }

        /// <summary>
        /// Gets the configuration for the framework environment
        /// </summary>
        public static IConfiguration Configuration => IoC.Resolve<IConfiguration>();

        /// <summary>
        /// Gets the default logger for the framework
        /// </summary>
        public static ILogger Logger => IoC.Resolve<ILogger>();

        /// <summary>
        /// Gets the framework environment of this class
        /// </summary>
        public static IFrameworkEnvironment Environment => IoC.Resolve<IFrameworkEnvironment>();

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Should be called at the very start of your application to configure and setup
        /// the Dna Framework
        /// </summary>
        /// <param name="configure">The action to add custom configurations to the configuration builder</param>
        /// <param name="injection">The action to inject services into the service collection</param>
        public static void Startup(Action<IConfigurationBuilder> configure = null, Action<IContainer, IConfiguration> injection = null)
        {
            // Process passed in module list
            IoC = new Container();

            // Load internal based modules
            // This step is required as they contain information which are used from this point on
            IoC.Register<IFrameworkEnvironment, FrameworkEnvironment>(Reuse.Singleton);

            var configBuilder = ConfigurationBuilder();

            // Let custom configuration happen
            configure?.Invoke(configBuilder);

            // Construct a configuration binding
            IoC.Register(made: Made.Of(() => ConfigurationFactory.CreateConfiguration(configBuilder)), reuse: Reuse.Singleton);

            // Allow custom service injection
            injection?.Invoke(IoC, Configuration);

            // Log the startup complete
            Logger.Information("Perft Framework started in {0}...", Environment.Configuration);
        }

        #endregion Public Methods

        #region Private ioc helper methods

        private static IConfigurationBuilder ConfigurationBuilder()
        {
            // Create our configuration sources
            return new ConfigurationBuilder()
                // Add environment variables
                .AddEnvironmentVariables()
                // Set base path for Json files as the startup location of the application
                .SetBasePath(Directory.GetCurrentDirectory())
                // Add application settings json files
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.Configuration}.json", optional: true, reloadOnChange: true);
        }

        #endregion Private ioc helper methods
    }
}