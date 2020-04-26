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

        private readonly CastlelingRights[] castlingRightsMask;
        private readonly Square[] castlingRookSquare;
        private readonly BitBoard[] castlingPath;


        // private readonly Square[] _rookCastlesFrom; // indexed by position of the king
        //
        // private readonly Square[] _castleShortKingFrom;
        //
        // private readonly Square[] _castleLongKingFrom;

        public Position()
        {
            // _castleLongKingFrom = new Square[2];
            // _rookCastlesFrom = new Square[64];
            // _castleShortKingFrom = new Square[2];
            BoardLayout = new Piece[64];
            castlingPath = new BitBoard[CastlelingRights.CastleRightsNb.AsInt()];
            castlingRookSquare = new Square[CastlelingRights.CastleRightsNb.AsInt()];
            castlingRightsMask = new CastlelingRights[64];
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

        public bool Chess960 { get; private set; }

        public void Clear()
        {
            BoardLayout.Fill(Enums.Pieces.NoPiece);
            OccupiedBySide.Fill(BitBoards.EmptyBitBoard);
            BoardPieces.Fill(BitBoards.EmptyBitBoard);
            castlingPath.Fill(BitBoards.EmptyBitBoard);
            castlingRightsMask.Fill(CastlelingRights.None);
            castlingRookSquare.Fill(Squares.none);
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
                DoCastleling(from, ref to, out var rfrom, out var rto, true, us);

                var rook = PieceTypes.Rook.MakePiece(us);
                var king = PieceTypes.King.MakePiece(us);
                
                State.Key ^= rook.GetZobristPst(rfrom) ^ rook.GetZobristPst(rto);
                
                // MovePiece(_rookCastlesFrom[to.AsInt()], to.GetRookCastleTo());
                // MovePiece(m.GetFromSquare(), to);
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
                DoCastleling(from, ref to, out _, out _, false, us);

                //MovePiece(to, from);
                //MovePiece(to.GetRookCastleTo(), _rookCastlesFrom[to.AsInt()]);
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

        public BitBoard CheckedSquares(PieceTypes pt)
        {
            return State.CheckedSquares[pt.AsInt()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square sq) => BoardLayout[sq.AsInt()] != Enums.Pieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square sq, Player c) => AttackedBySlider(sq, c) || AttackedByKnight(sq, c) || AttackedByPawn(sq, c) || AttackedByKing(sq, c);

        public bool GivesCheck(Move m)
        {
            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var pt = GetPieceType(from);
            var us = State.SideToMove;
            
            // Is there a direct check?
            if ((CheckedSquares(pt) & to) != 0)
                return true;

            var ksq = GetPieceSquare(PieceTypes.King, us);
            
            // Is there a discovered check?
            if (State.DicoveredCheckers != 0
                && (State.DicoveredCheckers & from) != 0
                && !from.Aligned(to, ksq))
                return true;

            switch (m.GetMoveType())
            {
                case MoveTypes.Normal:
                    return false;

                case MoveTypes.Promotion:
                    return (to.GetAttacks(m.GetPromotedPiece().Type(), Pieces() ^ from) & ksq) != 0;

                // En passant capture with check? We have already handled the case of direct checks
                // and ordinary discovered check, so the only case we need to handle is the unusual
                // case of a discovered check through the captured pawn.
                case MoveTypes.Epcapture:
                    {
                        var capsq = new Square(from.Rank(), to.File());
                        var b = Pieces();
                        b ^= from;
                        b ^= capsq;
                        b |= to;

                        return ((ksq.GetAttacks(PieceTypes.Rook, b) & Pieces(PieceTypes.Rook, PieceTypes.Queen, us))
                            | (ksq.GetAttacks(PieceTypes.Bishop, b) & Pieces(PieceTypes.Bishop, PieceTypes.Queen, us))) != 0;
                    }

                case MoveTypes.Castle:
                    {
                        var kfrom = from;
                        var rfrom = to; // Castling is encoded as 'King captures the rook'
                        var kingSide = rfrom > kfrom;
                        var (kto, rto) = kingSide
                            ? (Squares.g1.RelativeSquare(us), Squares.f1.RelativeSquare(us))
                            : (Squares.c1.RelativeSquare(us), Squares.d1.RelativeSquare(us));

                        return (BitBoards.PseudoAttacks(PieceTypes.Rook, rto) & ksq) != 0
                                && (rto.GetAttacks(PieceTypes.Rook, (Pieces() ^ kfrom ^ rfrom) | rto | kto) & ksq) != 0;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

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
        public bool AttackedByKnight(Square sq, Player c)
            => (Pieces(PieceTypes.Knight, c) & sq.GetAttacks(PieceTypes.Knight)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square sq, Player c) =>
            (Pieces(PieceTypes.Pawn, c) & sq.PawnAttack(~c)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square sq, Player c)
            => (sq.GetAttacks(PieceTypes.King) & GetPieceSquare(PieceTypes.King, c)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(CastlelingRights cr)
            => State.CastlelingRights.HasFlagFast(cr);

        public bool CanCastle(Player color)
        {
            var c = (CastlelingRights) ((int)(CastlelingRights.WhiteOo | CastlelingRights.WhiteOoo) << (2 * color.Side));
            return State.CastlelingRights.HasFlagFast(c);
        }

        public bool CastlingImpeded(CastlelingRights cr)
        {
            var v = cr.AsInt();
            return (Pieces() & castlingPath[v]) != 0;
        }

        public Square CastlingRookSquare(CastlelingRights cr)
            => castlingRookSquare[cr.AsInt()];

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
            => IsLegal(m, m.GetMovingPiece(), m.GetFromSquare(), m.GetMoveType());

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
                //if (CanCastle(m.GetFromSquare() < to ? CastlelingSides.King : CastlelingSides.Queen))
                //    return true;

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

        /// Position::set_castle_right() is an helper function used to set castling rights given the
        /// corresponding color and the rook starting square.
        public CastlelingRights SetCastlingRight(Player stm, Square rookFrom)
        {
            var ksq = GetPieceSquare(PieceTypes.King, stm);
            var cs = ksq < rookFrom ? CastlelingSides.King : CastlelingSides.Queen;
            var cr = OrCastlingRight(stm, cs);

            castlingRightsMask[ksq.AsInt()] |= cr;
            castlingRightsMask[rookFrom.AsInt()] |= cr;
            castlingRookSquare[cr.AsInt()] = rookFrom;

            var kingTo = (cs == CastlelingSides.King ? Squares.g1 : Squares.c1).RelativeSquare(stm);
            var rookTo = (cs == CastlelingSides.King ? Squares.f1 : Squares.d1).RelativeSquare(stm);

            var maxSquare = rookFrom.Max(rookTo);
            for (var s = rookFrom.Min(rookTo); s <= maxSquare; ++s)
                if (s != ksq && s != rookFrom)
                    castlingPath[cr.AsInt()] |= s;

            maxSquare = ksq.Max(kingTo);
            for (var s = ksq.Min(kingTo); s <= maxSquare; ++s)
                if (s != ksq && s != rookFrom)
                    castlingPath[cr.AsInt()] |= s;

            return cr;
        }

        /// <summary>
        /// Converts a move data type to move notation string format which chess engines understand.
        /// e.g. "a2a4", "a7a8q"
        /// </summary>
        /// <param name="m">The move to convert</param>
        /// <param name="output">The string builder used to generate the string with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToString(Move m, StringBuilder output)
        {
            if (m.IsNullMove())
            {
                output.Append("(none)");
                return;
            }

            var from = m.GetFromSquare();
            var to = m.GetToSquare();

            if (m.IsCastlelingMove() && !Chess960)
            {
                var file = to > from ? Files.FileG : Files.FileC;
                to = new Square(from.Rank(), file);
            }

            output.Append(from.ToString()).Append(to.ToString());

            if (m.IsPromotionMove())
                output.Append(char.ToLower(m.GetPromotedPiece().Type().GetPieceChar()));
        }
        private void DoCastleling(Square from, ref Square to, out Square rfrom, out Square rto, bool doCastleling, Player stm)
        {
            var kingSide = to > from;
            rfrom = to; // Castling is encoded as "king captures friendly rook"
            rto = (kingSide ? Squares.f1 : Squares.d1).RelativeSquare(stm);
            to = (kingSide ? Squares.g1 : Squares.c1).RelativeSquare(stm);

            // Remove both pieces first since squares could overlap in Chess960
            RemovePiece(doCastleling ? from : to);
            RemovePiece(doCastleling ? rfrom : rto);
            BoardLayout[(doCastleling ? from : to).AsInt()] = BoardLayout[(doCastleling ? rfrom : rto).AsInt()] = Enums.Pieces.NoPiece;
            AddPiece(PieceTypes.King, doCastleling ? to : from, stm);
            AddPiece(PieceTypes.Rook, doCastleling ? rto : rfrom, stm);
        }
        
        public IEnumerator<Piece> GetEnumerator() => BoardLayout.Cast<Piece>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static CastlelingRights OrCastlingRight(Player c, CastlelingSides s)
        {
            return (CastlelingRights)((int) CastlelingRights.WhiteOo << (s == CastlelingSides.Queen ? 1 : 0) + 2 * c.Side);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CastlelingSides ShredderFunc(Square from, Square to) =>
            GetPiece(from).Value == Enums.Pieces.WhiteKing && GetPiece(to).Value == Enums.Pieces.WhiteRook || GetPiece(from).Value == Enums.Pieces.BlackKing && GetPiece(to).Value == Enums.Pieces.BlackRook
                ? to > from
                    ? CastlelingSides.King
                    : CastlelingSides.Queen
                : CastlelingSides.None;
    }
}