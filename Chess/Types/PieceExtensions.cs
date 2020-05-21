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
    using System.Runtime.CompilerServices;

    public static class PieceExtensions
    {
        private const string PgnPieceChars = " PNBRQK";

        internal const string PieceChars = PgnPieceChars + "  pnbrqk";

        private const string PromotionPieceNotation = "  nbrq";

        // for polyglot support in the future
        public const string BookPieceNames = "pPnNbBrRqQkK";

        private static readonly string[] PieceStrings = { " ", "P", "N", "B", "R", "Q", "K", " ", " ", "p", "n", "b", "r", "q", "k" };

        private static readonly string[] PieceNames = { "None", "Pawn", "Knight", "Bishop", "Rook", "Queen", "King" };

        /*
 * white chess king 	♔ 	U+2654 	&#9812;
 * white chess queen 	♕ 	U+2655 	&#9813;
 * white chess rook 	♖ 	U+2656 	&#9814;
 * white chess bishop 	♗ 	U+2657 	&#9815;
 * white chess knight 	♘ 	U+2658 	&#9816;
 * white chess pawn 	♙ 	U+2659 	&#9817;
 * black chess king 	♚ 	U+265A 	&#9818;
 * black chess queen 	♛ 	U+265B 	&#9819;
 * black chess rook 	♜ 	U+265C 	&#9820;
 * black chess bishop 	♝ 	U+265D 	&#9821;
 * black chess knight 	♞ 	U+265E 	&#9822;
 * black chess pawn 	♟ 	U+265F 	&#9823;
         */

        private static readonly char[] PieceUnicodeChar = { ' ', '\u2659', '\u2658', '\u2657', '\u2656', '\u2655', '\u2654', ' ', ' ', '\u265F', '\u265E', '\u265D', '\u265C', '\u265B', '\u265A', ' ' };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPieceChar(this Piece p) => PieceChars[p.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPieceChar(this PieceTypes p) => PieceChars[(int)p];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPieceString(this Piece p) => PieceStrings[p.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName(this Piece p) => PieceNames[p.Type().AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPromotionChar(this PieceTypes p) => PromotionPieceNotation[p.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPgnChar(this Piece p) => PgnPieceChars[(int)p.Type()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetUnicodeChar(this Piece p) => PieceUnicodeChar[p.AsInt()];
    }
}