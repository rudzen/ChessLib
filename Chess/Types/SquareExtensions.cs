/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2018 Rudy Alex Kohn

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

    public static class SquareExtensions
    {
        // TODO : Add distance for files/ranks as well
        private static readonly int[,] Distance = new int[64, 64]; // chebyshev distance

        private static readonly char[] RankChars =
            {
                '1', '2', '3', '4', '5', '6', '7', '8'
            };

        private static readonly char[] FileChars =
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'
            };

        private static readonly string[] SquareStrings =
            {
                "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
                "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
                "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
                "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
                "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
                "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
                "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
                "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8"
            };

        // TODO : Replace with relative methods
        private static readonly Square[,] Flip = new Square[2, 64];

        private static readonly Square[] RookCastlesTo = new Square[64]; // indexed by position of the king

        static SquareExtensions()
        {
            // generate square flipping array for both sides
            foreach (Square square in BitBoards.AllSquares)
            {
                int file = square.File();
                ERank rank = square.RankOf();
                Flip[0, square.ToInt()] = file + ((7 - (int)rank) << 3);
                Flip[1, square.ToInt()] = file + ((int)rank << 3);

                // distance computation
                foreach (Square distSquare in BitBoards.AllSquares)
                {
                    int ranks = Math.Abs(rank - distSquare.RankOf());
                    int files = Math.Abs((int)rank - distSquare.File());
                    Distance[square.ToInt(), distSquare.ToInt()] = ranks.Max(files);
                }
            }

            // generate rook castleling destination squares for both sides
            RookCastlesTo[Flip[PlayerExtensions.White.Side, (int)ESquare.g1].ToInt()] = Flip[PlayerExtensions.White.Side, (int)ESquare.f1];
            RookCastlesTo[Flip[PlayerExtensions.White.Side, (int)ESquare.c1].ToInt()] = Flip[PlayerExtensions.White.Side, (int)ESquare.d1];
            RookCastlesTo[Flip[PlayerExtensions.Black.Side, (int)ESquare.g1].ToInt()] = Flip[PlayerExtensions.Black.Side, (int)ESquare.f1];
            RookCastlesTo[Flip[PlayerExtensions.Black.Side, (int)ESquare.c1].ToInt()] = Flip[PlayerExtensions.Black.Side, (int)ESquare.d1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid(this Square s) => s.Value <= ESquare.h8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidEp(this Square s) => s.RankOf() == ERank.Rank3 || s.RankOf() == ERank.Rank6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDark(this Square s) => (s & BitBoards.DarkSquares) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this Square s) => (int)s.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ERank RankOf(this Square s) => (ERank)(s.ToInt() >> 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char RankOfChar(this Square s) => RankChars[(int)s.RankOf()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ERank RelativeRank(this ERank rank, Player color) => (ERank)((int)rank ^ (color.Side * 7));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ERank RelativeRank(this Square s, Player color) => s.RankOf().RelativeRank(color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int File(this Square s) => s.ToInt() & 7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char FileChar(this Square s) => FileChars[s.File()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOppositeOf(this Square @this, Square other)
        {
            int s = @this.ToInt() ^ other.ToInt();
            return (((s >> 3) ^ s) & 1) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref string GetSquareString(this Square s) => ref SquareStrings[s.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetDistance(this Square source, Square destination) => Distance[source.ToInt(), destination.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPromotionRank(this Square square) => (BitBoards.PromotionRanks & square) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardSquare(this Square sq) => BitBoards.BbSquares[sq.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardSquare(this ESquare sq) => BitBoards.BbSquares[(int)sq];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Square GetRookCastleTo(this Square square) => RookCastlesTo[square.ToInt()];

        /// <summary>
        /// Uses a file(column) index and not a square
        /// </summary>
        /// <param name="fileIndex">The file</param>
        /// <param name="player">The player</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Square GetFlip(int fileIndex, Player player) => Flip[player.Side, fileIndex];
    }
}