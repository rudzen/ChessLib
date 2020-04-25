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
    using Extensions;
    using Fen;
    using Hash;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Types;

    /// <summary>
    /// The main board representation class. It stores all the information about the current board
    /// in a simple structure. It also serves the purpose of being able to give the UI controller
    /// feedback on various things on the board
    /// </summary>
    public sealed class Position : IPosition
    {
        private static readonly Func<Square, Square>[] EnPasCapturePos = { s => s + Directions.South, s => s + Directions.North };

        private readonly Square[] _rookCastlesFrom; // indexed by position of the king

        private readonly Square[] _castleShortKingFrom;

        private readonly Square[] _castleLongKingFrom;

        public Position()
        {
            _castleLongKingFrom = new Square[2];
            _rookCastlesFrom = new Square[64];
            _castleShortKingFrom = new Square[2];
            BoardLayout = new Piece[64];
            BoardPieces = new BitBoard[PieceTypes.PieceTypeNb.AsInt()];
            OccupiedBySide = new BitBoard[2];
            IsProbing = true;
            Clear();
        }

        public BitBoard[] BoardPieces { get; }

        public BitBoard[] OccupiedBySide { get; }

        public bool IsProbing { get; set; }

        public Piece[] BoardLayout { get; }

        /// <summary>
        /// To let something outside the library be aware of changes (like a UI etc)
        /// </summary>
        public Action<Piece, Square> PieceUpdated { get; set; }

        public State State { get; set; }

        public void Clear()
        {
            BoardLayout.Fill(Enums.Pieces.NoPiece);
            OccupiedBySide.Fill(BitBoards.EmptyBitBoard);
            BoardPieces.Fill(BitBoards.EmptyBitBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(Piece pc, Square sq)
        {
            var color = pc.ColorOf();
            BoardPieces[PieceTypes.AllPieces.AsInt()] |= sq;
            BoardPieces[pc.Type().AsInt()] |= sq;
            OccupiedBySide[color] |= sq;
            BoardLayout[sq.AsInt()] = pc;

            if (!IsProbing)
                PieceUpdated?.Invoke(pc, sq);
        }

        public void MovePiece(Square from, Square to)
        {
            var pc = GetPiece(from);
            var fromTo = from | to;
            BoardPieces[PieceTypes.AllPieces.AsInt()] ^= fromTo;
            BoardPieces[pc.Type().AsInt()] ^= fromTo;
            OccupiedBySide[pc.ColorOf()] ^= fromTo;
            BoardLayout[from.AsInt()] = Enums.Pieces.NoPiece;
            BoardLayout[to.AsInt()] = pc;

            if (!IsProbing)
                PieceUpdated?.Invoke(pc, to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(PieceTypes pt, Square sq, Player c) => AddPiece(pt.MakePiece(c), sq);

        public bool MakeMove(Move m)
        {
            if (m.IsNullMove())
                return false;

            var to = m.GetToSquare();
            var from = m.GetFromSquare();
            var us = m.GetMovingSide();
            var them = ~us;

            if (m.IsCastlelingMove())
            {
                MovePiece(_rookCastlesFrom[to.AsInt()], to.GetRookCastleTo());
                MovePiece(m.GetFromSquare(), to);
                return true;
            }

            // reverse sideToMove as it has not been changed yet.
            if (IsAttacked(GetPieceSquare(PieceTypes.King, them), us))
                return false;

            if (m.IsEnPassantMove())
            {
                var t = EnPasCapturePos[us.Side](to);
                RemovePiece(t);
            }
            else if (m.IsCaptureMove())
                RemovePiece(to);

            MovePiece(from, to);

            if (m.IsPromotionMove())
            {
                RemovePiece(to);
                AddPiece(m.GetPromotedPiece(), to);
            }

            return true;
        }

        public void TakeMove(Move m)
        {
            var to = m.GetToSquare();
            var from = m.GetFromSquare();
            var pc = m.GetMovingPiece();
            var us = m.GetMovingSide();

            if (m.IsCastlelingMove())
            {
                MovePiece(to, from);
                MovePiece(to.GetRookCastleTo(), _rookCastlesFrom[to.AsInt()]);
                return;
            }

            RemovePiece(to);

            if (m.IsEnPassantMove())
            {
                var t = EnPasCapturePos[us.Side](to);
                AddPiece(m.GetCapturedPiece(), t);
            }
            else if (m.IsCaptureMove())
                AddPiece(m.GetCapturedPiece(), to);

            AddPiece(pc, from);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square sq) => BoardLayout[sq.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceTypes GetPieceType(Square sq) => GetPiece(sq).Type();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPieceTypeOnSquare(Square sq, PieceTypes pt) => GetPieceType(sq) == pt;

        /// <summary>
        /// Detects any pinned pieces For more info : https://en.wikipedia.org/wiki/Pin_(chess)
        /// </summary>
        /// <param name="sq">The square</param>
        /// <param name="c">The side</param>
        /// <returns>Pinned pieces as BitBoard</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPinnedPieces(Square sq, Player c)
        {
            // TODO : Move into state data structure instead of real-time calculation

            var pinnedPieces = BitBoards.EmptyBitBoard;
            var them = ~c;

            var opponentQueens = Pieces(PieceTypes.Queen, them);
            var ourPieces = Pieces(c);
            var pieces = Pieces();

            var pinners
                = sq.XrayBishopAttacks(pieces, ourPieces) & (Pieces(PieceTypes.Bishop, them) | opponentQueens)
                | sq.XrayRookAttacks(pieces, ourPieces) & (Pieces(PieceTypes.Rook, them) | opponentQueens);

            while (pinners)
            {
                pinnedPieces |= pinners.Lsb().BitboardBetween(sq) & ourPieces;
                pinners--;
            }

            return pinnedPieces;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square sq) => BoardLayout[sq.AsInt()] != Enums.Pieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square sq, Player c) => AttackedBySlider(sq, c) || AttackedByKnight(sq, c) || AttackedByPawn(sq, c) || AttackedByKing(sq, c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard PieceAttacks(Square sq, PieceTypes pt) => sq.GetAttacks(pt, Pieces());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces() => BoardPieces[PieceTypes.AllPieces.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Player c) => OccupiedBySide[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Piece pc) => BoardPieces[pc.Type().AsInt()] & OccupiedBySide[pc.ColorOf()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt) => BoardPieces[pt.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2) => Pieces(pt1) | Pieces(pt2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt, Player side) => BoardPieces[pt.AsInt()] & OccupiedBySide[side.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player c) => (BoardPieces[pt1.AsInt()] | BoardPieces[pt2.AsInt()]) & OccupiedBySide[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetPieceSquare(PieceTypes pt, Player c) => Pieces(pt, c).Lsb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PieceOnFile(Square sq, Player c, PieceTypes pt) => (BoardPieces[pt.MakePiece(c).Type().AsInt()] & sq) != 0;

        /// <summary>
        /// Determine if a pawn is isolated e.i. no own pawns on either of it's neighboring files
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PawnIsolated(Square sq, Player c)
        {
            var b = (sq.PawnAttackSpan(c) | sq.PawnAttackSpan(~c)) & Pieces(PieceTypes.Pawn, c);
            return b.Empty();
        }

        /// <summary>
        /// Determine if a specific square is a passed pawn
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PassedPawn(Square sq)
        {
            var pc = BoardLayout[sq.AsInt()];

            if (pc.Type() != PieceTypes.Pawn)
                return false;

            Player c = pc.ColorOf();

            return (sq.PassedPawnFrontAttackSpan(c) & Pieces(PieceTypes.Pawn, c)).Empty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square sq)
        {
            var pc = BoardLayout[sq.AsInt()];
            var invertedSq = ~sq;
            BoardPieces[PieceTypes.AllPieces.AsInt()] &= invertedSq;
            BoardPieces[pc.Type().AsInt()] &= invertedSq;
            OccupiedBySide[pc.ColorOf()] &= invertedSq;
            BoardLayout[sq.AsInt()] = PieceExtensions.EmptyPiece;
            if (!IsProbing)
                PieceUpdated?.Invoke(Enums.Pieces.NoPiece, sq);
        }

        public BitBoard AttacksTo(Square sq, BitBoard occupied)
        {
            // TODO : needs testing
            return (sq.PawnAttack(PlayerExtensions.White) & OccupiedBySide[PlayerExtensions.Black.Side])
                  | (sq.PawnAttack(PlayerExtensions.Black) & OccupiedBySide[PlayerExtensions.White.Side])
                  | (sq.GetAttacks(PieceTypes.Knight) & Pieces(PieceTypes.Knight))
                  | (sq.GetAttacks(PieceTypes.Rook, occupied) & Pieces(PieceTypes.Rook, PieceTypes.Queen))
                  | (sq.GetAttacks(PieceTypes.Bishop, occupied) & Pieces(PieceTypes.Bishop, PieceTypes.Queen))
                  | (sq.GetAttacks(PieceTypes.King) & Pieces(PieceTypes.King));
        }

        public BitBoard AttacksTo(Square sq) => AttacksTo(sq, Pieces());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedBySlider(Square sq, Player c)
        {
            var occupied = Pieces();
            var rookAttacks = sq.RookAttacks(occupied);
            if (Pieces(PieceTypes.Rook, c) & rookAttacks)
                return true;

            var bishopAttacks = sq.BishopAttacks(occupied);
            if (Pieces(PieceTypes.Bishop, c) & bishopAttacks)
                return true;

            return (Pieces(PieceTypes.Queen, c) & (bishopAttacks | rookAttacks)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKnight(Square sq, Player c) => (Pieces(PieceTypes.Knight, c) & sq.GetAttacks(PieceTypes.Knight)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square sq, Player c) => (Pieces(PieceTypes.Pawn, c) & sq.PawnAttack(~c)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square sq, Player c) => (sq.GetAttacks(PieceTypes.King) & GetPieceSquare(PieceTypes.King, c)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetRookCastleFrom(Square sq) => _rookCastlesFrom[sq.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRookCastleFrom(Square indexSq, Square sq) => _rookCastlesFrom[indexSq.AsInt()] = sq;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetKingCastleFrom(Player c, CastlelingSides sides)
        {
            return sides switch
            {
                CastlelingSides.King => _castleShortKingFrom[c.Side],
                CastlelingSides.Queen => _castleLongKingFrom[c.Side],
                _ => throw new ArgumentOutOfRangeException(nameof(sides), sides, null)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetKingCastleFrom(Player c, Square sq, CastlelingSides sides)
        {
            switch (sides)
            {
                case CastlelingSides.King:
                    _castleShortKingFrom[c.Side] = sq;
                    break;

                case CastlelingSides.Queen:
                    _castleLongKingFrom[c.Side] = sq;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(sides), sides, null);
            }
        }

        /// <summary>
        /// Checks from a string if the move actually is a castle move. Note:
        /// - The unique cases with amended piece location check is a *shallow* detection, it should
        /// be the sender that guarantee that it's a real move.
        /// </summary>
        /// <param name="m">The string containing the move</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CastlelingSides IsCastleMove(string m)
        {
            return m switch
            {
                "O-O" => CastlelingSides.King,
                "e1g1" when IsPieceTypeOnSquare(Squares.e1, PieceTypes.King) => CastlelingSides.King,
                "e8g8" when IsPieceTypeOnSquare(Squares.e8, PieceTypes.King) => CastlelingSides.King,
                "OO" => CastlelingSides.King,
                "0-0" => CastlelingSides.King,
                "00" => CastlelingSides.King,
                "O-O-O" => CastlelingSides.Queen,
                "e1c1" when IsPieceTypeOnSquare(Squares.e1, PieceTypes.King) => CastlelingSides.Queen,
                "e8c8" when IsPieceTypeOnSquare(Squares.e8, PieceTypes.King) => CastlelingSides.Queen,
                "OOO" => CastlelingSides.Queen,
                "0-0-0" => CastlelingSides.Queen,
                "000" => CastlelingSides.Queen,
                _ => CastlelingSides.None
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(CastlelingSides sides)
            => State.CastlelingRights.HasFlagFast(sides.GetCastleAllowedMask(State.SideToMove)) && IsCastleAllowed(sides.GetKingCastleTo(State.SideToMove));

        public bool IsCastleAllowed(Square sq)
        {
            var c = State.SideToMove;
            // The complexity of this function is mainly due to the support for Chess960 variant.
            var rookTo = sq.GetRookCastleTo();
            var rookFrom = GetRookCastleFrom(sq);
            var ksq = GetPieceSquare(PieceTypes.King, c);

            // The pieces in question.. rook and king
            var castlePieces = rookFrom | ksq;

            // The span between the rook and the king
            var castleSpan = ksq.BitboardBetween(rookFrom) | rookFrom.BitboardBetween(rookTo) | castlePieces | rookTo | sq;

            // check that the span AND current occupied pieces are no different that the piece themselves.
            if ((castleSpan & Pieces()) != castlePieces)
                return false;

            // Check that no square between the king's initial and final squares (including the
            // initial and final squares) may be under attack by an enemy piece. Initial square was
            // already checked a this point.

            c = ~c;

            castleSpan = ksq.BitboardBetween(sq) | sq;
            return !castleSpan.Any(x => IsAttacked(x, c));
        }

        /// <summary>
        /// Determine if a move is legal or not, by performing the move and checking if the king is
        /// under attack afterwards.
        /// </summary>
        /// <param name="m">The move to check</param>
        /// <param name="pc">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="type">The move type</param>
        /// <returns>true if legal, otherwise false</returns>
        public bool IsLegal(Move m, Piece pc, Square from, MoveTypes type)
        {
            if (!State.InCheck && pc.Type() != PieceTypes.King && (State.Pinned & from).Empty() && !type.HasFlagFast(MoveTypes.Epcapture))
                return true;

            IsProbing = true;
            MakeMove(m);
            var opponentAttacking = IsAttacked(GetPieceSquare(PieceTypes.King, State.SideToMove), ~State.SideToMove);
            TakeMove(m);
            IsProbing = false;
            return !opponentAttacking;
        }

        public bool IsLegal(Move m)
        {
            return IsLegal(m, m.GetMovingPiece(), m.GetFromSquare(), m.GetMoveType());
        }

        /// <summary>
        /// <para>
        /// "Validates" a move using simple logic. For example that the piece actually being moved
        /// exists etc.
        /// </para>
        /// <para>This is basically only useful while developing and/or debugging</para>
        /// </summary>
        /// <param name="m">The move to check for logical errors</param>
        /// <returns>True if move "appears" to be legal, otherwise false</returns>
        public bool IsPseudoLegal(Move m)
        {
            // Verify that the piece actually exists on the board at the location defined by the
            // move struct
            if ((Pieces(m.GetMovingPiece()) & m.GetFromSquare()).Empty())
                return false;

            var to = m.GetToSquare();

            if (m.IsCastlelingMove())
            {
                // TODO : Basic castleling verification
                if (CanCastle(m.GetFromSquare() < to ? CastlelingSides.King : CastlelingSides.Queen))
                    return true;

                return this.GenerateMoves().Contains(m);
            }
            else if (m.IsEnPassantMove())
            {
                // TODO : En-passant here

                // TODO : Test with unit test
                var opponent = ~m.GetMovingSide();
                if (State.EnPassantSquare.PawnAttack(opponent) & Pieces(PieceTypes.Pawn, opponent))
                    return true;
            }
            else if (m.IsCaptureMove())
            {
                var opponent = ~m.GetMovingSide();
                if ((OccupiedBySide[opponent.Side] & to).Empty())
                    return false;

                if ((Pieces(m.GetCapturedPiece()) & to).Empty())
                    return false;
            }
            else if ((Pieces() & to) != 0)
                return false;

            switch (m.GetMovingPiece().Type())
            {
                case PieceTypes.Bishop:
                case PieceTypes.Rook:
                case PieceTypes.Queen:
                    if (m.GetFromSquare().BitboardBetween(to) & Pieces())
                        return false;

                    break;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMate()
            => this.GenerateMoves().Count == 0;

        /// <summary>
        /// Parses the board layout to a FEN representation.. Beware, goblins are a foot.
        /// </summary>
        /// <returns>The FenData which contains the fen string that was generated.</returns>
        public FenData GenerateFen()
        {
            var sb = new StringBuilder(Fen.Fen.MaxFenLen);

            for (var rank = Ranks.Rank8; rank >= Ranks.Rank1; rank--)
            {
                var empty = 0;

                for (var file = Files.FileA; file < Files.FileNb; file++)
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
                        sb.Append(empty);
                        empty = 0;
                    }

                    sb.Append(piece.GetPieceChar());
                }

                if (empty != 0)
                    sb.Append(empty);

                if (rank > Ranks.Rank1)
                    sb.Append('/');
            }

            sb.Append(State.SideToMove.IsWhite() ? " w " : " b ");

            var castleRights = State.CastlelingRights;

            if (castleRights != CastlelingRights.None)
            {
                if (castleRights.HasFlagFast(CastlelingRights.WhiteOo))
                    sb.Append('K');

                if (castleRights.HasFlagFast(CastlelingRights.WhiteOoo))
                    sb.Append('Q');

                if (castleRights.HasFlagFast(CastlelingRights.BlackOo))
                    sb.Append('k');

                if (castleRights.HasFlagFast(CastlelingRights.BlackOoo))
                    sb.Append('q');
            }
            else
                sb.Append('-');

            sb.Append(' ');

            if (State.EnPassantSquare == Squares.none)
                sb.Append('-');
            else
                sb.Append(State.EnPassantSquare.ToString());

            sb.Append(' ');

            sb.Append(State.ReversibleHalfMoveCount);
            sb.Append(' ');
            sb.Append(State.HalfMoveCount + 1);
            return new FenData(sb.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashKey GetPiecesKey()
        {
            var result = new HashKey();

            var pieces = Pieces();
            while (pieces)
            {
                var sq = pieces.Lsb();
                var pc = GetPiece(sq);
                result ^= pc.GetZobristPst(sq);
                BitBoards.ResetLsb(ref pieces);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashKey GetPawnKey()
        {
            var result = Zobrist.ZobristNoPawn;

            var pieces = Pieces(PieceTypes.Pawn);
            while (pieces)
            {
                var sq = pieces.Lsb();
                var pc = GetPiece(sq);
                result ^= pc.GetZobristPst(sq);
                BitBoards.ResetLsb(ref pieces);
            }

            return result;
        }

        public IEnumerator<Piece> GetEnumerator() => BoardLayout.Cast<Piece>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CastlelingSides ShredderFunc(Square from, Square to) =>
            GetPiece(from).Value == Enums.Pieces.WhiteKing && GetPiece(to).Value == Enums.Pieces.WhiteRook || GetPiece(from).Value == Enums.Pieces.BlackKing && GetPiece(to).Value == Enums.Pieces.BlackRook
                ? to > from
                    ? CastlelingSides.King
                    : CastlelingSides.Queen
                : CastlelingSides.None;
    }
}