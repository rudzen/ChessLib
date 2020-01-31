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

        private static readonly Square[] OoKingTo = { ESquare.g1, ESquare.g8 };

        private static readonly Square[] OooKingTo = { ESquare.c1, ESquare.c8 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square GetKingCastleTo(this CastlelingSides castleType, Player side)
        {
            switch (castleType)
            {
                case CastlelingSides.King:
                    return OoKingTo[side.Side];

                case CastlelingSides.Queen:
                    return OooKingTo[side.Side];

                default:
                    throw new ArgumentOutOfRangeException(nameof(castleType), castleType, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CastlelingRights GetCastleAllowedMask(this CastlelingSides castleType, Player side)
        {
            switch (castleType)
            {
                case CastlelingSides.King:
                    return OoAllowedMask[side.Side];

                case CastlelingSides.Queen:
                    return OooAllowedMask[side.Side];

                default:
                    throw new ArgumentOutOfRangeException(nameof(castleType), castleType, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetCastlelingString(this CastlelingSides @this)
        {
            switch (@this)
            {
                case CastlelingSides.King:
                    return "O-O";

                case CastlelingSides.Queen:
                    return "O-O-O";

                case CastlelingSides.None:
                    return string.Empty;

                case CastlelingSides.CastleNb:
                    return string.Empty;

                default:
                    throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
            }
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