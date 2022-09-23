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

using Rudzoft.ChessLib.Extensions;
using System;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

namespace Rudzoft.ChessLib.Types;

public enum Files
{
    FileA = 0,
    FileB = 1,
    FileC = 2,
    FileD = 3,
    FileE = 4,
    FileF = 5,
    FileG = 6,
    FileH = 7,
    FileNb = 8
}

public static class FilesExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AsInt(this Files f)
        => (int)f;
}

public readonly record struct File(Files Value) : IComparable<File>, ISpanFormattable, IValidationType
{
    private static readonly string[] FileStrings = { "a", "b", "c", "d", "e", "f", "g", "h" };

    public static File FileA { get; } = new(Files.FileA);
    public static File FileB { get; } = new(Files.FileB);
    public static File FileC { get; } = new(Files.FileC);
    public static File FileD { get; } = new(Files.FileD);
    public static File FileE { get; } = new(Files.FileE);
    public static File FileF { get; } = new(Files.FileF);
    public static File FileG { get; } = new(Files.FileG);
    public static File FileH { get; } = new(Files.FileH);
    public static File[] AllFiles { get; } = new[] { FileA, FileB, FileC, FileD, FileE, FileF, FileG, FileH };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File(int file)
        : this((Files)file) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File(File f)
        : this(f.Value) { }

    public char Char
        => (char)('a' + Value);

    public bool IsOk
        => Value.AsInt().InBetween(Files.FileA.AsInt(), Files.FileH.AsInt());

    public const int Count = (int)Files.FileNb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator File(int value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator File(Files value)
        => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(File left, Files right)
        => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(File left, Files right)
        => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(File left, int right)
        => left.Value == (Files)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(File left, int right)
        => left.Value != (Files)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator +(File left, File right)
        => new(left.AsInt() + right.AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator +(File left, int right)
        => new(left.AsInt() + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator +(File left, Files right)
        => new(left.AsInt() + (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator -(File left, File right)
        => new(left.AsInt() - right.AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator -(File left, int right)
        => new(left.AsInt() - right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator -(File left, Files right)
        => new(left.AsInt() - (int)right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static File operator ++(File f)
        => new(f.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(File left, ulong right)
        => left.BitBoardFile() & right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(ulong left, File right)
        => left & right.BitBoardFile();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(File left, File right)
        => left.BitBoardFile() | right.BitBoardFile();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(ulong left, File right)
        => left | right.BitBoardFile();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator |(File left, int right)
        => left.AsInt() | right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ~(File left)
        => ~left.BitBoardFile();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator >>(File left, int right)
        => left.AsInt() >> right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(File left, File right)
        => left.AsInt() > right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(File left, File right)
        => left.AsInt() < right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(File left, File right)
        => left.AsInt() >= right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(File left, File right)
        => left.AsInt() <= right.AsInt();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(File f)
        => f.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(File f)
        => !f.IsOk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AsInt()
        => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
        => FileStrings[AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider)
        => Value.AsInt().ToString(format, formatProvider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider provider)
        => Value.AsInt().TryFormat(destination, out charsWritten, format, provider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(File other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(File other)
        => Value.AsInt().CompareTo(other.Value.AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => AsInt();

    /// <summary>
    /// Fold file [ABCDEFGH] to file [ABCDDCBA]
    /// </summary>
    /// <returns>The distance to the edge file</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EdgeDistance()
        => Math.Min(AsInt() - FileA.AsInt(), FileH.AsInt() - AsInt());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File Clamp(File min, File max)
        => new(Value.AsInt().Clamp(min.AsInt(), max.AsInt()));
}