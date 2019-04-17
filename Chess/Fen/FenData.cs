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

// ReSharper disable PossibleNullReferenceException
// ReSharper disable ExceptionNotDocumentedOptional

namespace Rudz.Chess.Fen
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// FenData contains a FEN string and an index pointer to a location in the FEN string.
    /// For more information about the format, see
    /// https://chessprogramming.wikispaces.com/Forsyth-Edwards+Notation
    /// </summary>
    public sealed class FenData : EventArgs, IFenData
    {
        private int _index;

        static FenData() => FenComparer = new FenEqualityComparer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData(string fen, int index = 0)
        {
            Fen = fen;
            _index = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FenData() : this(string.Empty)
        { }

        public static IEqualityComparer<FenData> FenComparer { get; }

        public string Fen { get; set; }

        public int Index => _index;

        public char GetAdvance => Fen[_index++];

        public char Get => Fen[_index];

        public char this[int index] => Fen[_index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FenData(int value) => new FenData(Chess.Fen.Fen.StartPositionFen, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FenData(string fen) => new FenData(fen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FenData left, FenData right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FenData left, FenData right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance() => _index++;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count) => _index += count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Fen.GetHashCode() ^ (_index << 24);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((FenData)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(FenData other) => string.Equals(Fen, other.Fen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Fen ?? string.Empty;

        private sealed class FenEqualityComparer : IEqualityComparer<FenData>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(FenData x, FenData y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                return x.GetType() == y.GetType() && string.Equals(x.Fen, y.Fen);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetHashCode(FenData obj) => obj.Fen != null ? obj.Fen.GetHashCode() : 0;
        }
    }
}