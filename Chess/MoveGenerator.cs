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
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveGenerator
    {
        private static readonly ConcurrentDictionary<ulong, List<Move>> Table;

        private readonly Position _position;

        static MoveGenerator()
        {
            Table = new ConcurrentDictionary<ulong, List<Move>>();

            // Force cleaning when process exists
            AppDomain.CurrentDomain.ProcessExit += Clean;
        }

        public MoveGenerator(Position position)
        {
            EnsureArg.IsNotNull(position, nameof(position));
            _position = position;
        }

        public Emgf Flags { get; set; } = Emgf.Legalmoves;

        public List<Move> Moves { get; private set; }

        public static void ClearMoveCache() => Table.Clear();

        public void GenerateMoves(bool force = false)
        {
            // Align current structure to position.
            Reset(MoveExtensions.EmptyMove);

            // this is only preparation for true engine integration (not used yet)
            if (Flags.HasFlagFast(Emgf.Stages))
                return;

            if (Table.TryGetValue(_position.State.Key, out var moves) && !force)
                Moves = moves;
            else
            {
                moves = new List<Move>(256);
                // relax the gc while generating moves.
                //var old = GCSettings.LatencyMode;
                GCSettings.LatencyMode = GCLatencyMode.Batch;
                GenerateCapturesAndPromotions(moves);
                GenerateQuietMoves(moves);
                Table.TryAdd(_position.State.Key, moves);
                Moves = moves;
                GCSettings.LatencyMode = GCLatencyMode.Interactive;
            }
        }

        /// <summary>
        /// <para>"Validates" a move using simple logic. For example that the piece actually being moved exists etc.</para>
        /// <para>This is basically only useful while developing and/or debugging</para>
        /// </summary>
        /// <param name="move">The move to check for logical errors</param>
        /// <returns>True if move "appears" to be legal, otherwise false</returns>
        public bool IsPseudoLegal(Move move)
        {
            // Verify that the piece actually exists on the board at the location defined by the move struct
            if ((_position.BoardPieces[move.GetMovingPiece().ToInt()] & move.GetFromSquare()).Empty())
                return false;

            var to = move.GetToSquare();

            if (move.IsCastlelingMove())
            {
                // TODO : Basic castleling verification
                if (_position.CanCastle(move.GetFromSquare() < to ? ECastleling.Short : ECastleling.Long))
                    return true;

                var mg = new MoveGenerator(_position);
                mg.GenerateMoves();
                return mg.Moves.Contains(move);
            }
            else if (move.IsEnPassantMove())
            {
                // TODO : En-passant here

                // TODO : Test with unit test
                var opponent = ~move.GetMovingSide();
                if (_position.State.EnPassantSquare.PawnAttack(opponent) & _position.Pieces(EPieceType.Pawn, opponent))
                    return true;
            }
            else if (move.IsCaptureMove())
            {
                var opponent = ~move.GetMovingSide();
                if ((_position.OccupiedBySide[opponent.Side] & to).Empty())
                    return false;

                if ((_position.BoardPieces[move.GetCapturedPiece().ToInt()] & to).Empty())
                    return false;
            }
            else if ((_position.Occupied & to) != 0)
                return false;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (move.GetMovingPiece().Type())
            {
                case EPieceType.Bishop:
                case EPieceType.Rook:
                case EPieceType.Queen:
                    if (move.GetFromSquare().BitboardBetween(to) & _position.Occupied)
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
        private static void Clean(object sender, EventArgs ea) => ClearMoveCache();

        /// <summary>
        /// Aligns the current positional data to the position data.
        /// </summary>
        /// <param name="move">Not used atm, but for future library completion</param>
        private void Reset(Move move)
        {
            var flags = Flags;

            // not being used in the current version
            if (!move.IsNullMove() && (move.IsCastlelingMove() || move.IsEnPassantMove()))
                flags &= ~Emgf.Stages;

            //_position.State.Pinned = flags.HasFlagFast(Emgf.Legalmoves)
            //    ? _position.GetPinnedPieces(_position.GetPieceSquare(EPieceType.King, _position.State.SideToMove), _position.State.SideToMove)
            //    : BitBoards.EmptyBitBoard;

            Flags = flags;
        }

        private void GenerateCapturesAndPromotions(ICollection<Move> moves)
        {
            var currentSide = _position.State.SideToMove;
            var them = ~currentSide;
            var occupiedByThem = _position.OccupiedBySide[them.Side];
            var (northEast, northWest) = currentSide == PlayerExtensions.White ? (EDirection.NorthEast, EDirection.NorthWest) : (EDirection.SouthEast, EDirection.SouthWest);

            AddMoves(moves, occupiedByThem);

            var pawns = _position.Pieces(EPieceType.Pawn, currentSide);

            AddPawnMoves(moves, currentSide.PawnPush(pawns & currentSide.Rank7()) & ~_position.Occupied, currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(moves, pawns.Shift(northEast) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture);
            AddPawnMoves(moves, pawns.Shift(northWest) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture);

            if (_position.State.EnPassantSquare == ESquare.none)
                return;

            AddPawnMoves(moves, pawns.Shift(northEast) & _position.State.EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture);
            AddPawnMoves(moves, pawns.Shift(northWest) & _position.State.EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture);
        }

        private void GenerateQuietMoves(ICollection<Move> moves)
        {
            var currentSide = _position.State.SideToMove;
            var up = currentSide == PlayerExtensions.White ? EDirection.North : EDirection.South;
            var notOccupied = ~_position.Occupied;
            var pushed = (_position.Pieces(EPieceType.Pawn, currentSide) & ~currentSide.Rank7()).Shift(up) & notOccupied;
            AddPawnMoves(moves, pushed, currentSide.PawnPushDistance(), EMoveType.Quiet);

            pushed &= currentSide.Rank3();
            AddPawnMoves(moves, pushed.Shift(up) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush);

            AddMoves(moves, notOccupied);

            if (_position.InCheck)
                return;

            for (var castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++)
                if (_position.CanCastle(castleType))
                    AddCastleMove(moves, _position.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide));
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

            if (type.HasFlagFast(EMoveType.Capture))
                move = new Move(piece, _position.GetPiece(to), from, to, type, promoted);
            else if (type.HasFlagFast(EMoveType.Epcapture))
                move = new Move(piece, EPieceType.Pawn.MakePiece(~_position.State.SideToMove), from, to, type, promoted);
            else
                move = new Move(piece, from, to, type, promoted);

            // check if move is actual a legal move if the flag is enabled
            if (Flags.HasFlagFast(Emgf.Legalmoves) && !_position.IsLegal(move, piece, from, type))
                return;

            moves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMoves(ICollection<Move> moves, Piece piece, Square from, BitBoard attacks)
        {
            var target = _position.Occupied & attacks;
            foreach (var to in target)
                AddMove(moves, piece, from, to, EPieces.NoPiece, EMoveType.Capture);

            target = ~_position.Occupied & attacks;
            foreach (var to in target)
                AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece);
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
            var c = _position.State.SideToMove;

            var occupied = _position.Occupied;

            for (var pt = EPieceType.King; pt >= EPieceType.Knight; --pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = _position.BoardPieces[pc.ToInt()];
                foreach (var from in pieces)
                    AddMoves(moves, pc, from, from.GetAttacks(pt, occupied) & targetSquares);
            }
        }

        private void AddPawnMoves(ICollection<Move> moves, BitBoard targetSquares, Direction direction, EMoveType type)
        {
            if (targetSquares.Empty())
                return;

            var piece = EPieceType.Pawn.MakePiece(_position.State.SideToMove);

            foreach (var squareTo in targetSquares)
            {
                var squareFrom = squareTo - direction;
                if (squareTo.IsPromotionRank())
                {
                    if (Flags.HasFlagFast(Emgf.Queenpromotion))
                        AddMove(moves, piece, squareFrom, squareTo, EPieceType.Queen.MakePiece(_position.State.SideToMove),
                            type | EMoveType.Promotion);
                    else
                        for (var promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                            AddMove(moves, piece, squareFrom, squareTo, promotedPiece.MakePiece(_position.State.SideToMove),
                                type | EMoveType.Promotion);
                }
                else
                    AddMove(moves, piece, squareFrom, squareTo, PieceExtensions.EmptyPiece, type);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCastleMove(ICollection<Move> moves, Square from, Square to) => AddMove(moves, EPieceType.King.MakePiece(_position.State.SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);
    }
}