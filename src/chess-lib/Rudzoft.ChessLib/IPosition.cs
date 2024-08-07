﻿/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using System.Text;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;

namespace Rudzoft.ChessLib;

public interface IPosition : IEnumerable<Piece>
{
    bool IsProbing { get; set; }

    Action<IPieceSquare> PieceUpdated { get; set; }

    ChessMode ChessMode { get; set; }

    Color SideToMove { get; }

    Square EnPassantSquare { get; }

    string FenNotation { get; }

    IBoard Board { get; }

    IZobrist Zobrist { get; }

    IValues Values { get; }

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

    PieceType GetPieceType(Square sq);

    bool IsPieceTypeOnSquare(Square sq, PieceType pt);

    BitBoard CheckedSquares(PieceType pt);

    BitBoard PinnedPieces(Color c);

    BitBoard KingBlockers(Color c);

    bool IsKingBlocker(Color c, Square sq);

    BitBoard SliderBlockerOn(Square sq, BitBoard attackers, ref BitBoard pinners, ref BitBoard hidders);

    bool IsOccupied(Square sq);

    bool IsAttacked(Square sq, Color c);

    bool GivesCheck(Move m);

    BitBoard Pieces();

    BitBoard Pieces(Color c);

    BitBoard Pieces(Piece pc);

    BitBoard Pieces(PieceType pt);

    BitBoard Pieces(PieceType pt1, PieceType pt2);

    BitBoard Pieces(PieceType pt, Color c);

    BitBoard Pieces(PieceType pt1, PieceType pt2, Color c);

    int PieceCount();

    int PieceCount(Piece pc);

    int PieceCount(PieceType pt);

    int PieceCount(PieceType pt, Color c);

    BitBoard PawnsOnColor(Color c, Square sq);

    bool SemiOpenFileOn(Color c, Square sq);

    bool BishopPaired(Color c);

    bool BishopOpposed();

    Square GetPieceSquare(PieceType pt, Color c);

    Square GetKingSquare(Color c);

    Piece MovedPiece(Move m);

    bool PieceOnFile(Square sq, Color c, PieceType pt);

    bool PawnIsolated(Square sq, Color c);

    bool PassedPawn(Square sq);

    void RemovePiece(Square sq);

    BitBoard AttacksTo(Square sq, in BitBoard occ);

    BitBoard AttacksTo(Square sq);

    bool AttackedBySlider(Square sq, Color c);

    bool AttackedByKnight(Square sq, Color c);

    bool AttackedByPawn(Square sq, Color c);

    bool AttackedByKing(Square sq, Color c);

    BitBoard AttacksBy(PieceType pt, Color c);

    bool IsCapture(Move m);

    bool IsCaptureOrPromotion(Move m);

    bool IsPawnPassedAt(Color c, Square sq);

    BitBoard PawnPassSpan(Color c, Square sq);

    bool CanCastle(CastleRight cr);

    bool CanCastle(Color c);

    public ref BitBoard CastleKingPath(CastleRight cr);

    bool CastlingImpeded(CastleRight cr);

    Square CastlingRookSquare(CastleRight cr);

    CastleRight GetCastleRightsMask(Square sq);

    bool IsPseudoLegal(Move m);

    bool IsLegal(Move m);

    FenData GenerateFen();

    IPosition Set(in FenData fenData, ChessMode chessMode, in State state, bool validate = false, int searcher = 0);

    IPosition Set(string fen, ChessMode chessMode, in State state, bool validate = false, int searcher = 0);

    IPosition Set(ReadOnlySpan<char> code, Color c, in State state);

    HashKey GetKey(State state);

    HashKey GetPawnKey();

    BitBoard GetAttacks(Square sq, PieceType pt, in BitBoard occ);

    BitBoard GetAttacks(Square sq, PieceType pt);

    void MoveToString(Move m, in StringBuilder output);

    bool HasGameCycle(int ply);

    bool HasRepetition();

    bool IsDraw(int ply);

    bool SeeGe(Move m, Value threshold);

    Value NonPawnMaterial(Color c);

    Value NonPawnMaterial();

    HashKey MovePositionKey(Move m);

    PositionValidationResult Validate(PositionValidationTypes type = PositionValidationTypes.Basic);
}
