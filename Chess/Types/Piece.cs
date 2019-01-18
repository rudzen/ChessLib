/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

using Rudz.Chess.Extensions;

namespace Rudz.Chess.Types
{
    using Enums;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Piece.
    /// Contains the piece type which indicate what type and colour the piece is
    /// </summary>
    public struct Piece
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(int piece) => Value = (EPieces)piece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(Piece piece) => Value = piece.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(EPieces piece) => Value = piece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(EPieceType pieceType) => Value = (EPieces)pieceType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(EPieceType pieceType, Player side)
            : this(pieceType) => Value += side.Side << 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece(EPieceType pieceType, int offset)
            : this(pieceType) => Value += offset;

        public static Comparer<Piece> PieceComparer { get; } = new PieceRelationalComparer();

        public EPieces Value { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(char value) => new Piece(GetPiece(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(int value) => new Piece(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(EPieces value) => new Piece(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Piece(EPieceType pieceType) => new Piece(pieceType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator +(Piece left, Player right) => new Piece(left.Value + (right << 3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator >>(Piece left, int right) => (int)left.Value >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator <<(Piece left, int right) => (int)left.Value << right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, Piece right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, Piece right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Piece left, EPieces right) => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Piece left, EPieces right) => left.Value != right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Piece left, EPieces right) => left.Value <= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Piece left, EPieces right) => left.Value >= right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator ++(Piece left) => new Piece(++left.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Piece operator --(Piece left) => new Piece(--left.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Piece piece) => piece.Value != EPieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Piece piece) => piece.Value == EPieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ColorOf() => (int)Value >> 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToInt() => (int)Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhite() => ToInt().InBetween((int)EPieces.WhitePawn, (int)EPieces.WhiteKing);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlack() => ToInt().InBetween((int)EPieces.BlackPawn, (int)EPieces.BlackKing);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Piece other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Piece piece && Equals(piece);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => ToInt() << 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => this.GetPieceString();

        private static Piece GetPiece(char character)
        {
            switch (character)
            {
                case 'P':
                    return EPieces.WhitePawn;

                case 'N':
                    return EPieces.WhiteKnight;

                case 'B':
                    return EPieces.WhiteBishop;

                case 'R':
                    return EPieces.WhiteRook;

                case 'Q':
                    return EPieces.WhiteQueen;

                case 'K':
                    return EPieces.WhiteKing;

                case 'p':
                    return EPieces.BlackPawn;

                case 'n':
                    return EPieces.BlackKnight;

                case 'b':
                    return EPieces.BlackBishop;

                case 'r':
                    return EPieces.BlackRook;

                case 'q':
                    return EPieces.BlackQueen;

                case 'k':
                    return EPieces.BlackKing;

                default:
                    return EPieces.NoPiece;
            }
        }

        private sealed class PieceRelationalComparer : Comparer<Piece>
        {
            public override int Compare(Piece x, Piece y)
            {
                if (x.Value == y.Value)
                    return 0;

                // this is dangerous (fear king leopold III ?), king has no value and is considered to be uniq
                if (x.Type() == EPieceType.King || y.Type() == EPieceType.King)
                    return 1;

                if (x.PieceValue() < y.PieceValue())
                    return -1;

                if (x.PieceValue() == y.PieceValue())
                    return 0;

                return x.PieceValue() > y.PieceValue() ? 1 : x.ToInt().CompareTo(y.ToInt());
            }
        }
    }
}