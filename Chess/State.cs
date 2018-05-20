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
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Enums;
    using Extensions;
    using Properties;
    using Types;

    public sealed class State : MoveGenerator
    {
        public Move LastMove { get; set; }

        public Material Material;

        public ulong PawnStructureKey;

        public int ReversibleHalfMoveCount;

        public int NullMovesInRow;

        public int FiftyMoveRuleCounter { get; set; }

        public State([NotNull] ChessBoard chessBoard)
            : base(chessBoard)
        {
            Material = new Material();
            Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMate() => !Moves.Any(move => IsLegal(move, move.GetMovingPiece(), move.GetFromSquare(), move.GetMoveType()));

        public void Clear()
        {
            InCheck = false;
            CastlelingRights = 0;
            ReversibleHalfMoveCount = 0;
            PawnStructureKey = 0;
            Key = 0;
            Material.Clear();
            LastMove = MoveExtensions.EmptyMove;
        }

        /// <summary>
        /// TODO : This method is incomplete, and is not ment to be used atm.
        /// Parse a string and convert to a valid move. If the move is not valid.. hell breaks loose.
        /// * NO EXCEPTIONS IS ALLOWED IN THIS FUNCTION *
        /// </summary>
        /// <param name="m">string representaion of the move to parse</param>
        /// <returns>
        /// On fail : Move containing from and to squares as ESquare.none (empty move)
        /// On Ok   : The move!
        /// </returns>
        public Move StringToMove(string m)
        {
            // guards
            if (string.IsNullOrWhiteSpace(m))
                return MoveExtensions.EmptyMove;

            if (m.Equals(@"\"))
                return MoveExtensions.EmptyMove;

            // only lenghts of 4 and 5 are acceptable.
            if (!m.Length.InBetween(4, 5))
                return MoveExtensions.EmptyMove;

            ECastleling castleType = IsCastleMove(m);

            if (castleType == ECastleling.None && (!m[0].InBetween('a', 'h') || !m[1].InBetween('1', '8') || !m[2].InBetween('a', 'h') || !m[3].InBetween('1', '8')))
                return MoveExtensions.EmptyMove;

            /*
             * Needs to be assigned here.
             * Otherwise it won't compile because of later split check using both two independant IF and optional reassignment through local method.
             * (bug in VS!?)
             */
            Square from = new Square(m[1] - '1', m[0] - 'a');
            Square to = new Square(m[3] - '1', m[2] - 'a');

            // local function to determin if the move is actually a castleling move by looking at the piece location of the squares
            ECastleling ShredderFunc(Square fromSquare, Square toSquare) =>
                ChessBoard.GetPiece(fromSquare).Value == EPieces.WhiteKing && ChessBoard.GetPiece(toSquare).Value == EPieces.WhiteRook
                || ChessBoard.GetPiece(fromSquare).Value == EPieces.BlackKing && ChessBoard.GetPiece(toSquare).Value == EPieces.BlackRook
                    ? toSquare > fromSquare
                          ? ECastleling.Short
                          : ECastleling.Long
                    : ECastleling.None;

            // part one of pillaging the castleType.. detection of chess 960 - shredder fen
            if (castleType == ECastleling.None)
                castleType = ShredderFunc(from, to); /* look for the airballon */

            // part two of pillaging the castleType var, since it might have changed
            if (castleType != ECastleling.None) {
                from = ChessBoard.GetKingCastleFrom(SideToMove, castleType);
                to = castleType.GetKingCastleTo(SideToMove);
            }

            GenerateMoves();

            // ** untested area **
            foreach (Move move in Moves) {
                if (move.GetFromSquare() != from || move.GetToSquare() != to)
                    continue;
                if (castleType == ECastleling.None && move.IsCastlelingMove())
                    continue;
                if (!move.IsPromotionMove())
                    return move;
                if (char.ToLower(m[m.Length - 1]) != move.GetPromotedPiece().GetPromotionChar())
                    continue;

                return move;
            }

            return MoveExtensions.EmptyMove;
        }

        /// <summary>
        /// Checks from a string if the move actually is a castle move.
        /// Note:
        /// - The unique cases with ammended piece location check
        ///   is a *shallow* detection, it should be the sender
        ///   that guarentee that it's a real move.
        /// </summary>
        /// <param name="m">The string containing the move</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ECastleling IsCastleMove(string m)
        {
            switch (m) {
                case "O-O":
                case "OO":
                case "0-0":
                case "00":
                case "e1g1" when ChessBoard.IsPieceTypeOnSquare(ESquare.e1, EPieceType.King): // bug (stylecop) fake vs syntax error
                case "e8g8" when ChessBoard.IsPieceTypeOnSquare(ESquare.e8, EPieceType.King):
                    return ECastleling.Short;
                case "O-O-O":
                case "OOO":
                case "0-0-0":
                case "000":
                case "e1c1" when ChessBoard.IsPieceTypeOnSquare(ESquare.e1, EPieceType.King):
                case "e8c8" when ChessBoard.IsPieceTypeOnSquare(ESquare.e8, EPieceType.King):
                    return ECastleling.Long;
            }
            return ECastleling.None;
        }
    }
}