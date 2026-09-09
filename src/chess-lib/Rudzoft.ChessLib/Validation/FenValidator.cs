/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2025 Rudy Alex Kohn

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

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Validation;

public static class FenValidator
{
    private const int SeparatorCount = 7;
    private const char Space = ' ';

    private static readonly SearchValues<char> ValidChars = SearchValues.Create("0123456789pPnNbBrRqQkK/ w-abcdefgh");
    private static readonly SearchValues<char> CastlingRights = SearchValues.Create("KQkq-");

    private const string FenRankRegexSnippet = "[1-8KkQqRrBbNnPp]{1,8}";

    private static readonly Regex ValidFenRegex = new(
        string.Format(
            """^ \s* {0}/{0}/{0}/{0}/{0}/{0}/{0}/{0} \s+ (?:w|b) \s+ (?:[KkQq]+|\-) \s+ (?:[a-h][1-8]|\-) \s+ \d+ \s+ \d+ \s* $""",
            FenRankRegexSnippet),
        RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline |
        RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture);

    public static ValidationResult Validate(this IFenData fenData)
    {
        return Validate(fenData.Fen.Span.Trim());
    }

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
    public static ValidationResult Validate(string fen)
    {
        var f = fen.AsSpan().Trim();
        return Validate(f);
    }

    private static ValidationResult Validate(ReadOnlySpan<char> f)
    {
        if (f.IsEmpty)
            throw new InvalidFenException("Fen is empty.");

        var invalidCharIndex = f.IndexOfAnyExcept(ValidChars);

        if (invalidCharIndex > -1)
            return new($"Invalid char detected in fen. fen={f}, pos={invalidCharIndex}");

        if (f.Length >= FenData.MaxFenLen)
            return new($"Invalid length for fen {f}.");

        if (!ValidFenRegex.IsMatch(f))
            return new($"Invalid format for fen {f}.");

        return CountPieceValidity(f);
    }


    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValidationResult CountPieceValidity(ReadOnlySpan<char> s)
    {
        // catches some stuff which are pretty gnarly to catch using the regex

        const int sectionCount = 6;
        const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        Span<Range> ranges = stackalloc Range[sectionCount];

        var sections = s.Split(ranges, Space, splitOptions);

        if (sections != sectionCount)
            return new($"Invalid fen, expected {sectionCount} sections, got {sections} sections. {s.ToString()}");

        var rangePos = 0;
        var range = ranges[rangePos];

        var chunk = s[range];

        var result = ValidateMainSection(chunk);

        if (!result.IsSuccess)
            return result;

        // side to move
        rangePos++;
        range = ranges[rangePos];
        chunk = s[range];

        if (chunk.Length != 1 || (chunk[0] != 'w' && chunk[0] != 'b'))
            return new($"Invalid fen, expected 'w' or 'b' for side to move, got {chunk.ToString()}");

        // castling rights
        rangePos++;
        range = ranges[rangePos];
        chunk = s[range];

        if (chunk.Length is 0 or > 4)
            return new($"Invalid fen, expected castling rights, got {chunk.ToString()}");

        if (chunk.IndexOfAnyExcept(CastlingRights) != -1)
            return new($"Invalid fen, invalid castling rights {chunk.ToString()}");

        // en passant square
        rangePos++;
        range = ranges[rangePos];
        chunk = s[range];
        if (chunk.Length is 0 or > 2)
            return new($"Invalid fen, expected en passant square, got {chunk.ToString()}");

        if (chunk.Length == 2 && !char.IsBetween(chunk[0], 'a', 'h') && !char.IsBetween(chunk[1], '1', '8'))
            return new($"Invalid fen, invalid en passant square {chunk.ToString()}");

        if (chunk.Length == 1 && chunk[0] != '-')
            return new($"Invalid fen, expected '-' for no en passant square, got {chunk.ToString()}");

        // half move clock
        rangePos++;
        range = ranges[rangePos];
        chunk = s[range];

        if (chunk.Length == 0 || !int.TryParse(chunk, out var halfMoveCount) || halfMoveCount < 0)
            return new($"Invalid fen, expected half move count, got {chunk.ToString()}");

        if (halfMoveCount >= 2048)
            return new($"Invalid fen, half move count exceeds limit (2048), got {halfMoveCount}");

        // full move number
        rangePos++;
        range = ranges[rangePos];
        chunk = s[range];
        if (chunk.Length == 0 || !int.TryParse(chunk, out var fullMoveCount) || fullMoveCount < 0)
            return new($"Invalid fen, expected full move count, got {chunk.ToString()}");

        if (fullMoveCount >= 2048)
            return new($"Invalid fen, full move count exceeds limit (2048), got {fullMoveCount}");

        return result;
    }

    [SkipLocalsInit]
    private static ValidationResult ValidateMainSection(ReadOnlySpan<char> mainSection)
    {
        Span<int> limits = [32, 8, 10, 10, 10, 9, 1];

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
                    return new($"Invalid fen (too many separators) {mainSection.ToString()}");

                continue;
            }

            if (char.IsNumber(t))
            {
                if (!char.IsBetween(t, '1', '8'))
                    return new($"Invalid fen (not a valid square jump) {mainSection.ToString()}");

                continue;
            }

            var pieceIndex = PieceExtensions.PieceChars.IndexOf(t);

            if (pieceIndex == -1)
                return new($"Invalid fen (unknown piece) {mainSection.ToString()}");

            var pc = new Piece((Pieces)pieceIndex);
            var pt = pc.Type();

            pieceCount[pc]++;

            var limit = limits[pt];

            if (pieceCount[pc] > limit)
                return new($"Invalid fen (piece limit exceeded for {pc}. index={i},limit={limit},count={pieceCount[pc]}) {mainSection.ToString()}");
        }

        var valid = GetSpanSum(pieceCount.Slice(1, 5), 15);

        if (!valid)
            return new($"Invalid fen (white piece count exceeds limit) {mainSection.ToString()}");

        valid = GetSpanSum(pieceCount.Slice(9, 5), 15);

        if (!valid)
            return new($"Invalid fen (black piece count exceeds limit) {mainSection.ToString()}");

        return new();

        // check for summed up values
        static bool GetSpanSum(ReadOnlySpan<int> span, int limit)
        {
            var sum = span[0] + span[1] + span[2] + span[3] + span[4];
            return sum <= limit;
        }
    }
}