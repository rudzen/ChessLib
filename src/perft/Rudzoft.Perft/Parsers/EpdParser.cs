﻿/*
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

using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Perft.Interfaces;

namespace Rudzoft.Perft.Parsers;

/// <summary>
/// Fast epd file parser
/// </summary>
public sealed class EpdParser : IEpdParser
{
    public async IAsyncEnumerable<IEpdSet> Parse(string epdFile)
    {
        const char space = ' ';
        await using var fs = File.Open(epdFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var bs = new BufferedStream(fs);
        using var sr = new StreamReader(bs);
        var id = string.Empty;
        var epd = string.Empty;
        var idSet = false;
        var epdSet = false;
        var perftData = new List<string>(16);
        while (await sr.ReadLineAsync().ConfigureAwait(ConfigureAwaitOptions.None) is { } s)
        {
            // skip comments
            if (s.Length < 4 || s[0] == '#')
            {
                if (idSet && epdSet)
                {
                    yield return new EpdSet { Epd = epd, Id = id, Perft = perftData.Select(ParsePerftLines).ToList() };
                    id = epd = string.Empty;
                    idSet = epdSet = false;
                    perftData.Clear();
                }

                continue;
            }

            if (!idSet && s[0] == 'i' && s[1] == 'd')
            {
                id = s.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                idSet = true;
                continue;
            }

            if (s[0] == 'e' && s[1] == 'p' && s[2] == 'd')
            {
                var firstSpace = s.IndexOf(space);
                epd = s[firstSpace..].TrimStart();
                epdSet = true;
                continue;
            }

            if (s.StartsWith("perft"))
            {
                var firstSpace = s.IndexOf(space);
                perftData.Add(s[firstSpace..]);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PerftPositionValue ParsePerftLines(string perftData)
    {
        var s = perftData.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        Maths.ToIntegral(s[0], out int depth);
        Maths.ToIntegral(s[1], out ulong count);
        return new(depth, count);
    }
}