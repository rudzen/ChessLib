namespace Perft.Options
{
    using System;

    [Flags]
    public enum OptionType
    {
        None = 0,
        EdpOptions = 1,
        FenOptions = 2,
        TTOptions = 4
    }
}