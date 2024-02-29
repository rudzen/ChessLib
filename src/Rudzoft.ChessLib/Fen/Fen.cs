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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Fen;

public static class Fen
{
    public const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public const int MaxFenLen = 128;

    private const string FenRankRegexSnippet = "[1-8KkQqRrBbNnPp]{1,8}";

    private const string ValidChars = "012345678pPnNbBrRqQkK/ w-abcdefgh";

    private const char Space = ' ';

    private const int SpaceCount = 3;

    private const char Separator = '/';

    private const int SeparatorCount = 7;

    private static readonly Regex ValidFenRegex = new(
        string.Format(
            """^ \s* {0}/{0}/{0}/{0}/{0}/{0}/{0}/{0} \s+ (?:w|b) \s+ (?:[KkQq]+|\-) \s+ (?:[a-h][1-8]|\-) \s+ \d+ \s+ \d+ \s* $""",
            FenRankRegexSnippet),
        RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline |
        RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture);

    /// <summary>
    /// Performs basic validation of FEN string.
    /// Requirements for a valid FEN string is:
    /// - Must not be null
    /// - No more than 128 chars in length
    /// - Must have a valid count of spaces and backslashes
    /// </summary>
    /// <param name="fen">The FEN string to validate</param>
    /// <returns>true if all requirements are met, otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Validate(string fen)
    {
        var f = fen.AsSpan().Trim();

        if (f.IsEmpty)
            throw new InvalidFenException("Fen is empty.");

        var invalidCharIndex = f.IndexOfAnyExcept(ValidChars.AsSpan());

        if (invalidCharIndex > -1)
            throw new InvalidFenException($"Invalid char detected in fen. fen={f}, pos={invalidCharIndex}");

        if (f.Length >= MaxFenLen)
            throw new InvalidFenException($"Invalid length for fen {fen}.");

        if (!ValidFenRegex.IsMatch(fen))
            throw new InvalidFenException($"Invalid format for fen {fen}.");

        CountValidity(f);
        if (!CountPieceValidity(f))
            throw new InvalidFenException($"Invalid piece validity for fen {fen}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDelimiter(char c) => c == Space;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CountPieceValidity(ReadOnlySpan<char> s)
    {
        var spaceIndex = s.IndexOf(Space);
        if (spaceIndex == -1)
            throw new InvalidFenException($"Invalid fen {s.ToString()}");

        var mainSection = s[..spaceIndex];

        Span<int> limits = stackalloc int[] { 32, 8, 10, 10, 10, 9, 1 };

        // piece count storage, using index 0 = '/' count
        Span<int> pieceCount = stackalloc int[Pieces.PieceNb.AsInt()];
        pieceCount.Clear();

        ref var mainSectionSpace = ref MemoryMarshal.GetReference(mainSection);

        for (var i = 0; i < mainSection.Length; ++i)
        {
            var t = Unsafe.Add(ref mainSectionSpace, i);
            if (t == '/')
            {
                if (++pieceCount[0] > SeparatorCount)
                    throw new InvalidFenException($"Invalid fen (too many separators) {s.ToString()}");

                continue;
            }

            if (char.IsNumber(t))
            {
                if (!char.IsBetween(t, '1', '8'))
                    throw new InvalidFenException($"Invalid fen (not a valid square jump) {s.ToString()}");

                continue;
            }

            var pieceIndex = PieceExtensions.PieceChars.IndexOf(t);

            if (pieceIndex == -1)
                throw new InvalidFenException($"Invalid fen (unknown piece) {s.ToString()}");

            var pc = new Piece((Pieces)pieceIndex);
            var pt = pc.Type();

            pieceCount[pc]++;

            var limit = limits[pt.AsInt()];

            if (pieceCount[pc] > limit)
                throw new InvalidFenException(
                    $"Invalid fen (piece limit exceeded for {pc}. index={i},limit={limit},count={pieceCount[pc]}) {s.ToString()}");
        }

        var whitePieces = pieceCount.Slice(1, 5);

        var valid = GetSpanSum(whitePieces, 15);

        if (!valid)
            throw new InvalidFenException($"Invalid fen (white piece count exceeds limit) {s.ToString()}");

        var blackPieces = pieceCount.Slice(9, 5);

        valid = GetSpanSum(blackPieces, 15);

        if (!valid)
            throw new InvalidFenException($"Invalid fen (black piece count exceeds limit) {s.ToString()}");

        spaceIndex = s.LastIndexOf(' ');
        var endSection = s[spaceIndex..];

        if (Maths.ToIntegral(endSection) >= 2048)
            throw new InvalidFenException($"Invalid half move count for fen {s.ToString()}");

        return true;

        // check for summed up values
        static bool GetSpanSum(ReadOnlySpan<int> span, int limit)
        {
            var sum = 0;

            ref var spanSpace = ref MemoryMarshal.GetReference(span);
            for (var i = 0; i < span.Length; ++i)
                sum += Unsafe.Add(ref spanSpace, i);

            return sum <= limit;
        }
    }

    /// <summary>
    /// Validate char counts of space and backslash.
    /// FEN strings must have 5 space and 7 slashes to be considered valid format.
    /// </summary>
    /// <param name="str">The fen string to check</param>
    /// <returns>true if format seems ok, otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CountValidity(ReadOnlySpan<char> str)
    {
        var (spaceCount, separatorCount) = CountSpacesAndSeparators(str);

        var valid = spaceCount >= SpaceCount;

        if (!valid)
            throw new InvalidFenException($"Invalid space character count in fen {str.ToString()}");

        valid = separatorCount == SeparatorCount;

        if (!valid)
            throw new InvalidFenException($"Invalid separator count in fen {str.ToString()}");
    }

    private static (int, int) CountSpacesAndSeparators(ReadOnlySpan<char> str)
    {
        var result = (spaceCount: 0, separatorCount: 0);

        ref var strSpace = ref MemoryMarshal.GetReference(str);
        for (var i = 0; i < str.Length; ++i)
        {
            var c = Unsafe.Add(ref strSpace, i);
            switch (c)
            {
                case Separator:
                    result.separatorCount++;
                    break;

                case Space:
                    result.spaceCount++;
                    break;
            }
        }

        return result;
    }
}