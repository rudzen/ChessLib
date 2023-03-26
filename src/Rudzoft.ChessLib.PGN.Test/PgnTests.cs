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

using Microsoft.Extensions.DependencyInjection;

namespace Rudzoft.ChessLib.PGN.Test;

public sealed class PgnTests
{
    private const string SampleFilePath = @"samples/sample.pgn";
    private const int ExpectedGameCount = 2;

    private readonly IServiceProvider _serviceProvider;

    public PgnTests()
    {
        _serviceProvider = new ServiceCollection()
                .AddPgnParser()
                .BuildServiceProvider();
    }
    
    [Fact]
    public async Task ParseFile_WithTestContent_ReturnsCorrectGamesAndMoves()
    {
        var parser = _serviceProvider.GetRequiredService<IPgnParser>();
        
        var games = new List<PgnGame>();

        await foreach (var game in parser.ParseFile(SampleFilePath))
            games.Add(game);
        
        Assert.Equal(ExpectedGameCount, games.Count);

        var game1 = games[0];
        Assert.Equal("London", game1.Tags["Event"]);
        Assert.Equal(44, game1.Moves.Count);
        Assert.Equal("e4", game1.Moves[0].WhiteMove);
        Assert.Equal("e5", game1.Moves[0].BlackMove);

        var game2 = games[1];
        Assert.Equal("Reykjavik", game2.Tags["Event"]);
        Assert.Equal(28, game2.Moves.Count);
        Assert.Equal("e4", game2.Moves[0].WhiteMove);
        Assert.Equal("c5", game2.Moves[0].BlackMove);
    }
}
