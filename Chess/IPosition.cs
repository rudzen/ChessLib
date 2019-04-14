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
    using System;
    using System.Collections.Generic;
    using Types;

    public interface IPosition : IEnumerable<Piece>
    {
        BitBoard[] BoardPieces { get; }

        BitBoard[] OccupiedBySide { get; }

        Square[] KingSquares { get; }

        bool IsProbing { get; set; }

        BitBoard Occupied { get; set; }

        Piece[] BoardLayout { get; }

        Action<Piece, Square> PieceUpdated { get; }

        void Clear();

        void AddPiece(Piece piece, Square square);

        void AddPiece(EPieceType pieceType, Square square, Player side);

        void MakeMove(Move move);

        void TakeMove(Move move);

        Piece GetPiece(Square square);

        EPieceType GetPieceType(Square square);

        bool IsPieceTypeOnSquare(Square square, EPieceType pieceType);

        BitBoard GetPinnedPieces(Square square, Player side);

        bool IsOccupied(Square square);

        bool IsAttacked(Square square, Player side);

        BitBoard PieceAttacks(Square square, EPieceType pieceType);

        BitBoard Pieces(Player side);

        BitBoard Pieces(EPieceType type);

        BitBoard Pieces(EPieceType type1, EPieceType type2);

        BitBoard Pieces(EPieceType type, Player side);

        BitBoard Pieces(EPieceType type1, EPieceType type2, Player side);

        bool PieceOnFile(Square square, Player side, EPieceType pieceType);

        bool PawnIsolated(Square square, Player side);

        bool PassedPawn(Square square);

        void RemovePiece(Square square, Piece piece);

        BitBoard AttacksTo(Square square, BitBoard occupied);

        BitBoard AttacksTo(Square square);

        bool AttackedBySlider(Square square, Player side);

        bool AttackedByKnight(Square square, Player side);

        bool AttackedByPawn(Square square, Player side);

        bool AttackedByKing(Square square, Player side);

        Square GetRookCastleFrom(Square index);

        void SetRookCastleFrom(Square index, Square square);

        Square GetKingCastleFrom(Player side, ECastleling castleType);

        void SetKingCastleFrom(Player side, Square square, ECastleling castleType);

        ECastleling IsCastleMove(string m);

        Move StringToMove(string m, State state);
    }
}