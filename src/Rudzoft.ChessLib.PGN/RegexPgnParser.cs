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

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Rudzoft.ChessLib.PGN;

public sealed partial class RegexPgnParser : IPgnParser
{
    private const int DefaultTagCapacity = 7;
    private const int DefaultMoveListCapacity = 24;

    [GeneratedRegex(@"\[(?<tagName>\w+)\s+""(?<tagValue>[^""]+)""\]",  RegexOptions.NonBacktracking)]
    private static partial Regex TagPairRegex();

    [GeneratedRegex(@"(?<moveNumber>\d+)\.\s*(?<whiteMove>\S+)(\s+(?<blackMove>\S+))?",  RegexOptions.NonBacktracking)]
    private static partial Regex MoveTextRegex();

    private static readonly PgnMove EmptyPgnMove = new(0, string.Empty, string.Empty);

    public async IAsyncEnumerable<PgnGame> ParseFile(
        string pgnFile,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pgnFile))
            yield break;

        await using var fileStream = new FileStream(pgnFile, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(fileStream);
        var currentGameTags = new Dictionary<string, string>(DefaultTagCapacity);
        var currentGameMoves = new List<PgnMove>(DefaultMoveListCapacity);
        var inMoveSection = false;

        var line = await streamReader.ReadLineAsync(cancellationToken);

        while (line != null)
        {
            if (line.AsSpan().IsWhiteSpace())
                inMoveSection = currentGameTags.Count > 0;
            else if (inMoveSection)
            {
                var lineMoves = ParseMovesLine(line);

                currentGameMoves.AddRange(lineMoves);

                if (IsPgnGameResult(line))
                {
                    yield return new(currentGameTags, currentGameMoves);
                    currentGameTags = new(DefaultTagCapacity);
                    currentGameMoves = new(DefaultMoveListCapacity);
                    inMoveSection = false;
                }
            }
            else
                ParseTag(line, currentGameTags);

            line = await streamReader.ReadLineAsync(cancellationToken);
        }

        if (currentGameTags.Count > 0 && currentGameMoves.Count > 0)
            yield return new(currentGameTags, currentGameMoves);
    }

    private static bool IsPgnGameResult(ReadOnlySpan<char> line)
    {
        var trimmedLine = line.TrimEnd();
        
        if (trimmedLine.EndsWith(stackalloc char[] { '*' }, StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (trimmedLine.EndsWith(stackalloc char[] { '1', '-', '0' }, StringComparison.InvariantCultureIgnoreCase))
            return true;

        if (trimmedLine.EndsWith(stackalloc char[] { '0', '-', '1' }, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return trimmedLine.EndsWith(stackalloc char[] { '1', '/', '2', '-', '1', '/', '2' },
            StringComparison.InvariantCultureIgnoreCase);
    }

    private static void ParseTag(string line, IDictionary<string, string> currentGameTags)
    {
        var tagMatch = TagPairRegex().Match(line);

        if (!tagMatch.Success)
            return;

        var tagName = tagMatch.Groups["tagName"].Value;
        var tagValue = tagMatch.Groups["tagValue"].Value;
        currentGameTags[tagName] = tagValue;
    }

    private static IEnumerable<PgnMove> ParseMovesLine(string line)
    {
        var moveMatches = MoveTextRegex().Matches(line);
        return moveMatches.Select(static x =>
        {
            var moveNumber = int.Parse(x.Groups["moveNumber"].Value);
            var whiteMove = x.Groups["whiteMove"].Value;
            var blackMove = x.Groups["blackMove"].Value;

            if (string.IsNullOrWhiteSpace(blackMove))
                return EmptyPgnMove with { MoveNumber = moveNumber, WhiteMove = whiteMove };

            return new(moveNumber, whiteMove, blackMove);
        });
    }
}