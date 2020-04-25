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
    using Fen;
    using System;
    using System.Collections.Generic;
    using Types;

    public interface IPosition : IEnumerable<Piece>
    {
        BitBoard[] BoardPieces { get; }

        BitBoard[] OccupiedBySide { get; }

        bool IsProbing { get; set; }

        Piece[] BoardLayout { get; }

        Action<Piece, Square> PieceUpdated { get; set; }

        State State { get; set; }

        void Clear();

        void AddPiece(Piece pc, Square sq);

        void AddPiece(PieceTypes pt, Square sq, Player c);

        void MovePiece(Square from, Square to);

        bool MakeMove(Move m);

        void TakeMove(Move m);

        Piece GetPiece(Square sq);

        PieceTypes GetPieceType(Square sq);

        bool IsPieceTypeOnSquare(Square sq, PieceTypes pt);

        BitBoard GetPinnedPieces(Square sq, Player c);

        bool IsOccupied(Square sq);

        bool IsAttacked(Square sq, Player c);

        BitBoard PieceAttacks(Square sq, PieceTypes pt);

        BitBoard Pieces();

        BitBoard Pieces(Player c);

        BitBoard Pieces(Piece pc);

        BitBoard Pieces(PieceTypes pt);

        BitBoard Pieces(PieceTypes pt1, PieceTypes pt2);

        BitBoard Pieces(PieceTypes pt, Player side);

        BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player side);

        Square GetPieceSquare(PieceTypes pt, Player color);

        bool PieceOnFile(Square square, Player side, PieceTypes pieceType);

        bool PawnIsolated(Square square, Player side);

        bool PassedPawn(Square square);

        void RemovePiece(Square square);

        BitBoard AttacksTo(Square sq, BitBoard occupied);

        BitBoard AttacksTo(Square sq);

        bool AttackedBySlider(Square sq, Player c);

        bool AttackedByKnight(Square sq, Player c);

        bool AttackedByPawn(Square sq, Player c);

        bool AttackedByKing(Square sq, Player c);

        Square GetRookCastleFrom(Square sq);

        void SetRookCastleFrom(Square indexSq, Square sq);

        Square GetKingCastleFrom(Player c, CastlelingSides sides);

        void SetKingCastleFrom(Player c, Square sq, CastlelingSides sides);

        CastlelingSides IsCastleMove(string m);

        bool CanCastle(CastlelingSides sides);

        bool IsCastleAllowed(Square sq);

        bool IsPseudoLegal(Move m);

        bool IsLegal(Move m, Piece pc, Square from, MoveTypes type);

        bool IsLegal(Move m);

        bool IsMate();

        FenData GenerateFen();

        HashKey GetPiecesKey();

        HashKey GetPawnKey();
    }
}