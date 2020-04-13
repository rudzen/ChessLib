/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2020 Rudy Alex Kohn

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
    using Enums;
    using System;
    using System.Runtime.CompilerServices;
    using Types;

    public static class CastlelingExtensions
    {
        private static readonly CastlelingRights[] OoAllowedMask = { CastlelingRights.WhiteOo, CastlelingRights.BlackOo };

        private static readonly CastlelingRights[] OooAllowedMask = { CastlelingRights.WhiteOoo, CastlelingRights.BlackOoo };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square GetKingCastleTo(this CastlelingSides castleType, Player side)
        {
            var isKingSide = castleType == CastlelingSides.King;
            return (isKingSide ? Squares.g1 : Squares.c1).RelativeSquare(side);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CastlelingRights GetCastleAllowedMask(this CastlelingSides castleType, Player side)
        {
            return castleType switch
            {
                CastlelingSides.King => OoAllowedMask[side.Side],
                CastlelingSides.Queen => OooAllowedMask[side.Side],
                _ => throw new ArgumentOutOfRangeException(nameof(castleType), castleType, null)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCastlelingString(this CastlelingSides @this)
        {
            return @this switch
            {
                CastlelingSides.King => "O-O",
                CastlelingSides.Queen => "O-O-O",
                CastlelingSides.None => string.Empty,
                CastlelingSides.CastleNb => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCastlelingString(Square toSquare, Square fromSquare) => toSquare < fromSquare ? "O-O-O" : "O-O";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this CastlelingSides value, CastlelingSides flag) => (value & flag) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this CastlelingRights value, CastlelingRights flag) => (value & flag) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt(this CastlelingRights value) => (int)value;
    }
}