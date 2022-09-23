/*
ChessLib, a chess data structure library

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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Protocol.UCI;

/// <summary>
/// Contains the information related to search parameters for a UCI chess engine.
/// </summary>
public sealed class SearchParameters : ISearchParameters, ISpanFormattable
{
    private readonly Clock _clock;
    private ulong _movesToGo;
    private ulong _moveTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters()
    {
        _clock = new Clock(new ulong[Player.Count], new ulong[Player.Count]);
        SearchMoves = new List<Move>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters(bool infinite)
        : this() => Infinite = infinite;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters(ulong whiteTimeMilliseconds, ulong blackTimeMilliseconds)
        : this()
    {
        WhiteTimeMilliseconds = whiteTimeMilliseconds;
        BlackTimeMilliseconds = blackTimeMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters(ulong whiteTimeMilliseconds, ulong blackTimeMilliseconds,
        ulong whiteIncrementTimeMilliseconds, ulong blackIncrementTimeMilliseconds)
        : this(whiteTimeMilliseconds, blackTimeMilliseconds)
    {
        WhiteIncrementTimeMilliseconds = whiteIncrementTimeMilliseconds;
        BlackIncrementTimeMilliseconds = blackIncrementTimeMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters(ulong whiteTimeMilliseconds, ulong blackTimeMilliseconds,
        ulong whiteIncrementTimeMilliseconds, ulong blackIncrementTimeMilliseconds, ulong moveTime)
        : this(whiteTimeMilliseconds, blackTimeMilliseconds, whiteIncrementTimeMilliseconds,
            blackIncrementTimeMilliseconds)
    {
        MoveTime = moveTime;
    }

    public List<Move> SearchMoves { get; set; }

    public bool Infinite { get; set; }

    public bool Ponder { get; set; }

    public ulong MoveTime
    {
        get => _moveTime;
        set => _moveTime = value;
    }

    public ulong MovesToGo
    {
        get => _movesToGo;
        set => _movesToGo = value;
    }

    public ulong Depth { get; set; }

    public ulong Nodes { get; set; }

    public ulong Mate { get; set; }

    public ulong WhiteTimeMilliseconds
    {
        get => _clock.Time[Player.White.Side];
        set => _clock.Time[Player.White.Side] = value;
    }

    public ulong BlackTimeMilliseconds
    {
        get => _clock.Time[Player.Black.Side];
        set => _clock.Time[Player.Black.Side] = value;
    }

    public ulong WhiteIncrementTimeMilliseconds
    {
        get => _clock.Inc[Player.White.Side];
        set => _clock.Inc[Player.White.Side] = value;
    }

    public ulong BlackIncrementTimeMilliseconds
    {
        get => _clock.Inc[Player.Black.Side];
        set => _clock.Inc[Player.Black.Side] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Time(Player p) => _clock.Time[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Inc(Player p) => _clock.Inc[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_clock.Time);
        Array.Clear(_clock.Inc);
        MoveTime = 0;
        Infinite = false;
    }

    public override string ToString()
    {
        Span<char> s = stackalloc char[128];
        TryFormat(s, out var written);
        return new string(s[..written]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DecreaseMovesToGo() => --_movesToGo == 0;

    public void AddSearchMove(Move move) => SearchMoves.Add(move);

    private static int ParseValue(int index, in ulong value, Span<char> target)
    {
        Span<char> number = stackalloc char[32];
        value.TryFormat(number, out var numericWritten);
        for (var i = 0; i < numericWritten; ++i)
            target[index++] = number[i];
        return index;
    }

    public string ToString(string format, IFormatProvider formatProvider)
        => string.Format(formatProvider, format, ToString());

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider provider = null)
    {
        var index = 0;

        destination[index++] = 'g';
        destination[index++] = 'o';
        destination[index++] = ' ';

        if (Infinite)
        {
            destination[index++] = 'i';
            destination[index++] = 'n';
            destination[index++] = 'f';
            destination[index++] = 'i';
            destination[index++] = 'n';
            destination[index++] = 'i';
            destination[index++] = 't';
            destination[index++] = 'e';
            charsWritten = index;
            return true;
        }

        destination[index++] = 'w';
        destination[index++] = 't';
        destination[index++] = 'i';
        destination[index++] = 'm';
        destination[index++] = 'e';
        destination[index++] = ' ';
        index = ParseValue(index, in _clock.Time[Player.White.Side], destination);

        destination[index++] = ' ';
        destination[index++] = 'b';
        destination[index++] = 't';
        destination[index++] = 'i';
        destination[index++] = 'm';
        destination[index++] = 'e';
        destination[index++] = ' ';
        index = ParseValue(index, in _clock.Time[Player.Black.Side], destination);

        if (MoveTime > ulong.MinValue)
        {
            destination[index++] = ' ';
            destination[index++] = 'm';
            destination[index++] = 'o';
            destination[index++] = 'v';
            destination[index++] = 'e';
            destination[index++] = 't';
            destination[index++] = 'i';
            destination[index++] = 'm';
            destination[index++] = 'e';
            destination[index++] = ' ';
            index = destination.Append(in _moveTime, index);
        }

        destination[index++] = ' ';
        destination[index++] = 'w';
        destination[index++] = 'i';
        destination[index++] = 'n';
        destination[index++] = 'c';
        destination[index++] = ' ';
        index = ParseValue(index, in _clock.Inc[Player.White.Side], destination);

        destination[index++] = ' ';
        destination[index++] = 'b';
        destination[index++] = 'i';
        destination[index++] = 'n';
        destination[index++] = 'c';
        destination[index++] = ' ';

        index = ParseValue(index, in _clock.Inc[Player.Black.Side], destination);

        if (_movesToGo == ulong.MinValue)
        {
            charsWritten = index;
            return true;
        }

        destination[index++] = ' ';
        destination[index++] = 'm';
        destination[index++] = 'o';
        destination[index++] = 'v';
        destination[index++] = 'e';
        destination[index++] = 's';
        destination[index++] = 't';
        destination[index++] = 'o';
        destination[index++] = 'g';
        destination[index++] = 'o';
        destination[index++] = ' ';
        index = destination.Append(in _movesToGo, index);

        charsWritten = index;
        return true;
    }
}