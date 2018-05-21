﻿/*
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Enums;
    using Extensions;
    using Properties;
    using Types;

    /// <summary>
    /// The main board representation class.
    /// It stores all the information about the current board in a simple structure.
    /// It also serves the purpose of being able to give the UI controller feedback on various things on the board
    /// </summary>
    public sealed class ChessBoard : IChessBoard
    {
        private const ulong Zero = 0;

        private static readonly Func<BitBoard, BitBoard>[] EnPasCapturePos =
            {
                BitBoards.SouthOne, BitBoards.NorthOne
            };

        [NotNull]
        private readonly Square[] _rookCastlesFrom; // indexed by position of the king

        [NotNull]
        private readonly Square[] _castleShortKingFrom;

        [NotNull]
        private readonly Square[] _castleLongKingFrom;

        public ChessBoard(Action<Piece, Square> pieceUpdateCallback)
        {
            PieceUpdated = pieceUpdateCallback;
            _castleLongKingFrom = new Square[2];
            _rookCastlesFrom = new Square[64];
            _castleShortKingFrom = new Square[2];
            BoardLayout = new Piece[64];
            BoardPieces = new BitBoard[2 << 3];
            OccupiedBySide = new BitBoard[2];
            KingSquares = new Square[2];
            Clear();
        }

        // TODO : redesign BoardPieces + OccupiedBySide into simple arrays
        
        [NotNull]
        public BitBoard[] BoardPieces { get; }

        [NotNull]
        public BitBoard[] OccupiedBySide { get; }

        [NotNull]
        public Square[] KingSquares { get; }

        public bool IsProbing { get; set; }

        public BitBoard Occupied { get; private set; }

        public Piece[] BoardLayout { get; }

        /// <summary>
        /// To let something outside the library be aware of changes (like a UI etc)
        /// </summary>
        public Action<Piece, Square> PieceUpdated { get; }

        public void Clear()
        {
            BoardLayout.Fill(EPieces.NoPiece);
            OccupiedBySide.Fill(Zero);
            KingSquares.Fill(ESquare.none);
            BoardPieces.Fill(Zero);
            Occupied = Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(Piece piece, Square square)
        {
            BitBoard bbsq = square;
            int color = piece.ColorOf();
            BoardPieces[piece.ToInt()] |= bbsq;
            OccupiedBySide[color] |= bbsq;
            Occupied |= bbsq;
            BoardLayout[square.ToInt()] = piece;
            if (piece.Type() == EPieceType.King)
                KingSquares[color] = square;
            if (IsProbing)
                return;
            PieceUpdated?.Invoke(piece, square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(EPieceType pieceType, Square square, Player side)
        {
            Piece piece = pieceType.MakePiece(side);
            BoardPieces[piece.ToInt()] |= square;
            OccupiedBySide[side.Side] |= square;
            Occupied |= square;
            BoardLayout[square.ToInt()] = piece;
            if (pieceType == EPieceType.King)
                KingSquares[side.Side] = square;

            if (IsProbing)
                return;

            PieceUpdated?.Invoke(piece, square);
        }

        public void MakeMove(Move move)
        {
            Square toSquare = move.GetToSquare();

            if (move.IsCastlelingMove()) {
                Piece rook = (int)EPieceType.Rook + move.GetSideMask();
                Piece king = move.GetMovingPiece();
                RemovePiece(_rookCastlesFrom[toSquare.ToInt()], rook);
                RemovePiece(move.GetFromSquare(), king);
                AddPiece(rook, toSquare.GetRookCastleTo());
                AddPiece(king, toSquare);
                KingSquares[move.GetMovingSide().Side] = toSquare;
                return;
            }

            RemovePiece(move.GetFromSquare(), move.GetMovingPiece());

            if (move.IsEnPassantMove()) {
                BitBoard targetSquare = toSquare;
                Square t = EnPasCapturePos[move.GetMovingSide().Side](targetSquare).First();
                RemovePiece(t, move.GetCapturedPiece());
            } else if (move.IsCaptureMove()) {
                RemovePiece(toSquare, move.GetCapturedPiece());
            }

            AddPiece(move.IsPromotionMove() ? move.GetPromotedPiece() : move.GetMovingPiece(), toSquare);

            if (move.GetMovingPieceType() == EPieceType.King)
                KingSquares[move.GetMovingSide().Side] = toSquare;
        }

        public void TakeMove(Move move)
        {
            Square toSquare = move.GetToSquare();

            if (move.IsCastlelingMove()) {
                Piece rook = (int)EPieceType.Rook + move.GetSideMask();
                Piece king = move.GetMovingPiece();
                RemovePiece(toSquare, king);
                RemovePiece(toSquare.GetRookCastleTo(), rook);
                AddPiece(king, move.GetFromSquare());
                AddPiece(rook, _rookCastlesFrom[toSquare.ToInt()]);
                KingSquares[move.GetMovingSide().Side] = move.GetFromSquare();
                return;
            }

            RemovePiece(toSquare, move.IsPromotionMove() ? move.GetPromotedPiece() : move.GetMovingPiece());

            if (move.IsEnPassantMove()) {
                BitBoard targetSquare = toSquare;
                Square t = EnPasCapturePos[move.GetMovingSide().Side](targetSquare).First();
                AddPiece(move.GetCapturedPiece(), t);
            } else if (move.IsCaptureMove()) {
                AddPiece(move.GetCapturedPiece(), toSquare);
            }

            AddPiece(move.GetMovingPiece(), move.GetFromSquare());

            if (move.GetMovingPieceType() == EPieceType.King)
                KingSquares[move.GetMovingSide().Side] = move.GetFromSquare();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square square) => BoardLayout[square.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPieceType GetPieceType(Square square) => BoardLayout[square.ToInt()].Type();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPieceTypeOnSquare(Square square, EPieceType pieceType) => GetPieceType(square) == pieceType;

        /// <summary>
        /// Detects any pinned pieces
        /// For more info : https://en.wikipedia.org/wiki/Pin_(chess)
        /// </summary>
        /// <param name="square">The square</param>
        /// <param name="side">The side</param>
        /// <returns>Pinned pieces as BitBoard</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPinnedPieces(Square square, Player side)
        {
            // TODO : Move into state data structure instead of real-time calculation
            
            BitBoard pinnedPieces = 0;
            int oppShift = ~side << 3;
            BitBoard pinners = square.XrayBishopAttacks(Occupied, OccupiedBySide[side.Side]) & (BoardPieces[(int)EPieceType.Bishop + oppShift] | BoardPieces[(int)EPieceType.Queen | oppShift]);

            while (pinners) {
                pinnedPieces |= pinners.Lsb().BitboardBetween(square) & OccupiedBySide[side.Side];
                pinners--;
            }

            pinners = square.XrayRookAttacks(Occupied, OccupiedBySide[side.Side]) & (BoardPieces[(int)EPieceType.Rook + oppShift] | BoardPieces[(int)EPieceType.Queen | oppShift]);

            while (pinners) {
                pinnedPieces |= pinners.Lsb().BitboardBetween(square) & OccupiedBySide[side.Side];
                pinners--;
            }

            return pinnedPieces;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square square) => (Occupied & square) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square square, Player side) => AttackedBySlider(square, side) || AttackedByKnight(square, side) || AttackedByPawn(square, side) || AttackedByKing(square, side);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard PieceAttacks(Square square, EPieceType pieceType)
        {
            // Impossible to determin which of these types are used more often :-)
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (pieceType) {
                case EPieceType.Knight:
                    return square.KnightAttacks();
                case EPieceType.Bishop:
                    return square.BishopAttacks(Occupied);
                case EPieceType.Rook:
                    return square.RookAttacks(Occupied);
                case EPieceType.Queen:
                    return square.QueenAttacks(Occupied);
                case EPieceType.King:
                    return square.KingAttacks();
            }
            return BitBoards.ZeroBb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Piece piece, Player side) => BoardPieces[(int)piece.Type() | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pawns(Player side) => BoardPieces[(int)EPieceType.Pawn | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Knights(Player side) => BoardPieces[(int)EPieceType.Knight | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Bishops(Player side) => BoardPieces[(int)EPieceType.Bishop | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Rooks(Player side) => BoardPieces[(int)EPieceType.Rook | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Queens(Player side) => BoardPieces[(int)EPieceType.Queen | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard King(Player side) => BoardPieces[(int)EPieceType.King | (side << 3)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PieceOnFile(Square square, Player side, EPieceType pieceType) => (BoardPieces[(int)(pieceType + (side << 3))] & square) != 0;

        /// <summary>
        /// Determin if a pawn is isolated e.i. no own pawns on either neighboor files
        /// </summary>
        /// <param name="square"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PawnIsolated(Square square, Player side)
        {
            BitBoard b = (square.PawnAttackSpan(side) | square.PawnAttackSpan(~side)) & Pawns(side);
            return b.Empty();
        }

        /// <summary>
        /// Determin if a specific square is a passed pawn
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PassedPawn(Square square)
        {
            Piece pc = BoardLayout[square.ToInt()];
            Player c = pc.ColorOf();
            if (pc.Type() != EPieceType.Pawn)
                return false;

            return (square.PassedPawnFrontAttackSpan(c) & Pawns(c)) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square square, Piece piece)
        {
            BitBoard invertedSq = square;
            BoardPieces[piece.ToInt()] &= ~invertedSq;
            OccupiedBySide[piece.ColorOf()] &= ~invertedSq;
            Occupied &= ~invertedSq;
            BoardLayout[square.ToInt()] = PieceExtensions.EmptyPiece;
            if (IsProbing)
                return;
            PieceUpdated?.Invoke(EPieces.NoPiece, square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedBySlider(Square square, Player side)
        {
            int pieceOffset = side << 3;
            BitBoard rookAttacks = square.RookAttacks(Occupied);
            if (BoardPieces[(int)(EPieces)EPieceType.Rook + pieceOffset] & rookAttacks)
                return true;

            BitBoard bishopAttacks = square.BishopAttacks(Occupied);
            if (BoardPieces[(int)(EPieces)EPieceType.Bishop + pieceOffset] & bishopAttacks)
                return true;

            return (BoardPieces[(int)(EPieces)EPieceType.Queen + pieceOffset] & (bishopAttacks | rookAttacks)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKnight(Square square, Player side) => (BoardPieces[(int)(EPieces)EPieceType.Knight + (side << 3)] & square.KnightAttacks()) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square square, Player side) => (BoardPieces[(int)(EPieces)EPieceType.Pawn | (side << 3)] & BitBoards.PawnAttacksBB[square.ToInt() | (~side << 6)]) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square square, Player side) => (BoardPieces[(int)(EPieces)EPieceType.King + (side << 3)] & square.KingAttacks()) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetRookCastleFrom(Square index) => _rookCastlesFrom[index.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRookCastleFrom(Square index, Square square) => _rookCastlesFrom[index.ToInt()] = square;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetKingCastleFrom(Player side, ECastleling castleType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (castleType) {
                case ECastleling.Short:
                    return _castleShortKingFrom[side.Side];
                case ECastleling.Long:
                    return _castleLongKingFrom[side.Side];
                default:
                    throw new ArgumentOutOfRangeException(nameof(castleType), castleType, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetKingCastleFrom(Player side, Square square, ECastleling castleType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (castleType) {
                case ECastleling.Short:
                    _castleShortKingFrom[side.Side] = square;
                    break;
                case ECastleling.Long:
                    _castleLongKingFrom[side.Side] = square;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(castleType), castleType, null);
            }
        }

        public IEnumerator<Piece> GetEnumerator()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int index = 0; index < BoardLayout.Length; index++)
                yield return BoardLayout[index];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}