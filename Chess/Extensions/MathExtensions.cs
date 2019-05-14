/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Rudz.Chess.Extensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Types;

    public static class MathExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InBetween(this int v, int min, int max) => (uint)v - (uint)min <= (uint)max - (uint)min;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InBetween(this char v, char min, char max) => v - (uint)min <= max - min;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InBetween(this uint v, int min, int max) => v - (uint)min <= (uint)max - (uint)min;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(this int v, int min, int max) => v < min ? min : (v > max ? max : v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(this double v, double min, double max) => v < min ? min : (v > max ? max : v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(this int @this) => Math.Abs(@this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(this int @this, int value) => Math.Max(@this, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Round(this double @this, int digits) => Math.Round(@this, digits);

        /// <summary>
        /// Converts a string to an int.
        /// Approx. 17 times faster than int.Parse.
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The resulting number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToIntegral(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0;

            var x = 0;
            var neg = false;
            var pos = 0;
            var max = str.Length - 1;
            if (str[pos] == '-')
            {
                neg = true;
                pos++;
            }

            while (pos <= max && InBetween(str[pos], '0', '9'))
            {
                x = x * 10 + (str[pos] - '0');
                pos++;
            }

            return neg ? -x : x;
        }

        public static bool ToIntegral(this string str, out int result)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                result = 0;
                return false;
            }

            result = str.ToIntegral();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ToIntegral(this string str, out ulong result)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                result = 0;
                return false;
            }

            var x = 0ul;
            var pos = 0;
            var max = str.Length - 1;
            while (pos <= max && InBetween(str[pos], '0', '9'))
            {
                x = x * 10 + (ulong)(str[pos] - '0');
                pos++;
            }

            result = x;
            return true;
        }

        /// <summary>
        /// Modulo for pow^2 values...
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1615:ElementReturnValueMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ModPow2(int input, int ceil) => input & (ceil - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Pow2(this int value) => 1 << BitBoards.Msb(value).AsInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEven(this int value) => (value & 1) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOdd(this int value) => !value.IsEven();
    }
}