/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2021 Rudy Alex Kohn

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
    using System;
    using System.Collections.Generic;
    using Types;

    public interface IBoard : IEnumerable<Piece>
    {
        void Clear();
        Piece PieceAt(Square sq);
        bool IsEmpty(Square sq);
        void AddPiece(Piece pc, Square sq);
        void RemovePiece(Square sq);
        void ClearPiece(Square sq);
        void MovePiece(Square from, Square to);
        Piece MovedPiece(Move move);
        BitBoard Pieces();
        BitBoard Pieces(PieceTypes pt);
        BitBoard Pieces(PieceTypes pt1, PieceTypes pt2);
        BitBoard Pieces(Player c);
        BitBoard Pieces(Player c, PieceTypes pt);
        BitBoard Pieces(Player c, PieceTypes pt1, PieceTypes pt2);
        Square Square(PieceTypes pt, Player c);
        ReadOnlySpan<Square> Squares(PieceTypes pt, Player c);
        int PieceCount(PieceTypes pt, Player c);
        int PieceCount(PieceTypes pt);
        int PieceCount();
    }
}