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

namespace Rudz.Chess.Types
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Enums;
    using Exceptions;

    /// <summary>
    /// Made in a hurry.
    /// TODO : to be organized in a more fluent manner
    /// </summary>
    public static class MoveExtensions
    {
        public static readonly Move EmptyMove = new Move();

        private static readonly Dictionary<EMoveNotation, Func<Move, State, string>> NotationFuncs;

        static MoveExtensions() => NotationFuncs = new Dictionary<EMoveNotation, Func<Move, State, string>>
                                                       {
                                                           { EMoveNotation.Fan, ToFan },
                                                           { EMoveNotation.San, ToSan },
                                                           { EMoveNotation.Lan, ToLan },
                                                           { EMoveNotation.Ran, ToRan },
                                                           { EMoveNotation.Uci, ToUci }
                                                       };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToNotation(this Move move, State state, EMoveNotation notation = EMoveNotation.Fan)
        {
            if (move.IsNullMove())
                return "(none)";

            if (!NotationFuncs.TryGetValue(notation, out Func<Move, State, string> func))
                throw new InvalidMoveException("Invalid move notation detected.");

            return func(move, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (bool, Move) Locate(this Move move, Game game)
        {
            // force position to contain the latests moves for the position moves to be searched in
            game.State.GenerateMoves();

            for (int i = 0; i < game.State.Moves.Count; i++) {
                if (move.GetFromSquare() == game.State.Moves[i].GetFromSquare() && move.GetToSquare() == game.State.Moves[i].GetToSquare())
                    return (true, game.State.Moves[i]);
            }

            return (false, EmptyMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToUci(Move move, State state = null) => move.ToString();

        /// <summary>
        /// <para>
        /// Converts a move to FAN notation.
        /// </para>
        /// </summary>
        /// <param name="move">
        /// The move to convert
        /// </param>
        /// <param name="state">
        /// The position from where the move exist
        /// </param>
        /// <returns>
        /// FAN move string
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToFan(Move move, State state)
        {
            Square from = move.GetFromSquare();
            Square to = move.GetToSquare();

            StringBuilder notation = new StringBuilder(12);

            if (move.IsCastlelingMove()) {
                notation.Append(to < from ? "O-O-O" : "O-O");
            } else {
                EPieceType pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn) {
                    notation.Append(move.GetMovingPiece().GetUnicodeChar());

                    // Disambiguation.
                    // if we have more then one piece with destination 'to'
                    // note that for pawns is not needed because starting file is explicit.
                    BitBoard simularTypeAttacks = move.GetSimularAttacks(state);

                    (bool ambiguousMove, bool ambiguousFile, bool ambiguousRank) = move.Ambiguity(ref simularTypeAttacks, state);

                    if (ambiguousMove)
                        if (!ambiguousFile)
                            notation.Append(from.FileChar());
                        else if (!ambiguousRank)
                            notation.Append(from.RankOfChar());
                        else
                            notation.Append(from);
                }

                if (move.IsEnPassantMove()) {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                } else if (move.IsCaptureMove()) {
                    if (pt == EPieceType.Pawn)
                        notation.Append(from.FileChar());
                    notation.Append('x');
                }

                notation.Append(to);

                if (move.IsPromotionMove()) {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (!state.InCheck) return notation.ToString();

            notation.Append(state.GetCheckChar());
            return notation.ToString();
        }

        /// <summary>
        /// <para>
        /// Converts a move to SAN notation.
        /// </para>
        /// </summary>
        /// <param name="move">
        /// The move to convert
        /// </param>
        /// <param name="state">
        /// The position from where the move exist
        /// </param>
        /// <returns>
        /// SAN move string
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToSan(this Move move, State state)
        {
            Square from = move.GetFromSquare();
            Square to = move.GetToSquare();

            StringBuilder notation = new StringBuilder(12);

            if (move.IsCastlelingMove()) {
                notation.Append(to < from ? "O-O-O" : "O-O");
            } else {
                EPieceType pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn) {
                    notation.Append(move.GetMovingPiece().GetPgnChar());

                    // Disambiguation.
                    // if we have more then one piece with destination 'to'
                    // note that for pawns is not needed because starting file is explicit.
                    BitBoard simularTypeAttacks = move.GetSimularAttacks(state);

                    (bool ambiguousMove, bool ambiguousFile, bool ambiguousRank) = move.Ambiguity(ref simularTypeAttacks, state);

                    if (ambiguousMove) {
                        if (!ambiguousFile)
                            notation.Append(@from.FileChar());
                        else if (!ambiguousRank)
                            notation.Append(@from.RankOfChar());
                        else
                            notation.Append(@from);
                    }
                }

                if (move.IsEnPassantMove()) {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                } else if (move.IsCaptureMove()) {
                    if (pt == EPieceType.Pawn)
                        notation.Append(@from.FileChar());

                    notation.Append('x');
                }

                notation.Append(to);

                if (move.IsPromotionMove()) {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetPgnChar());
                }
            }

            if (!state.InCheck)
                return notation.ToString();

            notation.Append(state.GetCheckChar());
            return notation.ToString();
        }

        /// <summary>
        /// <para>
        /// Converts a move to LAN notation.
        /// </para>
        /// </summary>
        /// <param name="move">
        /// The move to convert
        /// </param>
        /// <param name="position">
        /// The position from where the move exist
        /// </param>
        /// <returns>
        /// LAN move string
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToLan(Move move, MoveGenerator position)
        {
            Square from = move.GetFromSquare();
            Square to = move.GetToSquare();

            StringBuilder notation = new StringBuilder(12);

            if (move.IsCastlelingMove()) {
                notation.Append(to < from ? "O-O-O" : "O-O");
            } else {
                EPieceType pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                    notation.Append(pt.GetPieceChar());

                notation.Append(from);

                if (move.IsEnPassantMove()) {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                } else if (move.IsCaptureMove()) {
                    if (pt == EPieceType.Pawn)
                        notation.Append(@from.FileChar());

                    notation.Append('x');
                } else {
                    notation.Append('-');
                }

                notation.Append(to);

                if (move.IsPromotionMove()) {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (!position.InCheck)
                return notation.ToString();

            notation.Append(position.GetCheckChar());
            return notation.ToString();
        }

        /// <summary>
        /// <para>
        /// Converts a move to RAN notation.
        /// </para>
        /// </summary>
        /// <param name="move">
        /// The move to convert
        /// </param>
        /// <param name="position">
        /// The position from where the move exist
        /// </param>
        /// <returns>
        /// RAN move string
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ToRan(Move move, MoveGenerator position)
        {
            Square from = move.GetFromSquare();
            Square to = move.GetToSquare();

            StringBuilder notation = new StringBuilder(12);

            if (move.IsCastlelingMove()) {
                notation.Append(to < from ? "O-O-O" : "O-O");
            } else {
                EPieceType pt = move.GetMovingPieceType();

                if (pt != EPieceType.Pawn)
                    notation.Append(pt.GetPieceChar());

                notation.Append(from);

                if (move.IsEnPassantMove()) {
                    notation.Append("ep");
                    notation.Append(from.FileChar());
                } else if (move.IsCaptureMove()) {
                    if (pt == EPieceType.Pawn)
                        notation.Append(@from.FileChar());

                    notation.Append('x');
                    notation.Append(move.GetCapturedPiece().Type().GetPieceChar());
                } else {
                    notation.Append('-');
                }

                notation.Append(to);

                if (move.IsPromotionMove()) {
                    notation.Append('=');
                    notation.Append(move.GetPromotedPiece().GetUnicodeChar());
                }
            }

            if (!position.InCheck)
                return notation.ToString();

            notation.Append(position.GetCheckChar());
            return notation.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetCheckChar(this MoveGenerator moveGenerator)
        {
            moveGenerator.GenerateMoves();
            return moveGenerator.Moves.Count > 0 ? '+' : '#';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (bool, bool, bool) Ambiguity(this Move move, ref BitBoard simularTypeAttacks, MoveGenerator moveGenerator)
        {
            bool ambiguousMove = false;
            bool ambiguousFile = false;
            bool ambiguousRank = false;

            foreach (Square square in simularTypeAttacks) {
                BitBoard pinned = moveGenerator.ChessBoard.GetPinnedPieces(square, move.GetMovingSide());
                
                if (simularTypeAttacks & pinned)
                    continue;

                if (move.GetMovingPieceType() != moveGenerator.ChessBoard.GetPieceType(square))
                    continue;

                // ReSharper disable once InvertIf
                if (moveGenerator.ChessBoard.OccupiedBySide[move.GetMovingSide().Side] & square) {
                    if (square.File() == move.GetFromSquare().File())
                        ambiguousFile = true;
                    else if (square.RankOf() == move.GetFromSquare().RankOf())
                        ambiguousRank = true;

                    ambiguousMove = true;
                }
            }

            return (ambiguousMove, ambiguousFile, ambiguousRank);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetSimularAttacks(this Move move, MoveGenerator position)
        {
            BitBoard resultBitBoard = 0;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (move.GetMovingPieceType()) {
                case EPieceType.Bishop:
                case EPieceType.Rook:
                case EPieceType.Queen:
                    resultBitBoard |= move.GetToSquare().GetAttacks(move.GetMovingPieceType(), position.ChessBoard.Occupied);
                    break;
                case EPieceType.Knight:
                    resultBitBoard |= move.GetToSquare().GetAttacks(move.GetMovingPieceType());
                    break;
            }

            return resultBitBoard ^ move.GetFromSquare();
        }
    }
}