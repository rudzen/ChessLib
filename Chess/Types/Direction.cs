/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2021 Rudy Alex Kohn

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
    using System;
    using System.Runtime.CompilerServices;

    public readonly struct Direction : IEquatable<Direction>
    {
        public Directions Value { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Direction(Directions d) => Value = d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Direction(int d) => Value = (Directions)d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Direction(int value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Direction(Directions value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator +(Direction left, Direction right) => left.Value + (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator +(Direction left, Directions right) => left.Value + (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator +(Direction left, int right) => left.Value + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator -(Direction left, Direction right) => left.Value - (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator -(Direction left, Directions right) => left.Value - (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator -(Direction left, int right) => left.Value - right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(Direction left, Direction right) => (int)left.Value * (int)right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(Direction left, Directions right) => (int)left.Value * (int)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction operator *(Direction left, int right) => (int)left.Value * right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Direction left, Direction right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Direction left, Direction right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Direction other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Direction other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => AsInt();

        private int AsInt() => (int)Value;
    }
}