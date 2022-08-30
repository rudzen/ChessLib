/*
Perft, a chess perft test library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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
using DryIoc;
using Microsoft.Extensions.Configuration;
using Perft.Environment;
using Perft.Factories;
using Serilog;

namespace Perft;

/// <summary>
/// The main entry point into the Dna Framework library
/// </summary>
public static class Framework
{
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
    public static void Startup(Action<IConfigurationBuilder> configure = null,
        Action<IContainer, IConfiguration> injection = null)
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
        IoC.Register(made: Made.Of(() => ConfigurationFactory.CreateConfiguration(configBuilder)),
            reuse: Reuse.Singleton);

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