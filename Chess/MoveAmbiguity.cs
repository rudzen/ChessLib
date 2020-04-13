namespace Rudz.Chess
{
    using Enums;
    using Exceptions;
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Types;

    public sealed class MoveAmbiguity : IMoveAmbiguity
    {
        private readonly IDictionary<MoveNotations, Func<Move, string>> _notationFuncs;

        private readonly IPosition _pos;

        public MoveAmbiguity(IPosition pos)
        {
            _pos = pos;
            _notationFuncs = new Dictionary<MoveNotations, Func<Move, string>>
        {
            {MoveNotations.Fan, ToFan},
            {MoveNotations.San, ToSan},
            {MoveNotations.Lan, ToLan},
            {MoveNotations.Ran, ToRan},
            {MoveNotations.Uci, ToUci}
        };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToNotation(Move move, MoveNotations notation = MoveNotations.Fan)
        {
            if (move.IsNullMove())
                return "(none)";

            if (!_notationFuncs.TryGetValue(notation, out var func))
                throw new InvalidMoveException("Invalid move notation detected.");

            return func(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToUci(Move move) => move.ToString();

        /// <summary>
        /// <para>Converts a move to FAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>FAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToFan(Move move)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pc = move.GetMovingPiece();
                var pt = pc.Type();

                if (pt != PieceTypes.Pawn)
                {
                    notation.Append(pc.GetUnicodeChar());
                    Disambiguation(move, from, notation);
                }

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar());
                    notation.Append('x');
                }

                notation.Append(to.ToString());

                if (move.IsPromotionMove())
                {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (_pos.State.InCheck)
                notation.Append(GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to SAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>SAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToSan(Move move)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != PieceTypes.Pawn)
                {
                    notation.Append(move.GetMovingPiece().GetPgnChar());
                    Disambiguation(move, from, notation);
                }

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar());

                    notation.Append('x');
                }

                notation.Append(to.ToString());

                if (move.IsPromotionMove())
                {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetPgnChar());
                }
            }

            if (_pos.State.InCheck)
                notation.Append(GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to LAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>LAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToLan(Move move)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != PieceTypes.Pawn)
                    notation.Append(pt.GetPieceChar());

                notation.Append(from.ToString());

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar());

                    notation.Append('x');
                }
                else
                    notation.Append('-');

                notation.Append(to.ToString());

                if (move.IsPromotionMove())
                {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (_pos.State.InCheck)
                notation.Append(GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to RAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>RAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToRan(Move move)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != PieceTypes.Pawn)
                    notation.Append(pt.GetPieceChar());

                notation.Append(from.ToString());

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == PieceTypes.Pawn)
                        notation.Append(from.FileChar());

                    notation.Append('x');
                    notation.Append(move.GetCapturedPiece().Type().GetPieceChar());
                }
                else
                    notation.Append('-');

                notation.Append(to.ToString());

                if (move.IsPromotionMove())
                {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (_pos.State.InCheck)
                notation.Append(GetCheckChar());

            return notation.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char GetCheckChar() => _pos.GenerateMoves().Any() ? '+' : '#';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveAmbiguities Ambiguity(Move move, BitBoard similarTypeAttacks)
        {
            var ambiguity = MoveAmbiguities.None;

            foreach (var square in similarTypeAttacks)
            {
                var c = move.GetMovingSide();
                var pinned = _pos.GetPinnedPieces(square, c);

                if (similarTypeAttacks & pinned)
                    continue;

                if (move.GetMovingPieceType() != _pos.GetPieceType(square))
                    continue;

                if (_pos.Pieces(c) & square)
                {
                    if (square.File() == move.GetFromSquare().File())
                        ambiguity |= MoveAmbiguities.File;
                    else if (square.Rank() == move.GetFromSquare().Rank())
                        ambiguity |= MoveAmbiguities.Rank;

                    ambiguity |= MoveAmbiguities.Move;
                }
            }

            return ambiguity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetSimilarAttacks(Move move)
        {
            var pt = move.GetMovingPieceType();

            return pt == PieceTypes.Pawn || pt == PieceTypes.King
                ? BitBoards.EmptyBitBoard
                : move.GetToSquare().GetAttacks(pt, _pos.Pieces()) ^ move.GetFromSquare();
        }

        /// <summary>
        /// Disambiguation.
        /// <para>If we have more then one piece with destination 'to'.</para>
        /// <para>Note that for pawns is not needed because starting file is explicit.</para>
        /// </summary>
        /// <param name="move">The move to check</param>
        /// <param name="from">The from square</param>
        /// <param name="position">The current used position</param>
        /// <param name="sb">The StringBuilder to append to if needed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Disambiguation(Move move, Square from, StringBuilder sb)
        {
            var similarAttacks = GetSimilarAttacks(move);

            var ambiguity = Ambiguity(move, similarAttacks);

            if (!ambiguity.HasFlagFast(MoveAmbiguities.Move))
                return;

            if (!ambiguity.HasFlagFast(MoveAmbiguities.File))
                sb.Append(from.FileChar());
            else if (!ambiguity.HasFlagFast(MoveAmbiguities.Rank))
                sb.Append(from.RankChar());
            else
                sb.Append(from.ToString());
        }
    }
}