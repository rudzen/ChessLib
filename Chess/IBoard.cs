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
    }
}