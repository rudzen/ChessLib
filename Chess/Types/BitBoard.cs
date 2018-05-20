/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2018 Rudy Alex Kohn

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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Enums;

    /*
         * In general, the bitboard layout of a chess board matches that of a real chess board.
         *  
            56	57	58	59	60	61	62	63      (RANK 8)
            48	49	50	51	52	53	54	55      (RANK 7)
            40	41	42	43	44	45	46	47      (RANK 6)
            32	33	34	35	36	37	38	39      (RANK .)
            24	25	26	27	28	29	30	31      (RANK .)
            16	17	18	19	20	21	22	23
            08	09	10	11	12	13	14	15
            00	01	02	03	04	05	06	07
         *  
         *   A   B   C   D   E   F   G   H
         *   
         *  Direction of bits --->
         */

    /// <summary>
    /// Bitboard struct, wraps an unsigned long with some nifty helper functionality and operators.
    /// Enumeration will yield each set bit as a Square struct.
    /// </summary>
    public struct BitBoard : IEnumerable<Square>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard(ulong value)
            : this() => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard(BitBoard value)
            : this(value.Value) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard(Square square)
            : this(square.BitBoardSquare()) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard(int value)
            : this((ulong)value) { }

        /// <summary>
        /// Gets and sets the bits of the... tadaaaa.. bitboard
        /// </summary>
        public ulong Value { get; private set; }

        public int Count => BitBoards.PopCount(Value);

        public string String => Convert.ToString((long)Value, 2).PadLeft(64, '0');

        /// <summary>
        /// [] overload :>
        /// </summary>
        /// <param name="index">the damn index</param>
        /// <returns>the Bit object if assigning</returns>
        public BitBoard this[int index] {
            get => this.Get(index);
            set => Set(index); // TODO : Untested
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BitBoard(ulong value) => new BitBoard(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BitBoard(int value) => new BitBoard(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BitBoard(Square square) => new BitBoard(square.BitBoardSquare());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator *(BitBoard left, ulong right) => left.Value * right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator *(ulong left, BitBoard right) => left * right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator -(BitBoard left, int right) => left.Value - (ulong)right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator >>(BitBoard left, int right) => left.Value >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator <<(BitBoard left, int right) => left.Value << right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(BitBoard left, Square right) => left.Value | right.BitBoardSquare();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator |(BitBoard left, BitBoard right) => left.Value | right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong operator ^(BitBoard left, BitBoard right) => left.Value ^ right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(BitBoard left, BitBoard right) => left.Value & right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(ulong left, BitBoard right) => left & right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(BitBoard left, ulong right) => left.Value & right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(BitBoard left, Square right) => left.Value & right.BitBoardSquare();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator &(Square left, BitBoard right) => left.BitBoardSquare() & right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator ~(BitBoard bitBoard) => ~bitBoard.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard operator --(BitBoard bitBoard)
        {
            BitBoards.ResetLsb(ref bitBoard);
            return bitBoard;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BitBoard left, BitBoard right) => left.Count == right.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(BitBoard left, BitBoard right) => left.Count < right.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(BitBoard left, BitBoard right) => left.Count > right.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(BitBoard left, BitBoard right) => left.Count >= right.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(BitBoard left, BitBoard right) => left.Count <= right.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BitBoard left, BitBoard right) => left.Value != right.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(BitBoard bitBoard) => bitBoard.Value != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(BitBoard bitBoard) => bitBoard.Value == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Value = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int pos) => Value |= BitBoards.One << pos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong data) => Value = data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Xor(int pos) => Value ^= (uint)pos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<Square> GetEnumerator()
        {
            for (BitBoard bb = Value; bb; bb--)
                yield return bb.First();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            StringBuilder output = new StringBuilder(512);
            const string seperator = "\n  +BB-+---+---+---+---+---+---+---+\n";
            const char splitter = '|';
            output.Append(seperator);
            for (ERank rank = ERank.Rank8; rank >= ERank.Rank1; rank--) {
                output.Append((int)rank + 1);
                output.Append(' ');
                for (EFile file = EFile.FileA; file <= EFile.FileH; file++) {
                    output.Append(splitter);
                    output.Append(' ');
                    output.Append((Value & new Square(rank, file)) != 0 ? " 1 " : " . ");
                    output.Append(' ');
                }

                output.Append(splitter);
                output.Append(seperator);
            }

            output.Append("    a   b   c   d   e   f   g   h\n");
            return output.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BitBoard other) => Value == other.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && obj is BitBoard board && Equals(board);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)(Value >> 32);
    }
}