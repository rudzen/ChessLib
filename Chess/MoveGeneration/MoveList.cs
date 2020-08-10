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

namespace Rudz.Chess.MoveGeneration
{
    using Enums;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Types;

    public sealed class MoveList : IMoveList
    {
        private readonly ExtMove[] _moves;
        private int _cur, _last;

        public MoveList()
        {
            _moves = new ExtMove[218];
        }

        int IReadOnlyCollection<ExtMove>.Count => _last;

        public int Length => _last;

        public Move CurrentMove => _moves[_cur].Move;

        public static MoveList operator ++(MoveList moveList)
        {
            ++moveList._cur;
            return moveList;
        }

        public ExtMove this[int index]
        {
            get => _moves[index];
            set => _moves[index] = value;
        }

        public void Add(ExtMove item) => _moves[_last++] = item;

        public void Add(Move item) => _moves[_last++] = item;

        /// <summary>
        /// Reset the moves
        /// </summary>
        public void Clear()
        {
            _cur = _last = 0;
            _moves[0].Move = ExtMove.Empty;
        }

        public bool Contains(ExtMove item)
            => Contains(item.Move);

        public bool Contains(Move item)
        {
            for (var i = 0; i < _last; ++i)
                if (_moves[i].Move == item)
                    return true;

            return false;
        }

        public bool Contains(Square from, Square to)
        {
            for (var i = 0; i < _last; ++i)
            {
                var move = _moves[i].Move;
                if (move.FromSquare() == from && move.ToSquare() == to)
                    return true;
            }

            return false;
        }

        public void Generate(IPosition pos, MoveGenerationType type = MoveGenerationType.Legal)
        {
            _cur = 0;
            _last = MoveGenerator.Generate(pos, _moves, 0, pos.SideToMove, type);
            _moves[_last] = Move.EmptyMove;
        }

        public ReadOnlySpan<ExtMove> Get() =>
            _last == 0
                ? ReadOnlySpan<ExtMove>.Empty
                : _moves.AsSpan().Slice(0, _last);

        public IEnumerator<ExtMove> GetEnumerator()
            => _moves.Take(_last).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}