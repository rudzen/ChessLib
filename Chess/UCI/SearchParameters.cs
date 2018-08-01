/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2018 Rudy Alex Kohn

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

using System.Collections.Generic;

namespace Rudz.Chess.UCI
{
    using System;
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

        private readonly StringBuilder _output = new StringBuilder(256);

        #region ctors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SearchParameters()
        {
            _time = new ulong[2];
            _inc = new ulong[2];
            MovesToGo = new int[2];
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
        public SearchParameters(ulong whiteTimeMilliseconds, ulong blackTimeMilliseconds, ulong whiteIncrementTimeMilliseconds, ulong blackIncrementTimeMilliseconds, IReadOnlyList<int> movesToGo, int moveTime)
            : this(whiteTimeMilliseconds, blackTimeMilliseconds, whiteIncrementTimeMilliseconds, blackIncrementTimeMilliseconds)
        {
            MovesToGo = new[] { movesToGo[0], movesToGo[1] };
            MoveTime = moveTime;
        }

        public bool Infinite { get; set; }

        public int MoveTime { get; set; }

        public int[] MovesToGo { get; set; }

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

        #endregion ctors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Time(Player player) => _time[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Inc(Player player) => _inc[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_time, 0, 2);
            Array.Clear(_inc, 0, 2);
            Array.Clear(MovesToGo, 0, 2);
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

            _output.Append("wtime ");
            _output.Append(WhiteTimeMilliseconds);
            _output.Append(" btime ");
            _output.Append(BlackTimeMilliseconds);
            if (MoveTime > 0)
            {
                _output.Append(" movetime ");
                _output.Append(MoveTime);
            }

            _output.Append(" winc ");
            _output.Append(WhiteIncrementTimeMilliseconds);
            _output.Append(" binc ");
            _output.Append(BlackIncrementTimeMilliseconds);

            // ReSharper disable once InvertIf
            if (MovesToGo[side.Side] > 0)
            {
                _output.Append(" movestogo ");
                _output.Append(MovesToGo[side.Side]);
            }

            return _output.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DecreaseMovesToGo(Player side) => --MovesToGo[side.Side] == 0;
    }
}