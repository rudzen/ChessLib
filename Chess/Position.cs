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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Enums;
    using Exceptions;
    using Extensions;
    using Fen;
    using Hash;
    using Microsoft.Extensions.ObjectPool;
    using MoveGeneration;
    using Types;
    using Validation;

    /// <summary>
    /// The main board representation class. It stores all the information about the current board
    /// in a simple structure. It also serves the purpose of being able to give the UI controller
    /// feedback on various things on the board
    /// </summary>
    public sealed class Position : IPosition
    {
        private readonly BitBoard[] _castlingPath;
        private readonly CastlelingRights[] _castlingRightsMask;
        private readonly Square[] _castlingRookSquare;
        private readonly ObjectPool<StringBuilder> _outputObjectPool;
        private readonly IPositionValidator _positionValidator;
        private Player _sideToMove;

        public Position(IBoard board, IPieceValue pieceValues)
        {
            Board = board;
            PieceValue = pieceValues;
            _outputObjectPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
            _positionValidator = new PositionValidator(this, Board);
            _castlingPath = new BitBoard[CastlelingRights.CastleRightsNb.AsInt()];
            _castlingRookSquare = new Square[CastlelingRights.CastleRightsNb.AsInt()];
            _castlingRightsMask = new CastlelingRights[64];
            IsProbing = true;
            Clear();
        }

        public IBoard Board { get; }

        public BitBoard Checkers
            => State.Checkers;

        public bool Chess960 { get; set; }

        public Square EnPassantSquare
            => State.EnPassantSquare;

        public string FenNotation
            => GenerateFen().ToString();

        public bool InCheck
            => !State.Checkers.IsEmpty;

        public bool IsMate
            => !this.GenerateMoves().Any();

        public bool IsProbing { get; set; }

        public bool IsRepetition
            => State.Repetition >= 3;

        /// <summary>
        /// To let something outside the library be aware of changes (like a UI etc)
        /// </summary>
        public Action<Piece, Square> PieceUpdated { get; set; }

        public IPieceValue PieceValue { get; }

        public int Ply { get; private set; }

        public int Rule50 => State.Rule50;

        public Player SideToMove
            => _sideToMove;

        public State State { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(Piece pc, Square sq)
        {
            Board.AddPiece(pc, sq);

            if (IsProbing)
                return;

            PieceUpdated?.Invoke(pc, sq);
        }

        public bool AttackedByKing(Square sq, Player c)
            => !(GetAttacks(sq, PieceTypes.King) & GetKingSquare(c)).IsEmpty;

        public bool AttackedByKnight(Square sq, Player c)
            => !(Board.Pieces(c, PieceTypes.Knight) & GetAttacks(sq, PieceTypes.Knight)).IsEmpty;

        public bool AttackedByPawn(Square sq, Player c) =>
            !(Board.Pieces(c, PieceTypes.Pawn) & sq.PawnAttack(~c)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedBySlider(Square sq, Player c)
        {
            var occupied = Board.Pieces();
            var rookAttacks = sq.RookAttacks(occupied);
            if (Board.Pieces(c, PieceTypes.Rook) & rookAttacks)
                return true;

            var bishopAttacks = sq.BishopAttacks(occupied);
            if (Board.Pieces(c, PieceTypes.Bishop) & bishopAttacks)
                return true;

            return (Board.Pieces(c, PieceTypes.Queen) & (bishopAttacks | rookAttacks)) != 0;
        }

        public BitBoard AttacksTo(Square sq, BitBoard occupied)
        {
            Debug.Assert(sq >= Enums.Squares.a1 && sq <= Enums.Squares.h8);

            return (sq.PawnAttack(Player.White) & Board.Pieces(Player.Black, PieceTypes.Pawn))
                   | (sq.PawnAttack(Player.Black) & Board.Pieces(Player.White, PieceTypes.Pawn))
                   | (GetAttacks(sq, PieceTypes.Knight) & Board.Pieces(PieceTypes.Knight))
                   | (GetAttacks(sq, PieceTypes.Rook, occupied) & Board.Pieces(PieceTypes.Rook, PieceTypes.Queen))
                   | (GetAttacks(sq, PieceTypes.Bishop, occupied) & Board.Pieces(PieceTypes.Bishop, PieceTypes.Queen))
                   | (GetAttacks(sq, PieceTypes.King) & Board.Pieces(PieceTypes.King));
        }

        public BitBoard AttacksTo(Square sq)
            => AttacksTo(sq, Board.Pieces());

        public BitBoard BlockersForKing(Player c)
            => State.BlockersForKing[c.Side];

        public bool CanCastle(CastlelingRights cr)
            => State.CastlelingRights.HasFlagFast(cr);

        public bool CanCastle(Player color)
        {
            var c = (CastlelingRights)((int)(CastlelingRights.WhiteOo | CastlelingRights.WhiteOoo) << (2 * color.Side));
            return State.CastlelingRights.HasFlagFast(c);
        }

        public bool CastlingImpeded(CastlelingRights cr)
        {
            Debug.Assert(cr == CastlelingRights.WhiteOo || cr == CastlelingRights.WhiteOoo || cr == CastlelingRights.BlackOo || cr == CastlelingRights.BlackOoo);
            return !(Board.Pieces() & _castlingPath[cr.AsInt()]).IsEmpty;
        }

        public Square CastlingRookSquare(CastlelingRights cr)
        {
            Debug.Assert(cr == CastlelingRights.WhiteOo || cr == CastlelingRights.WhiteOoo || cr == CastlelingRights.BlackOo || cr == CastlelingRights.BlackOoo);
            return _castlingRookSquare[cr.AsInt()];
        }

        public BitBoard CheckedSquares(PieceTypes pt)
            => State.CheckedSquares[pt.AsInt()];

        public void Clear()
        {
            Board.Clear();
            _castlingPath.Fill(BitBoard.Empty);
            _castlingRightsMask.Fill(CastlelingRights.None);
            _castlingRookSquare.Fill(Square.None);
            _sideToMove = Players.White;
            Chess960 = false;
            if (State == null)
                State = new State();
            else
                State.Clear();
        }

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
                    var piece = Board.PieceAt(square);

                    if (piece == Piece.EmptyPiece)
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

            sb.Append(_sideToMove.IsWhite ? " w " : " b ");

            var castleRights = State.CastlelingRights;

            if (castleRights != CastlelingRights.None)
            {
                char castlelingChar;

                if (castleRights.HasFlagFast(CastlelingRights.WhiteOo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.WhiteOo).FileChar
                        : 'K';
                    sb.Append(castlelingChar);
                }

                if (castleRights.HasFlagFast(CastlelingRights.WhiteOoo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.WhiteOoo).FileChar
                        : 'Q';
                    sb.Append(castlelingChar);
                }

                if (castleRights.HasFlagFast(CastlelingRights.BlackOo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.BlackOo).FileChar
                        : 'k';
                    sb.Append(castlelingChar);
                }

                if (castleRights.HasFlagFast(CastlelingRights.BlackOoo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.BlackOoo).FileChar
                        : 'q';
                    sb.Append(castlelingChar);
                }
            }
            else
                sb.Append('-');

            sb.Append(' ');

            if (State.EnPassantSquare == Square.None)
                sb.Append('-');
            else
                sb.Append(State.EnPassantSquare.ToString());

            sb.Append(' ');

            sb.Append(State.Rule50);
            sb.Append(' ');
            sb.Append(1 + (Ply - _sideToMove.IsBlack.AsByte() / 2));

            return new FenData(sb.ToString());
        }

        public BitBoard GetAttacks(Square square, PieceTypes pt, BitBoard occupied)
        {
            Debug.Assert(pt != PieceTypes.Pawn, "Pawns need player");

            return pt switch
            {
                PieceTypes.Knight => pt.PseudoAttacks(square),
                PieceTypes.King => pt.PseudoAttacks(square),
                PieceTypes.Bishop => square.BishopAttacks(occupied),
                PieceTypes.Rook => square.RookAttacks(occupied),
                PieceTypes.Queen => square.QueenAttacks(occupied),
                _ => BitBoard.Empty
            };
        }

        public BitBoard GetAttacks(Square square, PieceTypes pt)
            => GetAttacks(square, pt, Pieces());

        public CastlelingRights GetCastlelingRightsMask(Square sq)
            => _castlingRightsMask[sq.AsInt()];

        public IEnumerator<Piece> GetEnumerator()
            => Board.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public Square GetKingSquare(Player color)
            => Board.Square(PieceTypes.King, color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashKey GetPawnKey()
        {
            var result = Zobrist.ZobristNoPawn;
            var pieces = Board.Pieces(PieceTypes.Pawn);
            while (pieces)
            {
                var sq = BitBoards.PopLsb(ref pieces);
                var pc = GetPiece(sq);
                result ^= pc.GetZobristPst(sq);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square sq) => Board.PieceAt(sq);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashKey GetPiecesKey()
        {
            var result = new HashKey();
            var pieces = Board.Pieces();
            while (pieces)
            {
                var sq = BitBoards.PopLsb(ref pieces);
                var pc = Board.PieceAt(sq);
                result ^= pc.GetZobristPst(sq);
            }

            return result;
        }

        public Square GetPieceSquare(PieceTypes pt, Player c)
            => Board.Square(pt, c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceTypes GetPieceType(Square sq) => Board.PieceAt(sq).Type();

        public bool GivesCheck(Move m)
        {
            Debug.Assert(!m.IsNullMove());
            Debug.Assert(MovedPiece(m).ColorOf() == _sideToMove);

            var from = m.FromSquare();
            var to = m.ToSquare();

            var pc = Board.PieceAt(from);
            var pt = pc.Type();

            // Is there a direct check?
            if (!(State.CheckedSquares[pt.AsInt()] & to).IsEmpty)
                return true;

            var us = _sideToMove;
            var them = ~us;

            // Is there a discovered check?
            if (!(State.BlockersForKing[them.Side] & from).IsEmpty
                && !from.Aligned(to, GetKingSquare(them)))
                return true;

            switch (m.MoveType())
            {
                case MoveTypes.Normal:
                    return false;

                case MoveTypes.Promotion:
                    return !(GetAttacks(to, m.PromotedPieceType(), Board.Pieces() ^ from) & GetKingSquare(them)).IsEmpty;

                // En passant capture with check? We have already handled the case of direct checks
                // and ordinary discovered check, so the only case we need to handle is the unusual
                // case of a discovered check through the captured pawn.
                case MoveTypes.Enpassant:
                    {
                        var captureSquare = new Square(from.Rank, to.File);
                        var b = (Board.Pieces() ^ from ^ captureSquare) | to;
                        var ksq = GetKingSquare(them);

                        var attacks = (GetAttacks(ksq, PieceTypes.Rook, b) & Board.Pieces(us, PieceTypes.Rook, PieceTypes.Queen))
                                      | (GetAttacks(ksq, PieceTypes.Bishop, b) & Board.Pieces(us, PieceTypes.Bishop, PieceTypes.Queen));
                        return !attacks.IsEmpty;
                    }
                case MoveTypes.Castling:
                    {
                        var kingFrom = from;
                        var rookFrom = to; // Castling is encoded as 'King captures the rook'
                        var kingTo = (rookFrom > kingFrom ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(us);
                        var rookTo = (rookFrom > kingFrom ? Enums.Squares.f1 : Enums.Squares.d1).RelativeSquare(us);
                        var ksq = GetKingSquare(them);

                        return !(PieceTypes.Rook.PseudoAttacks(rookTo) & ksq).IsEmpty && !(GetAttacks(rookTo, PieceTypes.Rook, Board.Pieces() ^ kingFrom ^ rookFrom | rookTo | kingTo) & ksq).IsEmpty;
                    }
                default:
                    Debug.Assert(false);
                    return false;
            }
        }

        public bool HasGameCycle(int ply)
        {
            var end = State.Rule50 < State.PliesFromNull
                ? State.Rule50
                : State.PliesFromNull;

            return end >= 3 && Cuckoo.HashCuckooCycle(this, end, ply);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square sq, Player c)
            => AttackedBySlider(sq, c) || AttackedByKnight(sq, c) || AttackedByPawn(sq, c) || AttackedByKing(sq, c);

        public bool IsLegal(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            var us = _sideToMove;
            var from = m.FromSquare();
            var to = m.ToSquare();
            var ksq = GetKingSquare(us);

            // Debug.Assert(movedPiece.ColorOf() == us);
            Debug.Assert(GetPiece(GetKingSquare(us)) == PieceTypes.King.MakePiece(us));

            // En passant captures are a tricky special case. Because they are rather uncommon, we
            // do it simply by testing whether the king is attacked after the move is made.
            if (m.IsEnPassantMove())
            {
                var captureSquare = to - us.PawnPushDistance();
                var occupied = (Board.Pieces() ^ from ^ captureSquare) | to;

                Debug.Assert(to == EnPassantSquare);
                Debug.Assert(MovedPiece(m) == PieceTypes.Pawn.MakePiece(us));
                Debug.Assert(GetPiece(captureSquare) == PieceTypes.Pawn.MakePiece(~us));
                Debug.Assert(GetPiece(to) == Piece.EmptyPiece);

                return (GetAttacks(ksq, PieceTypes.Rook, occupied) & Board.Pieces(~us, PieceTypes.Rook, PieceTypes.Queen)).IsEmpty
                       && (GetAttacks(ksq, PieceTypes.Bishop, occupied) & Board.Pieces(~us, PieceTypes.Bishop, PieceTypes.Queen)).IsEmpty;
            }

            // Check for legal castleling move
            if (m.IsCastlelingMove())
            {
                // After castling, the rook and king final positions are the same in Chess960 as
                // they would be in standard chess.

                var isKingSide = to > from;
                to = (isKingSide ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(us);
                var step = isKingSide ? Directions.West : Directions.East;

                for (var s = to; s != from; s += step)
                    if (AttacksTo(s) & Board.Pieces(~us))
                        return false;

                // In case of Chess960, verify that when moving the castling rook we do not discover
                // some hidden checker. For instance an enemy queen in SQ_A1 when castling rook is
                // in SQ_B1.
                return !Chess960
                       || (GetAttacks(to, PieceTypes.Rook, Board.Pieces() ^ m.ToSquare()) & Board.Pieces(~us, PieceTypes.Rook, PieceTypes.Queen)).IsEmpty;
            }

            // If the moving piece is a king, check whether the destination square is attacked by
            // the opponent.
            if (MovedPiece(m).Type() == PieceTypes.King)
                return m.IsCastlelingMove() || (AttacksTo(to) & Board.Pieces(~us)).IsEmpty;

            // A non-king move is legal if and only if it is not pinned or it is moving along the
            // ray towards or away from the king.
            return (BlockersForKing(us) & from).IsEmpty || from.Aligned(to, ksq);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square sq)
            => !Board.IsEmpty(sq);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPieceTypeOnSquare(Square sq, PieceTypes pt) => Board.PieceAt(sq).Type() == pt;

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
            // Use a slower but simpler function for uncommon cases
            if (m.MoveType() != MoveTypes.Normal)
                return this.GenerateMoves().Contains(m);

            // Is not a promotion, so promotion piece must be empty
            if (m.PromotedPieceType() - 2 != PieceTypes.NoPieceType)
                return false;

            var us = _sideToMove;
            var pc = MovedPiece(m);
            var from = m.FromSquare();
            var to = m.ToSquare();

            // If the from square is not occupied by a piece belonging to the side to move, the move
            // is obviously not legal.
            if (pc == Piece.EmptyPiece || pc.ColorOf() != us)
                return false;

            // The destination square cannot be occupied by a friendly piece
            if (!(Pieces(us) & to).IsEmpty)
                return false;

            // Handle the special case of a pawn move
            if (pc.Type() == PieceTypes.Pawn)
            {
                // We have already handled promotion moves, so destination cannot be on the 8th/1st rank.
                if (to.Rank == Ranks.Rank8.RelativeRank(us))
                    return false;

                if ((from.PawnAttack(us) & Pieces(~us) & to).IsEmpty // Not a capture
                    && !(from + us.PawnPushDistance() == to && !IsOccupied(to)) // Not a single push
                    && !(from + us.PawnDoublePushDistance() == to // Not a double push
                         && from.Rank == Ranks.Rank2.RelativeRank(us)
                         && !IsOccupied(to)
                         && !IsOccupied(to - us.PawnPushDistance())))
                    return false;
            }
            else if ((GetAttacks(from, pc.Type()) & to).IsEmpty)
                return false;

            // if king is not in check no need to proceed
            if (Checkers.IsEmpty)
                return true;

            // Evasions generator already takes care to avoid some kind of illegal moves and legal()
            // relies on this. We therefore have to take care that the same kind of moves are
            // filtered out here.

            if (pc.Type() != PieceTypes.King)
            {
                // Double check? In this case a king move is required
                if (Checkers.MoreThanOne())
                    return false;

                // Our move must be a blocking evasion or a capture of the checking piece
                if (((Checkers.Lsb().BitboardBetween(GetKingSquare(us)) | Checkers) & to).IsEmpty)
                    return false;
            }
            // In case of king moves under check we have to remove king so to catch as invalid moves
            // like b1a1 when opposite queen is on c1.
            else if (!(AttacksTo(to, Pieces() ^ from) & Pieces(~us)).IsEmpty)
                return false;

            return true;
        }

        public void MakeMove(Move m, State newState)
        {
            var givesCheck = GivesCheck(m);
            MakeMove(m, newState, givesCheck);
        }

        public void MakeMove(Move m, State newState, bool givesCheck)
        {
            State = State.CopyTo(newState);
            State.LastMove = m;

            var k = State.Key ^ Zobrist.GetZobristSide();

            Ply++;
            State.Rule50++;
            State.PliesFromNull++;

            var us = _sideToMove;
            var them = ~us;
            var to = m.ToSquare();
            var from = m.FromSquare();
            var pc = GetPiece(from);
            var pt = pc.Type();
            var isPawn = pt == PieceTypes.Pawn;
            var capturedPiece = m.IsEnPassantMove()
                ? PieceTypes.Pawn.MakePiece(them)
                : GetPiece(to);

            Debug.Assert(pc.ColorOf() == us);
            Debug.Assert(capturedPiece == Piece.EmptyPiece || capturedPiece.ColorOf() == (!m.IsCastlelingMove() ? them : us));
            Debug.Assert(capturedPiece.Type() != PieceTypes.King);

            if (m.IsCastlelingMove())
            {
                Debug.Assert(pc == PieceTypes.King.MakePiece(us));
                Debug.Assert(capturedPiece == PieceTypes.Rook.MakePiece(us));

                DoCastleling(us, from, ref to, out var rookFrom, out var rookTo, CastlelingPerform.Do);

                k ^= capturedPiece.GetZobristPst(rookFrom);
                k ^= capturedPiece.GetZobristPst(rookTo);

                // reset captured piece type as castleling is "king-captures-rook"
                capturedPiece = Piece.EmptyPiece;
            }

            if (capturedPiece != Piece.EmptyPiece)
            {
                var captureSquare = to;

                if (capturedPiece.Type() == PieceTypes.Pawn)
                {
                    if (m.IsEnPassantMove())
                    {
                        captureSquare -= us.PawnPushDistance();

                        Debug.Assert(pt == PieceTypes.Pawn);
                        Debug.Assert(to == State.EnPassantSquare);
                        Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                        Debug.Assert(!IsOccupied(to));
                        Debug.Assert(GetPiece(captureSquare) == pt.MakePiece(them));
                    }

                    State.PawnStructureKey ^= capturedPiece.GetZobristPst(captureSquare);
                }
                else
                {
                    State.NonPawnMaterial[them.Side] -= PieceValue.GetPieceValue(capturedPiece, Phases.Mg);
                    // TODO : Update material here
                }

                // Update board and piece lists
                RemovePiece(captureSquare);
                if (m.IsEnPassantMove())
                    Board.ClearPiece(captureSquare);

                k ^= capturedPiece.GetZobristPst(captureSquare);

                // TODO : Update other depending keys and psq values here

                // Reset rule 50 counter
                State.Rule50 = 0;
            }

            // update key with moved piece
            k ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

            // reset en-passant square if it is set
            if (State.EnPassantSquare != Square.None)
            {
                k ^= State.EnPassantSquare.File.GetZobristEnPessant();
                State.EnPassantSquare = Square.None;
            }

            // Update castling rights if needed
            if (State.CastlelingRights != CastlelingRights.None && (_castlingRightsMask[from.AsInt()] | _castlingRightsMask[to.AsInt()]) != 0)
            {
                var cr = _castlingRightsMask[from.AsInt()] | _castlingRightsMask[to.AsInt()];
                k ^= (State.CastlelingRights & cr).GetZobristCastleling();
                State.CastlelingRights &= ~cr;
            }

            // Move the piece. The tricky Chess960 castle is handled earlier
            if (!m.IsCastlelingMove())
                MovePiece(from, to);

            // If the moving piece is a pawn do some special extra work
            if (isPawn)
            {
                // Set en-passant square, only if moved pawn can be captured
                if ((to.Value.AsInt() ^ from.Value.AsInt()) == 16
                    && !((to - us.PawnPushDistance()).PawnAttack(us) & Pieces(PieceTypes.Pawn, them)).IsEmpty)
                {
                    State.EnPassantSquare = to - us.PawnPushDistance();
                    k ^= State.EnPassantSquare.File.GetZobristEnPessant();
                }
                else if (m.IsPromotionMove())
                {
                    var promotionPiece = m.PromotedPieceType().MakePiece(us);

                    Debug.Assert(to.RelativeRank(us) == Ranks.Rank8);
                    Debug.Assert(promotionPiece.Type() >= PieceTypes.Knight && promotionPiece.Type() <= PieceTypes.Queen);

                    RemovePiece(to);
                    AddPiece(promotionPiece, to);

                    // Update hash keys
                    k ^= pc.GetZobristPst(to) ^ promotionPiece.GetZobristPst(to);
                    State.PawnStructureKey ^= pc.GetZobristPst(to);

                    State.NonPawnMaterial[us.Side] += PieceValue.GetPieceValue(promotionPiece, Phases.Mg);
                }

                // Update pawn hash key
                State.PawnStructureKey ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

                // Reset rule 50 draw counter
                State.Rule50 = 0;
            }

            // TODO : Update piece values here

            Debug.Assert(GetKingSquare(us).IsOk);
            Debug.Assert(GetKingSquare(them).IsOk);

            // Update state properties
            State.Key = k;
            State.CapturedPiece = capturedPiece;

            State.Checkers = givesCheck ? AttacksTo(GetKingSquare(them)) & Board.Pieces(us) : BitBoard.Empty;

            _sideToMove = ~_sideToMove;

            SetCheckInfo(State);
            State.UpdateRepetition();

            //Debug.Assert(_positionValidator.Validate().IsOk);
        }

        public void MakeNullMove(State newState)
        {
            Debug.Assert(!InCheck);

            State = State.CopyTo(newState);

            if (State.EnPassantSquare != Square.None)
            {
                var enPassantFile = State.EnPassantSquare.File;
                State.Key ^= enPassantFile.GetZobristEnPessant();
                State.EnPassantSquare = Square.None;
            }

            State.Key ^= Zobrist.GetZobristSide();

            ++State.Rule50;
            State.PliesFromNull = 0;

            _sideToMove = ~_sideToMove;

            SetCheckInfo(State);

            State.Repetition = 0;

            Debug.Assert(_positionValidator.Validate(PositionValidationTypes.Basic).IsOk);
        }

        public Piece MovedPiece(Move m)
            => Board.MovedPiece(m);

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

            var from = m.FromSquare();
            var to = m.ToSquare();

            if (m.IsCastlelingMove() && !Chess960)
            {
                var file = to > from ? Files.FileG : Files.FileC;
                to = new Square(from.Rank, file);
            }

            output.Append(from.ToString()).Append(to.ToString());

            if (m.IsPromotionMove())
                output.Append(char.ToLower(m.PromotedPieceType().GetPieceChar()));
        }

        /// <summary>
        /// Determine if a specific square is a passed pawn
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PassedPawn(Square sq)
        {
            var pc = Board.PieceAt(sq);

            if (pc.Type() != PieceTypes.Pawn)
                return false;

            var c = pc.ColorOf();

            return (sq.PassedPawnFrontAttackSpan(c) & Board.Pieces(c, PieceTypes.Pawn)).IsEmpty;
        }

        /// <summary>
        /// Determine if a pawn is isolated e.i. no own pawns on either of it's neighboring files
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PawnIsolated(Square sq, Player c)
            => ((sq.PawnAttackSpan(c) | sq.PawnAttackSpan(~c)) & Board.Pieces(c, PieceTypes.Pawn)).IsEmpty;

        public bool PieceOnFile(Square sq, Player c, PieceTypes pt)
            => !(Board.Pieces(c, pt) & sq).IsEmpty;

        public BitBoard Pieces()
            => Board.Pieces();

        public BitBoard Pieces(Player c)
            => Board.Pieces(c);

        public BitBoard Pieces(Piece pc)
            => Board.Pieces(pc.ColorOf(), pc.Type());

        public BitBoard Pieces(PieceTypes pt)
            => Board.Pieces(pt);

        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2)
            => Board.Pieces(pt1, pt2);

        public BitBoard Pieces(PieceTypes pt, Player side)
            => Board.Pieces(side, pt);

        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player c)
            => Board.Pieces(c, pt1, pt2);

        public BitBoard PinnedPieces(Player c)
            => State.Pinners[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square sq)
        {
            Board.RemovePiece(sq);
            if (!IsProbing)
                PieceUpdated?.Invoke(Piece.EmptyPiece, sq);
        }

        public bool SeeGe(Move m, Value threshold)
        {
            Debug.Assert(m.IsNullMove());

            // Only deal with normal moves, assume others pass a simple see
            if (m.MoveType() != MoveTypes.Normal)
                return Value.ValueZero >= threshold;

            var from = m.FromSquare();
            var to = m.ToSquare();

            var swap = PieceValue.GetPieceValue(GetPiece(to), Phases.Mg) - threshold;
            if (swap < Value.ValueZero)
                return false;

            swap = PieceValue.GetPieceValue(GetPiece(from), Phases.Mg) - swap;
            if (swap <= Value.ValueZero)
                return true;

            var occupied = Board.Pieces() ^ from ^ to;
            var stm = GetPiece(from).ColorOf();
            var attackers = AttacksTo(to, occupied);
            var res = 1;

            while (true)
            {
                stm = ~stm;
                attackers &= occupied;

                // If stm has no more attackers then give up: stm loses
                var stmAttackers = attackers & Board.Pieces(stm);
                if (stmAttackers.IsEmpty)
                    break;

                // Don't allow pinned pieces to attack (except the king) as long as there are
                // pinners on their original square.
                if (PinnedPieces(~stm) & occupied)
                    stmAttackers &= ~State.BlockersForKing[stm.Side];

                if (stmAttackers.IsEmpty)
                    break;

                res ^= 1;

                // Locate and remove the next least valuable attacker, and add to the bitboard
                // 'attackers' any X-ray attackers behind it.
                var bb = stmAttackers & Board.Pieces(PieceTypes.Pawn);
                if (!bb.IsEmpty)
                {
                    if ((swap = PieceValue.PawnValueMg - swap) < res)
                        break;

                    occupied ^= bb.Lsb();
                    attackers |= GetAttacks(to, PieceTypes.Bishop, occupied) & Board.Pieces(PieceTypes.Bishop, PieceTypes.Queen);
                }
                else if (!(bb = stmAttackers & Board.Pieces(PieceTypes.Knight)).IsEmpty)
                {
                    if ((swap = PieceValue.KnightValueMg - swap) < res)
                        break;

                    occupied ^= bb.Lsb();
                }
                else if (!(bb = stmAttackers & Board.Pieces(PieceTypes.Bishop)).IsEmpty)
                {
                    if ((swap = PieceValue.BishopValueMg - swap) < res)
                        break;

                    occupied ^= bb.Lsb();
                    attackers |= GetAttacks(to, PieceTypes.Bishop, occupied) & Board.Pieces(PieceTypes.Bishop, PieceTypes.Queen);
                }
                else if (!(bb = stmAttackers & Board.Pieces(PieceTypes.Rook)).IsEmpty)
                {
                    if ((swap = PieceValue.RookValueMg - swap) < res)
                        break;

                    occupied ^= bb.Lsb();
                    attackers |= GetAttacks(to, PieceTypes.Rook, occupied) & Board.Pieces(PieceTypes.Rook, PieceTypes.Queen);
                }
                else if (!(bb = stmAttackers & Board.Pieces(PieceTypes.Queen)).IsEmpty)
                {
                    if ((swap = PieceValue.QueenValueMg - swap) < res)
                        break;

                    occupied ^= bb.Lsb();
                    attackers |= (GetAttacks(to, PieceTypes.Bishop, occupied) & Board.Pieces(PieceTypes.Bishop, PieceTypes.Queen))
                                 | (GetAttacks(to, PieceTypes.Rook, occupied) & Board.Pieces(PieceTypes.Rook, PieceTypes.Queen));
                }
                else // KING
                     // If we "capture" with the king but opponent still has attackers, reverse the result.
                {
                    bb = attackers & ~Board.Pieces(stm);
                    if (!bb.IsEmpty)
                        res ^= 1;
                    return res > 0;
                }
            }

            return res > 0;
        }

        /// <summary>
        /// Apply a FEN string board setup to the board structure.
        /// </summary>
        /// <param name="fen">The fen data to set</param>
        /// <param name="validate">If true, the fen string is validated, otherwise not</param>
        public void SetFen(FenData fen, bool validate = false)
        {
            if (validate)
                Fen.Fen.Validate(fen.Fen.ToString());

            Clear();

            var chunk = fen.Chunk();

            if (chunk.IsEmpty)
                throw new InvalidFen($"Invalid board layout detected for : {fen.Fen.ToString()}");

            var f = 1; // file (column)
            var r = 8; // rank (row)

            foreach (var c in chunk)
            {
                if (char.IsNumber(c))
                {
                    f += c - '0';
                    if (f > 9)
                        throw new InvalidFen($"File exceeded at index {fen.Index}");
                }
                else if (c == '/')
                {
                    if (f != 9)
                        throw new InvalidFen($"File value mismatch at index {fen.Index}");

                    r--;
                    f = 1;
                }
                else
                {
                    var pieceIndex = PieceExtensions.PieceChars.IndexOf(c);

                    if (pieceIndex == -1)
                        throw new InvalidFen($"Invalid piece information '{c}' at index {fen.Index}");

                    Player player = char.IsLower(PieceExtensions.PieceChars[pieceIndex]);

                    var square = new Square(r - 1, f - 1);

                    var pc = ((PieceTypes)pieceIndex).MakePiece(player);
                    AddPiece(pc, square);

                    f++;
                }
            }

            // player
            chunk = fen.Chunk();

            if (chunk.IsEmpty || chunk.Length != 1)
                throw new InvalidFen($"Player information not found at index {fen.Index}");

            _sideToMove = (chunk[0] != 'w').ToInt();

            // castleling
            chunk = fen.Chunk();

            if (chunk.IsEmpty)
                throw new InvalidFen($"Castleling information not found at index {fen.Index}");

            SetupCastleling(chunk);

            // en-passant
            chunk = fen.Chunk();

            State.EnPassantSquare = chunk.Length == 1 || chunk[0] == '-' || !chunk[0].InBetween('a', 'h')
                ? Square.None
              : chunk[1] != '3' && chunk[1] != '6'
                    ? Square.None
                    : new Square(chunk[1] - '1', chunk[0] - 'a').Value;

            // move number
            chunk = fen.Chunk();

            var moveNum = 0;
            var halfMoveNum = 0;

            if (!chunk.IsEmpty)
            {
                chunk.ToIntegral(out halfMoveNum);

                // half move number
                chunk = fen.Chunk();

                chunk.ToIntegral(out moveNum);

                if (moveNum > 0)
                    moveNum--;
            }

            State.Rule50 = halfMoveNum;
            Ply = moveNum;

            SetState();
        }

        public ReadOnlySpan<Square> Squares(PieceTypes pt, Player c)
            => Board.Squares(pt, c);

        public void TakeMove(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            // flip sides
            _sideToMove = ~_sideToMove;

            var us = _sideToMove;
            var from = m.FromSquare();
            var to = m.ToSquare();
            var pc = GetPiece(to);

            Debug.Assert(!IsOccupied(from) || m.IsCastlelingMove());
            Debug.Assert(State.CapturedPiece.Type() != PieceTypes.King);

            if (m.IsPromotionMove())
            {
                Debug.Assert(pc.Type() == m.PromotedPieceType());
                Debug.Assert(to.RelativeRank(us) == Ranks.Rank8);
                Debug.Assert(m.PromotedPieceType() >= PieceTypes.Knight && m.PromotedPieceType() <= PieceTypes.Queen);

                RemovePiece(to);
                pc = PieceTypes.Pawn.MakePiece(us);
                AddPiece(pc, to);
            }

            if (m.IsCastlelingMove())
                DoCastleling(us, from, ref to, out _, out _, CastlelingPerform.Undo);
            else
            {
                MovePiece(to, from);

                if (State.CapturedPiece != Piece.EmptyPiece)
                {
                    var captureSquare = to;

                    // En-Passant capture is not located on move square
                    if (m.IsEnPassantMove())
                    {
                        captureSquare -= us.PawnPushDistance();

                        Debug.Assert(pc.Type() == PieceTypes.Pawn);
                        Debug.Assert(to == State.Previous.EnPassantSquare);
                        Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                        Debug.Assert(!IsOccupied(captureSquare));
                    }

                    AddPiece(State.CapturedPiece, captureSquare);
                }
            }

            Debug.Assert(GetKingSquare(~us).IsOk);
            Debug.Assert(GetKingSquare(us).IsOk);

            // Set state to previous state
            State = State.Previous;
            Ply--;

            //Debug.Assert(_positionValidator.Validate().IsOk);
        }

        public void TakeNullMove()
        {
            Debug.Assert(!InCheck);
            State = State.Previous;
            _sideToMove = ~_sideToMove;
        }

        public override string ToString()
        {
            const string separator = "\n  +---+---+---+---+---+---+---+---+\n";
            const char splitter = '|';
            const char space = ' ';
            var output = _outputObjectPool.Get();
            output.Append(separator);
            for (var rank = Ranks.Rank8; rank >= Ranks.Rank1; rank--)
            {
                output.Append(rank.AsInt() + 1);
                output.Append(space);
                for (var file = Files.FileA; file <= Files.FileH; file++)
                {
                    var piece = GetPiece(new Square(rank, file));
                    output.AppendFormat("{0}{1}{2}{1}", splitter, space, piece.GetPieceChar());
                }

                output.Append(splitter);
                output.Append(separator);
            }

            output.AppendLine("    a   b   c   d   e   f   g   h");
            output.AppendLine($"Zobrist : 0x{State.Key.Key:X}");
            var result = output.ToString();
            _outputObjectPool.Return(output);
            return result;
        }

        public IPositionValidator Validate(PositionValidationTypes type = PositionValidationTypes.Basic)
            => _positionValidator.Validate(type);

        private static CastlelingRights OrCastlingRight(Player c, bool isKingSide)
            => (CastlelingRights)((int)CastlelingRights.WhiteOo << ((!isKingSide).AsByte() + 2 * c.Side));

        private void DoCastleling(Player us, Square from, ref Square to, out Square rookFrom, out Square rookTo, CastlelingPerform castlelingPerform)
        {
            var kingSide = to > from;
            var doCastleling = castlelingPerform == CastlelingPerform.Do;

            rookFrom = to; // Castling is encoded as "king captures friendly rook"
            rookTo = (kingSide ? Enums.Squares.f1 : Enums.Squares.d1).RelativeSquare(us);
            to = (kingSide ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(us);

            // Remove both pieces first since squares could overlap in Chess960
            RemovePiece(doCastleling ? from : to);
            RemovePiece(doCastleling ? rookFrom : rookTo);
            Board.ClearPiece(doCastleling ? from : to);
            Board.ClearPiece(doCastleling ? rookFrom : rookTo);
            AddPiece(PieceTypes.King.MakePiece(us), doCastleling ? to : from);
            AddPiece(PieceTypes.Rook.MakePiece(us), doCastleling ? rookTo : rookFrom);
        }

        private void MovePiece(Square from, Square to)
        {
            Board.MovePiece(from, to);

            if (IsProbing)
                return;

            var pc = Board.PieceAt(from);
            PieceUpdated?.Invoke(pc, to);
        }

        /// IPosition.SetCastlingRight() is an helper function used to set castling rights given the
        /// corresponding color and the rook starting square.
        private void SetCastlingRight(Player stm, Square rookFrom)
        {
            var kingFrom = GetKingSquare(stm);
            var isKingSide = kingFrom < rookFrom;
            var cr = OrCastlingRight(stm, isKingSide);

            State.CastlelingRights |= cr;
            _castlingRightsMask[kingFrom.AsInt()] |= cr;
            _castlingRightsMask[rookFrom.AsInt()] |= cr;
            _castlingRookSquare[cr.AsInt()] = rookFrom;

            var kingTo = (isKingSide ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(stm);
            var rookTo = (isKingSide ? Enums.Squares.f1 : Enums.Squares.d1).RelativeSquare(stm);

            _castlingPath[cr.AsInt()] = (rookFrom.BitboardBetween(rookTo) | kingFrom.BitboardBetween(kingTo) | rookTo | kingTo)
                                        & ~(kingFrom | rookFrom);
        }

        private void SetCheckInfo(State state)
        {
            (state.BlockersForKing[Player.White.Side], state.Pinners[Player.Black.Side]) = SliderBlockers(Board.Pieces(Player.Black), GetKingSquare(Player.White));
            (state.BlockersForKing[Player.Black.Side], state.Pinners[Player.White.Side]) = SliderBlockers(Board.Pieces(Player.White), GetKingSquare(Player.Black));

            var ksq = GetKingSquare(~_sideToMove);

            state.CheckedSquares[PieceTypes.Pawn.AsInt()] = ksq.PawnAttack(~_sideToMove);
            state.CheckedSquares[PieceTypes.Knight.AsInt()] = GetAttacks(ksq, PieceTypes.Knight);
            state.CheckedSquares[PieceTypes.Bishop.AsInt()] = GetAttacks(ksq, PieceTypes.Bishop);
            state.CheckedSquares[PieceTypes.Rook.AsInt()] = GetAttacks(ksq, PieceTypes.Rook);
            state.CheckedSquares[PieceTypes.Queen.AsInt()] = state.CheckedSquares[PieceTypes.Bishop.AsInt()] | state.CheckedSquares[PieceTypes.Rook.AsInt()];
            state.CheckedSquares[PieceTypes.King.AsInt()] = BitBoard.Empty;
        }

        private void SetState()
        {
            var key = new HashKey();
            var pawnKey = Zobrist.ZobristNoPawn;

            State.Checkers = AttacksTo(GetKingSquare(_sideToMove)) & Board.Pieces(~_sideToMove);
            State.NonPawnMaterial[Player.White.Side] = State.NonPawnMaterial[Player.Black.Side] = Value.ValueZero;
            SetCheckInfo(State);

            // compute hash keys
            for (var b = Board.Pieces(); !b.IsEmpty;)
            {
                var sq = BitBoards.PopLsb(ref b);
                var pc = GetPiece(sq);
                var pt = pc.Type();

                key ^= pc.GetZobristPst(sq);

                if (pt == PieceTypes.Pawn)
                    pawnKey ^= pc.GetZobristPst(sq);
                else if (pt != PieceTypes.King)
                    State.NonPawnMaterial[pc.ColorOf().Side] += PieceValue.GetPieceValue(pc, Phases.Mg);
            }

            if (State.EnPassantSquare != Square.None)
                key ^= State.EnPassantSquare.File.GetZobristEnPessant();

            if (_sideToMove.IsBlack)
                key ^= Zobrist.GetZobristSide();

            key ^= State.CastlelingRights.GetZobristCastleling();

            State.Key = key;
            State.PawnStructureKey = pawnKey;
        }

        private void SetupCastleling(ReadOnlySpan<char> castleling)
        {
            foreach (var ca in castleling)
            {
                Player c = char.IsLower(ca) ? 1 : 0;
                var rook = PieceTypes.Rook.MakePiece(c);
                var token = char.ToUpper(ca);

                Square rsq;
                switch (token)
                {
                    case 'K':
                        {
                            for (rsq = Enums.Squares.h1.RelativeSquare(c); GetPiece(rsq) != rook; --rsq)
                            { }

                            break;
                        }
                    case 'Q':
                        {
                            for (rsq = Enums.Squares.a1.RelativeSquare(c); GetPiece(rsq) != rook; --rsq)
                            { }

                            break;
                        }
                    default:
                        {
                            if (token.InBetween('A', 'H'))
                                rsq = new Square(Ranks.Rank1.RelativeRank(c), new File(token - 'A'));
                            else
                                continue;
                            break;
                        }
                }

                SetCastlingRight(c, rsq);
            }
        }

        private (BitBoard, BitBoard) SliderBlockers(BitBoard sliders, Square s)
        {
            var result = (blockers: BitBoard.Empty, pinners: BitBoard.Empty);

            // Snipers are sliders that attack 's' when a piece and other snipers are removed
            var snipers = (PieceTypes.Rook.PseudoAttacks(s) & Board.Pieces(PieceTypes.Queen, PieceTypes.Rook)
                               | (PieceTypes.Bishop.PseudoAttacks(s) & Board.Pieces(PieceTypes.Queen, PieceTypes.Bishop))) & sliders;
            var occupancy = Board.Pieces() ^ snipers;

            var pc = GetPiece(s);
            var uniColorPieces = Board.Pieces(pc.ColorOf());

            while (snipers)
            {
                var sniperSq = BitBoards.PopLsb(ref snipers);
                var b = s.BitboardBetween(sniperSq) & occupancy;

                if (b.IsEmpty || b.MoreThanOne())
                    continue;

                result.blockers |= b;

                if (b & uniColorPieces)
                    result.pinners |= sniperSq;

            }

            return result;

        }
    }
}