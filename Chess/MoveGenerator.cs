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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using Enums;
    using Properties;
    using Types;

    public class MoveGenerator : IMoveGenerator
    {
        [NotNull]
        public readonly ChessBoard ChessBoard;

        private static readonly ConcurrentDictionary<ulong, List<Move>> Table;

        private readonly Func<BitBoard, BitBoard>[] _pawnPush = { BitBoards.NorthOne, BitBoards.SouthOne };

        // to be replaced
        private readonly Func<BitBoard, BitBoard>[] _pawnAttacksWest = { BitBoards.NorthEastOne, BitBoards.SouthEastOne };

        // to be replaced
        private readonly Func<BitBoard, BitBoard>[] _pawnAttacksEast = { BitBoards.NorthWestOne, BitBoards.SouthWestOne };

        private BitBoard[] _bitboardPieces;

        private BitBoard[] _occupiedBySide;

        private BitBoard _occupied;

        static MoveGenerator()
        {
            Table = new ConcurrentDictionary<ulong, List<Move>>();
            
            // Force cleaning when process exists
            AppDomain.CurrentDomain.ProcessExit += Clean;
        }

        protected MoveGenerator(ChessBoard chessBoard)
        {
            ChessBoard = chessBoard;
            _bitboardPieces = chessBoard.BoardPieces;
            _occupiedBySide = chessBoard.OccupiedBySide;
        }

        public int CastlelingRights { get; set; }

        public bool InCheck { get; set; }

        public BitBoard EnPassantSquare { get; set; }

        public Player SideToMove { get; set; }

        public BitBoard Pinned { get; set; }

        public ulong Key { get; internal set; }

        public Emgf Flags { get; set; } = Emgf.Legalmoves;

        public IList<Move> Moves { get; private set; }
        
        public static void ClearMoveCache()
        {
            Table.Clear();
        }

        public void GenerateMoves(bool force = false)
        {
            // Align current structure to chessboard.
            Reset(MoveExtensions.EmptyMove);

            // this is only preparation for true engine integration (not used yet)
            if ((Flags & Emgf.Stages) != 0)
                return;

            if (Table.ContainsKey(Key) && !force) {
                Moves = Table[Key];
            } else {
                List<Move> moves = new List<Move>(256);

                // relax the gc while generating moves.
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                GenerateCapturesAndPromotions(moves);
                GenerateQuietMoves(moves);
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
                Moves = moves;
                Table.TryAdd(Key, moves);
            }
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
            if (!InCheck && piece.Type() != EPieceType.King && (Pinned & from).Empty() && (type & EMoveType.Epcapture) == 0)
                return true;

            ChessBoard.IsProbing = true;
            ChessBoard.MakeMove(move);
            bool opponentAttacking = ChessBoard.IsAttacked(ChessBoard.KingSquares[SideToMove.Side], ~SideToMove);
            ChessBoard.TakeMove(move);
            ChessBoard.IsProbing = false;
            return !opponentAttacking;
        }

        /// <summary>
        /// <para>"Validates" a move basic on simple logic. For example if a piece actually exists thats being moves etc.</para>
        /// <para>This is basicly only useful while developing and/or debugging</para>
        /// </summary>
        /// <param name="move">The move to check for logical errors</param>
        /// <returns>True if move "appears" to be legal, otherwise false</returns>
        public bool IsPseudoLegal(Move move)
        {
            // Verify that the piece actually exists on the board at the location defined by the move struct
            if ((_bitboardPieces[move.GetMovingPiece().ToInt()] & move.GetFromSquare()).Empty())
                return false;

            Square to = move.GetToSquare();

            if (move.IsCastlelingMove()) {
                // TODO : Basic castleling verification
            } else if (move.IsEnPassantMove()) {
                Player opponent = ~move.GetMovingSide();

                // TODO : En-passant here
            } else if (move.IsCaptureMove()) {
                Player opponent = ~move.GetMovingSide();
                if ((_occupiedBySide[opponent.Side] & to).Empty())
                    return false;

                if ((_bitboardPieces[move.GetCapturedPiece().ToInt()] & to).Empty())
                    return false;
            } else if ((_occupied & to) != 0) {
                return false;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (move.GetMovingPiece().Type()) {
                case EPieceType.Bishop:
                case EPieceType.Rook:
                case EPieceType.Queen:
                    if (move.GetFromSquare().BitboardBetween(to) & _occupied)
                        return false;

                    break;
            }

            return true;
        }

        /// <summary>
        /// Class clean up method, to be expanded at some point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private static void Clean(object sender, EventArgs ea)
        {
            ClearMoveCache();
        }
        
        /// <summary>
        /// Aligns the current positional data to the chessboards data.
        /// </summary>
        /// <param name="move">Not used atm, but for future library completion</param>
        private void Reset(Move move)
        {
            Emgf flags = Flags;

            // not being used in the current version
            if (!move.IsNullMove() && (move.IsCastlelingMove() || move.IsEnPassantMove()))
                flags &= ~Emgf.Stages;

            if ((flags & Emgf.Legalmoves) != 0)
                Pinned = ChessBoard.GetPinnedPieces(ChessBoard.KingSquares[SideToMove.Side], SideToMove);

            _occupied = ChessBoard.Occupied;
            _occupiedBySide = ChessBoard.OccupiedBySide;
            _bitboardPieces = ChessBoard.BoardPieces;

            Flags = flags;
        }

        private void GenerateCapturesAndPromotions(ICollection<Move> moves)
        {
            Player currentSide = SideToMove;
            Player them = ~currentSide;
            BitBoard occupiedByThem = _occupiedBySide[them.Side];

            AddMoves(moves, occupiedByThem);

            BitBoard pawns = ChessBoard.Pawns(currentSide);

            AddPawnMoves(moves, _pawnPush[currentSide.Side](pawns & currentSide.Rank7()) & ~_occupied, currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(moves, _pawnAttacksWest[currentSide.Side](pawns) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture);
            AddPawnMoves(moves, _pawnAttacksEast[currentSide.Side](pawns) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture);
            AddPawnMoves(moves, _pawnAttacksWest[currentSide.Side](pawns) & EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture);
            AddPawnMoves(moves, _pawnAttacksEast[currentSide.Side](pawns) & EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture);
        }

        private void GenerateQuietMoves(ICollection<Move> moves)
        {
            Player currentSide = SideToMove;
            if (!InCheck)
                for (ECastleling castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++) {
                    if (CanCastle(castleType))
                        AddCastleMove(moves, ChessBoard.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide));
                }

            BitBoard notOccupied = ~_occupied;
            BitBoard pushed = _pawnPush[currentSide.Side](ChessBoard.Pawns(currentSide).Value & ~currentSide.Rank7()) & notOccupied;
            AddPawnMoves(moves, pushed.Value, currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(moves, _pawnPush[currentSide.Side](pushed.Value & currentSide.Rank3()) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush);
            AddMoves(moves, notOccupied);
        }

        /// <summary>
        /// Move generation leaf method.
        /// Constructs the actual move based on the arguments.
        /// </summary>
        /// <param name="moves">The move list to add the generated (if any) moves into</param>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="promoted">The promotion piece (if any, defaults to NoPiece type)</param>
        /// <param name="type">The move type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMove(ICollection<Move> moves, Piece piece, Square from, Square to, Piece promoted, EMoveType type = EMoveType.Quiet)
        {
            Move move;

            if ((type & EMoveType.Capture) != 0)
                move = new Move(piece, ChessBoard.GetPiece(to), from, to, type, promoted);
            else if ((type & EMoveType.Epcapture) != 0)
                move = new Move(piece, (int)EPieceType.Pawn | (~SideToMove << 3), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actualy a legal move if the flag is enabled
            if ((Flags & Emgf.Legalmoves) != 0 && !IsLegal(move, piece, from, type))
                return;

            moves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMoves(ICollection<Move> moves, Piece piece, Square from, BitBoard attacks)
        {
            foreach (Square to in attacks) {
                if (ChessBoard.GetPiece(to) == EPieces.NoPiece)
                    AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece);
                else
                    AddMove(moves, piece, from, to, EPieces.NoPiece, EMoveType.Capture);
            }
        }

        /// <summary>
        /// Iterates through the piece types and generates moves based on their attacks.
        /// It does not contain any checks for moves that are invalid, as the leaf methods
        /// contains implicit denial of move generation if the target bitboard is empty.
        /// </summary>
        /// <param name="moves">The move list to add potential moves to.</param>
        /// <param name="targetSquares">The target squares to move to</param>
        private void AddMoves(ICollection<Move> moves, BitBoard targetSquares)
        {
            Player c = SideToMove;
            BitBoard occupied = ChessBoard.Occupied;

            // TODO : More generic way of adding moves?
            Piece piece = EPieceType.Queen.MakePiece(c);
            foreach (Square from in _bitboardPieces[piece.ToInt()])
                AddMoves(moves, piece, from, from.GetAttacks(EPieceType.Queen, occupied) & targetSquares);

            piece = EPieceType.Rook.MakePiece(c);
            foreach (Square from in _bitboardPieces[piece.ToInt()])
                AddMoves(moves, piece, from, from.GetAttacks(EPieceType.Rook, occupied) & targetSquares);

            piece = EPieceType.Bishop.MakePiece(c);
            foreach (Square from in _bitboardPieces[piece.ToInt()])
                AddMoves(moves, piece, from, from.GetAttacks(EPieceType.Bishop, occupied) & targetSquares);

            piece = EPieceType.Knight.MakePiece(c);
            foreach (Square square in _bitboardPieces[piece.ToInt()])
                AddMoves(moves, piece, square, square.GetAttacks(EPieceType.Knight) & targetSquares);

            piece = EPieceType.King.MakePiece(c);
            foreach (Square square in _bitboardPieces[piece.ToInt()])
                AddMoves(moves, piece, square, square.GetAttacks(EPieceType.King) & targetSquares);
        }

        private void AddPawnMoves(ICollection<Move> moves, BitBoard targetSquares, Direction direction, EMoveType type)
        {
            if (targetSquares.Empty())
                return;

            Piece piece = EPieceType.Pawn.MakePiece(SideToMove);

            foreach (Square squareTo in targetSquares) {
                Square squareFrom = squareTo - direction;
                if (!squareTo.IsPromotionRank()) {
                    AddMove(moves, piece, squareFrom, squareTo, PieceExtensions.EmptyPiece, type);
                    continue;
                }

                if ((Flags & Emgf.Queenpromotion) != 0) {
                    AddMove(moves, piece, squareFrom, squareTo, EPieceType.Queen.MakePiece(SideToMove), type | EMoveType.Promotion);
                    return;
                }

                for (EPieceType promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                    AddMove(moves, piece, squareFrom, squareTo, promotedPiece.MakePiece(SideToMove), type | EMoveType.Promotion);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCastleMove(ICollection<Move> moves, Square from, Square to) => AddMove(moves, EPieceType.King.MakePiece(SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanCastle(ECastleling type)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type) {
                case ECastleling.Short:
                case ECastleling.Long:
                    return (CastlelingRights & type.GetCastleAllowedMask(SideToMove)) != 0 && IsCastleAllowed(type.GetKingCastleTo(SideToMove));
                default:
                    throw new ArgumentException("Illegal castleling type.");
            }
        }

        private bool IsCastleAllowed(Square square)
        {
            Player c = SideToMove;
             // The complexity of this function is mainly due to the support for Chess960 variant.
            Square rookTo = square.GetRookCastleTo();
            Square rookFrom = ChessBoard.GetRookCastleFrom(square);
            Square kingSquare = ChessBoard.KingSquares[c.Side];

            // The pieces in question.. rook and king
            BitBoard castlePieces = rookFrom | kingSquare;

            // The span between the rook and the king
            BitBoard castleSpan = castlePieces | rookTo;
            castleSpan |= square;
            castleSpan |= kingSquare.BitboardBetween(rookFrom);
            castleSpan |= rookFrom.BitboardBetween(rookTo);

            // check that the span AND current occupied pieces are no different that the piece themselves.
            if ((castleSpan & _occupied) != castlePieces)
                return false;

            // Check that no square between the king's initial and final squares (including the initial and final squares)
            // may be under attack by an enemy piece. Initial square was already checked a this point.

            c = ~c;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Square s in kingSquare.BitboardBetween(square) | square) {
                if (ChessBoard.IsAttacked(s, c))
                    return false;
            }

            return true;
        }
    }
}