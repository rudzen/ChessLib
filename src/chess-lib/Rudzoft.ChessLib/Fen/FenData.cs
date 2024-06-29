﻿/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Rudzoft.ChessLib.Fen;

/// <summary>
/// FenData contains a FEN string and an index pointer to a location in the FEN string.
/// For more information about the format, see
/// https://chessprogramming.wikispaces.com/Forsyth-Edwards+Notation
/// </summary>
[DebuggerDisplay("Fen='{Fen.ToString()}', Chunks={_splitPoints.Count}")]
public sealed class FenData : EventArgs, IFenData
{
    private record struct SplitPoint(int Begin, int End);

    private readonly Queue<SplitPoint> _splitPoints = new(6);

    public FenData(ReadOnlyMemory<char> fen)
    {
        Fen = fen;
        var start = 0;

        var s = fen.Span;

        // determine split points
        ref var splitPointsSpace = ref MemoryMarshal.GetReference(s);
        for (var i = 0; i < s.Length; i++)
        {
            var splitPoint = Unsafe.Add(ref splitPointsSpace, i);
            if (splitPoint != ' ')
                continue;

            _splitPoints.Enqueue(new(start, i));
            start = i + 1;
        }

        // add last
        _splitPoints.Enqueue(new(start, s.Length));
    }

    public FenData(ReadOnlySpan<string> fen)
    {
        var sb = new StringBuilder(128);

        ref var fenSpace = ref MemoryMarshal.GetReference(fen);
        for (var i = 0; i < fen.Length; ++i)
        {
            sb.Append(Unsafe.Add(ref fenSpace, i));
            if (i < fen.Length - 1)
                sb.Append(' ');
        }

        Fen = sb.ToString().AsMemory();
        var start = 0;

        var s = Fen.Span;

        // determine split points
        ref var splitPointsSpace = ref MemoryMarshal.GetReference(s);
        for (var i = 0; i < s.Length; i++)
        {
            var splitPoint = Unsafe.Add(ref splitPointsSpace, i);
            if (splitPoint != ' ')
                continue;

            _splitPoints.Enqueue(new(start, i));
            start = i + 1;
        }

        // add last
        _splitPoints.Enqueue(new(start, s.Length));
    }

    public FenData(char[] fen) : this(fen.AsMemory())
    { }

    public FenData(string fen) : this(fen.AsMemory())
    { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FenData(string value)
        => new(value);

    public int Index { get; private set; }

    public ReadOnlyMemory<char> Fen { get; }

    public char this[int index] => Fen.Span[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> Chunk()
    {
        // ReSharper disable once InlineOutVariableDeclaration
        Index++;
        return _splitPoints.TryDequeue(out var splitPoint)
            ? Fen.Span[splitPoint.Begin..splitPoint.End]
            : ReadOnlySpan<char>.Empty;
    }

    public bool Equals(FenData other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Fen.Length != other.Fen.Length)
            return false;

        var span = Fen.Span;
        var otherSpan = other.Fen.Span;

        return span.Equals(otherSpan, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
        => ReferenceEquals(this, obj) || (obj is FenData other && Equals(other));

    public override int GetHashCode()
        => HashCode.Combine(_splitPoints, Fen);

    public override string ToString()
        => Fen.Span.ToString();
}