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
    using Microsoft.Extensions.ObjectPool;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Types;
    using Validation;

    /// <summary>
    /// The main board representation class. It stores all the information about the current board
    /// in a simple structure. It also serves the purpose of being able to give the UI controller
    /// feedback on various things on the board
    /// </summary>
    public sealed class Position : IPosition
    {
        private readonly ObjectPool<StringBuilder> _outputObjectPool;
        private readonly CastlelingRights[] _castlingRightsMask;
        private readonly Square[] _castlingRookSquare;
        private readonly BitBoard[] _castlingPath;
        private Player _sideToMove;
        private int _ply;
        private State _state;

#region cuckoo
        // Marcel van Kervinck's cuckoo algorithm for fast detection of "upcoming repetition"
        // situations. https://marcelk.net/2013-04-06/paper/upcoming-rep-v2.pdf
        private static readonly HashKey[] Cuckoo;
        private static readonly Move[] CuckooMove;

        private static readonly Func<HashKey, int> CuckooHashOne = key => (int) (key.Key & 0x1FFF);
        private static readonly Func<HashKey, int> CuckooHashTwo = key => (int) ((key.Key >> 16) & 0x1FFF);
#endregion cuckoo

        private readonly IBoard _board;
        private readonly IPositionValidator _positionValidator;

        static Position()
        {
            // initialize cuckoo tables
            Cuckoo = new HashKey[8192];
            CuckooMove = new Move[8192];

            Span<Piece> pieces = stackalloc Piece[]
            {
                Enums.Pieces.WhitePawn, Enums.Pieces.WhiteKnight, Enums.Pieces.WhiteBishop, Enums.Pieces.WhiteRook, Enums.Pieces.WhiteQueen, Enums.Pieces.WhiteKing,
                Enums.Pieces.BlackPawn, Enums.Pieces.BlackKnight, Enums.Pieces.BlackBishop, Enums.Pieces.BlackRook, Enums.Pieces.BlackQueen, Enums.Pieces.BlackKing
            };

            var count = 0;
            foreach (var pc in pieces)
            {
                foreach (var sq1 in BitBoards.AllSquares)
                {
                    foreach (var sq2 in BitBoards.AllSquares)
                    {
                        if ((pc.Type().PseudoAttacks(sq1) & sq2).IsEmpty)
                            continue;

                        var move = Move.MakeMove(sq1, sq2);
                        var key = pc.GetZobristPst(sq1) ^ pc.GetZobristPst(sq2) ^ Zobrist.GetZobristSide();
                        var i = CuckooHashOne(key);
                        while (true)
                        {
                            (Cuckoo[i], key) = (key, Cuckoo[i].Key);
                            (CuckooMove[i], move) = (move, CuckooMove[i]);

                            // check for empty slot
                            if (move.IsNullMove())
                                break;

                            // Push victim to alternative slot
                            i = i == CuckooHashOne(key) ? CuckooHashTwo(key) : CuckooHashOne(key);
                        }
                        count++;
                    }
                }
            }

            Debug.Assert(count == 3668);
        }

        public Position(IBoard board)
        {
            _board = board;
            _outputObjectPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
            _positionValidator = new PositionValidator(this, _board);
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

        public bool Chess960 { get; set; }

        public Player SideToMove
            => _sideToMove;

        public Square EnPassantSquare
            => _state.EnPassantSquare;

        public string FenNotation
            => GenerateFen().ToString();

        public IBoard Board
            => _board;

        public BitBoard Checkers
            => _state.Checkers;

        public int Rule50 => _state.Rule50;

        public int Ply => _ply;

        public bool InCheck => !_state.Checkers.IsEmpty;

        public bool IsRepetition => _state.Repetition >= 3;

        public State State => _state;

        public bool IsMate => !this.GenerateMoves().Any();

        public void Clear()
        {
            _board.Clear();
            _castlingPath.Fill(BitBoard.Empty);
            _castlingRightsMask.Fill(CastlelingRights.None);
            _castlingRookSquare.Fill(Square.None);
            _sideToMove = Players.White;
            Chess960 = false;
            if (_state == null)
                _state = new State();
            else
                _state.Clear();
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
            _state = _state.CopyTo(newState);
            _state.LastMove = m;

            //Debug.Assert(!_state.Equals(newState));

            var k = _state.Key ^ Zobrist.GetZobristSide();

            _ply++;
            _state.Rule50++;
            _state.PliesFromNull++;

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
                        Debug.Assert(to == _state.EnPassantSquare);
                        Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                        Debug.Assert(!IsOccupied(to));
                        Debug.Assert(GetPiece(captureSquare) == pt.MakePiece(them));
                    }

                    _state.PawnStructureKey ^= capturedPiece.GetZobristPst(captureSquare);
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
                _state.Rule50 = 0;
            }

            // update key with moved piece
            k ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

            // reset en-passant square if it is set
            if (_state.EnPassantSquare != Square.None)
            {
                k ^= _state.EnPassantSquare.File.GetZobristEnPessant();
                _state.EnPassantSquare = Square.None;
            }

            // Update castling rights if needed
            if (_state.CastlelingRights != CastlelingRights.None && (_castlingRightsMask[from.AsInt()] | _castlingRightsMask[to.AsInt()]) != 0)
            {
                var cr = _castlingRightsMask[from.AsInt()] | _castlingRightsMask[to.AsInt()];
                k ^= (_state.CastlelingRights & cr).GetZobristCastleling();
                _state.CastlelingRights &= ~cr;
            }

            // Move the piece. The tricky Chess960 castle is handled earlier
            if (!m.IsCastlelingMove())
                MovePiece(from, to);

            // If the moving piece is a pawn do some special extra work
            if (isPawn)
            {
                // Set en-passant square, only if moved pawn can be captured
                if (((int)to.Value ^ (int)from.Value) == 16
                    && !((to - us.PawnPushDistance()).PawnAttack(us) & Pieces(PieceTypes.Pawn, them)).IsEmpty)
                {
                    _state.EnPassantSquare = to - us.PawnPushDistance();
                    k ^= _state.EnPassantSquare.File.GetZobristEnPessant();
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
                    _state.PawnStructureKey ^= pc.GetZobristPst(to);

                    // TODO : Update values and other keys here
                }

                // Update pawn hash key
                _state.PawnStructureKey ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

                // Reset rule 50 draw counter
                _state.Rule50 = 0;
            }

            // TODO : Update piece values here

            Debug.Assert(GetKingSquare(us).IsOk);
            Debug.Assert(GetKingSquare(them).IsOk);

            // Update state properties
            _state.Key = k;
            _state.CapturedPiece = capturedPiece;

            var ksq = GetKingSquare(them);

            _state.Checkers = givesCheck ? AttacksTo(ksq) & _board.Pieces(us) : BitBoard.Empty;

            _sideToMove = ~_sideToMove;

            SetCheckInfo(_state);
            _state.UpdateRepetition();

            //Debug.Assert(_positionValidator.Validate().IsOk);
        }

        public void MakeNullMove(State newState)
        {
            Debug.Assert(!InCheck);

            _state = _state.CopyTo(newState);

            if (_state.EnPassantSquare != Square.None)
            {
                var enPassantFile = _state.EnPassantSquare.File;
                _state.Key ^= enPassantFile.GetZobristEnPessant();
                _state.EnPassantSquare = Square.None;
            }

            _state.Key.HashSide();

            ++_state.Rule50;
            _state.PliesFromNull = 0;

            _sideToMove = ~_sideToMove;
            
            SetCheckInfo(_state);

            _state.Repetition = 0;
            
            Debug.Assert(_positionValidator.Validate(PositionValidationTypes.Basic).IsOk);
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
            Debug.Assert(_state.CapturedPiece.Type() != PieceTypes.King);

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

                if (_state.CapturedPiece != Piece.EmptyPiece)
                {
                    var captureSquare = to;

                    // En-Passant capture is not located on move square
                    if (m.IsEnPassantMove())
                    {
                        captureSquare -= us.PawnPushDistance();

                        Debug.Assert(pc.Type() == PieceTypes.Pawn);
                        Debug.Assert(to == _state.Previous.EnPassantSquare);
                        Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                        Debug.Assert(!IsOccupied(captureSquare));
                    }

                    AddPiece(_state.CapturedPiece, captureSquare);
                }
            }

            Debug.Assert(GetKingSquare(~us).IsOk);
            Debug.Assert(GetKingSquare(us).IsOk);

            // Set state to previous state
            _state = _state.Previous;
            _ply--;

            //Debug.Assert(_positionValidator.Validate().IsOk);
        }

        public void TakeNullMove()
        {
            Debug.Assert(!InCheck);
            _state = _state.Previous;
            _sideToMove = ~_sideToMove;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square sq) => _board.PieceAt(sq);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PieceTypes GetPieceType(Square sq) => _board.PieceAt(sq).Type();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPieceTypeOnSquare(Square sq, PieceTypes pt) => _board.PieceAt(sq).Type() == pt;

        /// <summary>
        /// Detects any pinned pieces For more info : https://en.wikipedia.org/wiki/Pin_(chess)
        /// </summary>
        /// <param name="sq">The square</param>
        /// <param name="c">The side</param>
        /// <returns>Pinned pieces as BitBoard</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPinnedPieces(Square sq, Player c)
        {
            var pinnedPieces = BitBoard.Empty;
            var them = ~c;

            var opponentQueens = _board.Pieces(them, PieceTypes.Queen);
            var ourPieces = _board.Pieces(c);
            var pieces = _board.Pieces();

            var pinners
                = sq.XrayBishopAttacks(pieces, ourPieces) & (_board.Pieces(them, PieceTypes.Bishop) | opponentQueens)
                  | sq.XrayRookAttacks(pieces, ourPieces) & (_board.Pieces(them, PieceTypes.Rook) | opponentQueens);

            while (pinners)
                pinnedPieces |= BitBoards.PopLsb(ref pinners).BitboardBetween(sq) & ourPieces;

            return pinnedPieces;
        }

        public BitBoard CheckedSquares(PieceTypes pt)
            => _state.CheckedSquares[pt.AsInt()];

        public BitBoard PinnedPieces(Player c)
            => _state.Pinners[c.Side];

        public BitBoard BlockersForKing(Player c)
            => _state.BlockersForKing[c.Side];

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

            var pc = _board.PieceAt(from);
            var pt = pc.Type();

            // Is there a direct check?
            if (!(_state.CheckedSquares[pt.AsInt()] & to).IsEmpty)
                return true;

            var us = _sideToMove;
            var them = ~us;

            // Is there a discovered check?
            if (!(_state.BlockersForKing[them.Side] & from).IsEmpty
                   && !from.Aligned(to, GetKingSquare(them)))
                return true;

            switch (m.GetMoveType())
            {
                case MoveTypes.Normal:
                    return false;

                case MoveTypes.Promotion:
                    return !(GetAttacks(to, m.GetPromotedPieceType(), _board.Pieces() ^ from) & GetKingSquare(them)).IsEmpty;

                // En passant capture with check? We have already handled the case of direct checks
                // and ordinary discovered check, so the only case we need to handle is the unusual
                // case of a discovered check through the captured pawn.
                case MoveTypes.Enpassant:
                    {
                        var captureSquare = new Square(from.Rank, to.File);
                        var b = (_board.Pieces() ^ from ^ captureSquare) | to;
                        var ksq = GetKingSquare(them);

                        var attacks = (GetAttacks(ksq, PieceTypes.Rook, b) & _board.Pieces(us, PieceTypes.Rook, PieceTypes.Queen))
                            | (GetAttacks(ksq, PieceTypes.Bishop, b) & _board.Pieces(us, PieceTypes.Bishop, PieceTypes.Queen));
                        return !attacks.IsEmpty;
                    }
                case MoveTypes.Castling:
                    {
                        var kingFrom = from;
                        var rookFrom = to; // Castling is encoded as 'King captures the rook'
                        var kingTo = (rookFrom > kingFrom ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(us);
                        var rookTo = (rookFrom > kingFrom ? Enums.Squares.f1 : Enums.Squares.d1).RelativeSquare(us);
                        var ksq = GetKingSquare(them);

                        return !(PieceTypes.Rook.PseudoAttacks(rookTo) & ksq).IsEmpty && !(GetAttacks(rookTo, PieceTypes.Rook, _board.Pieces() ^ kingFrom ^ rookFrom | rookTo | kingTo) & ksq).IsEmpty;
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

        public Square GetKingSquare(Player color)
            => _board.Square(PieceTypes.King, color);

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
            => ((sq.PawnAttackSpan(c) | sq.PawnAttackSpan(~c)) & _board.Pieces(c, PieceTypes.Pawn)).IsEmpty;

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

            return (sq.PassedPawnFrontAttackSpan(c) & _board.Pieces(c, PieceTypes.Pawn)).IsEmpty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square sq)
        {
            _board.RemovePiece(sq);
            if (!IsProbing)
                PieceUpdated?.Invoke(Piece.EmptyPiece, sq);
        }

        public BitBoard AttacksTo(Square sq, BitBoard occupied)
        {
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
            var occupied = _board.Pieces();
            var rookAttacks = sq.RookAttacks(occupied);
            if (_board.Pieces(c, PieceTypes.Rook) & rookAttacks)
                return true;

            var bishopAttacks = sq.BishopAttacks(occupied);
            if (_board.Pieces(c, PieceTypes.Bishop) & bishopAttacks)
                return true;

            return (_board.Pieces(c, PieceTypes.Queen) & (bishopAttacks | rookAttacks)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKnight(Square sq, Player c)
            => !(_board.Pieces(c, PieceTypes.Knight) & GetAttacks(sq, PieceTypes.Knight)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square sq, Player c) =>
            !(_board.Pieces(c, PieceTypes.Pawn) & sq.PawnAttack(~c)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square sq, Player c)
            => !(GetAttacks(sq, PieceTypes.King) & GetKingSquare(c)).IsEmpty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanCastle(CastlelingRights cr)
            => _state.CastlelingRights.HasFlagFast(cr);

        public bool CanCastle(Player color)
        {
            var c = (CastlelingRights)((int)(CastlelingRights.WhiteOo | CastlelingRights.WhiteOoo) << (2 * color.Side));
            return _state.CastlelingRights.HasFlagFast(c);
        }

        public bool CastlingImpeded(CastlelingRights cr)
        {
            Debug.Assert(cr == CastlelingRights.WhiteOo || cr == CastlelingRights.WhiteOoo || cr == CastlelingRights.BlackOo || cr == CastlelingRights.BlackOoo);
            return !(_board.Pieces() & _castlingPath[cr.AsInt()]).IsEmpty;
        }

        public Square CastlingRookSquare(CastlelingRights cr)
        {
            Debug.Assert(cr == CastlelingRights.WhiteOo || cr == CastlelingRights.WhiteOoo || cr == CastlelingRights.BlackOo || cr == CastlelingRights.BlackOoo);
            return _castlingRookSquare[cr.AsInt()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CastlelingRights GetCastlelingRightsMask(Square sq)
            => _castlingRightsMask[sq.AsInt()];

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
            if (m.GetMoveType() != MoveTypes.Normal)
                return this.GenerateMoves().Contains(m);

            // Is not a promotion, so promotion piece must be empty
            if (m.GetPromotedPieceType() - 2 != PieceTypes.NoPieceType)
                return false;

            var us = _sideToMove;
            var pc = MovedPiece(m);
            var from = m.GetFromSquare();
            var to = m.GetToSquare();

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

                    && !((from + us.PawnPushDistance() == to) && !IsOccupied(to))       // Not a single push

                    && !((from + us.PawnDoublePushDistance() == to)              // Not a double push
                         && (from.Rank == Ranks.Rank2.RelativeRank(us))
                         && !IsOccupied(to)
                         && !IsOccupied(to - us.PawnPushDistance())))
                    return false;
            }
            else
                if ((GetAttacks(from, pc.Type()) & to).IsEmpty)
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
            // In case of king moves under check we have to remove king so to catch as invalid
            // moves like b1a1 when opposite queen is on c1.
            else if (!(AttacksTo(to, Pieces() ^ @from) & Pieces(~us)).IsEmpty)
                return false;

            return true;
        }

        public bool IsLegal(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            var us = _sideToMove;
            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var ksq = GetKingSquare(us);

            // Debug.Assert(movedPiece.ColorOf() == us);
            Debug.Assert(GetPiece(GetKingSquare(us)) == PieceTypes.King.MakePiece(us));

            // En passant captures are a tricky special case. Because they are rather uncommon, we
            // do it simply by testing whether the king is attacked after the move is made.
            if (m.IsEnPassantMove())
            {
                var captureSquare = to - us.PawnPushDistance();
                var occupied = (_board.Pieces() ^ from ^ captureSquare) | to;

                Debug.Assert(to == EnPassantSquare);
                Debug.Assert(MovedPiece(m) == PieceTypes.Pawn.MakePiece(us));
                Debug.Assert(GetPiece(captureSquare) == PieceTypes.Pawn.MakePiece(~us));
                Debug.Assert(GetPiece(to) == Piece.EmptyPiece);

                return (GetAttacks(ksq, PieceTypes.Rook, occupied) & _board.Pieces(~us, PieceTypes.Rook, PieceTypes.Queen)).IsEmpty
                    && (GetAttacks(ksq, PieceTypes.Bishop, occupied) & _board.Pieces(~us, PieceTypes.Bishop, PieceTypes.Queen)).IsEmpty;
            }

            // Check for legal castleling move
            if (m.IsCastlelingMove())
            {
                // After castling, the rook and king final positions are the same in Chess960 as
                // they would be in standard chess.

                to = (to > from ? Enums.Squares.g1 : Enums.Squares.c1).RelativeSquare(us);
                var step = to > from ? Directions.West : Directions.East;

                for (var s = to; s != from; s += step)
                    if (AttacksTo(s) & _board.Pieces(~us))
                        return false;

                // In case of Chess960, verify that when moving the castling rook we do not discover
                // some hidden checker. For instance an enemy queen in SQ_A1 when castling rook is
                // in SQ_B1.
                return !Chess960
                         || (GetAttacks(to, PieceTypes.Rook, _board.Pieces() ^ m.GetToSquare()) & _board.Pieces(~us, PieceTypes.Rook, PieceTypes.Queen)).IsEmpty;
            }

            // If the moving piece is a king, check whether the destination square is attacked by
            // the opponent.
            if (MovedPiece(m).Type() == PieceTypes.King)
                return m.IsCastlelingMove() || (AttacksTo(to) & _board.Pieces(~us)).IsEmpty;

            // A non-king move is legal if and only if it is not pinned or it is moving along the
            // ray towards or away from the king.
            return (BlockersForKing(us) & from).IsEmpty
                     || from.Aligned(to, ksq);
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

            var castleRights = _state.CastlelingRights;

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

            if (_state.EnPassantSquare == Square.None)
                sb.Append('-');
            else
                sb.Append(_state.EnPassantSquare.ToString());

            sb.Append(' ');

            sb.Append(_state.Rule50);
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

                    var pc = ((PieceTypes)pieceIndex).MakePiece(player);
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

            _state.EnPassantSquare = chunk.Length == 1 || chunk[0] == '-' || !chunk[0].InBetween('a', 'h')
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

            _state.Rule50 = halfMoveNum;
            _ply = moveNum;

            SetState();

            return 0;
        }

        private void SetState()
        {
            var key = new HashKey();
            var pawnKey = Zobrist.ZobristNoPawn;

            _state.Checkers = AttacksTo(GetKingSquare(_sideToMove)) & _board.Pieces(~_sideToMove);
            SetCheckInfo(_state);

            // compute hash keys
            for (var b = _board.Pieces(); !b.IsEmpty;)
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

            if (_state.EnPassantSquare != Square.None)
                key ^= _state.EnPassantSquare.File.GetZobristEnPessant();

            if (_sideToMove.IsBlack)
                key.HashSide();

            key ^= _state.CastlelingRights.GetZobristCastleling();

            _state.Key = key;
            _state.PawnStructureKey = pawnKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashKey GetPiecesKey()
        {
            var result = new HashKey();
            var pieces = _board.Pieces();
            while (pieces)
            {
                var sq = BitBoards.PopLsb(ref pieces);
                var pc = _board.PieceAt(sq);
                result ^= pc.GetZobristPst(sq);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashKey GetPawnKey()
        {
            var result = Zobrist.ZobristNoPawn;
            var pieces = _board.Pieces(PieceTypes.Pawn);
            while (pieces)
            {
                var sq = BitBoards.PopLsb(ref pieces);
                var pc = GetPiece(sq);
                result ^= pc.GetZobristPst(sq);
            }

            return result;
        }

        /// Position::set_castle_right() is an helper function used to set castling rights given the
        /// corresponding color and the rook starting square.
        private void SetCastlingRight(Player stm, Square rookFrom)
        {
            var ksq = GetKingSquare(stm);
            var cs = ksq < rookFrom ? CastlelingSides.King : CastlelingSides.Queen;
            var cr = OrCastlingRight(stm, cs);

            _state.CastlelingRights |= cr;
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
                to = new Square(from.Rank, file);
            }

            output.Append(from.ToString()).Append(to.ToString());

            if (m.IsPromotionMove())
                output.Append(char.ToLower(m.GetPromotedPieceType().GetPieceChar()));
        }

        public bool HasGameCycle(int ply)
        {
            var end = _state.Rule50 < _state.PliesFromNull ? _state.Rule50 : _state.PliesFromNull;

            if (end < 3)
                return false;

            var originalKey = _state.Key;
            var statePrevious = _state.Previous;

            for (var i = 3; i <= end; i += 2)
            {
                statePrevious = statePrevious.Previous.Previous;
                var moveKey = originalKey ^ statePrevious.Key;

                var j = CuckooHashOne(moveKey);
                var found = Cuckoo[j] == moveKey;

                if (!found)
                {
                    j = CuckooHashTwo(moveKey);
                    found = Cuckoo[j] == moveKey;
                }

                if (!found)
                    continue;

                var move = CuckooMove[j];
                var s1 = move.GetFromSquare();
                var s2 = move.GetToSquare();

                if ((s1.BitboardBetween(s2) & _board.Pieces()).IsEmpty)
                    continue;

                if (ply > i)
                    return true;

                // For nodes before or at the root, check that the move is a
                // repetition rather than a move to the current position.
                // In the cuckoo table, both moves Rc1c5 and Rc5c1 are stored in
                // the same location, so we have to select which square to check.
                if (GetPiece(!IsOccupied(s1) ? s2 : s1).ColorOf() != _sideToMove)
                    continue;

                // For repetitions before or at the root, require one more
                if (_state.Repetition > 0)
                    return true;
            }
            return false;
        }

        public IPositionValidator Validate(PositionValidationTypes type = PositionValidationTypes.Basic)
        {
            return _positionValidator.Validate(type);
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
        }

        private static CastlelingRights OrCastlingRight(Player c, CastlelingSides s)
            => (CastlelingRights)((int)CastlelingRights.WhiteOo << (s == CastlelingSides.Queen ? 1 : 0) + 2 * c.Side);

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

        public override string ToString()
        {
            const string separator = "\n  +---+---+---+---+---+---+---+---+\n";
            const char splitter = '|';
            const char space = ' ';
            var output = _outputObjectPool.Get();
            output.Append(separator);
            for (var rank = Ranks.Rank8; rank >= Ranks.Rank1; rank--)
            {
                output.Append((int)rank + 1);
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
            output.AppendLine($"Zobrist : 0x{_state.Key.Key:X}");
            var result = output.ToString();
            _outputObjectPool.Return(output);
            return result;
        }

        private (BitBoard, BitBoard) SliderBlockers(BitBoard sliders, Square s)
        {
            var result = (blockers: BitBoard.Empty, pinners: BitBoard.Empty);

            try
            {
                // Snipers are sliders that attack 's' when a piece and other snipers are removed
                var snipers = (PieceTypes.Rook.PseudoAttacks(s) & _board.Pieces(PieceTypes.Queen, PieceTypes.Rook)
                              | (PieceTypes.Bishop.PseudoAttacks(s) & _board.Pieces(PieceTypes.Queen, PieceTypes.Bishop))) & sliders;
                var occupancy = _board.Pieces() ^ snipers;

                var pc = GetPiece(s);
                var uniColorPieces = _board.Pieces(pc.ColorOf());

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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return result;
        }

        private void SetCheckInfo(State state)
        {
            (state.BlockersForKing[Player.White.Side], state.Pinners[Player.Black.Side]) = SliderBlockers(_board.Pieces(Player.Black), GetKingSquare(Player.White));
            (state.BlockersForKing[Player.Black.Side], state.Pinners[Player.White.Side]) = SliderBlockers(_board.Pieces(Player.White), GetKingSquare(Player.Black));

            var ksq = GetKingSquare(~_sideToMove);

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