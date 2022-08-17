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

namespace Rudz.Chess.Types;

using System;

/// <summary>
/// Model for data transfer of piece and square Used for notification when a piece is updated in
/// the chess structure
/// </summary>
public sealed class PieceSquareEventArgs : EventArgs, IPieceSquare
{
    public PieceSquareEventArgs(Piece piece, Square square)
    {
        Piece = piece;
        Square = square;
    }

    public Piece Piece { get; set; }

    public Square Square { get; set; }

    public bool Equals(IPieceSquare other)
        => other is not null && (ReferenceEquals(this, other) || Piece.Equals(other.Piece) && Square.Equals(other.Square));

    public override bool Equals(object obj)
        => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((PieceSquareEventArgs)obj));

    public override int GetHashCode()
    {
        return HashCode.Combine(Piece, Square);
    }
}
