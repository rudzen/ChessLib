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
    using System.Diagnostics;
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
        private readonly CastlelingRights[] _castlingRightsMask;
        private readonly Square[] _castlingRookSquare;
        private readonly BitBoard[] _castlingPath;
        private readonly List<State> _stateStack;
        private Player _sideToMove;
        private int _ply;

        private readonly Board _board;
        
        public Position()
        {
            _board = new Board();
            _stateStack = new List<State>(256);
            _castlingPath = new BitBoard[CastlelingRights.CastleRightsNb.AsInt()];
            _castlingRookSquare = new Square[CastlelingRights.CastleRightsNb.AsInt()];
            _castlingRightsMask = new CastlelingRights[64];
            IsProbing = true;
            Clear();
        }

        public bool IsProbing { get; set; }

        /// <summary>
        /// To let something outside the library be aware of changes (like a UI etc)
        /// </summary>
        public Action<Piece, Square> PieceUpdated { get; set; }

        public State State { get; set; }

        public bool Chess960 { get; private set; }

        public Player SideToMove
            => _sideToMove;
        
        public Square EnPassantSquare
            => State.EnPassantSquare;

        public string FenNotation
            => GenerateFen().ToString();

        public Board Board
            => _board;
        
        public BitBoard Checkers
            => State.Checkers;

        public void Clear()
        {
            _board.Clear();
            _castlingPath.Fill(BitBoard.Empty);
            _castlingRightsMask.Fill(CastlelingRights.None);
            _castlingRookSquare.Fill(Square.None);
            _sideToMove = Players.White;
            Chess960 = false;
            State?.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(Piece pc, Square sq)
        {
            _board.AddPiece(pc, sq);
            
            if (IsProbing)
                return;
            
            PieceUpdated?.Invoke(pc, sq);
        }

        public void MovePiece(Square from, Square to)
        {
            _board.MovePiece(from, to);
            
            if (IsProbing)
                return;
            
            var pc = _board.PieceAt(from);                
            PieceUpdated?.Invoke(pc, to);
        }

        public void MakeMove(Move m, State newState)
        {
            var givesCheck = GivesCheck(m);
            MakeMove(m, newState, givesCheck);
        }
        
        public void MakeMove(Move m, State newState, bool givesCheck)
        {
            State.CopyTo(newState, m);
            State = newState;
            
            // StateAdd(State);
            
            var k = State.Key ^ Zobrist.GetZobristSide();

            _ply++;
            State.Rule50++;
            State.PliesFromNull++;

            var us = _sideToMove;
            var them = ~us;
            var to = m.GetToSquare();
            var from = m.GetFromSquare();
            var pc = GetPiece(from);
            var pt = pc.Type();
            var isPawn = pt == PieceTypes.Pawn;
            var capturedPiece = m.IsEnPassantMove()
                ? PieceTypes.Pawn.MakePiece(them)
                : GetPiece(to);

            Debug.Assert(pc.ColorOf() == us);
            //Debug.Assert(capturedPiece == Enums.Pieces.NoPiece || capturedPiece.ColorOf() == (!m.IsCastlelingMove() ? them : us));
            Debug.Assert(capturedPiece.Type() != PieceTypes.King);
            
            if (m.IsCastlelingMove())
            {
                Debug.Assert(pc == PieceTypes.King.MakePiece(us));
                Debug.Assert(capturedPiece == PieceTypes.Rook.MakePiece(us));
                
                DoCastleling(us, from, ref to, out var rookFrom, out var rookTo, CastlelingPerform.Do);

                k ^= capturedPiece.GetZobristPst(rookFrom) ^ capturedPiece.GetZobristPst(rookTo);

                // reset captured piece type as castleling is "king-captures-rook"
                capturedPiece = Enums.Pieces.NoPiece;
            }

            if (capturedPiece != Enums.Pieces.NoPiece)
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
                    // TODO : Update material here
                }

                // Update board and piece lists
                RemovePiece(captureSquare);
                if (m.IsEnPassantMove())
                    _board.ClearPiece(captureSquare);

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
                k ^= State.EnPassantSquare.File().GetZobristEnPessant();
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
                if (((int) to.Value ^ (int) from.Value) == 16
                    && !((to - us.PawnPushDistance()).PawnAttack(us) & Pieces(PieceTypes.Pawn, them)).IsEmpty)
                {
                    State.EnPassantSquare = to - us.PawnPushDistance();
                    k ^= State.EnPassantSquare.File().GetZobristEnPessant();
                }
                else if (m.IsPromotionMove())
                {
                    var promotionPiece = m.GetPromotedPieceType().MakePiece(us);

                    Debug.Assert(to.RelativeRank(us) == Ranks.Rank8);
                    Debug.Assert(promotionPiece.Type() >= PieceTypes.Knight && promotionPiece.Type() <= PieceTypes.Queen);

                    RemovePiece(to);
                    AddPiece(promotionPiece, to);

                    // Update hash keys
                    k ^= pc.GetZobristPst(to) ^ promotionPiece.GetZobristPst(to);
                    State.PawnStructureKey ^= pc.GetZobristPst(to);

                    // TODO : Update values and other keys here
                }

                // Update pawn hash key
                State.PawnStructureKey ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

                // Reset rule 50 draw counter
                State.Rule50 = 0;
            }

            // TODO : Update piece values here

            Debug.Assert(GetPieceSquare(PieceTypes.King, us).IsOk());
            Debug.Assert(GetPieceSquare(PieceTypes.King, them).IsOk());

            if (pt == PieceTypes.Queen && us == Player.White && to == Enums.Squares.a4)
            {
                var a = 1;
            }
            
            // Update state properties
            State.Key = k;
            State.CapturedPiece = capturedPiece;

            var ksq = GetPieceSquare(PieceTypes.King, them);

            State.Checkers = AttacksTo(ksq) & Pieces(us);// givesCheck ? AttacksTo(ksq) & Pieces(us) : BitBoard.Empty;
            State.InCheck = !State.Checkers.IsEmpty;


                
            
            _sideToMove = ~_sideToMove;

            SetCheckInfo(State);
            UpdateRepetition(State);
        }

        private static void UpdateRepetition(State state)
        {
            state.Repetition = 0;
            
            var end = state.Rule50 < state.PliesFromNull
                ? state.Rule50
                : state.PliesFromNull;
            
            if (end >= 4)
            {
                var statePrevious = state.Previous.Previous;
                for (var i = 4; i <= end; i += 2)
                {
                    statePrevious = statePrevious.Previous.Previous;
                    if (statePrevious.Key == state.Key)
                    {
                        state.Repetition = state.Repetition != 0 ? -i : i;
                        break;
                    }
                }
            }
        }

        public void TakeMove(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            // flip sides
            _sideToMove = ~_sideToMove;
            var us = _sideToMove;

            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var pc = GetPiece(to);

            Debug.Assert(!IsOccupied(from) || m.IsCastlelingMove());
            Debug.Assert(State.CapturedPiece.Type() != PieceTypes.King);
            
            if (m.IsPromotionMove())
            {
                Debug.Assert(pc.Type() == m.GetPromotedPieceType());
                Debug.Assert(to.RelativeRank(us) == Ranks.Rank8);
                Debug.Assert(m.GetPromotedPieceType() >= PieceTypes.Knight && m.GetPromotedPieceType() <= PieceTypes.Queen);

                RemovePiece(to);
                pc = PieceTypes.Pawn.MakePiece(us);
                AddPiece(pc, to);
            }

            if (m.IsCastlelingMove())
                DoCastleling(us, from, ref to, out _, out _, CastlelingPerform.Undo);
            else
            {
                MovePiece(to, from);

                if (State.CapturedPiece != Enums.Pieces.NoPiece)
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

            Debug.Assert(GetPieceSquare(PieceTypes.King, ~us).IsOk());
            Debug.Assert(GetPieceSquare(PieceTypes.King, us).IsOk());

            // Set state to previous state
            State = State.Previous;
            _ply--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square sq) => _board.PieceAt(sq);

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

            var pinnedPieces = BitBoard.Empty;
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
            => State.CheckedSquares[pt.AsInt()];

        public BitBoard PinnedPieces(Player c)
            => State.Pinners[c.Side];

        public BitBoard BlockersForKing(Player c)
            => State.BlockersForKing[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square sq)
            => !_board.IsEmpty(sq);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square sq, Player c)
            => AttackedBySlider(sq, c) || AttackedByKnight(sq, c) || AttackedByPawn(sq, c) || AttackedByKing(sq, c);

        public bool GivesCheck(Move m)
        {
            Debug.Assert(!m.IsNullMove());
            Debug.Assert(MovedPiece(m).ColorOf() == _sideToMove);

            var from = m.GetFromSquare();
            var to = m.GetToSquare();

            // Is there a direct check?
            if (State.CheckedSquares[GetPiece(from).Type().AsInt()] & to)
                return true;

            var them = ~_sideToMove;
            
            // Is there a discovered check?
            if (   !(State.BlockersForKing[them.Side] & from).IsEmpty
                   && !from.Aligned(to, GetPieceSquare(PieceTypes.King, them)))
                return true;

            switch (m.GetMoveType())
            {
                case MoveTypes.Normal:
                    return false;

                case MoveTypes.Promotion:
                    return !(GetAttacks(to, m.GetPromotedPieceType(), Pieces() ^ from) & GetPieceSquare(PieceTypes.King, them)).IsEmpty;

                // En passant capture with check? We have already handled the case
                // of direct checks and ordinary discovered check, so the only case we
                // need to handle is the unusual case of a discovered check through
                // the captured pawn.
                case MoveTypes.Enpassant:
                {
                    var captureSquare = new Square(from.Rank(), to.File());
                    var b = (Pieces() ^ from ^ captureSquare) | to;
                    var ksq = GetPieceSquare(PieceTypes.King, them);
                    
                    var attacks = (GetAttacks(ksq, PieceTypes.Rook, b) & Pieces(PieceTypes.Rook, PieceTypes.Queen, _sideToMove))
                        | (GetAttacks(ksq, PieceTypes.Bishop, b) & Pieces(PieceTypes.Bishop, PieceTypes.Queen, _sideToMove));
                    return !attacks.IsEmpty;
                }
                case MoveTypes.Castling:
                {
                    var kfrom = from;
                    var rfrom = to; // Castling is encoded as 'King captures the rook'
                    var kingTo = (rfrom > kfrom ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(_sideToMove);
                    var rookTo = (rfrom > kfrom ? Enums.Squares.f1 : Enums.Squares.d1).RelativeSquare(_sideToMove);
                    var ksq = GetPieceSquare(PieceTypes.King, them);

                    return !(PieceTypes.Rook.PseudoAttacks(rookTo) & ksq).IsEmpty && !(GetAttacks(rookTo, PieceTypes.Rook, Pieces() ^ kfrom ^ rfrom | rookTo | kingTo) & ksq).IsEmpty;
                }
                default:
                    Debug.Assert(false);
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces()
            => _board.Pieces();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Player c)
            => _board.Pieces(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Piece pc)
            => _board.Pieces(pc.ColorOf(), pc.Type());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt)
            => _board.Pieces(pt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2)
            => _board.Pieces(pt1, pt2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt, Player side)
            => _board.Pieces(side, pt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player c)
            => _board.Pieces(c, pt1, pt2);

        public ReadOnlySpan<Square> Squares(PieceTypes pt, Player c)
            => _board.Squares(pt, c);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetPieceSquare(PieceTypes pt, Player c)
            => _board.Square(pt, c);

        public Piece MovedPiece(Move m)
            => _board.MovedPiece(m);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PieceOnFile(Square sq, Player c, PieceTypes pt)
            => !(_board.Pieces(c, pt) & sq).IsEmpty;

        /// <summary>
        /// Determine if a pawn is isolated e.i. no own pawns on either of it's neighboring files
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PawnIsolated(Square sq, Player c)
            => ((sq.PawnAttackSpan(c) | sq.PawnAttackSpan(~c)) & Pieces(PieceTypes.Pawn, c)).IsEmpty;

        /// <summary>
        /// Determine if a specific square is a passed pawn
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PassedPawn(Square sq)
        {
            var pc = _board.PieceAt(sq);

            if (pc.Type() != PieceTypes.Pawn)
                return false;

            var c = pc.ColorOf();

            return (sq.PassedPawnFrontAttackSpan(c) & Pieces(PieceTypes.Pawn, c)).IsEmpty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square sq)
        {
            _board.RemovePiece(sq);
            if (!IsProbing)
                PieceUpdated?.Invoke(Enums.Pieces.NoPiece, sq);
        }

        public BitBoard AttacksTo(Square sq, BitBoard occupied)
        {
            // TODO : needs testing

            Debug.Assert(sq >= Enums.Squares.a1 && sq <= Enums.Squares.h8);

            return (sq.PawnAttack(Player.White) & _board.Pieces(Player.Black, PieceTypes.Pawn))
                  | (sq.PawnAttack(Player.Black) & _board.Pieces(Player.White, PieceTypes.Pawn))
                  | (GetAttacks(sq, PieceTypes.Knight) & _board.Pieces(PieceTypes.Knight))
                  | (GetAttacks(sq, PieceTypes.Rook, occupied) & _board.Pieces(PieceTypes.Rook, PieceTypes.Queen))
                  | (GetAttacks(sq, PieceTypes.Bishop, occupied) & _board.Pieces(PieceTypes.Bishop, PieceTypes.Queen))
                  | (GetAttacks(sq, PieceTypes.King) & _board.Pieces(PieceTypes.King));
        }

        public BitBoard AttacksTo(Square sq) => AttacksTo(sq, _board.Pieces());

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
            => !(Pieces(PieceTypes.Knight, c) & GetAttacks(sq, PieceTypes.Knight)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square sq, Player c) =>
            !(Pieces(PieceTypes.Pawn, c) & sq.PawnAttack(~c)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square sq, Player c)
            => !(GetAttacks(sq, PieceTypes.King) & GetPieceSquare(PieceTypes.King, c)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(CastlelingRights cr)
            => State.CastlelingRights.HasFlagFast(cr);

        public bool CanCastle(Player color)
        {
            var c = (CastlelingRights) ((int) (CastlelingRights.WhiteOo | CastlelingRights.WhiteOoo) << (2 * color.Side));
            return State.CastlelingRights.HasFlagFast(c);
        }

        public bool CastlingImpeded(CastlelingRights cr)
        {
            var v = cr.AsInt();
            return (Pieces() & _castlingPath[v]) != 0;
        }

        public Square CastlingRookSquare(CastlelingRights cr)
            => _castlingRookSquare[cr.AsInt()];

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
            var us = _sideToMove;
            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var pc = MovedPiece(m);

            // Use a slower but simpler function for uncommon cases
            if (m.GetMoveType() != MoveTypes.Normal)
                return this.GenerateMoves().Contains(m);

            // Is not a promotion, so promotion piece must be empty
            if (m.GetPromotedPieceType() - 2 != PieceTypes.NoPieceType)
                return false;

            // If the from square is not occupied by a piece belonging to the side to
            // move, the move is obviously not legal.
            if (pc == Enums.Pieces.NoPiece || pc.ColorOf() != us)
                return false;

            // The destination square cannot be occupied by a friendly piece
            if (!(Pieces(us) & to).IsEmpty)
                return false;

            // Handle the special case of a pawn move
            if (pc.Type() == PieceTypes.Pawn)
            {
                // We have already handled promotion moves, so destination
                // cannot be on the 8th/1st rank.
                if (to.Rank() == Ranks.Rank8.RelativeRank(us))
                    return false;

                if ((from.PawnAttack(us) & Pieces(~us) & to).IsEmpty // Not a capture

                    && !((from + us.PawnPushDistance() == to) && !IsOccupied(to))       // Not a single push

                    && !((from + us.PawnDoublePushDistance() == to)              // Not a double push
                         && (from.Rank() == Ranks.Rank2.RelativeRank(us))
                         && !IsOccupied(to)
                         && !IsOccupied(to - us.PawnPushDistance())))
                    return false;
            }
            else
                if ((GetAttacks(from, pc.Type()) & to).IsEmpty)
                    return false;

            // Evasions generator already takes care to avoid some kind of illegal moves
            // and legal() relies on this. We therefore have to take care that the same
            // kind of moves are filtered out here.
            if (!Checkers.IsEmpty)
            {
                if (pc.Type() != PieceTypes.King)
                {
                    // Double check? In this case a king move is required
                    if (Checkers.MoreThanOne())
                        return false;

                    // Our move must be a blocking evasion or a capture of the checking piece
                    if (((Checkers.Lsb().BitboardBetween(GetPieceSquare(PieceTypes.King, us)) | Checkers) & to).IsEmpty)
                        return false;
                }
                // In case of king moves under check we have to remove king so to catch
                // as invalid moves like b1a1 when opposite queen is on c1.
                else if (!(AttacksTo(to, Pieces() ^ from) & Pieces(~us)).IsEmpty)
                    return false;
            }
            
            return true;
        }

        public bool IsLegal(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            var us = _sideToMove;
            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var ksq = GetPieceSquare(PieceTypes.King, us);

            var movedPiece = MovedPiece(m);

            if (movedPiece.ColorOf() != us)
            {
                movedPiece = Enums.Pieces.BlackBishop;
            }
            
            Debug.Assert(movedPiece.ColorOf() == us);
            Debug.Assert(GetPiece(GetPieceSquare(PieceTypes.King, us)) == PieceTypes.King.MakePiece(us));

            // En passant captures are a tricky special case. Because they are rather
            // uncommon, we do it simply by testing whether the king is attacked after
            // the move is made.
            if (m.IsEnPassantMove())
            {
                var capsq = to - us.PawnPushDistance();
                var occ = (Pieces() ^ from ^ capsq) | to;

                Debug.Assert(to == EnPassantSquare);
                Debug.Assert(MovedPiece(m) == PieceTypes.Pawn.MakePiece(us));
                Debug.Assert(GetPiece(capsq) == PieceTypes.Pawn.MakePiece(~us));
                Debug.Assert(GetPiece(to) == Enums.Pieces.NoPiece);

                return (GetAttacks(ksq, PieceTypes.Rook, occ) & Pieces(PieceTypes.Rook, PieceTypes.Queen, ~us)).IsEmpty
                    && (GetAttacks(ksq, PieceTypes.Bishop, occ) & Pieces(PieceTypes.Bishop, PieceTypes.Queen, ~us)).IsEmpty;
            }

            // If the moving piece is a king, check whether the destination
            // square is attacked by the opponent. Castling moves are checked
            // for legality during move generation.
            if (MovedPiece(m).Type() == PieceTypes.King)
                return m.IsCastlelingMove() || (AttacksTo(to) & Pieces(~us)).IsEmpty;

            var pinned = PinnedPieces(us);
            
            // A non-king move is legal if and only if it is not pinned or it
            // is moving along the ray towards or away from the king.

            return pinned.IsEmpty || (pinned & from).IsEmpty || from.Aligned(to, ksq);
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
                    var piece = _board.PieceAt(square);

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
                        ? CastlingRookSquare(CastlelingRights.WhiteOo).FileChar()
                        : 'K';
                    sb.Append(castlelingChar);
                }

                if (castleRights.HasFlagFast(CastlelingRights.WhiteOoo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.WhiteOoo).FileChar()
                        : 'Q';
                    sb.Append(castlelingChar);
                }

                if (castleRights.HasFlagFast(CastlelingRights.BlackOo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.BlackOo).FileChar()
                        : 'k';
                    sb.Append(castlelingChar);
                }

                if (castleRights.HasFlagFast(CastlelingRights.BlackOoo))
                {
                    castlelingChar = Chess960
                        ? CastlingRookSquare(CastlelingRights.BlackOoo).FileChar()
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
            sb.Append(1 + (_ply - (_sideToMove.IsBlack.AsByte()) / 2));
            
            return new FenData(sb.ToString());
        }

        /// <summary>
        /// Apply a FEN string board setup to the board structure.
        /// </summary>
        /// <param name="fen">The fen data to set</param>
        /// <param name="validate">If true, the fen string is validated, otherwise not</param>
        /// <returns>
        /// 0 = all ok.
        /// -1 = Error in piece file layout parsing
        /// -2 = Error in piece rank layout parsing
        /// -3 = Unknown piece detected
        /// -4 = Error while parsing moving side
        /// -5 = Error while parsing castleling
        /// -6 = Error while parsing en-passant square
        /// -9 = FEN length exceeding maximum
        /// </returns>
        public FenError SetFen(FenData fen, bool validate = false)
        {
            if (validate)
                Fen.Fen.Validate(fen.Fen.ToString());

            Clear();
            StateAdd(new State());

            var chunk = fen.Chunk();

            if (chunk.IsEmpty)
                return new FenError();

            var f = 1; // file (column)
            var r = 8; // rank (row)

            foreach (var c in chunk)
            {
                if (char.IsNumber(c))
                {
                    f += c - '0';
                    if (f > 9)
                        return new FenError(-1, fen.Index);
                }
                else if (c == '/')
                {
                    if (f != 9)
                        return new FenError(-2, fen.Index);

                    r--;
                    f = 1;
                }
                else
                {
                    var pieceIndex = PieceExtensions.PieceChars.IndexOf(c);

                    if (pieceIndex == -1)
                        return new FenError(-3, fen.Index);

                    Player player = char.IsLower(PieceExtensions.PieceChars[pieceIndex]);

                    var square = new Square(r - 1, f - 1);

                    var pc = ((PieceTypes) pieceIndex).MakePiece(player);
                    AddPiece(pc, square);

                    f++;
                }
            }

            // player
            chunk = fen.Chunk();

            if (chunk.IsEmpty || chunk.Length != 1)
                return new FenError(-3, fen.Index);

            _sideToMove = (chunk[0] != 'w').ToInt();

            // castleling
            chunk = fen.Chunk();

            if (chunk.IsEmpty)
                return new FenError(-5, fen.Index);

            SetupCastleling(chunk);

            // en-passant
            chunk = fen.Chunk();

            State.EnPassantSquare = chunk.Length == 1 || chunk[0] == '-' || !chunk[0].InBetween('a', 'h')
                ? Square.None
                : chunk[1].InBetween('3', '6')
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

            // PositionStart = moveNum;

            SetState(halfMoveNum);

            return 0;
        }

        private void SetState(int halfMoveNum)
        {
            var key = State.Key;
            var pawnKey = Zobrist.ZobristNoPawn;

            State.Checkers = AttacksTo(GetPieceSquare(PieceTypes.King, _sideToMove)) & Pieces(~_sideToMove);
            SetCheckInfo(State);

            // compute hash keys
            for (var b = Pieces(); !b.IsEmpty;)
            {
                var sq = BitBoards.PopLsb(ref b);
                var pc = GetPiece(sq);
                var pt = pc.Type();
                
                key ^= pc.GetZobristPst(sq);
                
                if (pt == PieceTypes.Pawn)
                    pawnKey ^= pc.GetZobristPst(sq);
                else if (pt != PieceTypes.King)
                {
                    // something
                }
            }

            if (State.EnPassantSquare != Square.None)
                key ^= State.EnPassantSquare.File().GetZobristEnPessant();

            if (_sideToMove.IsBlack)
                key ^= Zobrist.GetZobristSide();

            key ^= State.CastlelingRights.GetZobristCastleling();

            State.PliesFromNull = halfMoveNum;
            State.InCheck = !State.Checkers.IsEmpty;
            State.Key = key;
            State.PawnStructureKey = pawnKey;
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
        private void SetCastlingRight(Player stm, Square rookFrom)
        {
            var ksq = GetPieceSquare(PieceTypes.King, stm);
            var cs = ksq < rookFrom ? CastlelingSides.King : CastlelingSides.Queen;
            var cr = OrCastlingRight(stm, cs);

            State.CastlelingRights |= cr;
            _castlingRightsMask[ksq.AsInt()] |= cr;
            _castlingRightsMask[rookFrom.AsInt()] |= cr;
            _castlingRookSquare[cr.AsInt()] = rookFrom;

            var kingTo = (cs == CastlelingSides.King ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(stm);
            var rookTo = (cs == CastlelingSides.King ? Enums.Squares.f1 : Enums.Squares.d1).RelativeSquare(stm);

            var maxSquare = rookFrom.Max(rookTo);
            for (var s = rookFrom.Min(rookTo); s <= maxSquare; ++s)
                if (s != ksq && s != rookFrom)
                    _castlingPath[cr.AsInt()] |= s;

            maxSquare = ksq.Max(kingTo);
            for (var s = ksq.Min(kingTo); s <= maxSquare; ++s)
                if (s != ksq && s != rookFrom)
                    _castlingPath[cr.AsInt()] |= s;
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
                output.Append(char.ToLower(m.GetPromotedPieceType().GetPieceChar()));
        }

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
            _board.ClearPiece(doCastleling ? from : to);
            _board.ClearPiece(doCastleling ? rookFrom : rookTo);
            AddPiece(PieceTypes.King.MakePiece(us), doCastleling ? to : from);
            AddPiece(PieceTypes.Rook.MakePiece(us), doCastleling ? rookTo : rookFrom);
            
            // finally update state in case the move was undone
            // if (!doCastleling)
            //     SetCastlingRight(stm, rfrom);
        }

        private static CastlelingRights OrCastlingRight(Player c, CastlelingSides s)
            => (CastlelingRights) ((int) CastlelingRights.WhiteOo << (s == CastlelingSides.Queen ? 1 : 0) + 2 * c.Side);

        private void StateAdd(State currentState)
        {
            var newState = new State(currentState);
            State = newState;
            _stateStack.Add(newState);
        }

        private void SetupCastleling(ReadOnlySpan<char> castleling)
        {
            foreach (var ca in castleling)
            {
                Square rsq;
                Player c = char.IsLower(ca) ? 1 : 0;
                var token = char.ToUpper(ca);

                if (token == 'K')
                    for (rsq = Enums.Squares.h1.RelativeSquare(c); GetPieceType(rsq) != PieceTypes.Rook; --rsq)
                    { }
                else if (token == 'Q')
                    for (rsq = Enums.Squares.a1.RelativeSquare(c); GetPieceType(rsq) != PieceTypes.Rook; --rsq)
                    { }
                else if (token.InBetween('A', 'H'))
                    rsq = new Square(Ranks.Rank1.RelativeRank(c), new File(token - 'A'));
                else
                    continue;

                SetCastlingRight(c, rsq);
            }
        }

        public BitBoard GetAttacks(Square square, PieceTypes pt, BitBoard occupied)
        {
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

        private (BitBoard, BitBoard) SliderBlockers(BitBoard sliders, Square s)
        {
            var (blockers, pinners) = (BitBoard.Empty, BitBoard.Empty);

            try
            {
                // Snipers are sliders that attack 's' when a piece and other snipers are removed
                var snipers = (PieceTypes.Rook.PseudoAttacks(s) & Pieces(PieceTypes.Queen, PieceTypes.Rook))
                              | (PieceTypes.Bishop.PseudoAttacks(s) & Pieces(PieceTypes.Queen, PieceTypes.Bishop)) & sliders;
                var occupancy = Pieces() ^ snipers;

                var pc = GetPiece(s);
                var uniColorPieces = Pieces(pc.ColorOf());

                while (snipers)
                {
                    var sniperSq = BitBoards.PopLsb(ref snipers);
                    var b = s.BitboardBetween(sniperSq) & occupancy;

                    if (b.IsEmpty || b.MoreThanOne())
                        continue;
                
                    blockers |= b;
                    
                    if (b & uniColorPieces)
                        pinners |= sniperSq;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return (blockers, pinners);
        }

        private void SetCheckInfo(State state)
        {
            // var white = Player.White;
            // var black = Player.Black;

            var wksq = GetPieceSquare(PieceTypes.King, Player.White);
            var bksq = GetPieceSquare(PieceTypes.King, Player.Black);

            Debug.Assert(wksq.IsOk());
            Debug.Assert(bksq.IsOk());

            (state.BlockersForKing[Player.White.Side], state.Pinners[Player.Black.Side]) = SliderBlockers(Pieces(Player.Black), wksq);
            (state.BlockersForKing[Player.Black.Side], state.Pinners[Player.White.Side]) = SliderBlockers(Pieces(Player.White), bksq);
            
            var ksq = GetPieceSquare(PieceTypes.King, ~_sideToMove);

            state.CheckedSquares[PieceTypes.Pawn.AsInt()] = ksq.PawnAttack(~_sideToMove);
            state.CheckedSquares[PieceTypes.Knight.AsInt()] = GetAttacks(ksq, PieceTypes.Knight);
            state.CheckedSquares[PieceTypes.Bishop.AsInt()] = GetAttacks(ksq, PieceTypes.Bishop);
            state.CheckedSquares[PieceTypes.Rook.AsInt()] = GetAttacks(ksq, PieceTypes.Rook);
            state.CheckedSquares[PieceTypes.Queen.AsInt()] = state.CheckedSquares[PieceTypes.Bishop.AsInt()] | state.CheckedSquares[PieceTypes.Rook.AsInt()];
            state.CheckedSquares[PieceTypes.King.AsInt()] = BitBoard.Empty;
        }

        public IEnumerator<Piece> GetEnumerator() => _board.GetEnumerator();// BoardLayout.Cast<Piece>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}