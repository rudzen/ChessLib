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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Chess.Test")]

namespace Rudz.Chess.Types
{
    using Enums;
    using System.Runtime.CompilerServices;

    public static class SquareExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidEp(this Square s) => s.Rank == Ranks.Rank3 || s.Rank == Ranks.Rank6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidEp(this Square s, Player c) => s.RelativeRank(c) == Ranks.Rank3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPromotionRank(this Square square) => (BitBoards.PromotionRanksBB & square) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardSquare(this Squares sq) => BitBoards.BbSquares[(int)sq];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square RelativeSquare(this Squares sq, Player c) => (int)sq ^ (c.Side * 56);
    }
}