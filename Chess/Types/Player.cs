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

using Rudz.Chess.Extensions;

namespace Rudz.Chess.Types
{
    using Enums;
    using System.Runtime.CompilerServices;

    public struct Player
    {
        public Player(int side)
            : this() => Side = side;

        public Player(Player side)
            : this() => Side = side.Side;

        public Player(EPlayer side)
            : this((int)side) { }

        public int Side;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Player(int value) => new Player(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Player(uint value) => new Player((int)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Player(EPlayer value) => new Player(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Player(bool value) => new Player(value.AsByte());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player operator ~(Player player) => new Player(player.Side ^ 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Player left, Player right) => left.Side == right.Side;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Player left, Player right) => left.Side != right.Side;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator <<(Player left, int right) => left.Side << right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator >>(Player left, int right) => left.Side >> right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pieces operator +(EPieceType pieceType, Player side) => (Pieces)pieceType + (byte)(side.Side << 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Player player && Equals(player);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Player other) => Side == other.Side;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Side << 24;
    }
}