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
    using System.Collections;
    using System.Runtime.CompilerServices;
    using Enums;
    using Extensions;

    public static class PieceExtensions
    {
        public static readonly Piece EmptyPiece = EPieces.WhiteNoPiece;

        public static readonly EPieceValue[] PieceValues = { EPieceValue.Pawn, EPieceValue.Knight, EPieceValue.Bishop, EPieceValue.Rook, EPieceValue.Queen, EPieceValue.King };

        internal static readonly char[] PieceChars = { 'P', 'N', 'B', 'R', 'Q', 'K', ' ', '.', 'p', 'n', 'b', 'r', 'q', 'k', ' ', '.' };

        private static readonly char[] PromotionPieceNotation = { 'p', 'n', 'b', 'r', 'q', 'k' };

        private static readonly string[] PieceStrings = { "P", "N", "B", "R", "Q", "K", "  ", ".", "p", "n", "b", "r", "q", "k", "  ", "." };

        private static readonly char[] PgnPieceChars = { 'P', 'N', 'B', 'R', 'Q', 'K' };

        private static readonly string[] PieceNames = { "Pawn", "Knight", "Bishop", "Rook", "Queen", "King", "None", "None", "Pawn", "Knight", "Bishop", "Rook", "Queen", "King", "None", "None" };

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

        private static readonly string[] PieceUnicode = { "\u2659", "\u2658", "\u2657", "\u2656", "\u2655", "\u2654", "    ", ".", "\u265F", "\u265E", "\u265D", "\u265C", "\u265B", "\u265A", "    ", "." };

        public static readonly char[] PieceUnicodeChar = { '\u2659', '\u2658', '\u2657', '\u2656', '\u2655', '\u2654', ' ', '.', '\u265F', '\u265E', '\u265D', '\u265C', '\u265B', '\u265A', ' ', '.' };

        private static readonly BitArray WhitePieces = new BitArray(new[] { true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false });

        private static readonly BitArray BlackPieces = new BitArray(new[] { false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true });

        // for polyglot support in the future
        private static readonly char[] BookPieceNames = { 'p', 'P', 'n', 'N', 'b', 'B', 'r', 'R', 'q', 'Q', 'k', 'K' };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Empty(this Piece p) => p.ToInt().InBetween((int)EPieces.WhitePawn, (int)EPieces.WhiteKing) && p.ToInt().InBetween((int)EPieces.BlackPawn, (int)EPieces.BlackKing);

        // Generic helper functions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhite(this Piece p) => p.ToInt().InBetween((int)EPieces.WhitePawn, (int)EPieces.WhiteKing);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlack(this Piece p) => p.ToInt().InBetween((int)EPieces.BlackPawn, (int)EPieces.BlackKing);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Player Side(this Piece p) => p.IsWhite() ? 0 : p.IsBlack() ? 1 : 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this Piece p) => (int)p.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this EPieceType p) => (int)p;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EPieceType Type(this Piece p) => (EPieceType)(p.ToInt() & 0x7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NoPiece(this Piece p) => p.Type() == EPieceType.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPieceChar(this Piece p) => PieceChars[p.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPieceChar(this EPieceType p) => PieceChars[(int)p];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPieceString(this Piece p) => PieceStrings[p.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPieceCharUnicode(this Piece p) => PieceUnicodeChar[p.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPieceWhite(int code) => WhitePieces[code];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EPieceValue PieceValue(this Piece p) => PieceValues[(int)p.Type()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref string GetName(this Piece p) => ref PieceNames[p.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref string GetName(this EPieces p) => ref PieceNames[(int)p];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPromotionChar(this Piece p) => PromotionPieceNotation[(int)p.Type()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetPgnChar(this Piece p) => PgnPieceChars[(int)p.Type()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetUnicodeChar(this Piece p) => PieceUnicodeChar[p.ToInt()];
    }
}