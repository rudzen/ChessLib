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
    using EnsureThat;
    using Extensions;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveList : IEnumerable<Move>
    {
        private const int MaxPossibleMoves = 218;

        private readonly Move[] _moves;

        private int _moveIndex;

        public MoveList(Move[] moves)
        {
            EnsureArg.IsNotNull(moves, nameof(moves));
            _moveIndex = -1;
            _moves = moves;
        }

        public MoveList()
        {
            _moveIndex = -1;
            _moves = new Move[MaxPossibleMoves];
        }

        public int Count => _moveIndex + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveList operator +(MoveList left, Move right)
        {
            left.Add(right);
            return left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MoveList operator +(MoveList left, MoveList right)
        {
            foreach (var m in right._moves)
                left.Add(m);
            return left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Move move) => _moves[++_moveIndex] = move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetMove(int index) =>
            index.InBetween(0, _moveIndex)
                ? _moves[index]
                : MoveExtensions.EmptyMove;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetMove(Square fromSquare, Square toSquare)
        {
            var max = _moveIndex + 1;
            for (var i = 0; i < max; ++i)
            {
                var move = _moves[i];
                if (move.GetFromSquare() == fromSquare && move.GetToSquare() == toSquare)
                    return move;
            }

            return MoveExtensions.EmptyMove;
        }

        public Move this[int index] => GetMove(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<Move> GetEnumerator()
        {
            var max = _moveIndex + 1;
            for (var i = 0; i < max; ++i)
                yield return _moves[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}