using System;

namespace Rudz.Chess.Enums
{
    [Flags]
    public enum EMoveAmbiguity : byte
    {
        None = 0,
        File = 1,
        Rank = 2
    }

    public static class EMoveAmbiguityExtensions
    {
        public static bool HasFlagFast(this EMoveAmbiguity value, EMoveAmbiguity flag)
        {
            return (value & flag) != 0;
        }
    }
}