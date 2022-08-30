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
using System.Text;
using Rudz.Chess.Types;

namespace Rudz.Chess.Protocol.UCI;

/// <summary>
/// Contains the information related to search parameters for a UCI chess engine.
/// </summary>
public sealed class SearchParameters : ISearchParameters
{
    private readonly Clock _clock;

    private readonly StringBuilder _output = new(256);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters()
    {
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

    public IList<Move> SearchMoves { get; set; }

    public bool Infinite { get; set; }

    public bool Ponder { get; set; }

    public ulong MoveTime { get; set; }

    public ulong MovesToGo { get; set; }

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
    public ulong Time(Player player) => _clock.Time[player.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Inc(Player player) => _clock.Inc[player.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_clock.Time);
        Array.Clear(_clock.Inc);
        MoveTime = 0;
        Infinite = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Get(Player side)
    {
        _output.Clear();

        _output.Append("go ");

        if (Infinite)
        {
            _output.Append("infinite");
            return _output.ToString();
        }

        _output.Append($"wtime {WhiteTimeMilliseconds} btime {BlackTimeMilliseconds}");

        if (MoveTime > 0)
        {
            _output.Append(" movetime ");
            _output.Append(MoveTime);
        }

        _output.Append($" winc {WhiteIncrementTimeMilliseconds} binc {BlackIncrementTimeMilliseconds}");

        if (MovesToGo > 0)
        {
            _output.Append(" movestogo ");
            _output.Append(MovesToGo);
        }

        return _output.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DecreaseMovesToGo() => --MovesToGo == 0;

    public void AddSearchMove(Move move) => SearchMoves.Add(move);
}