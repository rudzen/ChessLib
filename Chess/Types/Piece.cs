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
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Piece.
    /// Contains the piece type which indicate what type and color the piece is
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public readonly struct Piece : IEquatable<Piece>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Piece(int piece) => Value = (Pieces)piece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Piece(Piece piece) => Value = piece.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(Pieces piece) => Value = piece;

        public static Comparer<Piece> PieceComparer { get; } = new PieceRelationalComparer();

        [FieldOffset(0)]
        public readonly Pieces Value;

        public bool IsWhite => ColorOf().IsWhite;

        public bool IsBlack => !IsWhite;
        
        public static readonly Piece EmptyPiece = Pieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(char value) => new Piece(GetPiece(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(int value) => new Piece(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(Pieces value) => new Piece(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator +(Piece left, Player right) => new Piece(left.Value + (byte)(right << 3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator >>(Piece left, int right) => (int)left.Value >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator <<(Piece left, int right) => (int)left.Value << right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, Piece right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, Piece right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, Pieces right) => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, Pieces right) => left.Value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Piece left, Pieces right) => left.Value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Piece left, Pieces right) => left.Value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator ++(Piece left) => new Piece(left.Value + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator --(Piece left) => new Piece(left.Value - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Piece piece) => piece.Value != Pieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Piece piece) => piece.Value == Pieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player ColorOf() => (int)Value >> 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Piece other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Piece piece && Equals(piece);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)Value << 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => this.GetPieceString();

        private static Piece GetPiece(char character)
        {
            return character switch
            {
                'P' => Pieces.WhitePawn,
                'N' => Pieces.WhiteKnight,
                'B' => Pieces.WhiteBishop,
                'R' => Pieces.WhiteRook,
                'Q' => Pieces.WhiteQueen,
                'K' => Pieces.WhiteKing,
                'p' => Pieces.BlackPawn,
                'n' => Pieces.BlackKnight,
                'b' => Pieces.BlackBishop,
                'r' => Pieces.BlackRook,
                'q' => Pieces.BlackQueen,
                'k' => Pieces.BlackKing,
                _ => Pieces.NoPiece
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AsInt() => (int)Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceTypes Type() => (PieceTypes)(AsInt() & 0x7);

        private sealed class PieceRelationalComparer : Comparer<Piece>
        {
            public override int Compare(Piece x, Piece y)
            {
                if (x.Value == y.Value)
                    return 0;

                // this is dangerous (fear king leopold III ?), king has no value and is considered to be uniq
                if (x.Type() == PieceTypes.King || y.Type() == PieceTypes.King)
                    return 1;

                var xValue = x.PieceValue();
                var yValue = y.PieceValue();

                if (xValue < yValue)
                    return -1;

                if (xValue == yValue)
                    return 0;

                return xValue > yValue ? 1 : x.AsInt().CompareTo(y.AsInt());
            }
        }
    }
}