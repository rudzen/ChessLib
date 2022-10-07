/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

using System;
using System.Collections.Generic;
using System.Text;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib;

public interface IPosition : IEnumerable<Piece>
{
    bool IsProbing { get; set; }

    Action<IPieceSquare> PieceUpdated { get; set; }

    ChessMode ChessMode { get; set; }

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

    int Searcher { get;}

    void Clear();

    void AddPiece(Piece pc, Square sq);

    void MakeMove(Move m, in State newState);

    void MakeMove(Move m, in State newState, bool givesCheck);

    void MakeNullMove(in State newState);

    void TakeMove(Move m);

    void TakeNullMove();

    Piece GetPiece(Square sq);

    PieceTypes GetPieceType(Square sq);

    bool IsPieceTypeOnSquare(Square sq, PieceTypes pt);

    BitBoard CheckedSquares(PieceTypes pt);

    BitBoard PinnedPieces(Player p);

    BitBoard KingBlockers(Player p);

    bool IsKingBlocker(Player p, Square sq);

    BitBoard SliderBlockerOn(Square sq, BitBoard attackers, ref BitBoard pinners, ref BitBoard hidders);

    bool IsOccupied(Square sq);

    bool IsAttacked(Square sq, Player p);

    bool GivesCheck(Move m);

    BitBoard Pieces();

    BitBoard Pieces(Player p);

    BitBoard Pieces(Piece pc);

    BitBoard Pieces(PieceTypes pt);

    BitBoard Pieces(PieceTypes pt1, PieceTypes pt2);

    BitBoard Pieces(PieceTypes pt, Player p);

    BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player p);

    BitBoard PawnsOnColor(Player p, Square sq);

    bool SemiOpenFileOn(Player p, Square sq);

    bool BishopPaired(Player p);

    bool BishopOpposed();

    ReadOnlySpan<Square> Squares(PieceTypes pt, Player p);

    Square GetPieceSquare(PieceTypes pt, Player p);

    Square GetKingSquare(Player p);

    Piece MovedPiece(Move m);

    bool PieceOnFile(Square sq, Player p, PieceTypes pt);

    bool PawnIsolated(Square sq, Player p);

    bool PassedPawn(Square sq);

    void RemovePiece(Square sq);

    BitBoard AttacksTo(Square sq, in BitBoard occ);

    BitBoard AttacksTo(Square sq);

    bool AttackedBySlider(Square sq, Player p);

    bool AttackedByKnight(Square sq, Player p);

    bool AttackedByPawn(Square sq, Player p);

    bool AttackedByKing(Square sq, Player p);

    BitBoard AttacksBy(PieceTypes pt, Player p);

    bool IsCapture(Move m);

    bool IsCaptureOrPromotion(Move m);

    bool IsPawnPassedAt(Player p, Square sq);

    bool CanCastle(CastleRight cr);

    bool CanCastle(Player p);

    public ref BitBoard CastleKingPath(CastleRight cr);

    bool CastlingImpeded(CastleRight cr);

    Square CastlingRookSquare(CastleRight cr);

    CastleRight GetCastleRightsMask(Square sq);

    bool IsPseudoLegal(Move m);

    bool IsLegal(Move m);

    FenData GenerateFen();

    void Set(in FenData fenData, ChessMode chessMode, State state, bool validate = false, int searcher = 0);

    HashKey GetPiecesKey();

    HashKey GetPawnKey();

    BitBoard GetAttacks(Square sq, PieceTypes pt, in BitBoard occ);

    BitBoard GetAttacks(Square sq, PieceTypes pt);

    void MoveToString(Move m, in StringBuilder output);

    bool HasGameCycle(int ply);

    bool HasRepetition();

    bool IsDraw(int ply);

    bool SeeGe(Move m, Value threshold);

    IPositionValidator Validate(PositionValidationTypes type = PositionValidationTypes.Basic);
}
