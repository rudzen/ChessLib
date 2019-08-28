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
    using Enums;
    using System.Runtime.CompilerServices;
    using Types;

    public sealed class MoveGenerator
    {
        private readonly IPosition _position;

        public MoveGenerator(IPosition position, bool generate = true, bool force = false)
        {
            _position = position;
            Moves = new MoveList();
            if (generate)
                GenerateMoves(force);
        }

        public Emgf Flags { get; set; } = Emgf.Legalmoves;

        public MoveList Moves { get; private set; }

        public void GenerateMoves(bool force = false)
        {
            // Align current structure to position.
            Reset(MoveExtensions.EmptyMove);

            // this is only preparation for true engine integration (not used yet)
            //if (Flags.HasFlagFast(Emgf.Stages))
            //    return;

            GenerateCapturesAndPromotions();
            GenerateQuietMoves();
        }

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

        private void GenerateCapturesAndPromotions()
        {
            var currentSide = _position.State.SideToMove;
            var them = ~currentSide;
            var occupiedByThem = _position.OccupiedBySide[them.Side];
            var (northEast, northWest) = currentSide == PlayerExtensions.White ? (EDirection.NorthEast, EDirection.NorthWest) : (EDirection.SouthEast, EDirection.SouthWest);

            AddMoves(occupiedByThem);

            var pawns = _position.Pieces(EPieceType.Pawn, currentSide);

            AddPawnMoves(currentSide.PawnPush(pawns & currentSide.Rank7()) & ~_position.Pieces(), currentSide.PawnPushDistance(), EMoveType.Quiet);
            AddPawnMoves(pawns.Shift(northEast) & occupiedByThem, currentSide.PawnWestAttackDistance(), EMoveType.Capture);
            AddPawnMoves(pawns.Shift(northWest) & occupiedByThem, currentSide.PawnEastAttackDistance(), EMoveType.Capture);

            if (_position.State.EnPassantSquare == ESquare.none)
                return;

            AddPawnMoves(pawns.Shift(northEast) & _position.State.EnPassantSquare, currentSide.PawnWestAttackDistance(), EMoveType.Epcapture);
            AddPawnMoves(pawns.Shift(northWest) & _position.State.EnPassantSquare, currentSide.PawnEastAttackDistance(), EMoveType.Epcapture);
        }

        private void GenerateQuietMoves()
        {
            var currentSide = _position.State.SideToMove;
            var up = currentSide == PlayerExtensions.White ? EDirection.North : EDirection.South;
            var notOccupied = ~_position.Pieces();
            var pushed = (_position.Pieces(EPieceType.Pawn, currentSide) & ~currentSide.Rank7()).Shift(up) & notOccupied;
            AddPawnMoves(pushed, currentSide.PawnPushDistance(), EMoveType.Quiet);

            pushed &= currentSide.Rank3();
            AddPawnMoves(pushed.Shift(up) & notOccupied, currentSide.PawnDoublePushDistance(), EMoveType.Doublepush);

            AddMoves(notOccupied);

            if (_position.InCheck)
                return;

            for (var castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++)
                if (_position.CanCastle(castleType))
                    AddCastleMove(_position.GetKingCastleFrom(currentSide, castleType), castleType.GetKingCastleTo(currentSide));
        }

        /// <summary>
        /// Move generation leaf method.
        /// Constructs the actual move based on the arguments.
        /// </summary>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="promoted">The promotion piece (if any, defaults to NoPiece type)</param>
        /// <param name="type">The move type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMove(Piece piece, Square from, Square to, Piece promoted, EMoveType type = EMoveType.Quiet)
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

            Moves += move;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMoves(Piece piece, Square from, BitBoard attacks)
        {
            var target = _position.Pieces(~_position.State.SideToMove) & attacks;
            while (target)
            {
                var to = target.Lsb();
                AddMove(piece, from, to, EPieces.NoPiece, EMoveType.Capture);
                BitBoards.ResetLsb(ref target);
            }

            target = ~_position.Pieces() & attacks;

            while (target)
            {
                var to = target.Lsb();
                AddMove(piece, from, to, PieceExtensions.EmptyPiece);
                BitBoards.ResetLsb(ref target);
            }
        }

        /// <summary>
        /// Iterates through the piece types and generates moves based on their attacks.
        /// It does not contain any checks for moves that are invalid, as the leaf methods
        /// contains implicit denial of move generation if the target bitboard is empty.
        /// </summary>
        /// <param name="targetSquares">The target squares to move to</param>
        private void AddMoves(BitBoard targetSquares)
        {
            var c = _position.State.SideToMove;

            var occupied = _position.Pieces();

            for (var pt = EPieceType.King; pt >= EPieceType.Knight; --pt)
            {
                var pc = pt.MakePiece(c);
                var pieces = _position.Pieces(pc);

                while (pieces)
                {
                    var from = pieces.Lsb();
                    AddMoves(pc, from, from.GetAttacks(pt, occupied) & targetSquares);
                    BitBoards.ResetLsb(ref pieces);
                }
            }
        }

        private void AddPawnMoves(BitBoard targetSquares, Direction direction, EMoveType type)
        {
            if (targetSquares.Empty())
                return;

            var stm = _position.State.SideToMove;
            var piece = EPieceType.Pawn.MakePiece(stm);

            var promotionRank = stm.PromotionRank();
            var promotionSquares = targetSquares & promotionRank;
            var nonPromotionSquares = targetSquares & ~promotionRank;

            while (nonPromotionSquares)
            {
                var sqTo = nonPromotionSquares.Lsb();
                var sqFrom = sqTo - direction;
                AddMove(piece, sqFrom, sqTo, PieceExtensions.EmptyPiece, type);
                BitBoards.ResetLsb(ref nonPromotionSquares);
            }

            type |= EMoveType.Promotion;

            while (promotionSquares)
            {
                var sqTo = promotionSquares.Lsb();
                var sqFrom = sqTo - direction;
                if (Flags.HasFlagFast(Emgf.Queenpromotion))
                    AddMove(piece, sqFrom, sqTo, EPieceType.Queen.MakePiece(stm), type);
                else
                    for (var promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
                        AddMove(piece, sqFrom, sqTo, promotedPiece.MakePiece(stm), type);

                BitBoards.ResetLsb(ref promotionSquares);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCastleMove(Square from, Square to)
            => AddMove(EPieceType.King.MakePiece(_position.State.SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);
    }
}