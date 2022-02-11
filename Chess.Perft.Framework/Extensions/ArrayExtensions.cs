namespace Perft.Extensions;

using System.Collections.Generic;
using System.Linq;

public static class ArrayExtensions
{
    public static bool IsEmpty<T>(T[] arr)
    {
        return arr == null || arr.Length == 0;
    }

    public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, T toAppend)
    {
        return @this.Concat(new[] { toAppend });
    }
}
