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
    using Types;

    public interface IMaterial
    {
        int MaterialValueTotal { get; }
        int MaterialValueWhite { get; }
        int MaterialValueBlack { get; }

        int[] MaterialValue { get; }

        int this[int index] { get; set; }

        void Add(Piece piece);

        void UpdateKey(Player side, PieceTypes pieceType, int delta);

        uint GetKey(int index);

        void SetKey(int index, uint value);

        void MakeMove(IPosition pos, Move move);

        int Count(Player side, PieceTypes pieceType);

        void Clear();

        void CopyFrom(IMaterial material);

        void CopyTo(IMaterial material);
    }
}