using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rudz.Chess.Enums;

namespace Rudz.Chess.Types
{
    /// <summary>
    /// Score type with support for Eg/Mg values
    /// </summary>
    public struct Score
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct ScoreUnion
        {
            [FieldOffset(0)] public int mg;
            [FieldOffset(16)]public int eg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score(int value)
        {
            _data.mg = value;
            _data.eg = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Score(int mg, int eg)
        {
            _data.mg = mg;
            _data.eg = eg;
        }

        public Score(Score s) => _data = s._data;

        public Score(ExtMove em) => _data = em.Score._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Score(int value) => new Score(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Score(ExtMove value) => new Score(value.Score._data.mg, value.Score._data.eg);

        private ScoreUnion _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMg(int v) => _data.mg = v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEg(int v) => _data.eg = v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Eg() => _data.eg;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Mg() => _data.mg;

    }
}