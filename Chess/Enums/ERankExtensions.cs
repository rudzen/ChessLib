namespace Rudz.Chess.Enums
{
    using System.Runtime.CompilerServices;
    using Types;

    public static class ERankExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ERank RelativeRank(this ERank rank, Player color) => (ERank)((int)rank ^ (color.Side * 7));
    }
}