/*
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

using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Extensions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using Rudzoft.ChessLib.Validation;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib;

/// <summary>
/// The main board representation class. It stores all the information about the current board
/// in a simple structure. It also serves the purpose of being able to give the UI controller
/// feedback on various things on the board
/// </summary>
public sealed class Position : IPosition
{
    private readonly BitBoard[] _castleKingPath;
    private readonly BitBoard[] _castleRookPath;
    private readonly CastleRight[] _castlingRightsMask;
    private readonly Square[] _castlingRookSquare;
    private readonly Value[] _nonPawnMaterial;
    private readonly ICuckoo _cuckoo;
    private readonly ObjectPool<StringBuilder> _outputObjectPool;
    private readonly ObjectPool<MoveList> _moveListPool;
    private readonly IPositionValidator _positionValidator;
    private Player _sideToMove;

    public Position(
        IBoard board,
        IValues values,
        IZobrist zobrist,
        ICuckoo cuckoo,
        IPositionValidator positionValidator,
        ObjectPool<MoveList> moveListPool)
    {
        _castleKingPath = new BitBoard[CastleRight.Count];
        _castleRookPath = new BitBoard[CastleRight.Count];
        _castlingRightsMask = new CastleRight[Square.Count];
        _castlingRightsMask.Fill(CastleRight.None);
        _castlingRookSquare = new Square[CastleRight.Count];
        _nonPawnMaterial = [Value.ValueZero, Value.ValueZero];
        _outputObjectPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
        _moveListPool = moveListPool;
        _positionValidator = positionValidator;
        Board = board;
        IsProbing = true;
        Values = values;
        Zobrist = zobrist;
        _cuckoo = cuckoo;
        State = new();
        Clear();
    }

    public IBoard Board { get; }

    public IZobrist Zobrist { get; }

    public BitBoard Checkers => State.Checkers;

    public ChessMode ChessMode { get; set; }

    public Square EnPassantSquare => State.EnPassantSquare;

    public string FenNotation => GenerateFen().ToString();

    public bool InCheck => State.Checkers.IsNotEmpty;

    public bool IsMate => !HasMoves();

    public bool IsProbing { get; set; }

    public bool IsRepetition => State.Repetition >= 3;

    /// <summary>
    /// To let something outside the library be aware of changes (like a UI etc)
    /// </summary>
    public Action<IPieceSquare> PieceUpdated { get; set; }

    public IValues Values { get; }

    public int Ply { get; private set; }

    public int Rule50 => State.ClockPly;

    public Player SideToMove => _sideToMove;

    public State State { get; private set; }

    public int Searcher { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddPiece(Piece pc, Square sq)
    {
        Board.AddPiece(pc, sq);

        if (!IsProbing)
            PieceUpdated?.Invoke(new PieceSquareEventArgs(pc, sq));
    }

    public bool AttackedByKing(Square sq, Player p)
        => (GetAttacks(sq, PieceType.King) & GetKingSquare(p)).IsNotEmpty;

    public bool AttackedByKnight(Square sq, Player p)
        => (Board.Pieces(p, PieceType.Knight) & GetAttacks(sq, PieceType.Knight)).IsNotEmpty;

    public bool AttackedByPawn(Square sq, Player p) =>
        (Board.Pieces(p, PieceType.Pawn) & sq.PawnAttack(~p)).IsNotEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AttackedBySlider(Square sq, Player p)
    {
        var occupied = Board.Pieces();
        var rookAttacks = sq.RookAttacks(in occupied);
        if (Board.Pieces(p, PieceType.Rook) & rookAttacks)
            return true;

        var bishopAttacks = sq.BishopAttacks(in occupied);
        if (Board.Pieces(p, PieceType.Bishop) & bishopAttacks)
            return true;

        return (Board.Pieces(p, PieceType.Queen) & (bishopAttacks | rookAttacks)).IsNotEmpty;
    }

    public BitBoard AttacksBy(PieceType pt, Player p)
    {
        var attackers = Pieces(pt, p);

        if (pt == PieceType.Pawn)
            return attackers.PawnAttacks(p);

        var threats = BitBoard.Empty;
        while (attackers)
            threats |= GetAttacks(BitBoards.PopLsb(ref attackers), pt);
        return threats;
    }

    public bool IsCapture(Move m)
    {
        var mt = m.MoveType();
        return mt == MoveTypes.Enpassant ||
               (mt != MoveTypes.Castling && Board.Pieces(~SideToMove).Contains(m.ToSquare()));
    }

    public bool IsCaptureOrPromotion(Move m)
    {
        var mt = m.MoveType();
        return mt switch
        {
            MoveTypes.Normal => Board.Pieces(~SideToMove).Contains(m.ToSquare()),
            var _            => mt != MoveTypes.Castling
        };
    }

    public bool IsPawnPassedAt(Player p, Square sq)
        => (Board.Pieces(~p, PieceType.Pawn) & sq.PassedPawnFrontAttackSpan(p)).IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard PawnPassSpan(Player p, Square sq) => p.FrontSquares(sq) | sq.PawnAttackSpan(p);

    public BitBoard AttacksTo(Square sq, in BitBoard occ)
    {
        Debug.Assert(sq.IsOk);

        return (sq.PawnAttack(Player.White) & Board.Pieces(Player.Black, PieceType.Pawn))
               | (sq.PawnAttack(Player.Black) & Board.Pieces(Player.White, PieceType.Pawn))
               | (GetAttacks(sq, PieceType.Knight) & Board.Pieces(PieceType.Knight))
               | (GetAttacks(sq, PieceType.Rook, in occ) & Board.Pieces(PieceType.Rook, PieceType.Queen))
               | (GetAttacks(sq, PieceType.Bishop, in occ) & Board.Pieces(PieceType.Bishop, PieceType.Queen))
               | (GetAttacks(sq, PieceType.King) & Board.Pieces(PieceType.King));
    }

    public BitBoard AttacksTo(Square sq) => AttacksTo(sq, Board.Pieces());

    public BitBoard KingBlockers(Player p) => State.BlockersForKing[p];

    public bool IsKingBlocker(Player p, Square sq) => KingBlockers(p).Contains(sq);

    public BitBoard SliderBlockerOn(Square sq, BitBoard attackers, ref BitBoard pinners, ref BitBoard hidders)
    {
        var blockers = BitBoards.EmptyBitBoard;
        var defenders = Board.Pieces(GetPiece(sq).ColorOf());

        // Snipers are X-ray slider attackers at 's'
        // No need to remove direct attackers at 's' as in check no evaluation

        var snipers = attackers & ((Pieces(PieceTypes.Bishop, PieceTypes.Queen) & GetAttacks(sq, PieceTypes.Bishop))
                                   | (Pieces(PieceTypes.Rook, PieceTypes.Queen) & GetAttacks(sq, PieceTypes.Rook)));

        var mocc = Pieces() ^ snipers;

        while (snipers)
        {
            var sniperSq = BitBoards.PopLsb(ref snipers);
            var b = sq.BitboardBetween(sniperSq) & mocc;

            if (b.IsEmpty || b.MoreThanOne())
                continue;

            blockers |= b;

            if ((b & defenders).IsNotEmpty)
                pinners |= sniperSq;
            else
                hidders |= sniperSq;
        }

        return blockers;
    }

    public bool CanCastle(CastleRight cr) => State.CastleRights.Has(cr);

    public bool CanCastle(Player p) => State.CastleRights.Has(p);

    public ref BitBoard CastleKingPath(CastleRight cr) => ref _castleKingPath[cr.AsInt()];

    public bool CastlingImpeded(CastleRight cr)
    {
        Debug.Assert(cr.Rights is CastleRights.WhiteKing or CastleRights.WhiteQueen or CastleRights.BlackKing
            or CastleRights.BlackQueen);
        return (Board.Pieces() & _castleRookPath[cr.Rights.AsInt()]).IsNotEmpty;
    }

    public Square CastlingRookSquare(CastleRight cr)
    {
        Debug.Assert(cr.Rights is CastleRights.WhiteKing or CastleRights.WhiteQueen or CastleRights.BlackKing
            or CastleRights.BlackQueen);
        return _castlingRookSquare[cr.Rights.AsInt()];
    }

    public BitBoard CheckedSquares(PieceType pt) => State.CheckedSquares[pt];

    public void Clear()
    {
        Board.Clear();
        _castleKingPath.Fill(BitBoard.Empty);
        _castleRookPath.Fill(BitBoard.Empty);
        _castlingRightsMask.Fill(CastleRight.None);
        _castlingRookSquare.Fill(Square.None);
        _sideToMove = Player.White;
        ChessMode = ChessMode.Normal;
    }

    /// <summary>
    /// Parses the board layout to a FEN representation.. Beware, goblins are a foot.
    /// </summary>
    /// <returns>The FenData which contains the fen string that was generated.</returns>
    [SkipLocalsInit]
    public FenData GenerateFen()
    {
        const char space = ' ';
        const char zero = '0';
        const char slash = '/';
        const char dash = '-';

        Span<char> fen = stackalloc char[Fen.Fen.MaxFenLen];
        var length = 0;

        for (var rank = Ranks.Rank8; rank >= Ranks.Rank1; rank--)
        {
            for (var file = Files.FileA; file <= Files.FileH; file++)
            {
                var empty = 0;
                for (; file <= Files.FileH && Board.IsEmpty(new(rank, file)); ++file)
                    ++empty;

                if (empty != 0)
                    fen[length++] = (char)(zero + empty);

                if (file <= Files.FileH)
                    fen[length++] = Board.PieceAt(new(rank, file)).GetPieceChar();
            }

            if (rank > Ranks.Rank1)
                fen[length++] = slash;
        }

        fen[length++] = space;
        fen[length++] = _sideToMove.Fen;
        fen[length++] = space;

        if (State.CastleRights == CastleRight.None)
            fen[length++] = dash;
        else
        {
            if (ChessMode == ChessMode.Normal)
            {
                if (CanCastle(CastleRight.WhiteKing))
                    fen[length++] = 'K';

                if (CanCastle(CastleRight.WhiteQueen))
                    fen[length++] = 'Q';

                if (CanCastle(CastleRight.BlackKing))
                    fen[length++] = 'k';

                if (CanCastle(CastleRight.BlackQueen))
                    fen[length++] = 'q';
            }
            else if (ChessMode == ChessMode.Chess960)
            {
                if (CanCastle(CastleRight.WhiteKing))
                    fen[length++] = CastlingRookSquare(CastleRight.WhiteKing).FileChar;

                if (CanCastle(CastleRight.WhiteQueen))
                    fen[length++] = CastlingRookSquare(CastleRight.WhiteQueen).FileChar;

                if (CanCastle(CastleRight.BlackKing))
                    fen[length++] = CastlingRookSquare(CastleRight.BlackQueen).FileChar;

                if (CanCastle(CastleRight.BlackQueen))
                    fen[length++] = CastlingRookSquare(CastleRight.BlackQueen).FileChar;
            }
            else
                throw new InvalidFenException($"Invalid chess mode. mode={ChessMode}");
        }

        fen[length++] = space;
        if (State.EnPassantSquare == Square.None)
            fen[length++] = dash;
        else
        {
            fen[length++] = State.EnPassantSquare.FileChar;
            fen[length++] = State.EnPassantSquare.RankChar;
        }

        fen[length++] = space;

        Span<char> format = stackalloc char[1] { 'D' };

        State.ClockPly.TryFormat(fen[length..], out var written, format);

        length += written;
        fen[length++] = space;

        var ply = 1 + (Ply - _sideToMove.IsBlack.AsByte() / 2);
        ply.TryFormat(fen[length..], out written, format);

        length += written;

        return new(new string(fen[..length]));
    }

    public BitBoard GetAttacks(Square sq, PieceType pt, in BitBoard occ)
    {
        Debug.Assert(pt != PieceTypes.Pawn, "Pawns need player");

        if (pt == PieceType.Knight || pt == PieceType.King)
            return pt.PseudoAttacks(sq);

        if (pt == PieceType.Bishop)
            return sq.BishopAttacks(in occ);

        if (pt == PieceType.Rook)
            return sq.RookAttacks(in occ);

        if (pt == PieceType.Queen)
            return sq.QueenAttacks(in occ);

        return BitBoards.EmptyBitBoard;
    }

    public BitBoard GetAttacks(Square sq, PieceType pt) => GetAttacks(sq, pt, Pieces());

    public CastleRight GetCastleRightsMask(Square sq) => _castlingRightsMask[sq];

    public IEnumerator<Piece> GetEnumerator() => Board.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Square GetKingSquare(Player p) => Board.Square(PieceType.King, p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey GetPawnKey()
    {
        var result = Zobrist.ZobristNoPawn;
        var pieces = Board.Pieces(PieceType.Pawn);
        while (pieces)
        {
            var sq = BitBoards.PopLsb(ref pieces);
            var pc = GetPiece(sq);
            result ^= Zobrist.Psq(sq, pc);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Piece GetPiece(Square sq) => Board.PieceAt(sq);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey GetKey(State state)
    {
        var result = HashKey.Empty;
        var pieces = Board.Pieces();
        while (pieces)
        {
            var sq = BitBoards.PopLsb(ref pieces);
            var pc = Board.PieceAt(sq);
            result ^= Zobrist.Psq(sq, pc);
        }

        if (state.EnPassantSquare != Square.None)
            result ^= Zobrist.EnPassant(state.EnPassantSquare);

        if (_sideToMove == Player.Black)
            result ^= Zobrist.Side();

        return result;
    }

    public Square GetPieceSquare(PieceType pt, Player p) => Board.Square(pt, p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceType GetPieceType(Square sq) => Board.PieceAt(sq).Type();

    public bool GivesCheck(Move m)
    {
        Debug.Assert(!m.IsNullMove());
        Debug.Assert(MovedPiece(m).ColorOf() == _sideToMove);

        var (from, to) = m;

        var pc = Board.PieceAt(from);
        var pt = pc.Type();

        // Is there a direct check?
        if ((State.CheckedSquares[pt] & to).IsNotEmpty)
            return true;

        var us = _sideToMove;
        var them = ~us;

        // Is there a discovered check?
        if ((State.BlockersForKing[them] & from).IsNotEmpty
            && !from.Aligned(to, GetKingSquare(them)))
            return true;

        switch (m.MoveType())
        {
            case MoveTypes.Normal:
                return false;

            case MoveTypes.Promotion:
                return (GetAttacks(to, m.PromotedPieceType(), Board.Pieces() ^ from) & GetKingSquare(them)).IsNotEmpty;

            // En passant capture with check? We have already handled the case of direct checks
            // and ordinary discovered check, so the only case we need to handle is the unusual
            // case of a discovered check through the captured pawn.
            case MoveTypes.Enpassant:
            {
                var captureSquare = new Square(from.Rank, to.File);
                var b = (Board.Pieces() ^ from ^ captureSquare) | to;
                var ksq = GetKingSquare(them);

                var attacks = (GetAttacks(ksq, PieceType.Rook, in b) &
                               Board.Pieces(us, PieceType.Rook, PieceType.Queen))
                              | (GetAttacks(ksq, PieceType.Bishop, in b) &
                                 Board.Pieces(us, PieceType.Bishop, PieceType.Queen));
                return attacks.IsNotEmpty;
            }
            case MoveTypes.Castling:
            {
                // ReSharper disable once InlineTemporaryVariable
                var kingFrom = from;
                // ReSharper disable once InlineTemporaryVariable
                var rookFrom = to; // Castling is encoded as 'King captures the rook'
                var kingTo = (rookFrom > kingFrom ? Square.G1 : Square.C1).Relative(us);
                var rookTo = (rookFrom > kingFrom ? Square.F1 : Square.D1).Relative(us);
                var ksq = GetKingSquare(them);

                return (PieceType.Rook.PseudoAttacks(rookTo) & ksq).IsNotEmpty
                       && (GetAttacks(rookTo, PieceType.Rook,
                           (Board.Pieces() ^ kingFrom ^ rookFrom) | rookTo | kingTo) & ksq).IsNotEmpty;
            }
            default:
                Debug.Assert(false);
                return false;
        }
    }

    public bool HasGameCycle(int ply)
    {
        var end = State.End();
        return end >= 3 && _cuckoo.HashCuckooCycle(this, end, ply);
    }

    public bool HasRepetition()
    {
        var currentState = State;
        var end = currentState.End();

        while (end-- >= 4)
        {
            if (currentState!.Repetition > 0)
                return true;

            currentState = currentState.Previous;
        }

        return false;
    }

    public bool IsDraw(int ply)
        => State.ClockPly switch
        {
            > 99 when State.Checkers.IsEmpty || HasMoves() => true,
            var _                                          => State.Repetition > 0 && State.Repetition < ply
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAttacked(Square sq, Player p)
        => AttackedBySlider(sq, p) || AttackedByKnight(sq, p) || AttackedByPawn(sq, p) || AttackedByKing(sq, p);

    public bool IsLegal(Move m)
    {
        Debug.Assert(!m.IsNullMove());

        var us = _sideToMove;
        var (from, to, type) = m;
        var ksq = GetKingSquare(us);

        Debug.Assert(GetPiece(GetKingSquare(us)) == PieceType.King.MakePiece(us));
        Debug.Assert(to != from);

        // En passant captures are a tricky special case. Because they are rather uncommon, we
        // do it simply by testing whether the king is attacked after the move is made.
        if (type == MoveTypes.Enpassant)
        {
            Debug.Assert(MovedPiece(m) == PieceType.Pawn.MakePiece(us));
            return IsEnPassantMoveLegal(to, us, from, ksq);
        }

        // Check for legal castleling move
        if (type == MoveTypes.Castling)
            return IsCastleMoveLegal(m, to, from, us);

        // If the moving piece is a king, check whether the destination square is attacked by
        // the opponent.
        if (MovedPiece(m).Type() == PieceType.King)
            return (AttacksTo(to) & Board.Pieces(~us)).IsEmpty;

        // A non-king move is legal if and only if it is not pinned or it is moving along the
        // ray towards or away from the king.
        return (KingBlockers(us) & from).IsEmpty || from.Aligned(to, ksq);
    }

    private bool IsCastleMoveLegal(Move m, Square to, Square from, Player us)
    {
        // After castling, the rook and king final positions are the same in Chess960 as
        // they would be in standard chess.

        var them = ~us;
        var isKingSide = to > from;
        Direction step;

        if (isKingSide)
        {
            to = Square.G1.Relative(us);
            step = Direction.West;
        }
        else
        {
            to = Square.C1.Relative(us);
            step = Direction.East;
        }

        for (var s = to; s != from; s += step)
            if (AttacksTo(s) & Board.Pieces(them))
                return false;

        // In case of Chess960, verify that when moving the castling rook we do not discover
        // some hidden checker. For instance an enemy queen in SQ_A1 when castling rook is
        // in SQ_B1.
        if (ChessMode == ChessMode.Normal)
            return true;

        var attacks = GetAttacks(
            sq: to,
            pt: PieceType.Rook,
            occ: Board.Pieces() ^ m.ToSquare()
        );

        var occupied = Board.Pieces(them, PieceType.Rook, PieceType.Queen);

        return (attacks & occupied).IsEmpty;

        static (Square, Direction) GetRookSquareAndDirection(Square toSq, Square fromSq, Player us)
        {
            return toSq > fromSq
                ? (Square.G1.Relative(us), Direction.West)
                : (Square.C1.Relative(us), Direction.East);
        }
    }

    private bool IsEnPassantMoveLegal(Square to, Player us, Square from, Square ksq)
    {
        var captureSquare = to - us.PawnPushDistance();
        var occupied = (Board.Pieces() ^ from ^ captureSquare) | to;

        Debug.Assert(to == EnPassantSquare);
        Debug.Assert(GetPiece(captureSquare) == PieceType.Pawn.MakePiece(~us));
        Debug.Assert(GetPiece(to) == Piece.EmptyPiece);

        return (GetAttacks(ksq, PieceType.Rook, in occupied) &
                Board.Pieces(~us, PieceType.Rook, PieceType.Queen)).IsEmpty
               && (GetAttacks(ksq, PieceType.Bishop, in occupied) &
                   Board.Pieces(~us, PieceType.Bishop, PieceType.Queen)).IsEmpty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOccupied(Square sq) => !Board.IsEmpty(sq);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPieceTypeOnSquare(Square sq, PieceType pt) => Board.PieceAt(sq).Type() == pt;

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
            return ContainsMove(m);

        // Is not a promotion, so promotion piece must be empty
        if (m.PromotedPieceType() - 2 != PieceType.NoPieceType)
            return false;

        var us = _sideToMove;
        var pc = MovedPiece(m);
        var (from, to) = m;

        // If the from square is not occupied by a piece belonging to the side to move, the move
        // is obviously not legal.
        if (pc == Piece.EmptyPiece || pc.ColorOf() != us)
            return false;

        // The destination square cannot be occupied by a friendly piece
        if ((Pieces(us) & to).IsNotEmpty)
            return false;

        // Handle the special case of a pawn move
        if (pc.Type() == PieceType.Pawn)
        {
            // We have already handled promotion moves, so destination cannot be on the 8th/1st rank.
            if (to.Rank == Rank.Rank8.Relative(us))
                return false;

            if ((from.PawnAttack(us) & Pieces(~us) & to).IsEmpty            // Not a capture
                && !(from + us.PawnPushDistance() == to && !IsOccupied(to)) // Not a single push
                && !(from + us.PawnDoublePushDistance() == to               // Not a double push
                     && from.Rank == Rank.Rank2.Relative(us)
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

        if (pc.Type() != PieceType.King)
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
        else if ((AttacksTo(to, Pieces() ^ from) & Pieces(~us)).IsNotEmpty)
            return false;

        return true;
    }

    public void MakeMove(Move m, in State newState) => MakeMove(m, in newState, GivesCheck(m));

    public void MakeMove(Move m, in State newState, bool givesCheck)
    {
        State.LastMove = m;

        var posKey = State.PositionKey ^ Zobrist.Side();

        State = State.CopyTo(newState);
        var state = State;

        Ply++;
        state.ClockPly++;
        state.NullPly++;

        var us = _sideToMove;
        var them = ~us;
        var (from, to, type) = m;
        var pc = GetPiece(from);
        var capturedPiece = m.IsEnPassantMove()
            ? PieceType.Pawn.MakePiece(them)
            : GetPiece(to);

        Debug.Assert(pc.ColorOf() == us);
        Debug.Assert(
            capturedPiece == Piece.EmptyPiece || capturedPiece.ColorOf() == (!m.IsCastleMove() ? them : us));
        Debug.Assert(capturedPiece.Type() != PieceTypes.King);

        if (type == MoveTypes.Castling)
        {
            Debug.Assert(pc == PieceType.King.MakePiece(us));
            Debug.Assert(capturedPiece == PieceType.Rook.MakePiece(us));

            DoCastle(us, from, ref to, out var rookFrom, out var rookTo, CastlePerform.Do);
            // var (rookFrom, rookTo) = DoCastle(us, from, ref to, CastlePerform.Do);

            posKey ^= Zobrist.Psq(rookFrom, capturedPiece) ^ Zobrist.Psq(rookTo, capturedPiece);

            // reset captured piece type as castleling is "king-captures-rook"
            capturedPiece = Piece.EmptyPiece;
        }

        if (capturedPiece != Piece.EmptyPiece)
        {
            var captureSquare = to;

            if (capturedPiece.Type() == PieceType.Pawn)
            {
                if (type == MoveTypes.Enpassant)
                {
                    captureSquare -= us.PawnPushDistance();

                    Debug.Assert(pc.Type() == PieceTypes.Pawn);
                    Debug.Assert(to == State.EnPassantSquare);
                    Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                    Debug.Assert(!IsOccupied(to));
                    Debug.Assert(GetPiece(captureSquare) == pc.Type().MakePiece(them));
                }

                state.PawnKey ^= Zobrist.Psq(captureSquare, capturedPiece);
            }
            else
                _nonPawnMaterial[them] -= Values.GetPieceValue(capturedPiece, Phases.Mg);

            // Update board and piece lists
            RemovePiece(captureSquare);
            if (type == MoveTypes.Enpassant)
                Board.ClearPiece(captureSquare);

            posKey ^= Zobrist.Psq(captureSquare, capturedPiece);
            State.MaterialKey ^= Zobrist.Psq(Board.PieceCount(capturedPiece), capturedPiece);

            // TODO : Update other depending keys and psq values here

            // Reset rule 50 counter
            state.ClockPly = 0;
        }

        // update key with moved piece
        posKey ^= Zobrist.Psq(from, pc) ^ Zobrist.Psq(to, pc);

        // reset en-passant square if it is set
        if (state.EnPassantSquare != Square.None)
        {
            posKey ^= Zobrist.EnPassant(state.EnPassantSquare);
            state.EnPassantSquare = Square.None;
        }

        // Update castling rights if needed
        if (state.CastleRights != CastleRight.None &&
            (_castlingRightsMask[from] | _castlingRightsMask[to]) != CastleRight.None)
        {
            posKey ^= Zobrist.Castle(state.CastleRights);
            state.CastleRights &= ~(_castlingRightsMask[from] | _castlingRightsMask[to]);
            posKey ^= Zobrist.Castle(state.CastleRights);
        }

        // Move the piece. The tricky Chess960 castle is handled earlier
        if (type != MoveTypes.Castling)
            MovePiece(from, to);

        // If the moving piece is a pawn do some special extra work
        if (pc.Type() == PieceType.Pawn)
        {
            // Set en-passant square, only if moved pawn can be captured
            if ((to.AsInt() ^ from.AsInt()) == 16
                && CanEnPassant(them, from + us.PawnPushDistance()))
            {
                state.EnPassantSquare = from + us.PawnPushDistance();
                posKey ^= Zobrist.EnPassant(state.EnPassantSquare.File);
            }
            else if (type == MoveTypes.Promotion)
            {
                var promotionPiece = m.PromotedPieceType().MakePiece(us);

                Debug.Assert(to.RelativeRank(us) == Rank.Rank8);
                Debug.Assert(promotionPiece.Type().InBetween(PieceTypes.Knight, PieceTypes.Queen));

                RemovePiece(to);
                AddPiece(promotionPiece, to);

                // Update hash keys
                posKey ^= Zobrist.Psq(to, pc) ^ Zobrist.Psq(to, promotionPiece);
                state.PawnKey ^= Zobrist.Psq(to, pc);

                _nonPawnMaterial[us] += Values.GetPieceValue(promotionPiece, Phases.Mg);
            }

            // Update pawn hash key
            state.PawnKey ^= Zobrist.Psq(from, pc) ^ Zobrist.Psq(to, pc);

            // Reset rule 50 draw counter
            state.ClockPly = 0;
        }

        // TODO : Update piece values here

        Debug.Assert(GetKingSquare(us).IsOk);
        Debug.Assert(GetKingSquare(them).IsOk);

        // Update state properties
        state.PositionKey = posKey;
        state.CapturedPiece = capturedPiece.Type();

        state.Checkers = givesCheck ? AttacksTo(GetKingSquare(them)) & Board.Pieces(us) : BitBoard.Empty;

        _sideToMove = ~_sideToMove;

        SetCheckInfo(state);
        state.UpdateRepetition();

        //Debug.Assert(_positionValidator.Validate().IsOk);
    }

    public void MakeNullMove(in State newState)
    {
        Debug.Assert(!InCheck);

        CopyState(in newState);

        if (State.EnPassantSquare != Square.None)
        {
            State.PositionKey ^= Zobrist.EnPassant(State.EnPassantSquare);
            State.EnPassantSquare = Square.None;
        }

        State.PositionKey ^= Zobrist.Side();

        ++State.ClockPly;
        State.NullPly = 0;

        _sideToMove = ~_sideToMove;

        SetCheckInfo(State);

        State.Repetition = 0;

        Debug.Assert(_positionValidator.Validate(this, PositionValidationTypes.Basic).Ok);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Piece MovedPiece(Move m) => Board.MovedPiece(m);

    /// <summary>
    /// Converts a move data type to move notation string format which chess engines understand.
    /// e.g. "a2a4", "a7a8q"
    /// </summary>
    /// <param name="m">The move to convert</param>
    /// <param name="output">The string builder used to generate the string with</param>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveToString(Move m, in StringBuilder output)
    {
        if (m.IsNullMove())
        {
            output.Append("(none)");
            return;
        }

        var (from, to, type) = m;

        if (type == MoveTypes.Castling && ChessMode == ChessMode.Normal)
        {
            var file = to > from ? Files.FileG : Files.FileC;
            to = new(from.Rank, file);
        }

        Span<char> s = stackalloc char[5];
        var index = 0;

        s[index++] = from.FileChar;
        s[index++] = from.RankChar;
        s[index++] = to.FileChar;
        s[index++] = to.RankChar;

        if (type == MoveTypes.Promotion)
            s[index++] = char.ToLower(m.PromotedPieceType().GetPieceChar());

        output.Append(new string(s[..index]));
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

        if (pc.Type() != PieceType.Pawn)
            return false;

        var c = pc.ColorOf();

        return (sq.PassedPawnFrontAttackSpan(c) & Board.Pieces(c, PieceType.Pawn)).IsEmpty;
    }

    /// <summary>
    /// Determine if a pawn is isolated e.i. no own pawns on either of it's neighboring files
    /// </summary>
    /// <param name="sq"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PawnIsolated(Square sq, Player p)
        => ((sq.PawnAttackSpan(p) | sq.PawnAttackSpan(~p)) & Board.Pieces(p, PieceType.Pawn)).IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PieceOnFile(Square sq, Player p, PieceType pt) => (Board.Pieces(p, pt) & sq).IsNotEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces() => Board.Pieces();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces(Player p) => Board.Pieces(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces(Piece pc) => Board.Pieces(pc.ColorOf(), pc.Type());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces(PieceType pt) => Board.Pieces(pt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces(PieceType pt1, PieceType pt2) => Board.Pieces(pt1, pt2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces(PieceType pt, Player p) => Board.Pieces(p, pt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard Pieces(PieceType pt1, PieceType pt2, Player p) => Board.Pieces(p, pt1, pt2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PieceCount() => Board.PieceCount();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PieceCount(Piece pc) => Board.PieceCount(pc.Type(), pc.ColorOf());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PieceCount(PieceType pt) => Board.PieceCount(pt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PieceCount(PieceType pt, Player p) => Board.PieceCount(pt, p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard PawnsOnColor(Player p, Square sq) => Pieces(PieceType.Pawn, p) & sq.Color().ColorBB();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SemiOpenFileOn(Player p, Square sq)
        => (Board.Pieces(p, PieceType.Pawn) & sq.File.BitBoardFile()).IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool BishopPaired(Player p) =>
        Board.PieceCount(PieceType.Bishop, p) >= 2
        && (Board.Pieces(p, PieceType.Bishop) & Player.White.ColorBB()).IsNotEmpty
        && (Board.Pieces(p, PieceType.Bishop) & Player.Black.ColorBB()).IsNotEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool BishopOpposed() =>
        Board.PieceCount(Piece.WhiteBishop) == 1
        && Board.PieceCount(Piece.BlackBishop) == 1
        && Board.Square(PieceType.Bishop, Player.White)
                .IsOppositeColor(Board.Square(PieceType.Bishop, Player.Black));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard PinnedPieces(Player p)
    {
        Debug.Assert(State != null);
        return State.Pinners[p];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemovePiece(Square sq)
    {
        Board.RemovePiece(sq);
        if (!IsProbing)
            PieceUpdated?.Invoke(new PieceSquareEventArgs(Piece.EmptyPiece, sq));
    }

    public bool SeeGe(Move m, Value threshold)
    {
        Debug.Assert(m.IsNullMove());

        // Only deal with normal moves, assume others pass a simple see
        if (m.MoveType() != MoveTypes.Normal)
            return Value.ValueZero >= threshold;

        var (from, to) = m;

        var swap = Values.GetPieceValue(GetPiece(to), Phases.Mg) - threshold;

        if (swap < Value.ValueZero)
            return false;

        swap = Values.GetPieceValue(GetPiece(from), Phases.Mg) - swap;

        if (swap <= Value.ValueZero)
            return true;

        var occupied = Board.Pieces() ^ from ^ to;
        var stm = GetPiece(from).ColorOf();
        var attackers = AttacksTo(to, in occupied);
        var res = 1;

        do
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
                stmAttackers &= ~State.BlockersForKing[stm];

            if (stmAttackers.IsEmpty)
                break;

            res ^= 1;

            // Locate and remove the next least valuable attacker, and add to the bitboard
            // 'attackers' any X-ray attackers behind it.
            var bb = stmAttackers & Board.Pieces(PieceType.Pawn);
            if (bb.IsNotEmpty)
            {
                if ((swap = Values.PawnValueMg - swap) < res)
                    break;

                occupied ^= bb.Lsb();
                attackers |= GetAttacks(to, PieceType.Bishop, in occupied) &
                             Board.Pieces(PieceType.Bishop, PieceType.Queen);
            }
            else if ((bb = stmAttackers & Board.Pieces(PieceType.Knight)).IsNotEmpty)
            {
                if ((swap = Values.KnightValueMg - swap) < res)
                    break;

                occupied ^= bb.Lsb();
            }
            else if ((bb = stmAttackers & Board.Pieces(PieceType.Bishop)).IsNotEmpty)
            {
                if ((swap = Values.BishopValueMg - swap) < res)
                    break;

                occupied ^= bb.Lsb();
                attackers |= GetAttacks(to, PieceType.Bishop, in occupied) &
                             Board.Pieces(PieceType.Bishop, PieceType.Queen);
            }
            else if ((bb = stmAttackers & Board.Pieces(PieceType.Rook)).IsNotEmpty)
            {
                if ((swap = Values.RookValueMg - swap) < res)
                    break;

                occupied ^= bb.Lsb();
                attackers |= GetAttacks(to, PieceType.Rook, in occupied) &
                             Board.Pieces(PieceType.Rook, PieceType.Queen);
            }
            else if ((bb = stmAttackers & Board.Pieces(PieceType.Queen)).IsNotEmpty)
            {
                if ((swap = Values.QueenValueMg - swap) < res)
                    break;

                occupied ^= bb.Lsb();
                attackers |= (GetAttacks(to, PieceType.Bishop, in occupied) &
                              Board.Pieces(PieceType.Bishop, PieceType.Queen))
                             | (GetAttacks(to, PieceType.Rook, in occupied) &
                                Board.Pieces(PieceType.Rook, PieceType.Queen));
            }
            else // KING
                // If we "capture" with the king but opponent still has attackers, reverse the result.
            {
                bb = attackers & ~Board.Pieces(stm);
                if (!bb.IsEmpty)
                    res ^= 1;
                return res > 0;
            }
        } while (true);

        return res > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Value NonPawnMaterial(Player p) => _nonPawnMaterial[p];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Value NonPawnMaterial() => _nonPawnMaterial[0] - _nonPawnMaterial[1];

    private void SetupPieces(ReadOnlySpan<char> fenChunk)
    {
        var f = 1; // file (column)
        var r = 8; // rank (row)

        ref var fenChunkSpace = ref MemoryMarshal.GetReference(fenChunk);
        for (var i = 0; i < fenChunk.Length; i++)
        {
            var c = Unsafe.Add(ref fenChunkSpace, i);
            if (char.IsNumber(c))
                f += c - '0';
            else if (c == '/')
            {
                r--;
                f = 1;
            }
            else
            {
                var pieceIndex = PieceExtensions.PieceChars.IndexOf(c);

                if (pieceIndex == -1)
                    throw new InvalidFenException("Invalid char detected");

                Player p = new(char.IsLower(PieceExtensions.PieceChars[pieceIndex]));

                var square = new Square(r - 1, f - 1);

                var pt = new PieceType(pieceIndex);
                var pc = pt.MakePiece(p);
                AddPiece(pc, square);

                f++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetupPlayer(ReadOnlySpan<char> fenChunk) => _sideToMove = (fenChunk[0] != 'w').AsByte();

    private void SetupEnPassant(ReadOnlySpan<char> fenChunk)
    {
        var enPassant = fenChunk.Length == 2
                        && fenChunk[0] != '-'
                        && char.IsBetween(fenChunk[0], 'a', 'h')
                        && fenChunk[1] == (_sideToMove.IsWhite ? '6' : '3');

        if (enPassant)
        {
            var r = new Rank(fenChunk[1] - '1');
            var f = new File(fenChunk[0] - 'a');
            var epSquare = new Square(r, f);

            var us = _sideToMove;
            var them = ~us;

            if (!(epSquare.PawnAttack(them) & Pieces(PieceType.Pawn, us)).IsEmpty
                && !(Pieces(PieceType.Pawn, them) & (epSquare + them.PawnPushDistance())).IsEmpty
                && (Pieces() & (epSquare | (epSquare + us.PawnPushDistance()))).IsEmpty)
                State.EnPassantSquare = epSquare;
        }
        else
            State.EnPassantSquare = Square.None;
    }

    private void SetupMoveNumber(ReadOnlySpan<char> fen, in Range halfMove, in Range moveNumber)
    {
        var moveNum = 0;
        var halfMoveNum = 0;

        var chunk = fen[halfMove];

        if (!chunk.IsEmpty)
        {
            if (!Maths.ToIntegral(chunk, out halfMoveNum))
                halfMoveNum = 0;

            // half move number
            chunk = fen[moveNumber];

            Maths.ToIntegral(chunk, out moveNum);

            if (moveNum > 0)
                moveNum--;
        }

        State.ClockPly = halfMoveNum;
        Ply = moveNum;
    }

    /// <summary>
    /// Apply FEN to this position.
    ///
    /// </summary>
    /// <param name="fenData">The fen data to set</param>
    /// <param name="chessMode">The chess mode to apply</param>
    /// <param name="state">State reference to use. Allows to keep track of states if pre-created (i.e. in a stack) before engine search start</param>
    /// <param name="validate">If true, the fen should be validated, otherwise not</param>
    /// <param name="searcher">Searcher index, to help point to a specific search index in thread-based search array</param>
    public IPosition Set(
        in FenData fenData, ChessMode chessMode, in State state, bool validate = false, int searcher = 0)
    {
        if (validate)
            Fen.Fen.Validate(fenData.Fen.ToString());

        Clear();

        ChessMode = chessMode;
        Searcher = searcher;

        CopyState(in state);

        const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

        var fen = fenData.Fen.Span;
        Span<Range> ranges = stackalloc Range[7];
        var splitCount = fen.Split(ranges, ' ', splitOptions);

        if (splitCount < 4)
            throw new InvalidFenException("Invalid FEN string");

        SetupPieces(fen[ranges[0]]);
        SetupPlayer(fen[ranges[1]]);
        SetupCastle(fen[ranges[2]]);
        SetupEnPassant(fen[ranges[3]]);
        SetupMoveNumber(fen, in ranges[4], in ranges[5]);

        SetState(State);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IPosition Set(string fen, ChessMode chessMode, in State state, bool validate = false, int searcher = 0)
        => Set(new FenData(fen), chessMode, state, validate, searcher);

    public IPosition Set(ReadOnlySpan<char> code, Player p, in State state)
    {
        Debug.Assert(code[0] == 'K' && code[1..].IndexOf('K') != -1);
        Debug.Assert(code.Length.IsBetween(0, 8));
        Debug.Assert(code[0] == 'K');

        var kingPos = code.LastIndexOf('K');
        var sides = new[] { code[kingPos..].ToString(), code[..kingPos].ToString() };

        sides[p] = sides[p].ToLower();

        var fenStr = $"{sides[0]}{8 - sides[0].Length}/8/8/8/8/8/8/{sides[1]}{8 - sides[1].Length} w - - 0 10";

        return Set(fenStr, ChessMode.Normal, in state);
    }

    public void TakeMove(Move m)
    {
        Debug.Assert(m.IsValidMove());

        // flip sides
        _sideToMove = ~_sideToMove;

        var us = _sideToMove;
        var (from, to, type) = m;

        Debug.Assert(!IsOccupied(from) || m.IsCastleMove());
        Debug.Assert(State.CapturedPiece != PieceTypes.King);

        if (type == MoveTypes.Promotion)
        {
            Debug.Assert(GetPiece(to).Type() == m.PromotedPieceType());
            Debug.Assert(to.RelativeRank(us) == Rank.Rank8);
            Debug.Assert(m.PromotedPieceType() >= PieceTypes.Knight && m.PromotedPieceType() <= PieceTypes.Queen);

            RemovePiece(to);
            var pc = PieceType.Pawn.MakePiece(us);
            AddPiece(pc, to);
            _nonPawnMaterial[_sideToMove] -= Values.GetPieceValue(pc, Phases.Mg);
        }

        if (type == MoveTypes.Castling)
            DoCastle(us, from, ref to, out var _, out var _, CastlePerform.Undo);
        else
        {
            // Note: The parameters are reversed, since we move the piece "back"
#pragma warning disable S2234 // Parameters should be passed in the correct order
            MovePiece(to, from);
#pragma warning restore S2234 // Parameters should be passed in the correct order

            if (State.CapturedPiece != PieceTypes.NoPieceType)
            {
                var captureSquare = to;

                // En-Passant capture is not located on move square
                if (type == MoveTypes.Enpassant)
                {
                    captureSquare -= us.PawnPushDistance();

                    // Debug.Assert(GetPiece(to).Type() == PieceTypes.Pawn);
                    Debug.Assert(to == State.Previous.EnPassantSquare);
                    Debug.Assert(to.RelativeRank(us) == Ranks.Rank6);
                    Debug.Assert(!IsOccupied(captureSquare));
                }

                AddPiece(State.CapturedPiece.MakePiece(~_sideToMove), captureSquare);

                if (State.CapturedPiece != PieceTypes.Pawn)
                {
                    var them = ~_sideToMove;
                    _nonPawnMaterial[them] += Values.GetPieceValue(State.CapturedPiece, Phases.Mg);
                }
            }
        }

        Debug.Assert(GetKingSquare(~us).IsOk);
        Debug.Assert(GetKingSquare(us).IsOk);

        // Set state to previous state
        State = State.Previous;
        Ply--;

#if DEBUG
        var validatorResult = _positionValidator.Validate(this);
        Debug.Assert(validatorResult.Ok, validatorResult.Errors);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TakeNullMove()
    {
        Debug.Assert(State != null);
        Debug.Assert(State.Previous != null);
        Debug.Assert(State.NullPly == 0);
        Debug.Assert(State.CapturedPiece == PieceTypes.NoPieceType);
        Debug.Assert(State.Checkers.IsEmpty);

        Debug.Assert(!InCheck);
        State = State.Previous;
        _sideToMove = ~_sideToMove;
    }

    [SkipLocalsInit]
    public override string ToString()
    {
        const string separator = "\n  +---+---+---+---+---+---+---+---+\n";
        const char splitter = '|';
        const char space = ' ';
        var output = _outputObjectPool.Get();
        output.Append(separator);
        Span<char> row = stackalloc char[35];
        for (var rank = Ranks.Rank8; rank >= Ranks.Rank1; rank--)
        {
            row.Clear();
            var rowIndex = 0;

            row[rowIndex++] = (char)('1' + rank.AsInt());
            row[rowIndex++] = space;

            for (var file = Files.FileA; file <= Files.FileH; file++)
            {
                var piece = GetPiece(new(rank, file));
                row[rowIndex++] = splitter;
                row[rowIndex++] = space;
                row[rowIndex++] = piece.GetPieceChar();
                row[rowIndex++] = space;
            }

            row[rowIndex] = splitter;

            output.Append(new string(row));
            output.Append(separator);
        }

        output.AppendLine("    a   b   c   d   e   f   g   h");
        output.AppendLine($"Zobrist : 0x{State.PositionKey.Key:X}");
        var result = output.ToString();
        _outputObjectPool.Return(output);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey MovePositionKey(Move m)
    {
        Debug.Assert(m.IsValidMove());

        var movePositionKey = State.PositionKey
                              ^ Zobrist.Side()
                              ^ Zobrist.Psq(m.FromSquare(), Board.MovedPiece(m))
                              ^ Zobrist.Psq(m.ToSquare(), Board.MovedPiece(m));

        if (!Board.IsEmpty(m.ToSquare()))
            movePositionKey ^= Zobrist.Psq(m.ToSquare(), Board.PieceAt(m.ToSquare()));

        movePositionKey ^= Zobrist.EnPassant(State.EnPassantSquare);

        return movePositionKey;
    }

    public PositionValidationResult Validate(PositionValidationTypes type = PositionValidationTypes.Basic)
        => _positionValidator.Validate(this, type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CastleRights OrCastlingRight(Player c, bool isKingSide)
        => (CastleRights)((int)CastleRights.WhiteKing << ((!isKingSide).AsByte() + 2 * c));

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void DoCastle(
        Player us,
        Square from,
        ref Square to,
        out Square rookFrom,
        out Square rookTo,
        CastlePerform castlePerform)
    {
        var kingSide = to > from;
        var doCastle = castlePerform == CastlePerform.Do;

        rookFrom = to; // Castling is encoded as "king captures friendly rook"
        if (kingSide)
        {
            rookTo = Square.F1.Relative(us);
            to = Square.G1.Relative(us);
        }
        else
        {
            rookTo = Square.D1.Relative(us);
            to = Square.C1.Relative(us);
        }

        // Remove both pieces first since squares could overlap in Chess960
        if (doCastle)
        {
            RemovePiece(from);
            RemovePiece(rookFrom);
            Board.ClearPiece(from);
            Board.ClearPiece(rookFrom);
            AddPiece(PieceType.King.MakePiece(us), to);
            AddPiece(PieceType.Rook.MakePiece(us), rookTo);
        }
        else
        {
            RemovePiece(to);
            RemovePiece(rookTo);
            Board.ClearPiece(to);
            Board.ClearPiece(rookTo);
            AddPiece(PieceType.King.MakePiece(us), from);
            AddPiece(PieceType.Rook.MakePiece(us), rookFrom);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MovePiece(Square from, Square to)
    {
        Board.MovePiece(from, to);

        if (IsProbing)
            return;

        PieceUpdated?.Invoke(new PieceSquareEventArgs(Board.PieceAt(from), to));
    }

    ///<summary>
    /// IPosition.SetCastlingRight() is an helper function used to set castling rights given the
    /// corresponding color and the rook starting square.
    /// </summary>
    /// <param name="stm"></param>
    /// <param name="rookFrom"></param>
    private void SetCastlingRight(Player stm, Square rookFrom)
    {
        var kingFrom = GetKingSquare(stm);
        var isKingSide = kingFrom < rookFrom;
        var cr = OrCastlingRight(stm, isKingSide);

        State.CastleRights |= cr;
        _castlingRightsMask[kingFrom] |= cr;
        _castlingRightsMask[rookFrom] |= cr;
        _castlingRookSquare[cr.AsInt()] = rookFrom;

        var kingTo = (isKingSide ? Square.G1 : Square.C1).Relative(stm);
        var rookTo = (isKingSide ? Square.F1 : Square.D1).Relative(stm);

        var kingPath = kingFrom.BitboardBetween(kingTo) | kingTo;
        _castleKingPath[cr.AsInt()] = kingPath;
        _castleRookPath[cr.AsInt()] = (kingPath | (rookFrom.BitboardBetween(rookTo) | rookTo)) & ~(kingFrom | rookFrom);
    }

    private void SetCheckInfo(in State state)
    {
        (state.BlockersForKing[Player.White], state.Pinners[Player.Black]) =
            SliderBlockers(Board.Pieces(Player.Black), GetKingSquare(Player.White));
        (state.BlockersForKing[Player.Black], state.Pinners[Player.White]) =
            SliderBlockers(Board.Pieces(Player.White), GetKingSquare(Player.Black));

        var ksq = GetKingSquare(~_sideToMove);

        state.CheckedSquares[PieceType.Pawn] = ksq.PawnAttack(~_sideToMove);
        state.CheckedSquares[PieceType.Knight] = GetAttacks(ksq, PieceType.Knight);
        state.CheckedSquares[PieceType.Bishop] = GetAttacks(ksq, PieceType.Bishop);
        state.CheckedSquares[PieceType.Rook] = GetAttacks(ksq, PieceType.Rook);
        state.CheckedSquares[PieceType.Queen] = state.CheckedSquares[PieceType.Bishop] |
                                                state.CheckedSquares[PieceType.Rook];
        state.CheckedSquares[PieceType.King] = BitBoard.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyState(in State newState)
        => State = State.CopyTo(newState);

    private void SetState(State state)
    {
        state.MaterialKey = HashKey.Empty;

        _nonPawnMaterial[Player.White] = _nonPawnMaterial[Player.Black] = Value.ValueZero;
        State.Checkers = AttacksTo(GetKingSquare(_sideToMove)) & Board.Pieces(~_sideToMove);

        SetCheckInfo(state);

        state.PositionKey = GetKey(state);
        state.PawnKey = GetPawnKey();

        var b = Pieces(PieceType.Pawn);
        while (b)
        {
            var sq = BitBoards.PopLsb(ref b);
            state.MaterialKey ^= Zobrist.Psq(sq, Board.PieceAt(sq));
        }

        ref var pieces = ref MemoryMarshal.GetArrayDataReference(Piece.All);

        for (var i = 0; i < Piece.All.Length; i++)
        {
            ref var pc = ref Unsafe.Add(ref pieces, i);
            var pt = pc.Type();
            if (pt != PieceType.Pawn && pt != PieceType.King)
            {
                var val = Values.GetPieceValue(pt, Phases.Mg).AsInt() * Board.PieceCount(pc);
                _nonPawnMaterial[pc.ColorOf()] += val;
            }

            for (var cnt = 0; cnt < Board.PieceCount(pc); ++cnt)
                state.MaterialKey ^= Zobrist.Psq(cnt, pc);
        }
    }

    private void SetupCastle(ReadOnlySpan<char> castle)
    {
        foreach (var ca in castle)
        {
            Player c = char.IsLower(ca);
            var rook = PieceType.Rook.MakePiece(c);
            var token = char.ToUpper(ca);

            var rsq = token switch
            {
                'K' => RookSquare(Square.H1.Relative(c), rook),
                'Q' => RookSquare(Square.A1.Relative(c), rook),
                var _ => char.IsBetween(token, 'A', 'H')
                    ? new(Rank.Rank1.Relative(c), new File(token - 'A'))
                    : Square.None
            };

            if (rsq != Square.None)
                SetCastlingRight(c, rsq);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Square RookSquare(Square startSq, Piece rook)
    {
        var targetSq = startSq;
        while (Board.PieceAt(targetSq) != rook)
            --targetSq;
        return targetSq;
    }

    private (BitBoard, BitBoard) SliderBlockers(in BitBoard sliders, Square s)
    {
        var result = (blockers: BitBoard.Empty, pinners: BitBoard.Empty);

        // Snipers are sliders that attack 's' when a piece and other snipers are removed
        var snipers = (PieceType.Rook.PseudoAttacks(s) & Board.Pieces(PieceType.Queen, PieceType.Rook)
                       | (PieceType.Bishop.PseudoAttacks(s) & Board.Pieces(PieceType.Queen, PieceType.Bishop)))
                      & sliders;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasMoves()
    {
        var ml = _moveListPool.Get();
        ml.Generate(this);
        var moves = ml.Get();
        var hasMoves = !moves.IsEmpty;
        _moveListPool.Return(ml);
        return hasMoves;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ContainsMove(Move m)
    {
        var ml = _moveListPool.Get();
        ml.Generate(this);
        var contains = ml.Contains(m);
        _moveListPool.Return(ml);
        return contains;
    }

    /// <summary>
    /// Checks if a given position allows for EnPassant by the given color
    /// </summary>
    /// <param name="us">The color to check</param>
    /// <param name="epSquare">The ep square to check</param>
    /// <param name="moved">flag if piece is moved or not</param>
    /// <returns>true if allows, otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanEnPassant(Player us, Square epSquare, bool moved = true)
    {
        Debug.Assert(epSquare.IsOk);
        Debug.Assert(epSquare.RelativeRank(us) != Rank.Rank3);

        var them = ~us;

        if (moved && !(Pieces(PieceType.Pawn, ~us).Contains(epSquare + them.PawnPushDistance()) &&
                       Board.IsEmpty(epSquare) && Board.IsEmpty(epSquare + us.PawnPushDistance())))
            return false;

        // En-passant attackers
        var attackers = Pieces(PieceType.Pawn, us) & epSquare.PawnAttack(them);

        Debug.Assert(attackers.Count <= 2);

        if (attackers.IsEmpty)
            return false;

        var cap = moved ? epSquare - us.PawnPushDistance() : epSquare + us.PawnPushDistance();
        Debug.Assert(Board.PieceAt(cap) == PieceType.Pawn.MakePiece(them));

        var ksq = Board.Square(PieceType.King, us);
        var bq = Pieces(PieceType.Bishop, PieceType.Queen, them) & GetAttacks(ksq, PieceType.Bishop);
        var rq = Pieces(PieceType.Rook, PieceType.Queen, them) & GetAttacks(ksq, PieceType.Rook);

        var mocc = (Pieces() ^ cap) | epSquare;

        while (attackers)
        {
            var sq = BitBoards.PopLsb(ref attackers);
            var amocc = mocc ^ sq;
            // Check en-passant is legal for the position
            if (
                (bq.IsEmpty || (bq & GetAttacks(ksq, PieceType.Bishop, in amocc)).IsEmpty) &&
                (rq.IsEmpty || (rq & GetAttacks(ksq, PieceType.Rook, in amocc)).IsEmpty))
                return true;
        }

        return false;
    }
}