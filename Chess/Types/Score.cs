/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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
    using System.Runtime.InteropServices;

    /// <summary>
    /// Score type with support for Eg/Mg values
    /// </summary>
    public struct Score
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct ScoreUnion
        {
            [FieldOffset(0)] public int mg;
            [FieldOffset(16)]public int eg;
        }

        private ScoreUnion _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score(int value)
        {
            _data.mg = value;
            _data.eg = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score(int mg, int eg)
        {
            _data.mg = mg;
            _data.eg = eg;
        }

        public Score(Score s) => _data = s._data;

        public Score(ExtMove em) => _data = em.Score._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Score(int value) => new Score(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Score(ExtMove value) => new Score(value.Score._data.mg, value.Score._data.eg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMg(int v) => _data.mg = v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEg(int v) => _data.eg = v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Eg() => _data.eg;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Mg() => _data.mg;

    }
}