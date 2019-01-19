//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using Rudz.Chess.Enums;
//using Rudz.Chess.Types;

//namespace Rudz.Chess
//{
//    public sealed class MG
//    {

//        private readonly Position Position;

//        private readonly Player SideToMove;

//        private readonly Emgf Flags;

//        private readonly BitBoard EnPassantSquare;

//        public MG(Position position, Emgf flags, BitBoard enPassantSquare)
//        {
//            Position = position;
//            Flags = flags;
//            EnPassantSquare = enPassantSquare;
//        }


//        private void GenerateCapturesAndPromotions(ICollection<Move> moves)
//        {
//            Player us = SideToMove;
//            Player them = ~us;
//            BitBoard occupiedByThem = Position.OccupiedBySide[them.Side];

//            AddMoves(moves, occupiedByThem);

//            BitBoard pawns = Position.Pieces(EPieceType.Pawn, us);
//            BitBoard targetSquares = pawns.GetPawnAttacks(us);

//            // pawn capture moves
//            AddPawnMoves(moves, pawns, targetSquares & occupiedByThem, EMoveType.Capture);

//            // pawn quiet moves
//            AddPawnMoves(moves, pawns, us.PawnPush(pawns), EMoveType.Quiet);

//            // pawn en-passant moves
//            AddPawnMoves(moves, pawns, targetSquares & EnPassantSquare, EMoveType.Epcapture);

//            //AddPawnMoves(moves, us.PawnPush(pawns & us.Rank7()) & ~Position.Occupied, us.PawnPushDistance(), EMoveType.Quiet);
//            //AddPawnMoves(moves, _pawnAttacksWest[us.Side](pawns) & occupiedByThem, us.PawnWestAttackDistance(), EMoveType.Capture);
//            //AddPawnMoves(moves, _pawnAttacksEast[us.Side](pawns) & occupiedByThem, us.PawnEastAttackDistance(), EMoveType.Capture);
//            //AddPawnMoves(moves, _pawnAttacksWest[us.Side](pawns) & EnPassantSquare, us.PawnWestAttackDistance(), EMoveType.Epcapture);
//            //AddPawnMoves(moves, _pawnAttacksEast[us.Side](pawns) & EnPassantSquare, us.PawnEastAttackDistance(), EMoveType.Epcapture);
//        }

//        private void GenerateQuietMoves(ICollection<Move> moves)
//        {
//            Player us = SideToMove;
//            if (!InCheck)
//                for (ECastleling castleType = ECastleling.Short; castleType < ECastleling.CastleNb; castleType++)
//                {
//                    if (CanCastle(castleType))
//                        AddCastleMove(moves, Position.GetKingCastleFrom(us, castleType), castleType.GetKingCastleTo(us));
//                }

//            BitBoard notOccupied = ~_occupied;
//            BitBoard pushed = us.PawnPush(Position.Pieces(EPieceType.Pawn, us).Value & ~us.Rank7()) & notOccupied;
//            AddPawnMoves(moves, pushed.Value, us.PawnPushDistance(), EMoveType.Quiet);
//            AddPawnMoves(moves, us.PawnPush(pushed.Value & us.Rank3()) & notOccupied, us.PawnDoublePushDistance(), EMoveType.Doublepush);
//            AddMoves(moves, notOccupied);
//        }

//        /// <summary>
//        /// Move generation leaf method.
//        /// Constructs the actual move based on the arguments.
//        /// </summary>
//        /// <param name="moves">The move list to add the generated (if any) moves into</param>
//        /// <param name="piece">The moving piece</param>
//        /// <param name="from">The from square</param>
//        /// <param name="to">The to square</param>
//        /// <param name="promoted">The promotion piece (if any, defaults to NoPiece type)</param>
//        /// <param name="type">The move type</param>
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private void AddMove(ICollection<Move> moves, Piece piece, Square from, Square to, Piece promoted, EMoveType type = EMoveType.Quiet)
//        {
//            Move move;

//            if (type.HasFlagFast(EMoveType.Capture))
//                move = new Move(piece, Position.GetPiece(to), from, to, type, promoted);
//            else if (type.HasFlagFast(EMoveType.Epcapture))
//                move = new Move(piece, EPieceType.Pawn.MakePiece(~SideToMove), from, to, type, promoted);
//            else
//                move = new Move(piece, from, to, type, promoted);

//            // check if move is actualy a legal move if the flag is enabled
//            if (Flags.HasFlagFast(Emgf.Legalmoves) && !IsLegal(move, piece, from, type))
//                return;

//            moves.Add(move);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private void AddMoves(ICollection<Move> moves, Piece piece, Square from, BitBoard attacks)
//        {
//            BitBoard target = Position.Occupied & attacks;
//            foreach (Square to in target)
//                AddMove(moves, piece, from, to, EPieces.NoPiece, EMoveType.Capture);

//            target = ~Position.Occupied & attacks;
//            foreach (Square to in target)
//                AddMove(moves, piece, from, to, PieceExtensions.EmptyPiece);
//        }

//        /// <summary>
//        /// Iterates through the piece types and generates moves based on their attacks.
//        /// It does not contain any checks for moves that are invalid, as the leaf methods
//        /// contains implicit denial of move generation if the target bitboard is empty.
//        /// </summary>
//        /// <param name="moves">The move list to add potential moves to.</param>
//        /// <param name="targetSquares">The target squares to move to</param>
//        private void AddMoves(ICollection<Move> moves, BitBoard targetSquares)
//        {
//            Player us = SideToMove;
//            BitBoard occupied = Position.Occupied;

