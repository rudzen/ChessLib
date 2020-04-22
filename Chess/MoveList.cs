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

namespace Rudz.Chess
{
    using System;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveList : IMoveList
    {
        private const int MaxPossibleMoves = 218;

        private readonly Memory<Move> _moves;

        private int _moveIndex;

        public MoveList()
        {
            _moveIndex = -1;
            _moves = new Memory<Move>(new Move[MaxPossibleMoves]);
        }

        public ulong Count => (ulong) (_moveIndex + 1);

        public Move this[int index] => _moves.Span[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveList operator +(MoveList left, Move right)
        {
            left.Add(right);
            return left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveList operator +(MoveList left, MoveList right)
        {
            var rightMoves = right.GetMoves();
            foreach (var move in rightMoves)
                left.Add(move);
            return left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Move move) => _moves.Span[++_moveIndex] = move;

        /// <summary>
        /// Primary use is for polyglot moves
        /// </summary>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <returns>The first move that matches from and to squares</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetMove(Square from, Square to)
        {
            var s = GetMoves();
            var result = MoveExtensions.EmptyMove;
            foreach (var m in s)
            {
                if (m.GetFromSquare() != from || m.GetToSquare() != to)
                    continue;
                result = m;
                break;
            }

            return result;
        }

        public bool Contains(Move move)
        {
            var moves = GetMoves();
            foreach (var m in moves)
                if (m == move)
                    return true;

            return false;
        }

        public bool Contains(Square from, Square to)
        {
            var moves = GetMoves();
            foreach (var m in moves)
                if (m.GetFromSquare() == from && m.GetToSquare() == to)
                    return true;

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<Move> GetMoves() => _moves.Span.Slice(0, _moveIndex + 1);
    }
}