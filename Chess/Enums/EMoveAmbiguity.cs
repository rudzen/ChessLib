namespace Rudz.Chess.Enums
{
    using System;

    [Flags]
    public enum EMoveAmbiguity
    {
        None = 0,
        Move = 1,
        File = 2,
        Rank = 4
    }
}