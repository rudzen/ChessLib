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

namespace Rudz.Chess.Protocol.UCI;

using Extensions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Types;

/// <summary>
/// Contains the information related to search parameters for a UCI chess engine.
/// </summary>
public sealed class SearchParameters : ISearchParameters
{
    private readonly ulong[] _time;

    private readonly ulong[] _inc;

    private readonly IList<Move> _searchMoves;

    private readonly StringBuilder _output = new(256);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters()
    {
        _time = _inc = new ulong[2];
        _searchMoves = new List<Move>();
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
    public SearchParameters(ulong whiteTimeMilliseconds, ulong blackTimeMilliseconds, ulong whiteIncrementTimeMilliseconds, ulong blackIncrementTimeMilliseconds)
        : this(whiteTimeMilliseconds, blackTimeMilliseconds)
    {
        WhiteIncrementTimeMilliseconds = whiteIncrementTimeMilliseconds;
        BlackIncrementTimeMilliseconds = blackIncrementTimeMilliseconds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SearchParameters(ulong whiteTimeMilliseconds, ulong blackTimeMilliseconds, ulong whiteIncrementTimeMilliseconds, ulong blackIncrementTimeMilliseconds, int moveTime)
        : this(whiteTimeMilliseconds, blackTimeMilliseconds, whiteIncrementTimeMilliseconds, blackIncrementTimeMilliseconds)
    {
        MoveTime = moveTime;
    }

    public bool Infinite { get; set; }

    public int MoveTime { get; set; }

    public int MovesToGo { get; set; }

    public int Depth { get; set; }

    public ulong WhiteTimeMilliseconds
    {
        get => _time[0];
        set => _time[0] = value;
    }

    public ulong BlackTimeMilliseconds
    {
        get => _time[1];
        set => _time[1] = value;
    }

    public ulong WhiteIncrementTimeMilliseconds
    {
        get => _inc[0];
        set => _inc[0] = value;
    }

    public ulong BlackIncrementTimeMilliseconds
    {
        get => _inc[1];
        set => _inc[1] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Time(Player player) => _time[player.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Inc(Player player) => _inc[player.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _time.Clear();
        _inc.Clear();
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

        const string timeFormatString = "wtime {0} btime {1}";
        const string incFormatString = " winc {0} binc {1}";

        _output.AppendFormat(timeFormatString, WhiteTimeMilliseconds, BlackTimeMilliseconds);

        if (MoveTime > 0)
        {
            _output.Append(" movetime ");
            _output.Append(MoveTime);
        }

        _output.AppendFormat(incFormatString, WhiteIncrementTimeMilliseconds, BlackIncrementTimeMilliseconds);

        if (MovesToGo > 0)
        {
            _output.Append(" movestogo ");
            _output.Append(MovesToGo);
        }

        return _output.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool DecreaseMovesToGo() => --MovesToGo == 0;

    public void AddSearchMove(Move move)
    {
        _searchMoves.Add(move);
    }
}
