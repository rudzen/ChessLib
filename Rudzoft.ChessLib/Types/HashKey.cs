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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Hash;

namespace Rudzoft.ChessLib.Types;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public readonly struct HashKey : IEquatable<HashKey>
{
    private HashKey(ulong key)
    {
        LowerKey = UpperKey = 0;
        Key16 = 0;
        Key = key;
    }

    private HashKey(uint key32)
    {
        UpperKey = 0;
        Key = 0;
        Key16 = 0;
        LowerKey = key32;
    }

    [field: FieldOffset(0)] public ushort Key16 { get; }

    [field: FieldOffset(0)] public uint LowerKey { get; }

    [field: FieldOffset(0)] public ulong Key { get; }

    [field: FieldOffset(4)] public uint UpperKey { get; }

    public static HashKey Empty => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HashKey(ulong value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HashKey(uint value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(HashKey left, HashKey right)
        => left.Key == right.Key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(HashKey left, HashKey right)
        => left.Key != right.Key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator >> (HashKey left, int right)
        => new(left.Key >> right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator <<(HashKey left, int right)
        => new(left.Key << right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator ^(HashKey left, int right)
        => new(left.Key ^ (ulong)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator ^(HashKey left, HashKey right)
        => new(left.Key ^ right.Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator ^(HashKey left, ulong right)
        => new(left.Key ^ right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator ^(HashKey left, CastleRights right)
        => new(left.Key ^ right.GetZobristCastleling().Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashKey operator ^(HashKey left, File right)
        => new(left.Key ^ right.GetZobristEnPassant().Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(HashKey other)
        => Key == other.Key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
        => obj is HashKey other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => Key.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"0x{Key:X}";
}