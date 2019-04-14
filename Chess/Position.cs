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
    using Enums;
    using Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Types;

    /// <summary>
    /// The main board representation class.
    /// It stores all the information about the current board in a simple structure.
    /// It also serves the purpose of being able to give the UI controller feedback on various things on the board
    /// </summary>
    public sealed class Position : IPosition
    {
        private const ulong Zero = 0;

        private static readonly Func<BitBoard, BitBoard>[] EnPasCapturePos;

        private readonly Square[] _rookCastlesFrom; // indexed by position of the king

        private readonly Square[] _castleShortKingFrom;

        private readonly Square[] _castleLongKingFrom;

        public Position(Action<Piece, Square> pieceUpdateCallback)
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

        static Position() => EnPasCapturePos = new Func<BitBoard, BitBoard>[] { BitBoards.SouthOne, BitBoards.NorthOne };

        public BitBoard[] BoardPieces { get; }

        public BitBoard[] OccupiedBySide { get; }

        public Square[] KingSquares { get; }

        public bool IsProbing { get; set; }

        public BitBoard Occupied { get; set; }

        public Piece[] BoardLayout { get; }

        /// <summary>
        /// To let something outside the library be aware of changes (like a UI etc)
        /// </summary>
        public Action<Piece, Square> PieceUpdated { get; }

        public bool InCheck { get; set; }

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
            var color = piece.ColorOf();
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
            var piece = pieceType.MakePiece(side);
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
            var toSquare = move.GetToSquare();

            if (move.IsCastlelingMove())
            {
                Piece rook = (int)EPieceType.Rook + move.GetSideMask();
                var king = move.GetMovingPiece();
                RemovePiece(_rookCastlesFrom[toSquare.ToInt()], rook);
                RemovePiece(move.GetFromSquare(), king);
                AddPiece(rook, toSquare.GetRookCastleTo());
                AddPiece(king, toSquare);
                KingSquares[move.GetMovingSide().Side] = toSquare;
                return;
            }

            RemovePiece(move.GetFromSquare(), move.GetMovingPiece());

            if (move.IsEnPassantMove())
            {
                BitBoard targetSquare = toSquare;
                var t = EnPasCapturePos[move.GetMovingSide().Side](targetSquare).First();
                RemovePiece(t, move.GetCapturedPiece());
            }
            else if (move.IsCaptureMove())
                RemovePiece(toSquare, move.GetCapturedPiece());

            AddPiece(move.IsPromotionMove() ? move.GetPromotedPiece() : move.GetMovingPiece(), toSquare);

            if (move.GetMovingPieceType() == EPieceType.King)
                KingSquares[move.GetMovingSide().Side] = toSquare;
        }

        public void TakeMove(Move move)
        {
            var toSquare = move.GetToSquare();

            if (move.IsCastlelingMove())
            {
                Piece rook = (int)EPieceType.Rook + move.GetSideMask();
                var king = move.GetMovingPiece();
                RemovePiece(toSquare, king);
                RemovePiece(toSquare.GetRookCastleTo(), rook);
                AddPiece(king, move.GetFromSquare());
                AddPiece(rook, _rookCastlesFrom[toSquare.ToInt()]);
                KingSquares[move.GetMovingSide().Side] = move.GetFromSquare();
                return;
            }

            RemovePiece(toSquare, move.IsPromotionMove() ? move.GetPromotedPiece() : move.GetMovingPiece());

            if (move.IsEnPassantMove())
            {
                BitBoard targetSquare = toSquare;
                var t = EnPasCapturePos[move.GetMovingSide().Side](targetSquare).First();
                AddPiece(move.GetCapturedPiece(), t);
            }
            else if (move.IsCaptureMove())
                AddPiece(move.GetCapturedPiece(), toSquare);

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
            var oppShift = ~side << 3;
            var pinners = square.XrayBishopAttacks(Occupied, OccupiedBySide[side.Side]) & (BoardPieces[(int)EPieceType.Bishop | oppShift] | BoardPieces[(int)EPieceType.Queen | oppShift]);
            pinners |= square.XrayRookAttacks(Occupied, OccupiedBySide[side.Side]) & (BoardPieces[(int)EPieceType.Rook | oppShift] | BoardPieces[(int)EPieceType.Queen | oppShift]);

            while (pinners)
            {
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
        public BitBoard PieceAttacks(Square square, EPieceType pieceType) => square.GetAttacks(pieceType, Occupied);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Player side) => OccupiedBySide[side.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type) => BoardPieces[type.MakePiece(PlayerExtensions.White).ToInt()] | BoardPieces[type.MakePiece(PlayerExtensions.Black).ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type1, EPieceType type2) => Pieces(type1) | Pieces(type2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type, Player side) => BoardPieces[type.MakePiece(side).ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type1, EPieceType type2, Player side) => BoardPieces[type1.MakePiece(side).ToInt()] | BoardPieces[type2.MakePiece(side).ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PieceOnFile(Square square, Player side, EPieceType pieceType) => (BoardPieces[(int)(pieceType + (side << 3))] & square) != 0;

        /// <summary>
        /// Determine if a pawn is isolated e.i. no own pawns on either of it's neighboring files
        /// </summary>
        /// <param name="square"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PawnIsolated(Square square, Player side)
        {
            var b = (square.PawnAttackSpan(side) | square.PawnAttackSpan(~side)) & Pieces(EPieceType.Pawn, side);
            return b.Empty();
        }

        /// <summary>
        /// Determine if a specific square is a passed pawn
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PassedPawn(Square square)
        {
            var pc = BoardLayout[square.ToInt()];

            if (pc.Type() != EPieceType.Pawn)
                return false;

            Player c = pc.ColorOf();

            return (square.PassedPawnFrontAttackSpan(c) & Pieces(EPieceType.Pawn, c)) == 0;
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

        public BitBoard AttacksTo(Square square, BitBoard occupied)
        {
            // TODO : needs testing
            return (square.PawnAttack(PlayerExtensions.White) & OccupiedBySide[PlayerExtensions.Black.Side])
                  | (square.PawnAttack(PlayerExtensions.Black) & OccupiedBySide[PlayerExtensions.White.Side])
                  | (square.GetAttacks(EPieceType.Knight) & Pieces(EPieceType.Knight))
                  | (square.GetAttacks(EPieceType.Rook, occupied) & Pieces(EPieceType.Rook, EPieceType.Queen))
                  | (square.GetAttacks(EPieceType.Bishop, occupied) & Pieces(EPieceType.Bishop, EPieceType.Queen))
                  | (square.GetAttacks(EPieceType.King) & Pieces(EPieceType.King));
        }

        public BitBoard AttacksTo(Square square) => AttacksTo(square, Occupied);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedBySlider(Square square, Player side)
        {
            var rookAttacks = square.RookAttacks(Occupied);
            if (Pieces(EPieceType.Rook, side) & rookAttacks)
                return true;

            var bishopAttacks = square.BishopAttacks(Occupied);
            if (Pieces(EPieceType.Bishop, side) & bishopAttacks)
                return true;

            return (Pieces(EPieceType.Queen, side) & (bishopAttacks | rookAttacks)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKnight(Square square, Player side) => (Pieces(EPieceType.Knight, side) & square.GetAttacks(EPieceType.Knight)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square square, Player side) => (Pieces(EPieceType.Pawn, side) & square.PawnAttack(~side)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square square, Player side) => (Pieces(EPieceType.King, side) & square.GetAttacks(EPieceType.King)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetRookCastleFrom(Square index) => _rookCastlesFrom[index.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRookCastleFrom(Square index, Square square) => _rookCastlesFrom[index.ToInt()] = square;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetKingCastleFrom(Player side, ECastleling castleType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (castleType)
            {
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
            switch (castleType)
            {
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

        /// <summary>
        /// Checks from a string if the move actually is a castle move.
        /// Note:
        /// - The unique cases with amended piece location check
        ///   is a *shallow* detection, it should be the sender
        ///   that guarantee that it's a real move.
        /// </summary>
        /// <param name="m">The string containing the move</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ECastleling IsCastleMove(string m)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (m)
            {
                case "O-O":
                case "OO":
                case "0-0":
                case "00":
                case "e1g1" when IsPieceTypeOnSquare(ESquare.e1, EPieceType.King):
                case "e8g8" when IsPieceTypeOnSquare(ESquare.e8, EPieceType.King):
                    return ECastleling.Short;

                case "O-O-O":
                case "OOO":
                case "0-0-0":
                case "000":
                case "e1c1" when IsPieceTypeOnSquare(ESquare.e1, EPieceType.King):
                case "e8c8" when IsPieceTypeOnSquare(ESquare.e8, EPieceType.King):
                    return ECastleling.Long;
            }

            return ECastleling.None;
        }

        /// <summary>
        /// TODO : This method is incomplete, and is not meant to be used atm.
        /// Parse a string and convert to a valid move. If the move is not valid.. hell breaks loose.
        /// * NO EXCEPTIONS IS ALLOWED IN THIS FUNCTION *
        /// </summary>
        /// <param name="m">string representation of the move to parse</param>
        /// <returns>
        /// On fail : Move containing from and to squares as ESquare.none (empty move)
        /// On Ok   : The move!
        /// </returns>
        public Move StringToMove(string m, State state)
        {
            // guards
            if (string.IsNullOrWhiteSpace(m))
                return MoveExtensions.EmptyMove;

            if (m.Equals(@"\"))
                return MoveExtensions.EmptyMove;

            // only lengths of 4 and 5 are acceptable.
            if (!m.Length.InBetween(4, 5))
                return MoveExtensions.EmptyMove;

            var castleType = IsCastleMove(m);

            if (castleType == ECastleling.None && (!m[0].InBetween('a', 'h') || !m[1].InBetween('1', '8') || !m[2].InBetween('a', 'h') || !m[3].InBetween('1', '8')))
                return MoveExtensions.EmptyMove;

            /*
             * Needs to be assigned here.
             * Otherwise it won't compile because of later split check using both two independent IF and optional reassignment through local method.
             */
            var from = new Square(m[1] - '1', m[0] - 'a');
            var to = new Square(m[3] - '1', m[2] - 'a');

            // local function to determine if the move is actually a castleling move by looking at the piece location of the squares
            ECastleling ShredderFunc(Square fromSquare, Square toSquare) =>
                GetPiece(fromSquare).Value == EPieces.WhiteKing && GetPiece(toSquare).Value == EPieces.WhiteRook
                || GetPiece(fromSquare).Value == EPieces.BlackKing && GetPiece(toSquare).Value == EPieces.BlackRook
                    ? toSquare > fromSquare
                          ? ECastleling.Short
                          : ECastleling.Long
                    : ECastleling.None;

            // part one of pillaging the castleType.. detection of chess 960 - shredder fen
            if (castleType == ECastleling.None)
                castleType = ShredderFunc(from, to); /* look for the air balloon */

            // part two of pillaging the castleType var, since it might have changed
            if (castleType != ECastleling.None)
            {
                from = GetKingCastleFrom(state.SideToMove, castleType);
                to = castleType.GetKingCastleTo(state.SideToMove);
            }

            var mg = new MoveGenerator(this);
            mg.GenerateMoves();

            // ** untested area **
            foreach (var move in mg.Moves)
            {
                if (move.GetFromSquare() != from || move.GetToSquare() != to)
                    continue;
                if (castleType == ECastleling.None && move.IsCastlelingMove())
                    continue;
                if (!move.IsPromotionMove())
                    return move;
                if (char.ToLower(m[m.Length - 1]) != move.GetPromotedPiece().GetPromotionChar())
                    continue;

                return move;
            }

            return MoveExtensions.EmptyMove;
        }

        public IEnumerator<Piece> GetEnumerator()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var index = 0; index < BoardLayout.Length; index++)
                yield return BoardLayout[index];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}