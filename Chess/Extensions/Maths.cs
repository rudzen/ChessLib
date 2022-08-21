using System;
using System.Runtime.CompilerServices;

namespace Rudz.Chess.Extensions;

public static class Maths
{
    /// <summary>
    /// Converts a string to an int.
    /// Approx. 17 times faster than int.Parse.
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>The resulting number</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToIntegral(ReadOnlySpan<char> str)
    {
        if (str.IsEmpty || str.IsWhiteSpace())
            return default;

        var x = 0;
        var neg = false;
        var pos = 0;
        var max = str.Length - 1;
        if (str[pos] == '-')
        {
            neg = true;
            pos++;
        }

        while (pos <= max && str[pos].InBetween('0', '9'))
        {
            x = x * 10 + (str[pos] - '0');
            pos++;
        }

        return neg ? -x : x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToIntegral(this ReadOnlySpan<char> str, out ulong result)
    {
        if (str.IsEmpty || str.IsWhiteSpace())
        {
            result = 0;
            return false;
        }

        var x = ulong.MinValue;
        var pos = 0;
        var max = str.Length - 1;
        while (pos <= max && str[pos].InBetween('0', '9'))
        {
            x = x * 10 + (ulong)(str[pos] - '0');
            pos++;
        }

        result = x;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToIntegral(ReadOnlySpan<char> str, out int result)
    {
        if (str.IsEmpty)
        {
            result = 0;
            return false;
        }

        var x = 0;
        var pos = 0;
        var max = str.Length - 1;
        while (pos <= max && str[pos].InBetween('0', '9'))
        {
            x = x * 10 + (str[pos] - '0');
            pos++;
        }

        result = x;
        return true;
    }

}