/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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

namespace Rudz.Chess.Protocol.UCI
{
    using System;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MovesToGoModel : EventArgs, IMovesToGoModel
    {
        private readonly ulong[] _time;

        public MovesToGoModel(ISearchParameters original)
        {
            _time = new[] { original.WhiteTimeMilliseconds, original.BlackTimeMilliseconds };
            MovesToGo = new[] { original.MovesToGo[0], original.MovesToGo[1] };
        }

        public int[] MovesToGo { get; set; }

        public int WhiteMovesToGo
        {
            get => MovesToGo[0];
            set => MovesToGo[0] = value;
        }

        public int BlackMovesToGo
        {
            get => MovesToGo[1];
            set => MovesToGo[1] = value;
        }

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

        public int this[Player side]
        {
            get => MovesToGo[side.Side];
            set => MovesToGo[side.Side] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Time(Player player) => _time[player.Side];
    }
}