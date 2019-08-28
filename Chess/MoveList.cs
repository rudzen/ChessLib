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

namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveList : IEnumerable<Move>
    {
        private const int MaxPossibleMoves = 218;

        private Move[] _moves;

        private int _moveIndex;

        public int Count => _moveIndex + 1;

        public Emgf Flags { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveList operator +(MoveList left, Move right)
        {
            left.Add(right);
            return left;
        }

        public void Initialize()
        {
            _moveIndex = -1;
            _moves = new Move[MaxPossibleMoves];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Move move) => _moves[++_moveIndex] = move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetMove(int index) =>
            index.InBetween(0, _moveIndex)
                ? _moves[index]
                : MoveExtensions.EmptyMove;

        /// <summary>
        /// Primary use is for polyglot moves
        /// </summary>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <returns>The first move that matches from and to squares</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetMove(Square from, Square to)
        {
            if (_moves[0] == MoveExtensions.EmptyMove)
                return MoveExtensions.EmptyMove;

            for (var i = 0; i < _moveIndex; ++i)
            {
                var move = _moves[i];
                if (move == MoveExtensions.EmptyMove)
                    return MoveExtensions.EmptyMove;
                if (move.GetFromSquare() == from & move.GetToSquare() == to)
                    return move;
            }

            return MoveExtensions.EmptyMove;
        }

        public Move this[int index] => GetMove(index);

        public IEnumerator<Move> GetEnumerator()
        {
            if (_moves[0] == MoveExtensions.EmptyMove)
                yield break;

            for (var i = 0; i < _moveIndex; ++i)
            {
                var move = _moves[i];
                if (move == MoveExtensions.EmptyMove)
                    yield break;
                yield return move;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}