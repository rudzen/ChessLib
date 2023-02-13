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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Perft.Environment;
using Rudzoft.Perft.Options;
using Rudzoft.Perft.Perft;
using Serilog;

namespace Rudzoft.Perft.Host;

public sealed class PerftHost : IHostedService
{
    private readonly IPerftRunner _perftRunner;
    private readonly ILogger _logger;

    public PerftHost(
        IOptionsFactory optionsFactory,
        IPerftRunner perftRunner,
        IFrameworkEnvironment environment,
        ILogger logger)
    {
        _perftRunner = perftRunner;
        _logger = logger;
        foreach (var option in optionsFactory.Parse())
        {
            if (option.Type == OptionType.TTOptions)
                _perftRunner.TranspositionTableOptions = option.PerftOptions;
            else
                _perftRunner.Options = option.PerftOptions;
        }
        logger.Information("Perft Framework started in {0}...", environment.Configuration);

    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var errors = await _perftRunner.Run(cancellationToken);
        if (errors != 0)
            _logger.Error("Total errors: {0}", errors);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Stopped");
        return Task.CompletedTask;
    }
}