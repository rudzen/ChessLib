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

namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class Material
    {
        public static readonly double MaxValueWithoutPawns;

        public static readonly int MaxValue;

        private static readonly int[] PieceBitShift;

        private readonly uint[] _key = { 0, 0 };

        public int[] MaterialValue { get; }

        public int MaterialValueTotal => MaterialValue[0] - MaterialValue[1];

        public int MaterialValueWhite => MaterialValue[0];

        public int MaterialValueBlack => MaterialValue[1];

        public int this[int index]
        {
            get => MaterialValue[index];
            set => MaterialValue[index] = value;
        }

        static Material()
        {
            MaxValueWithoutPawns = 2 * (2 * (double)PieceExtensions.PieceValues[1]) + 2 * (double)PieceExtensions.PieceValues[2] + 2 * (double)PieceExtensions.PieceValues[3] + (double)PieceExtensions.PieceValues[4];
            MaxValue = (int)(MaxValueWithoutPawns + 2 * 8 * (int)PieceExtensions.PieceValues[0]);
            PieceBitShift = new[] { 0, 4, 8, 12, 16, 20 };
        }

        public Material()
        {
            MaterialValue = new int[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Piece piece)
        {
            UpdateKey(piece.ColorOf(), piece.Type(), 1);
            MaterialValue[piece.ColorOf()] += (int)piece.PieceValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateKey(Player side, EPieceType pieceType, int delta)
        {
            if (pieceType == EPieceType.King)
                return;

            var x = Count(side, pieceType) + delta;

            _key[side.Side] &= ~(15u << PieceBitShift[(int)pieceType]);
            _key[side.Side] |= (uint)(x << PieceBitShift[(int)pieceType]);
        }

        public void MakeMove(Move move)
        {
            if (move.IsCaptureMove())
                Remove(move.GetCapturedPiece());

            if (!move.IsPromotionMove())
                return;

            Remove(move.GetMovingPiece());
            Add(move.GetPromotedPiece());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(Player side, EPieceType pieceType) => (int)((_key[side.Side] >> PieceBitShift[(int)pieceType]) & 15u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _key.Clear();
            MaterialValue.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove(Piece piece)
        {
            UpdateKey(piece.ColorOf(), piece.Type(), -1);
            MaterialValue[piece.ColorOf()] -= (int)piece.PieceValue();
        }
    }
}