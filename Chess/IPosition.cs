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
    using System.Text;
    using Types;
    using Validation;

    public interface IPosition : IEnumerable<Piece>
    {
        bool IsProbing { get; set; }

        Action<Piece, Square> PieceUpdated { get; set; }

        bool Chess960 { get; set; }

        Player SideToMove { get; }

        Square EnPassantSquare { get; }

        string FenNotation { get; }

        IBoard Board { get; }

        IPieceValue PieceValue { get; }

        BitBoard Checkers { get; }

        int Rule50 { get; }

        int Ply { get; }

        bool InCheck { get; }

        bool IsRepetition { get; }

        State State { get; }

        bool IsMate { get; }

        void Clear();

        void AddPiece(Piece pc, Square sq);

        void MakeMove(Move m, State newState);

        void MakeMove(Move m, State newState, bool givesCheck);

        void MakeNullMove(State newState);

        void TakeMove(Move m);

        void TakeNullMove();

        Piece GetPiece(Square sq);

        PieceTypes GetPieceType(Square sq);

        bool IsPieceTypeOnSquare(Square sq, PieceTypes pt);

        BitBoard GetPinnedPieces(Square sq, Player c);

        BitBoard CheckedSquares(PieceTypes pt);

        BitBoard PinnedPieces(Player c);

        BitBoard BlockersForKing(Player c);

        bool IsOccupied(Square sq);

        bool IsAttacked(Square sq, Player c);

        bool GivesCheck(Move m);

        BitBoard Pieces();

        BitBoard Pieces(Player c);

        BitBoard Pieces(Piece pc);

        BitBoard Pieces(PieceTypes pt);

        BitBoard Pieces(PieceTypes pt1, PieceTypes pt2);

        BitBoard Pieces(PieceTypes pt, Player side);

        BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player side);

        ReadOnlySpan<Square> Squares(PieceTypes pt, Player c);

        Square GetPieceSquare(PieceTypes pt, Player color);

        Square GetKingSquare(Player color);

        Piece MovedPiece(Move m);

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

        bool CanCastle(CastlelingRights cr);

        bool CanCastle(Player color);

        bool CastlingImpeded(CastlelingRights cr);

        Square CastlingRookSquare(CastlelingRights cr);

        CastlelingRights GetCastlelingRightsMask(Square sq);

        bool IsPseudoLegal(Move m);

        bool IsLegal(Move m);

        FenData GenerateFen();

        FenError SetFen(FenData fen, bool validate = false);

        HashKey GetPiecesKey();

        HashKey GetPawnKey();

        BitBoard GetAttacks(Square square, PieceTypes pt, BitBoard occupied);

        BitBoard GetAttacks(Square square, PieceTypes pt);

        void MoveToString(Move m, StringBuilder output);

        bool HasGameCycle(int ply);

        bool SeeGe(Move m, Value threshold);
        
        IPositionValidator Validate(PositionValidationTypes type = PositionValidationTypes.Basic);
    }
}