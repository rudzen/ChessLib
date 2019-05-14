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
    using Extensions;
    using Fen;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
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
            Clear();
        }

        static Position() => EnPasCapturePos = new Func<BitBoard, BitBoard>[] { BitBoards.SouthOne, BitBoards.NorthOne };

        public BitBoard[] BoardPieces { get; }

        public BitBoard[] OccupiedBySide { get; }

        public bool IsProbing { get; set; }

        public Piece[] BoardLayout { get; }

        /// <summary>
        /// To let something outside the library be aware of changes (like a UI etc)
        /// </summary>
        public Action<Piece, Square> PieceUpdated { get; }

        public bool InCheck { get; set; }

        public State State { get; set; }

        public void Clear()
        {
            BoardLayout.Fill(EPieces.NoPiece);
            OccupiedBySide.Fill(Zero);
            BoardPieces.Fill(Zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(Piece piece, Square square)
        {
            BitBoard bbsq = square;
            var color = piece.ColorOf();
            BoardPieces[piece.AsInt()] |= bbsq;
            OccupiedBySide[color] |= bbsq;
            BoardLayout[square.AsInt()] = piece;

            if (!IsProbing)
                PieceUpdated?.Invoke(piece, square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(EPieceType pieceType, Square square, Player side)
        {
            var piece = pieceType.MakePiece(side);
            BoardPieces[piece.AsInt()] |= square;
            OccupiedBySide[side.Side] |= square;
            BoardLayout[square.AsInt()] = piece;

            if (!IsProbing)
                PieceUpdated?.Invoke(piece, square);
        }

        public bool MakeMove(Move move)
        {
            if (move.IsNullMove())
                return false;

            var toSquare = move.GetToSquare();
            var movingSide = move.GetMovingSide();

            if (move.IsCastlelingMove())
            {
                var rook = EPieceType.Rook.MakePiece(movingSide);
                var king = move.GetMovingPiece();
                RemovePiece(_rookCastlesFrom[toSquare.AsInt()], rook);
                RemovePiece(move.GetFromSquare(), king);
                AddPiece(rook, toSquare.GetRookCastleTo());
                AddPiece(king, toSquare);
                return true;
            }

            // reverse sideToMove as it has not been changed yet.
            if (IsAttacked(GetPieceSquare(EPieceType.King, ~movingSide), movingSide))
                return false;

            RemovePiece(move.GetFromSquare(), move.GetMovingPiece());

            if (move.IsEnPassantMove())
            {
                var t = EnPasCapturePos[movingSide.Side](toSquare).First();
                RemovePiece(t, move.GetCapturedPiece());
            }
            else if (move.IsCaptureMove())
                RemovePiece(toSquare, move.GetCapturedPiece());

            AddPiece(move.IsPromotionMove() ? move.GetPromotedPiece() : move.GetMovingPiece(), toSquare);

            return true;
        }

        public void TakeMove(Move move)
        {
            var toSquare = move.GetToSquare();

            if (move.IsCastlelingMove())
            {
                var rook = EPieceType.Rook.MakePiece(move.GetMovingSide());
                var king = move.GetMovingPiece();
                RemovePiece(toSquare, king);
                RemovePiece(toSquare.GetRookCastleTo(), rook);
                AddPiece(king, move.GetFromSquare());
                AddPiece(rook, _rookCastlesFrom[toSquare.AsInt()]);
                return;
            }

            RemovePiece(toSquare, move.IsPromotionMove() ? move.GetPromotedPiece() : move.GetMovingPiece());

            if (move.IsEnPassantMove())
            {
                var t = EnPasCapturePos[move.GetMovingSide().Side](toSquare).First();
                AddPiece(move.GetCapturedPiece(), t);
            }
            else if (move.IsCaptureMove())
                AddPiece(move.GetCapturedPiece(), toSquare);

            AddPiece(move.GetMovingPiece(), move.GetFromSquare());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square square) => BoardLayout[square.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPieceType GetPieceType(Square square) => BoardLayout[square.AsInt()].Type();

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

            var pinnedPieces = BitBoards.EmptyBitBoard;
            var them = ~side;

            var opponentQueens = Pieces(EPieceType.Queen, them);
            var ourPieces = Pieces(side);
            var pieces = Pieces();

            var pinners = square.XrayBishopAttacks(pieces, ourPieces) & (Pieces(EPieceType.Bishop, them) | opponentQueens)
                        | square.XrayRookAttacks(pieces, ourPieces) & (Pieces(EPieceType.Rook, them) | opponentQueens);

            while (pinners)
            {
                pinnedPieces |= pinners.Lsb().BitboardBetween(square) & ourPieces;
                pinners--;
            }

            return pinnedPieces;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square square) => (Pieces() & square) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square square, Player side) => AttackedBySlider(square, side) || AttackedByKnight(square, side) || AttackedByPawn(square, side) || AttackedByKing(square, side);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard PieceAttacks(Square square, EPieceType pieceType) => square.GetAttacks(pieceType, Pieces());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces() => OccupiedBySide[PlayerExtensions.White.Side] | OccupiedBySide[PlayerExtensions.Black.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Player side) => OccupiedBySide[side.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Piece pc) => BoardPieces[pc.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type) => BoardPieces[type.MakePiece(PlayerExtensions.White).AsInt()] | BoardPieces[type.MakePiece(PlayerExtensions.Black).AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type1, EPieceType type2) => Pieces(type1) | Pieces(type2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type, Player side) => BoardPieces[type.MakePiece(side).AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(EPieceType type1, EPieceType type2, Player side) => BoardPieces[type1.MakePiece(side).AsInt()] | BoardPieces[type2.MakePiece(side).AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetPieceSquare(EPieceType pt, Player color) => Pieces(pt, color).Lsb();

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
            var pc = BoardLayout[square.AsInt()];

            if (pc.Type() != EPieceType.Pawn)
                return false;

            Player c = pc.ColorOf();

            return (square.PassedPawnFrontAttackSpan(c) & Pieces(EPieceType.Pawn, c)).Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square square, Piece piece)
        {
            BitBoard invertedSq = square;
            BoardPieces[piece.AsInt()] &= ~invertedSq;
            OccupiedBySide[piece.ColorOf()] &= ~invertedSq;
            BoardLayout[square.AsInt()] = PieceExtensions.EmptyPiece;
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

        public BitBoard AttacksTo(Square square) => AttacksTo(square, Pieces());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedBySlider(Square square, Player side)
        {
            var occupied = Pieces();
            var rookAttacks = square.RookAttacks(occupied);
            if (Pieces(EPieceType.Rook, side) & rookAttacks)
                return true;

            var bishopAttacks = square.BishopAttacks(occupied);
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
        public Square GetRookCastleFrom(Square index) => _rookCastlesFrom[index.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRookCastleFrom(Square index, Square square) => _rookCastlesFrom[index.AsInt()] = square;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetKingCastleFrom(Player side, ECastleling castleType)
        {
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
        public Move StringToMove(string m)
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

            // part one of pillaging the castleType.. detection of chess 960 - shredder fen
            if (castleType == ECastleling.None)
                castleType = ShredderFunc(from, to); /* look for the air balloon */

            // part two of pillaging the castleType var, since it might have changed
            if (castleType != ECastleling.None)
            {
                from = GetKingCastleFrom(State.SideToMove, castleType);
                to = castleType.GetKingCastleTo(State.SideToMove);
            }

            var mg = new MoveGenerator(this);
            mg.GenerateMoves();

            var matchingMoves = mg.Moves.Where(x => x.GetFromSquare() == from && x.GetToSquare() == to);

            // ** untested area **
            foreach (var move in matchingMoves)
            {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(ECastleling type)
            => State.CastlelingRights.HasFlagFast(type.GetCastleAllowedMask(State.SideToMove)) && IsCastleAllowed(type.GetKingCastleTo(State.SideToMove));

        public bool IsCastleAllowed(Square square)
        {
            var c = State.SideToMove;
            // The complexity of this function is mainly due to the support for Chess960 variant.
            var rookTo = square.GetRookCastleTo();
            var rookFrom = GetRookCastleFrom(square);
            var ksq = GetPieceSquare(EPieceType.King, c);

            // The pieces in question.. rook and king
            var castlePieces = rookFrom | ksq;

            // The span between the rook and the king
            var castleSpan = ksq.BitboardBetween(rookFrom) | rookFrom.BitboardBetween(rookTo) | castlePieces | rookTo | square;

            // check that the span AND current occupied pieces are no different that the piece themselves.
            if ((castleSpan & Pieces()) != castlePieces)
                return false;

            // Check that no square between the king's initial and final squares (including the initial and final squares)
            // may be under attack by an enemy piece. Initial square was already checked a this point.

            c = ~c;

            castleSpan = ksq.BitboardBetween(square) | square;
            return !castleSpan.Any(x => IsAttacked(x, c));
        }

        /// <summary>
        /// Determine if a move is legal or not, by performing the move and checking if the king is under attack afterwards.
        /// </summary>
        /// <param name="move">The move to check</param>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="type">The move type</param>
        /// <returns>true if legal, otherwise false</returns>
        public bool IsLegal(Move move, Piece piece, Square from, EMoveType type)
        {
            if (!InCheck && piece.Type() != EPieceType.King && (State.Pinned & from).Empty() && !type.HasFlagFast(EMoveType.Epcapture))
                return true;

            IsProbing = true;
            MakeMove(move);
            var opponentAttacking = IsAttacked(GetPieceSquare(EPieceType.King, State.SideToMove), ~State.SideToMove);
            TakeMove(move);
            IsProbing = false;
            return !opponentAttacking;
        }

        public bool IsLegal(Move move) => IsLegal(move, move.GetMovingPiece(), move.GetFromSquare(), move.GetMoveType());

        /// <summary>
        /// <para>"Validates" a move using simple logic. For example that the piece actually being moved exists etc.</para>
        /// <para>This is basically only useful while developing and/or debugging</para>
        /// </summary>
        /// <param name="move">The move to check for logical errors</param>
        /// <returns>True if move "appears" to be legal, otherwise false</returns>
        public bool IsPseudoLegal(Move move)
        {
            // Verify that the piece actually exists on the board at the location defined by the move struct
            if ((Pieces(move.GetMovingPiece()) & move.GetFromSquare()).Empty())
                return false;

            var to = move.GetToSquare();

            if (move.IsCastlelingMove())
            {
                // TODO : Basic castleling verification
                if (CanCastle(move.GetFromSquare() < to ? ECastleling.Short : ECastleling.Long))
                    return true;

                var mg = new MoveGenerator(this);
                mg.GenerateMoves();
                return mg.Moves.Contains(move);
            }
            else if (move.IsEnPassantMove())
            {
                // TODO : En-passant here

                // TODO : Test with unit test
                var opponent = ~move.GetMovingSide();
                if (State.EnPassantSquare.PawnAttack(opponent) & Pieces(EPieceType.Pawn, opponent))
                    return true;
            }
            else if (move.IsCaptureMove())
            {
                var opponent = ~move.GetMovingSide();
                if ((OccupiedBySide[opponent.Side] & to).Empty())
                    return false;

                if ((Pieces(move.GetCapturedPiece()) & to).Empty())
                    return false;
            }
            else if ((Pieces() & to) != 0)
                return false;

            switch (move.GetMovingPiece().Type())
            {
                case EPieceType.Bishop:
                case EPieceType.Rook:
                case EPieceType.Queen:
                    if (move.GetFromSquare().BitboardBetween(to) & Pieces())
                        return false;

                    break;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMate()
        {
            var mg = new MoveGenerator(this);
            mg.GenerateMoves();
            return !mg.Moves.Any(IsLegal);
        }

        /// <summary>
        /// Parses the board layout to a FEN representation..
        /// Beware, goblins are a foot.
        /// </summary>
        /// <returns>
        /// The FenData which contains the fen string that was generated.
        /// </returns>
        public FenData GenerateFen()
        {
            var sv = new StringBuilder(Fen.Fen.MaxFenLen);

            for (var rank = ERank.Rank8; rank >= ERank.Rank1; rank--)
            {
                var empty = 0;

                for (var file = EFile.FileA; file < EFile.FileNb; file++)
                {
                    var square = new Square(rank, file);
                    var piece = BoardLayout[square.AsInt()];

                    if (piece.IsNoPiece())
                    {
                        empty++;
                        continue;
                    }

                    if (empty != 0)
                    {
                        sv.Append(empty);
                        empty = 0;
                    }

                    sv.Append(piece.GetPieceChar());
                }

                if (empty != 0)
                    sv.Append(empty);

                if (rank > ERank.Rank1)
                    sv.Append('/');
            }

            sv.Append(State.SideToMove.IsWhite() ? " w " : " b ");

            var castleRights = State.CastlelingRights;

            if (castleRights != 0)
            {
                if (castleRights.HasFlagFast(ECastlelingRights.WhiteOo))
                    sv.Append('K');

                if (castleRights.HasFlagFast(ECastlelingRights.WhiteOoo))
                    sv.Append('Q');

                if (castleRights.HasFlagFast(ECastlelingRights.BlackOo))
                    sv.Append('k');

                if (castleRights.HasFlagFast(ECastlelingRights.BlackOoo))
                    sv.Append('q');
            }
            else
                sv.Append('-');

            sv.Append(' ');

            if (State.EnPassantSquare == ESquare.none)
                sv.Append('-');
            else
                sv.Append(State.EnPassantSquare.ToString());

            sv.Append(' ');

            sv.Append(State.ReversibleHalfMoveCount);
            sv.Append(' ');
            sv.Append(State.HalfMoveCount + 1);
            return new FenData(sv.ToString());
        }

        public IEnumerator<Piece> GetEnumerator() => ((IEnumerable<Piece>) BoardLayout).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ECastleling ShredderFunc(Square fromSquare, Square toSquare) =>
            GetPiece(fromSquare).Value == EPieces.WhiteKing && GetPiece(toSquare).Value == EPieces.WhiteRook || GetPiece(fromSquare).Value == EPieces.BlackKing && GetPiece(toSquare).Value == EPieces.BlackRook
                ? toSquare > fromSquare
                    ? ECastleling.Short
                    : ECastleling.Long
                : ECastleling.None;
    }
}