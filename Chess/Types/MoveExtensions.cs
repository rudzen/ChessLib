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

namespace Rudz.Chess.Types
{
    using Enums;
    using Exceptions;
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Made in a hurry.
    /// TODO : to be organized in a more fluent manner
    /// </summary>
    public static class MoveExtensions
    {
        public static readonly Move EmptyMove = new Move();

        private static readonly Dictionary<EMoveNotation, Func<Move, IPosition, string>> NotationFuncs = new Dictionary<EMoveNotation, Func<Move, IPosition, string>>
        {
            {EMoveNotation.Fan, ToFan},
            {EMoveNotation.San, ToSan},
            {EMoveNotation.Lan, ToLan},
            {EMoveNotation.Ran, ToRan},
            {EMoveNotation.Uci, ToUci}
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToNotation(this Move move, IPosition pos, EMoveNotation notation = EMoveNotation.Fan)
        {
            if (move.IsNullMove())
                return "(none)";

            if (!NotationFuncs.TryGetValue(notation, out var func))
                throw new InvalidMoveException("Invalid move notation detected.");

            return func(move, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool, Move) Locate(this Move move, IPosition pos)
        {
            // force position to contain the latest moves for the position moves to be searched in
            var moveList = pos.GenerateMoves();

            var element = moveList.GetMove(move.GetFromSquare(), move.GetToSquare());
            return element == null ? (false, EmptyMove) : (true, element);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToUci(Move move, IPosition pos = null) => move.ToString();

        /// <summary>
        /// <para>Converts a move to FAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>FAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToFan(Move move, IPosition pos)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                {
                    notation.Append(move.GetMovingPiece().GetUnicodeChar());
                    move.Disambiguation(from, pos, notation);
                }

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == EPieceType.Pawn)
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

            if (pos.InCheck)
                notation.Append(pos.GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to SAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>SAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToSan(this Move move, IPosition pos)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                {
                    notation.Append(move.GetMovingPiece().GetPgnChar());
                    move.Disambiguation(from, pos, notation);
                }

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == EPieceType.Pawn)
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

            if (pos.InCheck)
                notation.Append(pos.GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to LAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>LAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToLan(Move move, IPosition pos)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                    notation.Append(pt.GetPieceChar());

                notation.Append(from.ToString());

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == EPieceType.Pawn)
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

            if (pos.InCheck)
                notation.Append(pos.GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to RAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="pos">The position from where the move exist</param>
        /// <returns>RAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToRan(Move move, IPosition pos)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(CastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                    notation.Append(pt.GetPieceChar());

                notation.Append(from.ToString());

                if (move.IsEnPassantMove())
                {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                }
                else if (move.IsCaptureMove())
                {
                    if (pt == EPieceType.Pawn)
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

            if (pos.InCheck)
                notation.Append(pos.GetCheckChar());

            return notation.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetCheckChar(this IPosition pos) => pos.GenerateMoves().Any() ? '+' : '#';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MoveAmbiguities Ambiguity(this Move move, BitBoard similarTypeAttacks, IPosition position)
        {
            var ambiguity = MoveAmbiguities.None;

            foreach (var square in similarTypeAttacks)
            {
                var pinned = position.GetPinnedPieces(square, move.GetMovingSide());

                if (similarTypeAttacks & pinned)
                    continue;

                if (move.GetMovingPieceType() != position.GetPieceType(square))
                    continue;

                if (position.OccupiedBySide[move.GetMovingSide().Side] & square)
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
        private static BitBoard GetSimilarAttacks(this IPosition position, Move move)
        {
            var pt = move.GetMovingPieceType();

            return pt == EPieceType.Pawn || pt == EPieceType.King
                ? BitBoards.EmptyBitBoard
                : move.GetToSquare().GetAttacks(pt, position.Pieces()) ^ move.GetFromSquare();
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
        private static void Disambiguation(this Move move, Square from, IPosition position, StringBuilder sb)
        {
            var simularTypeAttacks = position.GetSimilarAttacks(move);

            var ambiguity = move.Ambiguity(simularTypeAttacks, position);

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