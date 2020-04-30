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
        private static readonly Func<Square, Square>[] EnPasCapturePos = {s => s + Directions.South, s => s + Directions.North};

        private readonly CastlelingRights[] _castlingRightsMask;
        private readonly Square[] _castlingRookSquare;
        private readonly BitBoard[] _castlingPath;
        private readonly List<State> _stateStack;
        private int _ply;
        
        public Position()
        {
            _stateStack = new List<State>(256);
            BoardLayout = new Piece[64];
            _castlingPath = new BitBoard[CastlelingRights.CastleRightsNb.AsInt()];
            _castlingRookSquare = new Square[CastlelingRights.CastleRightsNb.AsInt()];
            _castlingRightsMask = new CastlelingRights[64];
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

        public Player SideToMove { get; private set; }
        
        public Square EnPassantSquare => State.EnPassantSquare;

        public string FenNotation => GenerateFen().ToString();
        
        public void Clear()
        {
            BoardLayout.Fill(Enums.Pieces.NoPiece);
            OccupiedBySide.Fill(BitBoards.EmptyBitBoard);
            BoardPieces.Fill(BitBoards.EmptyBitBoard);
            _castlingPath.Fill(BitBoards.EmptyBitBoard);
            _castlingRightsMask.Fill(CastlelingRights.None);
            //Array.Clear(_castlingRookSquare, 0, _castlingRookSquare.Length);
             _castlingRookSquare.Fill(Squares.none);
            SideToMove = Players.White;
            _ply = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(Piece pc, Square sq)
        {
            var sqBb = sq.BitBoardSquare();
            BoardPieces[PieceTypes.AllPieces.AsInt()] |= sqBb;
            BoardPieces[pc.Type().AsInt()] |= sqBb;
            OccupiedBySide[pc.ColorOf().Side] |= sqBb;
            var sqi = sq.AsInt();
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
            OccupiedBySide[pc.ColorOf().Side] ^= fromTo;
            BoardLayout[from.AsInt()] = Enums.Pieces.NoPiece;
            BoardLayout[to.AsInt()] = pc;

            if (!IsProbing)
                PieceUpdated?.Invoke(pc, to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPiece(PieceTypes pt, Square sq, Player c) => AddPiece(pt.MakePiece(c), sq);

        public void MakeMove(Move m)
        {
            MakeMove(m, GivesCheck(m));
        }
        
        public void MakeMove(Move m, bool givesCheck)
        {
            StateAdd();

            var previousState = State.Previous;
            var k = new HashKey();

            State.CastlelingRights = previousState.CastlelingRights;
            State.PliesFromNull = previousState.PliesFromNull;
            State.Rule50 = previousState.Rule50;
            State.LastMove = m;
            State.Material.CopyFrom(previousState.Material);
            State.PawnStructureKey = previousState.PawnStructureKey;
            
            k ^= Zobrist.GetZobristSide();

            State.Rule50++;
            State.PliesFromNull++;
            _ply++;
            
            var to = m.GetToSquare();
            var from = m.GetFromSquare();
            var us = SideToMove;
            var them = ~us;
            var pc = GetPiece(from);
            var pt = pc.Type();
            var isPawn = pt == PieceTypes.Pawn;
            var capturedPieceType = m.IsEnPassantMove()
                ? PieceTypes.Pawn
                : GetPieceType(to);
            
            var fens = GenerateFen().ToString();
            //Console.WriteLine($"MakeMove FEN:{fens}");
            
            Debug.Assert(us == GetPiece(from).ColorOf());
            Debug.Assert(GetPieceSquare(PieceTypes.King, them).IsOk());
            Debug.Assert(GetPieceSquare(PieceTypes.King, us).IsOk());

            if (m.IsCastlelingMove())
            {
                DoCastleling(from, ref to, out var rookFrom, out var rookTo, CastlelingPerform.Do);

                var rook = PieceTypes.Rook.MakePiece(us);

                k ^= rook.GetZobristPst(rookFrom) ^ rook.GetZobristPst(rookTo);

                // reset captured piece type as castleling is "king-captures-rook"
                capturedPieceType = PieceTypes.NoPieceType;
            }

            if (capturedPieceType != PieceTypes.NoPieceType)
            {
                var captureSquare = to;

                // If the captured piece is a pawn, update pawn hash key, otherwise update non-pawn material.
                if (capturedPieceType == PieceTypes.Pawn)
                {
                    if (m.IsEnPassantMove())
                    {
                        captureSquare += them.PawnPushDistance();

                        Debug.Assert(pt == PieceTypes.Pawn);
                        Debug.Assert(to == State.EnPassantSquare);
                        Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                        Debug.Assert(!IsOccupied(to));
                        Debug.Assert(GetPiece(captureSquare) == pt.MakePiece(them));

                        BoardLayout[captureSquare.AsInt()] = PieceExtensions.EmptyPiece;
                    }

                    State.PawnStructureKey ^= pt.MakePiece(them).GetZobristPst(captureSquare);
                }
                // else
                    // st.npMaterial[them] -= PieceValue[PhaseS.MG][capture];

                // Update board and piece lists
                RemovePiece(captureSquare);

                // Update material hash key and prefetch access to materialTable

                k ^= capturedPieceType.MakePiece(them).GetZobristPst(captureSquare);
                // st.materialKey ^= Zobrist.psq[them][capture][pieceCount[them][capture]];

                // Update incremental scores
                // st.psq -= psq[them][capture][capsq];

                // Reset rule 50 counter
                State.Rule50 = 0;
            }
            
            // update key with moved piece
            k ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

            // reset en-passant square if it is set
            if (State.EnPassantSquare != Squares.none)
            {
                k ^= State.EnPassantSquare.File().GetZobristEnPessant();
                State.EnPassantSquare = Squares.none;
            }

            // Update castling rights if needed
            if (State.CastlelingRights != 0 && (_castlingRightsMask[from.AsInt()] | _castlingRightsMask[to.AsInt()]) != 0)
            {
                var cr = _castlingRightsMask[from.AsInt()] | _castlingRightsMask[to.AsInt()];
                k ^= (State.CastlelingRights & cr).GetZobristCastleling();
                //State.Previous.CastlelingRights = State.CastlelingRights;
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
                    && ((from + us.PawnPushDistance()).PawnAttack(us) & Pieces(PieceTypes.Pawn, them)) != 0)
                {
                    State.EnPassantSquare = (from.AsInt() + to.AsInt()) / 2;
                    k ^= State.EnPassantSquare.File().GetZobristEnPessant();
                }
                else if (m.IsPromotionMove())
                {
                    var promotion = m.GetPromotedPieceType();

                    Debug.Assert(to.RelativeRank(us) == Ranks.Rank8);
                    Debug.Assert(promotion >= PieceTypes.Knight && promotion <= PieceTypes.Queen);

                    RemovePiece(to);
                    AddPiece(PieceTypes.Pawn, to, us);

                    // Update hash keys
                    k ^= pc.GetZobristPst(to) ^ promotion.MakePiece(us).GetZobristPst(to);
                    State.PawnStructureKey ^= pc.GetZobristPst(to);

                    // TODO : Update values and other keys here
                }

                // Update pawn hash key
                State.PawnStructureKey ^= pc.GetZobristPst(from) ^ pc.GetZobristPst(to);

                // Reset rule 50 draw counter
                State.Rule50 = 0;
            }

            // TODO : Update piece values here
            // Potential set of captured piece in state when move is refactored

            State.Key = k;

            Debug.Assert(GetPieceSquare(PieceTypes.King, us).IsOk());

            // flip local players, as the rest of the functionality is towards "them"
            // (us, them) = (them, us);
            
            var ksq = GetPieceSquare(PieceTypes.King, them);

            // Update checkers bitboard
            State.Checkers = givesCheck ? AttacksTo(ksq) & Pieces(us) : BitBoards.EmptyBitBoard;
            State.InCheck = !State.Checkers.Empty;
            State.CapturedPiece = capturedPieceType;

            SideToMove = ~SideToMove;
            
            SetCheckInfo(State);
        }

        public void TakeMove(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            // flip sides
            SideToMove = ~SideToMove;
            var us = SideToMove;

            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var pt = GetPiece(to).Type();

            Debug.Assert(!IsOccupied(from) || m.IsCastlelingMove());

            if (m.IsPromotionMove())
            {
                Debug.Assert(pt == m.GetPromotedPieceType());
                Debug.Assert(to.RelativeRank(us) == Ranks.Rank8);
                Debug.Assert(m.GetPromotedPieceType() >= PieceTypes.Knight && m.GetPromotedPieceType() <= PieceTypes.Queen);

                pt = PieceTypes.Pawn;

                RemovePiece(to);
                AddPiece(pt, to, us);
            }

            if (m.IsCastlelingMove())
            {
                DoCastleling(from, ref to, out _, out _, CastlelingPerform.Undo);
            }
            else
            {
                RemovePiece(to);
                AddPiece(pt, from, us);

                if (State.CapturedPiece != PieceTypes.NoPieceType)
                {
                    var captureSquare = to;

                    // En-Passant capture is not located on move square
                    if (m.IsEnPassantMove())
                    {
                        captureSquare -= us.PawnPushDistance();

                        Debug.Assert(pt == PieceTypes.Pawn);
                        Debug.Assert(to == State.Previous.EnPassantSquare);
                        Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                        Debug.Assert(!IsOccupied(captureSquare));
                    }

                    AddPiece(State.CapturedPiece, captureSquare, ~us);
                }
            }

            Debug.Assert(GetPieceSquare(PieceTypes.King, ~us).IsOk());
            Debug.Assert(GetPieceSquare(PieceTypes.King, us).IsOk());

            // Set state to previous state
            State = State.Previous;
            State.Rule50--;
            _ply--;
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
            => State.CheckedSquares[pt.AsInt()];

        public BitBoard Checkers()
            => State.Checkers;

        public BitBoard PinnedPieces(Player c)
            => State.Pinners[c.Side];

        public BitBoard BlockersForKing(Player c)
            => State.BlockersForKing[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOccupied(Square sq)
            => BoardLayout[sq.AsInt()] != Enums.Pieces.NoPiece;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAttacked(Square sq, Player c)
            => AttackedBySlider(sq, c) || AttackedByKnight(sq, c) || AttackedByPawn(sq, c) || AttackedByKing(sq, c);

        public bool GivesCheck(Move m)
        {
            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var pt = GetPieceType(from);
            var us = SideToMove;

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
                    return (GetAttacks(to, m.GetPromotedPieceType(), Pieces() ^ from) & ksq) != 0;

                // En passant capture with check? We have already handled the case of direct checks
                // and ordinary discovered check, so the only case we need to handle is the unusual
                // case of a discovered check through the captured pawn.
                case MoveTypes.Enpassant:
                {
                    var capsq = new Square(from.Rank(), to.File());
                    var b = Pieces();
                    b ^= from;
                    b ^= capsq;
                    b |= to;

                    return ((GetAttacks(ksq, PieceTypes.Rook, b) & Pieces(PieceTypes.Rook, PieceTypes.Queen, us))
                            | (GetAttacks(ksq, PieceTypes.Bishop, b) & Pieces(PieceTypes.Bishop, PieceTypes.Queen, us))) != 0;
                }

                case MoveTypes.Castling:
                {
                    var kfrom = from;
                    var rfrom = to; // Castling is encoded as 'King captures the rook'
                    var kingSide = rfrom > kfrom;
                    var (kto, rto) = kingSide
                        ? (Squares.g1.RelativeSquare(us), Squares.f1.RelativeSquare(us))
                        : (Squares.c1.RelativeSquare(us), Squares.d1.RelativeSquare(us));

                    return !(PieceTypes.Rook.PseudoAttacks(rto) & ksq).Empty
                           && (GetAttacks(rto, PieceTypes.Rook, (Pieces() ^ kfrom ^ rfrom) | rto | kto) & ksq) != 0;
                }

                default:
                {
                    return false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces()
            => BoardPieces[PieceTypes.AllPieces.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Player c)
            => OccupiedBySide[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(Piece pc)
            => BoardPieces[pc.Type().AsInt()] & OccupiedBySide[pc.ColorOf().Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt)
            => BoardPieces[pt.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2)
            => Pieces(pt1) | Pieces(pt2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt, Player side)
            => BoardPieces[pt.AsInt()] & OccupiedBySide[side.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard Pieces(PieceTypes pt1, PieceTypes pt2, Player c)
            => (BoardPieces[pt1.AsInt()] | BoardPieces[pt2.AsInt()]) & OccupiedBySide[c.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetPieceSquare(PieceTypes pt, Player c)
            => Pieces(pt, c).Lsb();

        public Piece MovedPiece(Move m)
            => GetPiece(m.GetFromSquare());
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PieceOnFile(Square sq, Player c, PieceTypes pt)
            => !(BoardPieces[pt.MakePiece(c).Type().AsInt()] & sq).Empty;

        /// <summary>
        /// Determine if a pawn is isolated e.i. no own pawns on either of it's neighboring files
        /// </summary>
        /// <param name="sq"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PawnIsolated(Square sq, Player c)
            => ((sq.PawnAttackSpan(c) | sq.PawnAttackSpan(~c)) & Pieces(PieceTypes.Pawn, c)).Empty;

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

            var c = pc.ColorOf();

            return (sq.PassedPawnFrontAttackSpan(c) & Pieces(PieceTypes.Pawn, c)).Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemovePiece(Square sq)
        {
            var pc = BoardLayout[sq.AsInt()];
            var invertedSq = ~sq;
            BoardPieces[PieceTypes.AllPieces.AsInt()] &= invertedSq;
            BoardPieces[pc.Type().AsInt()] &= invertedSq;
            OccupiedBySide[pc.ColorOf().Side] &= invertedSq;
            BoardLayout[sq.AsInt()] = PieceExtensions.EmptyPiece;
            if (!IsProbing)
                PieceUpdated?.Invoke(Enums.Pieces.NoPiece, sq);
        }

        public BitBoard AttacksTo(Square sq, BitBoard occupied)
        {
            // TODO : needs testing

            Debug.Assert(sq >= Squares.a1 && sq <= Squares.h8);

            return (sq.PawnAttack(PlayerExtensions.White) & OccupiedBySide[PlayerExtensions.Black.Side])
                  | (sq.PawnAttack(PlayerExtensions.Black) & OccupiedBySide[PlayerExtensions.White.Side])
                  | (GetAttacks(sq, PieceTypes.Knight) & Pieces(PieceTypes.Knight))
                  | (GetAttacks(sq, PieceTypes.Rook, occupied) & Pieces(PieceTypes.Rook, PieceTypes.Queen))
                  | (GetAttacks(sq, PieceTypes.Bishop, occupied) & Pieces(PieceTypes.Bishop, PieceTypes.Queen))
                  | (GetAttacks(sq, PieceTypes.King) & Pieces(PieceTypes.King));
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
            => !(Pieces(PieceTypes.Knight, c) & GetAttacks(sq, PieceTypes.Knight)).Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByPawn(Square sq, Player c) =>
            !(Pieces(PieceTypes.Pawn, c) & sq.PawnAttack(~c)).Empty;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AttackedByKing(Square sq, Player c)
            => !(GetAttacks(sq, PieceTypes.King) & GetPieceSquare(PieceTypes.King, c)).Empty;

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
            var us = SideToMove;
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
            if (!(Pieces(us) & to).Empty)
                return false;

            // Handle the special case of a pawn move
            if (pc.Type() == PieceTypes.Pawn)
            {
                // We have already handled promotion moves, so destination
                // cannot be on the 8th/1st rank.
                if (to.Rank() == Ranks.Rank8.RelativeRank(us))
                    return false;

                if ((from.PawnAttack(us) & Pieces(~us) & to).Empty // Not a capture

                    && !((from + us.PawnPushDistance() == to) && !IsOccupied(to))       // Not a single push

                    && !((from + us.PawnDoublePushDistance() == to)              // Not a double push
                         && (from.Rank() == Ranks.Rank2.RelativeRank(us))
                         && !IsOccupied(to)
                         && !IsOccupied(to - us.PawnPushDistance())))
                    return false;
            }
            else
                if ((GetAttacks(from, pc.Type()) & to).Empty)
                    return false;

            // Evasions generator already takes care to avoid some kind of illegal moves
            // and legal() relies on this. We therefore have to take care that the same
            // kind of moves are filtered out here.
            if (!Checkers().Empty)
            {
                if (pc.Type() != PieceTypes.King)
                {
                    // Double check? In this case a king move is required
                    if (Checkers().MoreThanOne())
                        return false;

                    // Our move must be a blocking evasion or a capture of the checking piece
                    if (((Checkers().Lsb().BitboardBetween(GetPieceSquare(PieceTypes.King, us)) | Checkers()) & to).Empty)
                        return false;
                }
                // In case of king moves under check we have to remove king so to catch
                // as invalid moves like b1a1 when opposite queen is on c1.
                else if (!(AttacksTo(to, Pieces() ^ from) & Pieces(~us)).Empty)
                    return false;
            }
            
            return true;
        }

        public bool IsLegal(Move m)
        {
            Debug.Assert(!m.IsNullMove());

            var us = SideToMove;
            var from = m.GetFromSquare();
            var to = m.GetToSquare();
            var ksq = GetPieceSquare(PieceTypes.King, us);

            Debug.Assert(MovedPiece(m).ColorOf() == us);
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

                return (GetAttacks(ksq, PieceTypes.Rook, occ) & Pieces(PieceTypes.Rook, PieceTypes.Queen, ~us)).Empty
                    && (GetAttacks(ksq, PieceTypes.Bishop, occ) & Pieces(PieceTypes.Bishop, PieceTypes.Queen, ~us)).Empty;
            }

            // If the moving piece is a king, check whether the destination
            // square is attacked by the opponent. Castling moves are checked
            // for legality during move generation.
            if (MovedPiece(m).Type() == PieceTypes.King)
                return m.IsCastlelingMove() || (AttacksTo(to) & Pieces(~us)).Empty;

            var pinned = PinnedPieces(us);
            
            // A non-king move is legal if and only if it is not pinned or it
            // is moving along the ray towards or away from the king.

            return pinned.Empty || (pinned & from).Empty || from.Aligned(to, ksq);
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

            sb.Append(SideToMove.IsWhite ? " w " : " b ");

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

            sb.Append(State.PliesFromNull);
            sb.Append(' ');
            sb.Append(State.Rule50 + 1);
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

            // correctly clear all pieces and invoke possible notification(s)
            Clear();
            StateAdd();

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

            SideToMove = (chunk[0] != 'w').ToInt();

            // castleling
            chunk = fen.Chunk();

            if (chunk.IsEmpty)
                return new FenError(-5, fen.Index);

            SetupCastleling(chunk);

            // en-passant
            chunk = fen.Chunk();

            State.EnPassantSquare = chunk.Length == 1 || chunk[0] == '-' || !chunk[0].InBetween('a', 'h')
                ? Squares.none
                : chunk[1].InBetween('3', '6')
                    ? Squares.none
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

            State.Checkers = AttacksTo(GetPieceSquare(PieceTypes.King, SideToMove)) & Pieces(~SideToMove);
            SetCheckInfo(State);

            // compute hash keys
            for (var b = Pieces(); !b.Empty;)
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

            if (State.EnPassantSquare != Squares.none)
                key ^= State.EnPassantSquare.File().GetZobristEnPessant();

            if (SideToMove.IsBlack)
                key ^= Zobrist.GetZobristSide();

            key ^= State.CastlelingRights.GetZobristCastleling();

            State.PliesFromNull = halfMoveNum;
            State.InCheck = !State.Checkers.Empty;
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

            var kingTo = (cs == CastlelingSides.King ? Squares.g1 : Squares.c1).RelativeSquare(stm);
            var rookTo = (cs == CastlelingSides.King ? Squares.f1 : Squares.d1).RelativeSquare(stm);

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

        private void DoCastleling(Square from, ref Square to, out Square rookFrom, out Square rookTo, CastlelingPerform castlelingPerform)
        {
            var kingSide = to > from;
            var stm = SideToMove;
            var doCastleling = castlelingPerform == CastlelingPerform.Do;

            rookFrom = to; // Castling is encoded as "king captures friendly rook"
            rookTo = (kingSide ? Squares.f1 : Squares.d1).RelativeSquare(stm);
            to = (kingSide ? Squares.g1 : Squares.c1).RelativeSquare(stm);

            // Remove both pieces first since squares could overlap in Chess960
            RemovePiece(doCastleling ? from : to);
            RemovePiece(doCastleling ? rookFrom : rookTo);
            BoardLayout[(doCastleling ? from : to).AsInt()] = BoardLayout[(doCastleling ? rookFrom : rookTo).AsInt()] = PieceExtensions.EmptyPiece;
            AddPiece(PieceTypes.King, doCastleling ? to : from, stm);
            AddPiece(PieceTypes.Rook, doCastleling ? rookTo : rookFrom, stm);
            
            // finally update state in case the move was undone
            // if (!doCastleling)
            //     SetCastlingRight(stm, rfrom);
        }

        private static CastlelingRights OrCastlingRight(Player c, CastlelingSides s)
            => (CastlelingRights) ((int) CastlelingRights.WhiteOo << (s == CastlelingSides.Queen ? 1 : 0) + 2 * c.Side);

        private void StateAdd()
        {
            var previous = State;
            State = new State {Previous = previous};
            _stateStack.Add(State);
        }

        private void SetupCastleling(ReadOnlySpan<char> castleling)
        {
            foreach (var ca in castleling)
            {
                Square rsq;
                Player c = char.IsLower(ca) ? 1 : 0;
                var token = char.ToUpper(ca);

                if (token == 'K')
                    for (rsq = Squares.h1.RelativeSquare(c); GetPieceType(rsq) != PieceTypes.Rook; --rsq)
                    { }
                else if (token == 'Q')
                    for (rsq = Squares.a1.RelativeSquare(c); GetPieceType(rsq) != PieceTypes.Rook; --rsq)
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
                _ => BitBoards.EmptyBitBoard
            };
        }

        public BitBoard GetAttacks(Square square, PieceTypes pt)
            => GetAttacks(square, pt, Pieces());

        private (BitBoard, BitBoard) SliderBlockers(BitBoard sliders, Square s)
        {
            var (blockers, pinners) = (BitBoards.EmptyBitBoard, BitBoards.EmptyBitBoard);

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

                    if (b.Empty || b.MoreThanOne())
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
            var white = PlayerExtensions.White;
            var black = PlayerExtensions.Black;
            
            Debug.Assert(GetPieceSquare(PieceTypes.King, white).IsOk());
            Debug.Assert(GetPieceSquare(PieceTypes.King, black).IsOk());
            
            (state.BlockersForKing[white.Side], state.Pinners[black.Side]) = SliderBlockers(Pieces(black), GetPieceSquare(PieceTypes.King, white));
            (state.BlockersForKing[black.Side], state.Pinners[white.Side]) = SliderBlockers(Pieces(white), GetPieceSquare(PieceTypes.King, black));
            
            var ksq = GetPieceSquare(PieceTypes.King, ~SideToMove);

            state.CheckedSquares[PieceTypes.Pawn.AsInt()] = ksq.PawnAttack(~SideToMove);
            state.CheckedSquares[PieceTypes.Knight.AsInt()] = GetAttacks(ksq, PieceTypes.Knight);
            state.CheckedSquares[PieceTypes.Bishop.AsInt()] = GetAttacks(ksq, PieceTypes.Bishop);
            state.CheckedSquares[PieceTypes.Rook.AsInt()] = GetAttacks(ksq, PieceTypes.Rook);
            state.CheckedSquares[PieceTypes.Queen.AsInt()] = state.CheckedSquares[PieceTypes.Bishop.AsInt()] | state.CheckedSquares[PieceTypes.Rook.AsInt()];
            state.CheckedSquares[PieceTypes.King.AsInt()] = BitBoards.EmptyBitBoard;
        }

        public IEnumerator<Piece> GetEnumerator() => BoardLayout.Cast<Piece>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}