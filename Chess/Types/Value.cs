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
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Represents a single value. Used for e.g. Piece value etc
    /// </summary>
    public readonly struct Value : IEquatable<Value>
    {
        [JsonProperty("Value")]
        public readonly PieceValues Raw;

        public Value(Value value) : this(value.Raw)
        { }

        public Value(int value) : this((PieceValues)value)
        { }

        private Value(PieceValues value) => Raw = value;

        public static Value ValueZero = new(PieceValues.ValueZero);

        public static implicit operator Value(int value)
            => new(value);

        public static implicit operator Value(PieceValues value)
            => new(value);

        public static bool operator ==(Value left, Value right)
            => left.Equals(right);

        public static bool operator !=(Value left, Value right)
            => !left.Equals(right);

        public static Value operator +(Value left, Value right)
            => (int)left.Raw + (int)right.Raw;

        public static Value operator +(Value left, int right)
            => (int)left.Raw + right;

        public static Value operator +(Value left, PieceValues right)
            => (int)left.Raw + (int)right;

        public static Value operator +(int left, Value right)
            => left + (int)right.Raw;

        public static Value operator -(Value left, Value right)
            => left.Raw - right.Raw;

        public static Value operator -(Value left, int right)
            => (int)left.Raw - right;

        public static Value operator -(PieceValues left, Value right)
            => (int)left - right.Raw;

        public static Value operator -(Value left, PieceValues right)
            => left.Raw - (int)right;

        public static Value operator -(int left, Value right)
            => left - (int)right.Raw;

        public static Value operator ++(Value value)
            => (int)value.Raw + 1;

        public static Value operator --(Value value)
            => (int)value.Raw - 1;

        public static Value operator *(Value left, int right)
            => (int)left.Raw * right;

        public static bool operator >(Value left, Value right)
            => left.Raw > right.Raw;

        public static bool operator <(Value left, Value right)
            => left.Raw < right.Raw;

        public static bool operator <=(Value left, Value right)
            => left.Raw <= right.Raw;

        public static bool operator >=(Value left, Value right)
            => left.Raw >= right.Raw;

        public static bool operator >(Value left, PieceValues right)
            => left.Raw > right;

        public static bool operator <(Value left, PieceValues right)
            => left.Raw < right;

        public static bool operator <=(Value left, PieceValues right)
            => left.Raw <= right;

        public static bool operator >=(Value left, PieceValues right)
            => left.Raw >= right;

        public static bool operator true(Value value)
            => value.Raw > 0;

        public static bool operator false(Value value)
            => value.Raw <= 0;

        public bool Equals(Value other)
            => Raw == other.Raw;

        public override bool Equals(object obj)
            => obj is Value other && Equals(other);

        public override int GetHashCode()
            => (int)Raw;

        public override string ToString()
            => Raw.ToString();
    }
}