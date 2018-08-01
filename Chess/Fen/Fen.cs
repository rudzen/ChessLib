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

namespace Rudz.Chess.Fen
{
    using Enums;
    using Extensions;
    using Properties;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using Types;

    public static class Fen
    {
        public const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private const string FenRankRegexSnippet = @"[1-8KkQqRrBbNnPp]{1,8}";

        private const int MaxFenLen = 128;

        private const char Space = ' ';

        private const int SpaceCount = 3;

        private const char Seperator = '/';

        private const int SeperatorCount = 7;

        private static readonly Regex ValidFenRegex = new Regex(
            string.Format(@"^ \s* {0}/{0}/{0}/{0}/{0}/{0}/{0}/{0} \s+ (?:w|b) \s+ (?:[KkQq]+|\-) \s+ (?:[a-h][1-8]|\-) \s+ \d+ \s+ \d+ \s* $", FenRankRegexSnippet),
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        /// <summary>
        /// Parses the board layout to a FEN representaion..
        /// Beware, goblins are a foot.
        /// </summary>
        /// <param name="state">
        /// The position class which contains relevant information about the status of the board.
        /// </param>
        /// <param name="halfMoveCount">
        /// The half Move Count.
        /// </param>
        /// <returns>
        /// The FenData which contains the fen string that was generated.
        /// </returns>
        public static FenData GenerateFen([NotNull] this State state, int halfMoveCount)
        {
            StringBuilder sv = new StringBuilder(MaxFenLen);

            for (ERank rank = ERank.Rank8; rank >= ERank.Rank1; rank--)
            {
                int empty = 0;

                for (EFile file = EFile.FileA; file <= EFile.FileH; file++)
                {
                    Square square = new Square(rank, file);
                    Piece piece = state.ChessBoard.BoardLayout[square.ToInt()];

                    if (piece.IsNoPiece())
                    {
                        empty++;
                        continue;
                    }

                    if (empty != 0)
                    {
                        sv.Append(empty);
                        empty = 0;
                    }

                    sv.Append(piece.GetPieceChar());
                }

                if (empty != 0)
                    sv.Append(empty);

                if (rank > ERank.Rank1)
                    sv.Append('/');
            }

            sv.Append(state.SideToMove.IsWhite() ? " w " : " b ");

            int castleRights = state.CastlelingRights;

            if (castleRights != 0)
            {
                if ((castleRights & 1) != 0)
                    sv.Append('K');

                if ((castleRights & 2) != 0)
                    sv.Append('Q');

                if ((castleRights & 4) != 0)
                    sv.Append('k');

                if ((castleRights & 8) != 0)
                    sv.Append('q');
            }
            else
            {
                sv.Append('-');
            }

            sv.Append(' ');

            if (state.EnPassantSquare.Empty())
                sv.Append('-');
            else
                sv.Append(state.EnPassantSquare.First());

            sv.Append(' ');

            sv.Append(state.ReversibleHalfMoveCount);
            sv.Append(' ');
            sv.Append(halfMoveCount + 1);
            return new FenData(sv.ToString());
        }

        /// <summary>
        /// Performs basic validation of FEN string.
        /// Requirements for a valid FEN string is:
        /// - Must not be null
        /// - No more than 128 chars in length
        /// - Must have a valid count of spaces and backslashes
        /// </summary>
        /// <param name="fen">The FEN string to validate</param>
        /// <returns>true if all requirements are met, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate([CanBeNull] string fen)
        {
            if (fen == null)
                return false;

            string f = fen.Trim();

            return f.Length <= MaxFenLen && CountValidity(f) && ValidFenRegex.IsMatch(fen) && CountPieceValidity(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDelimiter(char c) => c == Space;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ESquare GetEpSquare(this FenData fen)
        {
            char c = fen.GetAdvance();

            if (c == '-')
                return ESquare.none;

            if (!c.InBetween('a', 'h'))
                return ESquare.fail;

            char c2 = fen.GetAdvance();

            return c.InBetween('3', '6') ? ESquare.fail : new Square(c2 - '1', c - 'a').Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CountPieceValidity([NotNull] string str)
        {
            string[] testArray = str.Split();
            short whitePawnCount = 0;
            short whiteRookCount = 0;
            short whiteQueenCount = 0;
            short whiteKnightCount = 0;
            short whiteKingCount = 0;
            short whiteBishopCount = 0;

            short blackPawnCount = 0;
            short blackRookCount = 0;
            short blackQueenCount = 0;
            short blackKnightCount = 0;
            short blackKingCount = 0;
            short blackBishopCount = 0;

            foreach (char c in testArray[0])
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (c)
                {
                    case 'p':
                        if (++whitePawnCount > 8 && whiteRookCount + whitePawnCount + whiteBishopCount + whiteKnightCount + whiteQueenCount < 15)
                            return false;
                        break;

                    case 'r':
                        if (++whiteRookCount > 10 && whiteRookCount + whitePawnCount + whiteBishopCount + whiteKnightCount + whiteQueenCount < 15)
                            return false;
                        break;

                    case 'q':
                        if (++whiteQueenCount > 9)
                            return false;
                        break;

                    case 'n':
                        if (++whiteKnightCount > 10 && whiteRookCount + whitePawnCount + whiteBishopCount + whiteKnightCount + whiteQueenCount < 15)
                            return false;
                        break;

                    case 'k':
                        if (++whiteKingCount > 1)
                            return false;
                        break;

                    case 'b':
                        if (++whiteBishopCount > 10 && whiteRookCount + whitePawnCount + whiteBishopCount + whiteKnightCount + whiteQueenCount < 15)
                            return false;
                        break;

                    case 'P':
                        if (++blackPawnCount > 8 && blackRookCount + blackPawnCount + blackBishopCount + blackKnightCount + blackQueenCount < 16)
                            return false;
                        break;

                    case 'R':
                        if (++blackRookCount > 10 && blackRookCount + blackPawnCount + blackBishopCount + blackKnightCount + blackQueenCount < 16)
                            return false;
                        break;

                    case 'Q':
                        if (++blackQueenCount > 9 && blackRookCount + blackPawnCount + blackBishopCount + blackKnightCount + blackQueenCount < 16)
                            return false;
                        break;

                    case 'N':
                        if (++blackKnightCount > 10 && blackRookCount + blackPawnCount + blackBishopCount + blackKnightCount + blackQueenCount < 16)
                            return false;
                        break;

                    case 'K':
                        if (++blackKingCount > 1)
                            return false;
                        break;

                    case 'B':
                        if (++blackBishopCount > 10 && blackRookCount + blackPawnCount + blackBishopCount + blackKnightCount + blackQueenCount < 16)
                            return false;
                        break;
                }
            }
            return int.Parse(testArray[testArray.Length - 1]) <= 2048;
        }

        /// <summary>
        /// Validate char counts of space and backslash.
        /// FEN strings must have 5 space and 7 slashes to be considered valid format.
        /// </summary>
        /// <param name="str">The fen string to check</param>
        /// <returns>true if format seems ok, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CountValidity([NotNull] string str)
        {
            int spaceCount = 0;
            int seperatorCount = 0;
            foreach (char c in str)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (c)
                {
                    case Seperator:
                        seperatorCount++;
                        continue;
                    case Space:
                        spaceCount++;
                        continue;
                }
            }

            return spaceCount >= SpaceCount && seperatorCount == SeperatorCount;
        }
    }
}