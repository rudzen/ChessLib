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

namespace Rudz.Chess.Types
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extended move structure which combines Move and Score
    /// </summary>
    public struct ExtMove
    {
        public Move Move;

        public Score Score;

        public static readonly ExtMove Empty;

        static ExtMove()
        {
            Empty = new ExtMove();
        }

        private ExtMove(Move m, Score s)
        {
            Move = m;
            Score = s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ExtMove(Move m) => new ExtMove(m, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ExtMove(Score s) => new ExtMove(Move.EmptyMove, s);

        public override string ToString() => $"{Move}, {Score}";
    }
}