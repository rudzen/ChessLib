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

namespace Rudz.Chess.Types
{
    using Enums;
    using Exceptions;
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
        public static readonly Move EmptyMove;

        private static readonly Dictionary<EMoveNotation, Func<Move, MoveGenerator, string>> NotationFuncs;

        static MoveExtensions()
        {
            EmptyMove = new Move();
            NotationFuncs = new Dictionary<EMoveNotation, Func<Move, MoveGenerator, string>>
            {
                {EMoveNotation.Fan, ToFan},
                {EMoveNotation.San, ToSan},
                {EMoveNotation.Lan, ToLan},
                {EMoveNotation.Ran, ToRan},
                {EMoveNotation.Uci, ToUci}
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToNotation(this Move move, State state, EMoveNotation notation = EMoveNotation.Fan)
        {
            if (move.IsNullMove())
                return "(none)";

            if (!NotationFuncs.TryGetValue(notation, out var func))
                throw new InvalidMoveException("Invalid move notation detected.");

            return func(move, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool, Move) Locate(this Move move, Game game)
        {
            // force position to contain the latest moves for the position moves to be searched in
            game.State.GenerateMoves();

            var element = game.State.Moves.FirstOrDefault(x => x.GetFromSquare() == move.GetFromSquare() && x.GetToSquare() == move.GetToSquare());
            return element == null ? (false, EmptyMove) : (true, element);

            //for (int i = 0; i < game.State.Moves.Count; i++) {
            //    if (move.GetFromSquare() == game.State.Moves[i].GetFromSquare() && move.GetToSquare() == game.State.Moves[i].GetToSquare())
            //        return (true, game.State.Moves[i]);
            //}

            //return (false, EmptyMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToUci(Move move, MoveGenerator moveGenerator = null) => move.ToString();

        /// <summary>
        /// <para>Converts a move to FAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="moveGenerator"></param>
        /// <returns>FAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToFan(Move move, MoveGenerator moveGenerator)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(ECastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                {
                    notation.Append(move.GetMovingPiece().GetUnicodeChar());
                    move.Disambiguation(from, moveGenerator.Position, notation);
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

            if (moveGenerator.InCheck)
                notation.Append(moveGenerator.GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to SAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="moveGenerator"></param>
        /// <returns>SAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToSan(this Move move, MoveGenerator moveGenerator)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(ECastlelingExtensions.GetCastlelingString(to, from));
            else
            {
                var pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                {
                    notation.Append(move.GetMovingPiece().GetPgnChar());
                    move.Disambiguation(from, moveGenerator.Position, notation);
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

            if (moveGenerator.InCheck)
                notation.Append(moveGenerator.GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to LAN notation.</para>
        /// </summary><param name="move">The move to convert</param>
        /// <param name="moveGenerator"></param>
        /// <returns>LAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToLan(Move move, MoveGenerator moveGenerator)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(ECastlelingExtensions.GetCastlelingString(to, from));
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
                {
                    notation.Append('-');
                }

                notation.Append(to.ToString());

                if (move.IsPromotionMove())
                {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (moveGenerator.InCheck)
                notation.Append(moveGenerator.GetCheckChar());

            return notation.ToString();
        }

        /// <summary>
        /// <para>Converts a move to RAN notation.</para>
        /// </summary>
        /// <param name="move">The move to convert</param>
        /// <param name="moveGenerator">The position from where the move exist</param>
        /// <returns>RAN move string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToRan(Move move, MoveGenerator moveGenerator)
        {
            var from = move.GetFromSquare();
            var to = move.GetToSquare();

            var notation = new StringBuilder(12);

            if (move.IsCastlelingMove())
                notation.Append(ECastlelingExtensions.GetCastlelingString(to, from));
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
                {
                    notation.Append('-');
                }

                notation.Append(to.ToString());

                if (move.IsPromotionMove())
                {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (moveGenerator.InCheck)
                notation.Append(moveGenerator.GetCheckChar());

            return notation.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetCheckChar(this MoveGenerator moveGenerator)
        {
            moveGenerator.GenerateMoves();
            return moveGenerator.Moves.Count > 0 ? '+' : '#';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static EMoveAmbiguity Ambiguity(this Move move, BitBoard similarTypeAttacks, IPosition position)
        {
            var ambiguity = EMoveAmbiguity.None;

            foreach (var square in similarTypeAttacks)
            {
                var pinned = position.GetPinnedPieces(square, move.GetMovingSide());

                if (similarTypeAttacks & pinned)
                    continue;

                if (move.GetMovingPieceType() != position.GetPieceType(square))
                    continue;

                // ReSharper disable once InvertIf
                if (position.OccupiedBySide[move.GetMovingSide().Side] & square)
                {
                    if (square.File() == move.GetFromSquare().File())
                        ambiguity |= EMoveAmbiguity.File;
                    else if (square.RankOf() == move.GetFromSquare().RankOf())
                        ambiguity |= EMoveAmbiguity.Rank;

                    ambiguity |= EMoveAmbiguity.Move;
                }
            }

            return ambiguity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetSimilarAttacks(this IPosition position, Move move)
        {
            var pt = move.GetMovingPieceType();

            return pt == EPieceType.Pawn || pt == EPieceType.King
                ? BitBoards.ZeroBb
                : move.GetToSquare().GetAttacks(pt, position.Occupied) ^ move.GetFromSquare();
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

            if (!ambiguity.HasFlagFast(EMoveAmbiguity.Move))
                return;

            if (!ambiguity.HasFlagFast(EMoveAmbiguity.File))
                sb.Append(from.FileChar());
            else if (!ambiguity.HasFlagFast(EMoveAmbiguity.Rank))
                sb.Append(from.RankOfChar());
            else
                sb.Append(from.ToString());
        }
    }
}