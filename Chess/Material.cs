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

namespace Rudz.Chess
{
    using Enums;
    using Extensions;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class Material : IMaterial
    {
        private static readonly int[] PieceBitShift = { 0, 4, 8, 12, 16, 20 };

        private readonly uint[] _key;

        public static double MaxValueWithoutPawns { get; } = 2.0 * (2 * (int)PieceValues.Knight + 2 * (int)PieceValues.Bishop + 2 * (int)PieceValues.Rook + (int)PieceValues.Queen);

        public static double MaxValue { get; } = (int)(MaxValueWithoutPawns + 2 * 8 * (int)PieceValues.Pawn);

        public int[] MaterialValue { get; }

        public int MaterialValueTotal => MaterialValue[0] - MaterialValue[1];

        public int MaterialValueWhite => MaterialValue[0];

        public int MaterialValueBlack => MaterialValue[1];

        int IMaterial.this[int index]
        {
            get => MaterialValue[index];
            set => MaterialValue[index] = value;
        }

        public Material()
        {
            MaterialValue = new int[2];
            _key = new uint[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Piece piece)
        {
            UpdateKey(piece.ColorOf(), piece.Type(), 1);
            MaterialValue[piece.ColorOf()] += (int)piece.PieceValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateKey(Player side, PieceTypes pieceType, int delta)
        {
            if (pieceType == PieceTypes.King)
                return;

            var x = Count(side, pieceType) + delta;

            _key[side.Side] &= ~(15u << PieceBitShift[(int)pieceType]);
            _key[side.Side] |= (uint)(x << PieceBitShift[(int)pieceType]);
        }

        public uint GetKey(int index)
        {
            return _key[index];
        }

        public void MakeMove(Move move)
        {
            if (move.IsCaptureMove())
                Remove(move.GetCapturedPiece());

            if (move.IsPromotionMove())
            {
                Remove(move.GetMovingPiece());
                Add(move.GetPromotedPiece());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(Player side, PieceTypes pieceType) => (int)((_key[side.Side] >> PieceBitShift[(int)pieceType]) & 15u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _key.Clear();
            MaterialValue.Clear();
        }

        public void CopyFrom(IMaterial material)
        {
            if (material == null)
                return;
            material.MaterialValue.CopyTo(MaterialValue, 0);
            _key[0] = material.GetKey(0);
            _key[1] = material.GetKey(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove(Piece piece)
        {
            UpdateKey(piece.ColorOf(), piece.Type(), -1);
            MaterialValue[piece.ColorOf()] -= (int)piece.PieceValue();
        }
    }
}