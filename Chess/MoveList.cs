namespace Rudz.Chess
{
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
        public void Add(Move move) => _moves[++_moveIndex] = move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetMove(int index) =>
            index.InBetween(0, _moveIndex)
                ? _moves[index]
                : MoveExtensions.EmptyMove;

        public Move this[int index] => GetMove(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<Move> GetEnumerator()
        {
            for (int i = 0; i < _moveIndex; ++i)
                yield return _moves[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}