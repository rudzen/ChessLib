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

// ReSharper disable ConvertToAutoProperty
namespace Rudz.Chess.Types
{
    using Enums;
    using Hash;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct HashKey : IEquatable<HashKey>
    {
        [FieldOffset(0)]
        private readonly uint _lowerKey32;

        [FieldOffset(4)]
        private readonly uint _upperKey32;

        [FieldOffset(0)]
        private readonly ulong _key;

        private HashKey(ulong key)
        {
            _lowerKey32 = _upperKey32 = 0;
            _key = key;
        }

        private HashKey(uint key32)
        {
            _upperKey32 = 0;
            _key = 0;
            _lowerKey32 = key32;
        }

        public readonly uint UpperKey => _upperKey32;

        public readonly uint LowerKey => _lowerKey32;

        public readonly ulong Key => _key;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator HashKey(ulong value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator HashKey(uint value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(HashKey left, HashKey right)
            => left._key == right._key;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(HashKey left, HashKey right)
            => left._key != right._key;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator >>(HashKey left, int right)
            => left._key >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator <<(HashKey left, int right)
            => left._key << right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator ^(HashKey left, int right)
            => left._key ^ (ulong)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator ^(HashKey left, HashKey right)
            => left._key ^ right._key;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator ^(HashKey left, ulong right)
            => left._key ^ right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator ^(HashKey left, CastlelingRights right)
            => left._key ^ right.GetZobristCastleling();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashKey operator ^(HashKey left, File right)
            => left._key ^ right.GetZobristEnPessant();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(HashKey other)
            => _key == other._key;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj)
            => obj is HashKey other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode()
            => _key.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"0x{Key:X}";
    }
}