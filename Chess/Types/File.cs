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
    using Enums;
    using System.Runtime.CompilerServices;

    public struct File
    {
        public EFile Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public File(int file) => Value = (EFile)file;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public File(EFile file) => Value = file;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public File(File file) => Value = file.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator File(int value) => new File(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator File(EFile value) => new File(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(File left, File right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(File left, File right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(File left, EFile right) => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(File left, EFile right) => left.Value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(File left, int right) => left.Value == (EFile)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(File left, int right) => left.Value != (EFile)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator +(File left, File right) => left.AsInt() + right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator +(File left, int right) => left.AsInt() + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator +(File left, EFile right) => left.AsInt() + (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator -(File left, File right) => left.AsInt() - right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator -(File left, int right) => left.AsInt() - right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator -(File left, EFile right) => left.AsInt() - (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static File operator ++(File file) => ++file.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(File left, ulong right) => left.BitBoardFile() & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(ulong left, File right) => left & right.BitBoardFile();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(File left, File right) => left.BitBoardFile() | right.BitBoardFile();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(ulong left, File right) => left | right.BitBoardFile();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator |(File left, int right) => left.AsInt() | right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ~(File left) => ~left.BitBoardFile();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(File left, int right) => left.AsInt() >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(File left, File right) => left.AsInt() > right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(File left, File right) => left.AsInt() < right.AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(File sq) => sq.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(File sq) => !sq.IsValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AsInt() => (int)Value;

        public bool IsValid() => Value <= EFile.FileH;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => this.FileString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(File other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is File file && Equals(file);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => AsInt();
    }
}