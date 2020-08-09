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
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Move struct. Contains a single ushort for move related information. Also includes set and
    /// get functions for the relevant data stored in the bits.
    /// </summary>
    public readonly struct Move : IEquatable<Move>
    {
        private readonly ushort _data;

        private Move(ushort value)
            => _data = value;

        public Move(Square from, Square to)
            => _data = (ushort)(to | (from.AsInt() << 6));

        public Move(Square from, Square to, MoveTypes moveType, PieceTypes promoPt = PieceTypes.Knight)
            => _data = (ushort)(to | (from.AsInt() << 6) | moveType.AsInt() | ((promoPt - PieceTypes.Knight) << 12));

        public static readonly Move EmptyMove = new Move();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Move(string value)
            => new Move(new Square(value[1] - '1', value[0] - 'a'), new Square(value[3] - '1', value[2] - 'a'));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Move(ExtMove extMove)
            => extMove.Move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Move(ushort value)
            => new Move(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Create(Square from, Square to)
            => new Move(from, to);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Create(ExtMove[] moves, int index, Square from, BitBoard to)
        {
            while (!to.IsEmpty)
                moves[index++].Move = Create(from, BitBoards.PopLsb(ref to));
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Move Create(Square from, Square to, MoveTypes moveType, PieceTypes promoPt = PieceTypes.Knight)
            => new Move(from, to, moveType, promoPt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Move left, Move right)
            => left._data == right._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Move left, Move right)
            => left._data != right._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square FromSquare()
            => (_data >> 6) & 0x3F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square ToSquare()
            => new Square(_data & 0x3F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceTypes PromotedPieceType()
            => (PieceTypes)(((_data >> 12) & 3) + 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsQueenPromotion()
            => PromotedPieceType() == PieceTypes.Queen;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveTypes MoveType()
            => (MoveTypes)(_data & (3 << 14));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsType(MoveTypes moveType)
            => MoveType() == moveType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnPassantMove()
            => MoveType() == MoveTypes.Enpassant;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCastlelingMove()
            => MoveType() == MoveTypes.Castling;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPromotionMove()
            => MoveType() == MoveTypes.Promotion;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullMove()
            => _data == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidMove()
            => FromSquare().AsInt() != ToSquare().AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Move other)
            => _data == other._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is Move move && Equals(move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return IsNullMove()
                ? "(null)"
                : MoveType() == MoveTypes.Normal
                    ? $"{FromSquare()}{ToSquare()}"
                    : MoveType() == MoveTypes.Castling
                        ? FromSquare() < ToSquare() ? "0-0" : "0-0-0"
                        : $"{FromSquare()}{ToSquare()}{PromotedPieceType().GetPromotionChar()}";
        }
    }
}