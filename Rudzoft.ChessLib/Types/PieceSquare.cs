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
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Types;

/// <summary>
/// Model for data transfer of piece and square Used for notification when a piece is updated in
/// the chess structure
/// </summary>
public sealed class PieceSquareEventArgs : EventArgs, ISpanFormattable, IPieceSquare
{
    public PieceSquareEventArgs(Piece pc, Square sq)
    {
        Piece = pc;
        Square = sq;
    }

    public Piece Piece { get; set; }

    public Square Square { get; set; }

    public void Deconstruct(out Piece pc, out Square sq)
    {
        pc = Piece;
        sq = Square;
    }

    public bool Equals(IPieceSquare other)
        => other is not null &&
           (ReferenceEquals(this, other) || (Piece.Equals(other.Piece) && Square.Equals(other.Square)));

    public override bool Equals(object obj)
        => obj is not null &&
           (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && Equals((PieceSquareEventArgs)obj)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => HashCode.Combine(Piece, Square);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, IFormatProvider formatProvider)
        => string.Format(formatProvider, format, Piece + Square.ToString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider provider = null)
    {
        Span<char> s = stackalloc char[3];
        Square.TryFormat(s, out charsWritten, format, provider);
        s[2] = Piece.GetPieceChar();
        charsWritten++;
        return true;
    }
}