//            for (EPieceType pieceType = EPieceType.Queen; pieceType >= EPieceType.Bishop; --pieceType)
//            {
//                Piece p = pieceType.MakePiece(us);
//                foreach (Square sq in _bitboardPieces[p.ToInt()])
//                    AddMoves(moves, p, sq, sq.GetAttacks(pieceType, occupied) & targetSquares);
//            }

//            // TODO : More generic way of adding moves?
//            //Piece piece = EPieceType.Queen.MakePiece(c);
//            //foreach (Square from in _bitboardPieces[piece.ToInt()])
//            //    AddMoves(moves, piece, from, from.GetAttacks(EPieceType.Queen, occupied) & targetSquares);

//            //piece = EPieceType.Rook.MakePiece(c);
//            //foreach (Square from in _bitboardPieces[piece.ToInt()])
//            //    AddMoves(moves, piece, from, from.GetAttacks(EPieceType.Rook, occupied) & targetSquares);

//            //piece = EPieceType.Bishop.MakePiece(c);
//            //foreach (Square from in _bitboardPieces[piece.ToInt()])
//            //    AddMoves(moves, piece, from, from.GetAttacks(EPieceType.Bishop, occupied) & targetSquares);

//            Piece piece = EPieceType.Knight.MakePiece(us);
//            foreach (Square square in _bitboardPieces[piece.ToInt()])
//                AddMoves(moves, piece, square, square.GetAttacks(EPieceType.Knight) & targetSquares);

//            piece = EPieceType.King.MakePiece(us);
//            foreach (Square square in _bitboardPieces[piece.ToInt()])
//                AddMoves(moves, piece, square, square.GetAttacks(EPieceType.King) & targetSquares);
//        }

//        private void AddPawnMoves(ICollection<Move> moves, BitBoard pawns, BitBoard attackSquares, EMoveType type)
//        {
//            if (attackSquares.Empty())
//                return;

//            Piece pawn = EPieceType.Pawn.MakePiece(SideToMove);

//            foreach (var fromSquare in pawns)
//            {
//                foreach (var toSquare in attackSquares)
//                {
//                    if (!toSquare.IsPromotionRank())
//                    {
//                        AddMove(moves, pawn, fromSquare, toSquare, PieceExtensions.EmptyPiece, type);
//                        continue;
//                    }

//                    if (Flags.HasFlagFast(Emgf.Queenpromotion))
//                    {
//                        AddMove(moves, pawn, fromSquare, toSquare, EPieceType.Queen.MakePiece(SideToMove), type | EMoveType.Promotion);
//                        return;
//                    }

//                    for (EPieceType promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
//                        AddMove(moves, pawn, fromSquare, toSquare, promotedPiece.MakePiece(SideToMove), type | EMoveType.Promotion);
//                }
//            }
//        }

//        private void AddPawnMoves(ICollection<Move> moves, BitBoard targetSquares, Direction direction, EMoveType type)
//        {
//            if (targetSquares.Empty())
//                return;

//            Piece piece = EPieceType.Pawn.MakePiece(SideToMove);

//            foreach (Square squareTo in targetSquares)
//            {
//                Square squareFrom = squareTo - direction;
//                if (!squareTo.IsPromotionRank())
//                {
//                    AddMove(moves, piece, squareFrom, squareTo, PieceExtensions.EmptyPiece, type);
//                    continue;
//                }

//                if (Flags.HasFlagFast(Emgf.Queenpromotion))
//                {
//                    AddMove(moves, piece, squareFrom, squareTo, EPieceType.Queen.MakePiece(SideToMove), type | EMoveType.Promotion);
//                    return;
//                }

//                for (EPieceType promotedPiece = EPieceType.Queen; promotedPiece >= EPieceType.Knight; promotedPiece--)
//                    AddMove(moves, piece, squareFrom, squareTo, promotedPiece.MakePiece(SideToMove), type | EMoveType.Promotion);
//            }
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private void AddCastleMove(ICollection<Move> moves, Square from, Square to) => AddMove(moves, EPieceType.King.MakePiece(SideToMove), from, to, PieceExtensions.EmptyPiece, EMoveType.Castle);

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private bool CanCastle(ECastleling type)
//        {
//            // ReSharper disable once SwitchStatementMissingSomeCases
//            switch (type)
//            {
//                case ECastleling.Short:
//                case ECastleling.Long:
//                    return (CastlelingRights & type.GetCastleAllowedMask(SideToMove)) != 0 && IsCastleAllowed(type.GetKingCastleTo(SideToMove));

//                default:
//                    throw new ArgumentException("Illegal castleling type.");
//            }
//        }

//        private bool IsCastleAllowed(Square square)
//        {
//            Player us = SideToMove;
//            // The complexity of this function is mainly due to the support for Chess960 variant.
//            Square rookTo = square.GetRookCastleTo();
//            Square rookFrom = Position.GetRookCastleFrom(square);
//            Square kingSquare = Position.KingSquares[us.Side];

//            // The pieces in question.. rook and king
//            BitBoard castlePieces = rookFrom | kingSquare;

//            // The span between the rook and the king
//            BitBoard castleSpan = castlePieces | rookTo;
//            castleSpan |= square;
//            castleSpan |= kingSquare.BitboardBetween(rookFrom) | rookFrom.BitboardBetween(rookTo);

//            // check that the span AND current occupied pieces are no different that the piece themselves.
//            if ((castleSpan & _occupied) != castlePieces)
//                return false;

//            // Check that no square between the king's initial and final squares (including the initial and final squares)
//            // may be under attack by an enemy piece. Initial square was already checked a this point.

//            us = ~us;

//            BitBoard targets = kingSquare.BitboardBetween(square) | square;

//            foreach (Square s in targets)
//            {
//                if (Position.IsAttacked(s, us))
//                    return false;
//            }

//            return true;
//        }
//    }
//}