namespace Rudz.Chess.Enums
{
    using System;
    using System.Runtime.CompilerServices;

    [Flags]
    public enum EMoveAmbiguity
    {
        None = 0,
        Move = 1,
        File = 2,
        Rank = 4
    }

    public static class EMoveAmbiguityExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this EMoveAmbiguity value, EMoveAmbiguity flag) => (value & flag) != 0;
    }
}