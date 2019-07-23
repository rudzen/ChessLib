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
    using EnsureThat;
    using Enums;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveGenerator
    {
        private static readonly ConcurrentDictionary<ulong, MoveList> Table = new ConcurrentDictionary<ulong, MoveList>();

        private readonly IPosition _position;

        public MoveGenerator(IPosition position, bool generate = true, bool force = false)
        {
            EnsureArg.IsNotNull(position, nameof(position));
            _position = position;
            if (generate)
                GenerateMoves(force);
        }

        public Emgf Flags { get; set; } = Emgf.Legalmoves;

        public MoveList Moves { get; private set; }

        public static void ClearMoveCache() => Table.Clear();

        public void GenerateMoves(bool force = false)
        {
            // Align current structure to position.
            Reset(MoveExtensions.EmptyMove);

            // this is only preparation for true engine integration (not used yet)
            //if (Flags.HasFlagFast(Emgf.Stages))
            //    return;

            if (Table.TryGetValue(_position.State.Key, out var moves) && !force)
                Moves = moves;
            else
            {
                moves = new MoveList();
                // relax the gc while generating moves.
                var old = GCSettings.LatencyMode;
                GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                moves = GenerateCapturesAndPromotions(moves);
                moves = GenerateQuietMoves(moves);
                Table.TryAdd(_position.State.Key, moves);
                Moves = moves;
                GCSettings.LatencyMode = old;
            }
        }

        /// <summary>
        /// Class clean up method, to be expanded at some point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private static void Clean(object sender, EventArgs ea) => ClearMoveCache();

        /// <summary>
        /// Aligns the current positional data to the position data.
        /// </summary>
        /// <param name="move">Not used atm, but for future library completion</param>
        private void Reset(Move move)
        {
            // not being used in the current version
            //if (!move.IsNullMove() && (move.IsCastlelingMove() || move.IsEnPassantMove()))
            //    Flags &= ~Emgf.Stages;

            _position.State.Pinned = Flags.HasFlagFast(Emgf.Legalmoves)
                ? _position.GetPinnedPieces(_position.GetPieceSquare(EPieceType.King, _position.State.SideToMove), _position.State.SideToMove)
                : BitBoards.EmptyBitBoard;
        }

        private MoveList GenerateCapturesAndPromotions(MoveList moves)
        {
            var currentSide = _position.State.SideToMove;
            var them = ~currentSide;
            var occupiedByThem = _position.OccupiedBySide[them.Side];
            var (northEast, northWest) = currentSide == PlayerExtensions.White ? (EDirection.NorthEast, EDirection.NorthWest) : (EDirection.SouthEast, EDirection.SouthWest);

            moves = AddMoves(moves, occupiedByThem);

            var pawns = _position.Pieces(EPieceType.Pawn, currentSide);

            moves = AddPawnMoves(moves, currentSide.PawnPush(pawns & currentSide.Rank7()) & ~_position.Pieces(), currentSide.PawnPushDistance(), EMoveType.Quiet);
            moves = AddPawnMoves(moves, pawns.Shift(northEast) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture);
            moves = AddPawnMoves(moves, pawns.Shift(northWest) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture);

            if (_position.State.EnPassantSquare == ESquare.none)
                return moves;

            moves = AddPawnMoves(moves, pawns.Shift(northEast) & _position.State.EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture);
            moves = AddPawnMoves(moves, pawns.Shift(northWest) & _position.State.EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture);

            return moves;
        }

        private MoveList GenerateQuietMoves(MoveList moves)
        {
            var currentSide = _position.State.SideToMove;
            var up = currentSide == PlayerExtensions.White ? EDirection.North : EDirection.South;
            var notOccupied = ~_position.Pieces();
            var pushed = (_position.Pieces(EPieceType.Pawn, currentSide) & ~currentSide.Rank7()).Shift(up) & notOccupied;
            moves = AddPawnMoves(moves, pushed, currentSide.PawnPushDistance(), EMoveType.Quiet);

            pushed &= currentSide.Rank3();
            moves = AddPawnMoves(moves, pushed.Shift(up) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush);

            moves = AddMoves(moves, notOccupied);

            if (_position.InCheck)
                return moves;

            for (var castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++)
                if (_position.CanCastle(castleType))
                    moves = AddCastleMove(moves, _position.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide));

            return moves;
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
        private MoveList AddMove(MoveList moves, Piece piece, Square from, Square to, Piece promoted, EMoveType type = EMoveType.Quiet)
        {
            Move move;

            if (type.HasFlagFast(EMoveType.Capture))
                move = new Move(piece, _position.GetPiece(to), from, to, type, promoted);
            else if (type.HasFlagFast(EMoveType.Epcapture))
                move = new Move(piece, EPieceType.Pawn.MakePiece(~_position.State.SideToMove), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actual a legal move if the flag is enabled
            if (Flags.HasFlagFast(Emgf.Legalmoves) && !_position.IsLegal(move, piece, from, type))
                return moves;

            moves += move;
            return moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList AddMoves(MoveList moves, Piece piece, Square from, BitBoard attacks)
        {
            var target = _position.Pieces(~_position.State.SideToMove) & attacks;
            foreach (var to in target)
                moves = AddMove(moves, piece, from, to, EPieces.NoPiece, EMoveType.Capture);

            target = ~_position.Pieces() & attacks;
            foreach (var to in target)
                moves = AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece);

            return moves;
        }

        /// <summary>
        /// Iterates through the piece types and generates moves based on their attacks.
        /// It does not contain any checks for moves that are invalid, as the leaf methods
        /// contains implicit denial of move generation if the target bitboard is empty.
        /// </summary>
        /// <param name="moves">The move list to add potential moves to.</param>
        /// <param name="targetSquares">The target squares to move to</param>
        private MoveList AddMoves(MoveList moves, BitBoard targetSquares)
        {
            var c = _position.State.SideToMove;

            var occupied = _position.Pieces();

            for (var pt = EPieceType.King; pt >= EPieceType.Knight; --pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = _position.Pieces(pc);
                foreach (var from in pieces)
                    moves = AddMoves(moves, pc, from, from.GetAttacks(pt, occupied) & targetSquares);
            }

            return moves;
        }

        private MoveList AddPawnMoves(MoveList moves, BitBoard targetSquares, Direction direction, EMoveType type)
        {
            if (targetSquares.Empty())
                return moves;

            var stm = _position.State.SideToMove;
            var piece = EPieceType.Pawn.MakePiece(stm);

            var promotionRank = stm.PromotionRank();
            var promotionSquares = targetSquares & promotionRank;
            var nonPromotionSquares = targetSquares & ~promotionRank;

            foreach (var sqTo in nonPromotionSquares)
            {
                var sqFrom = sqTo - direction;
                moves = AddMove(moves, piece, sqFrom, sqTo, PieceExtensions.EmptyPiece, type);
            }

            type |= EMoveType.Promotion;

            foreach (var sqTo in promotionSquares)
            {
                var sqFrom = sqTo - direction;
                if (Flags.HasFlagFast(Emgf.Queenpromotion))
                    AddMove(moves, piece, sqFrom, sqTo, EPieceType.Queen.MakePiece(stm), type);
                else
                    for (var promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                        AddMove(moves, piece, sqFrom, sqTo, promotedPiece.MakePiece(stm), type);
            }

            return moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList AddCastleMove(MoveList moves, Square from, Square to)
        {
            moves = AddMove(moves, EPieceType.King.MakePiece(_position.State.SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);
            return moves;
        }
    }
}