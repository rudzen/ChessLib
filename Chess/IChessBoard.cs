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

namespace Rudz.Chess
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Properties;
    using Types;

    public interface IChessBoard : IEnumerable<Piece>
    {
        [NotNull]
        Piece[] BoardLayout { get; }

        BitBoard Occupied { get; }

        Action<Piece, Square> PieceUpdated { get; }

        void AddPiece(EPieceType pieceType, Square square, Player side);

        void AddPiece(Piece piece, Square square);

        bool AttackedByKing(Square square, Player side);

        bool AttackedByKnight(Square square, Player side);

        bool AttackedByPawn(Square square, Player side);

        bool AttackedBySlider(Square square, Player side);

        BitBoard Bishops(Player side);

        void Clear();

        bool Equals(object obj);

        int GetHashCode();

        Piece GetPiece(Square square);

        EPieceType GetPieceType(Square square);

        BitBoard GetPinnedPieces(Square square, Player side);

        Square GetRookCastleFrom(Square index);

        bool IsAttacked(Square square, Player side);

        bool IsOccupied(Square square);

        bool IsPieceTypeOnSquare(Square square, EPieceType pieceType);

        BitBoard King(Player side);

        BitBoard Knights(Player side);

        void MakeMove(Move move);

        bool PawnIsolated(Square square, Player side);

        BitBoard Pawns(Player side);

        BitBoard PieceAttacks(Square square, EPieceType pieceType);

        bool PieceOnFile(Square square, Player side, EPieceType pieceType);

        BitBoard Pieces(Piece piece, Player side);

        BitBoard Queens(Player side);

        void RemovePiece(Square square, Piece piece);

        BitBoard Rooks(Player side);

        void SetRookCastleFrom(Square index, Square square);

        void TakeMove(Move move);
    }
}