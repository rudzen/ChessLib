using System;
using System.Runtime.CompilerServices;

namespace Rudz.Chess.Fen
{
    public ref struct FenData2
    {
        private int _index;

        public ReadOnlyMemory<char> Fen;

        public FenData2(string fen, int index = 0)
        {
            _index = index;
            Fen = string.IsNullOrWhiteSpace(fen) ? ReadOnlyMemory<char>.Empty : fen.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FenData2(int value) => new FenData2(Chess.Fen.Fen.StartPositionFen, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator FenData2(string fen) => new FenData2(fen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FenData2 left, FenData2 right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FenData2 left, FenData2 right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Get() => Fen.Span[_index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Get(int index) => Fen.Span[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance() => _index++;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count) => _index += count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetAdvance() => Fen.Span[_index++];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetAdvance(int count)
        {
            Advance(count);
            return Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex() => _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Fen.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(FenData2 other) => Fen.Equals(other.Fen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Fen.IsEmpty ? string.Empty : new string(Fen.ToArray());
    }
}