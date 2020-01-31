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

namespace Rudz.Chess.Types
{
    using Enums;
    using Extensions;
    using System;
    using System.Runtime.CompilerServices;

    public static class PlayerExtensions
    {
        public static readonly Player White = 0;

        public static readonly Player Black = 1;

        private static readonly Direction[] PawnPushDist = { Directions.North, Directions.South };

        private static readonly Direction[] PawnDoublePushDist = { Directions.NorthDouble, Directions.SouthDouble };

        private static readonly Direction[] PawnWestAttackDist = { Directions.NorthEast, Directions.SouthEast };

        private static readonly Direction[] PawnEastAttackDist = { Directions.NorthWest, Directions.SouthWest };

        private static readonly string[] PlayerColors = { "White", "Black" };

        private static readonly Func<BitBoard, BitBoard>[] PawnPushModifiers = { BitBoards.NorthOne, BitBoards.SouthOne };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLegalPlayer(this Player player) => player.Side.InBetween(White.Side, Black.Side);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref string GetName(this Player player) => ref PlayerColors[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhite(this Player player) => player.Side == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlack(this Player player) => player.Side != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction PawnPushDistance(this Player player) => PawnPushDist[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction PawnDoublePushDistance(this Player player) => PawnDoublePushDist[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction PawnWestAttackDistance(this Player player) => PawnWestAttackDist[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Direction PawnEastAttackDistance(this Player player) => PawnEastAttackDist[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PawnPush(this Player player, BitBoard bitBoard) => PawnPushModifiers[player.Side](bitBoard);
    }
}