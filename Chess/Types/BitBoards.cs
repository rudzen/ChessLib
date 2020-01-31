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

// ReSharper disable InconsistentNaming

using System.Linq;

namespace Rudz.Chess.Types
{
    using Enums;
    using Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class BitBoards
    {
        internal const ulong One = 0x1ul;

        public static readonly BitBoard WhiteArea = 0x00000000FFFFFFFF;

        public static readonly BitBoard BlackArea = ~WhiteArea;

        public static readonly BitBoard LightSquares = 0x55AA55AA55AA55AA;

        public static readonly BitBoard DarkSquares = ~LightSquares;

        public static readonly BitBoard FILEA = 0x0101010101010101;

        public static readonly BitBoard FILEB = 0x0202020202020202;

        public static readonly BitBoard FILEC = 0x404040404040404;

        public static readonly BitBoard FILED = 0x808080808080808;

        public static readonly BitBoard FILEE = 0x1010101010101010;

        public static readonly BitBoard FILEF = 0x2020202020202020;

        public static readonly BitBoard FILEG = 0x4040404040404040;

        public static readonly BitBoard FILEH = 0x8080808080808080;

        public static readonly BitBoard RANK1 = 0x00000000000000ff;

        public static readonly BitBoard RANK2 = 0x000000000000ff00;

        public static readonly BitBoard RANK3 = 0x0000000000ff0000;

        public static readonly BitBoard RANK4 = 0x00000000ff000000;

        public static readonly BitBoard RANK5 = 0x000000ff00000000;

        public static readonly BitBoard RANK6 = 0x0000ff0000000000;

        public static readonly BitBoard RANK7 = 0x00ff000000000000;

        public static readonly BitBoard RANK8 = 0xff00000000000000;

        public static readonly BitBoard EmptyBitBoard = new BitBoard(0UL);

        public static readonly BitBoard AllSquares = ~EmptyBitBoard;

        public static readonly BitBoard CornerA1;

        public static readonly BitBoard CornerA8;

        public static readonly BitBoard CornerH1;

        public static readonly BitBoard CornerH8;

        public static readonly BitBoard QueenSide = new BitBoard(FILEA | FILEB | FILEC | FILED);

        public static readonly BitBoard CenterFiles = new BitBoard(FILEC | FILED | FILEE | FILEF);

        public static readonly BitBoard KingSide = new BitBoard(FILEE | FILEF | FILEG | FILEH);

        public static readonly BitBoard Center = new BitBoard((FILED | FILEE) & (RANK4 | RANK5));

        public static readonly BitBoard[] PromotionRanks = { RANK8, RANK1 };

        public static readonly BitBoard PromotionRanksBB = RANK1 | RANK8;

        internal static readonly BitBoard[] BbSquares =
            {
                0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000, 0x8000000,
                0x10000000, 0x20000000, 0x40000000, 0x80000000, 0x100000000, 0x200000000, 0x400000000, 0x800000000, 0x1000000000, 0x2000000000, 0x4000000000, 0x8000000000, 0x10000000000, 0x20000000000, 0x40000000000, 0x80000000000,
                0x100000000000, 0x200000000000, 0x400000000000, 0x800000000000, 0x1000000000000, 0x2000000000000, 0x4000000000000, 0x8000000000000, 0x10000000000000, 0x20000000000000, 0x40000000000000, 0x80000000000000, 0x100000000000000,
                0x200000000000000, 0x400000000000000, 0x800000000000000, 0x1000000000000000, 0x2000000000000000, 0x4000000000000000, 0x8000000000000000
            };

        private static readonly BitBoard[] FileBB = { FILEA, FILEB, FILEC, FILED, FILEE, FILEF, FILEG, FILEH };

        private static readonly BitBoard[] RankBB = { RANK1, RANK2, RANK3, RANK4, RANK5, RANK6, RANK7, RANK8 };

        private static readonly BitBoard[] Rank1 = { RANK1, RANK8 };

        private static readonly BitBoard[] Rank3BitBoards = { RANK3, RANK6 };

        private static readonly BitBoard[] Rank7BitBoards = { RANK7, RANK2 };

        private static readonly BitBoard[] Rank6And7 = { RANK6 | RANK7, RANK2 | RANK3 };

        private static readonly BitBoard[] Rank7And8 = { RANK7 | RANK8, RANK1 | RANK2 };

        private static readonly int[] Lsb64Table =
            {
                63, 30,  3, 32, 59, 14, 11, 33,
                60, 24, 50,  9, 55, 19, 21, 34,
                61, 29,  2, 53, 51, 23, 41, 18,
                56, 28,  1, 43, 46, 27,  0, 35,
                62, 31, 58,  4,  5, 49, 54,  6,
                15, 52, 12, 40,  7, 42, 45, 16,
                25, 57, 48, 13, 10, 39,  8, 44,
                20, 47, 38, 22, 17, 37, 36, 26
            };

        private static readonly int[] Msb64Table =
            {
                 0, 47,  1, 56, 48, 27,  2, 60,
                57, 49, 41, 37, 28, 16,  3, 61,
                54, 58, 35, 52, 50, 42, 21, 44,
                38, 32, 29, 23, 17, 11,  4, 62,
                46, 55, 26, 59, 40, 36, 15, 53,
                34, 51, 20, 43, 31, 22, 10, 45,
                25, 39, 14, 33, 19, 30,  9, 24,
                13, 18,  8, 12,  7,  6,  5, 63
            };

        /// <summary>
        /// PseudoAttacks are just that, full attack range for all squares for all pieces.
        /// The pawns are a special case, as index range 0,sq are for White and 1,sq are for Black.
        /// This is possible because index 0 is NoPiece type.
        /// </summary>
        private static readonly BitBoard[,] PseudoAttacksBB = new BitBoard[EPieceType.PieceTypeNb.AsInt(), 64];

        private static readonly BitBoard[] AdjacentFilesBB = { FILEB, FILEA | FILEC, FILEB | FILED, FILEC | FILEE, FILED | FILEF, FILEE | FILEG, FILEF | FILEH, FILEG };

        private static readonly BitBoard[,] BetweenBB = new BitBoard[64, 64];

        private static readonly BitBoard[,] PawnAttackSpanBB = new BitBoard[2, 64];

        private static readonly BitBoard[,] PassedPawnMaskBB = new BitBoard[2, 64];

        private static readonly BitBoard[,] ForwardRanksBB = new BitBoard[2, 64];

        private static readonly BitBoard[,] ForwardFileBB = new BitBoard[2, 64];

        private static readonly BitBoard[,] LineBB = new BitBoard[64, 64];

        private static readonly BitBoard[,] KingRingBB = new BitBoard[2, 64];

        private static readonly byte[,] SquareDistance = new byte[64, 64]; // chebyshev distance

        private static readonly BitBoard[,] DistanceRingBB = new BitBoard[64, 8];

        private static readonly IDictionary<Directions, Func<BitBoard, BitBoard>> ShiftFuncs = MakeShiftFuncs();

        static BitBoards()
        {
            CornerA1 = MakeBitboard(ESquare.a1, ESquare.b1, ESquare.a2, ESquare.b2);
            CornerA8 = MakeBitboard(ESquare.a8, ESquare.b8, ESquare.a7, ESquare.b7);
            CornerH1 = MakeBitboard(ESquare.h1, ESquare.g1, ESquare.h2, ESquare.g2);
            CornerH8 = MakeBitboard(ESquare.h8, ESquare.g8, ESquare.h7, ESquare.g7);

            // local helper functions to calculate distance
            int distance(int x, int y) { return Math.Abs(x - y); }
            int distanceFile(Square x, Square y) { return distance(x.File().AsInt(), y.File().AsInt()); }
            int distanceRank(Square x, Square y) { return distance(x.Rank().AsInt(), y.Rank().AsInt()); }

            Span<EPieceType> validMagicPieces = stackalloc EPieceType[] { EPieceType.Bishop, EPieceType.Rook };

            // ForwardRanksBB population loop idea from sf
            for (var r = ERank.Rank1; r < ERank.RankNb; ++r)
            {
                var rank = (int)r;
                ForwardRanksBB[0, rank] = ~(ForwardRanksBB[1, rank + 1] = ForwardRanksBB[1, rank] | RankBB[rank]);
            }

            for (var side = EPlayer.White; side < EPlayer.PlayerNb; ++side)
            {
                var c = (int)side;
                foreach (var square in AllSquares)
                {
                    var s = square.AsInt();
                    ForwardFileBB[c, s] = ForwardRanksBB[c, square.Rank().AsInt()] & FileBB[square.File().AsInt()];
                    PawnAttackSpanBB[c, s] = ForwardRanksBB[c, square.Rank().AsInt()] & AdjacentFilesBB[square.File().AsInt()];
                    PassedPawnMaskBB[c, s] = ForwardFileBB[c, s] | PawnAttackSpanBB[c, s];
                }
            }

            // mini local helpers
            BitBoard ComputeKnightAttack(BitBoard b)
            {
                BitBoard res = (b & ~(FILEA | FILEB)) << 6;
                res |= (b & ~FILEA) << 15;
                res |= (b & ~FILEH) << 17;
                res |= (b & ~(FILEG | FILEH)) << 10;
                res |= (b & ~(FILEG | FILEH)) >> 6;
                res |= (b & ~FILEH) >> 15;
                res |= (b & ~FILEA) >> 17;
                res |= (b & ~(FILEA | FILEB)) >> 10;
                return res;
            }

            // Pseudo attacks for all pieces
            foreach (var s1 in AllSquares)
            {
                var sq = s1.AsInt();
                var b = s1.BitBoardSquare();

                var file = s1.File();

                // distance computation
                foreach (var s2 in AllSquares)
                {
                    SquareDistance[sq, s2.AsInt()] = (byte)distanceFile(s1, s2).Max(distanceRank(s1, s2));
                    DistanceRingBB[sq, SquareDistance[sq, s2.AsInt()]] |= s2;
                }

                PseudoAttacksBB[0, sq] = b.NorthEastOne() | b.NorthWestOne();
                PseudoAttacksBB[1, sq] = b.SouthWestOne() | b.SouthEastOne();

                var pt = EPieceType.Knight.AsInt();
                PseudoAttacksBB[pt, sq] = ComputeKnightAttack(b);

                var bishopAttacks = s1.BishopAttacks(EmptyBitBoard);
                var rookAttacks = s1.RookAttacks(EmptyBitBoard);

                pt = EPieceType.Bishop.AsInt();
                PseudoAttacksBB[pt, sq] = bishopAttacks;

                pt = EPieceType.Rook.AsInt();
                PseudoAttacksBB[pt, sq] = rookAttacks;

                pt = EPieceType.Queen.AsInt();
                PseudoAttacksBB[pt, sq] = bishopAttacks | rookAttacks;

                pt = EPieceType.King.AsInt();
                PseudoAttacksBB[pt, sq] = b.NorthOne() | b.SouthOne() | b.EastOne() | b.WestOne()
                                        | b.NorthEastOne() | b.NorthWestOne() | b.SouthEastOne() | b.SouthWestOne();

                // Compute lines and betweens
                foreach (var validMagicPiece in validMagicPieces)
                {
                    pt = validMagicPiece.AsInt();
                    foreach (var s2 in AllSquares)
                    {
                        if ((PseudoAttacksBB[pt, sq] & s2).Empty())
                            continue;

                        LineBB[sq, s2.AsInt()] = GetAttacks(s1, validMagicPiece, EmptyBitBoard) & GetAttacks(s2, validMagicPiece, EmptyBitBoard) | s1 | s2;
                        BetweenBB[sq, s2.AsInt()] = GetAttacks(s1, validMagicPiece, BbSquares[s2.AsInt()]) & GetAttacks(s2, validMagicPiece, BbSquares[sq]);
                    }
                }

                // Compute KingRings
                pt = EPieceType.King.AsInt();
                for (var side = EPlayer.White; side < EPlayer.PlayerNb; ++side)
                {
                    var c = (int)side;
                    KingRingBB[c, sq] = PseudoAttacksBB[pt, sq];
                    if (s1.RelativeRank(side) == ERank.Rank1)
                        KingRingBB[c, sq] |= KingRingBB[c, sq].Shift(side == EPlayer.White ? Directions.North : Directions.South);

                    if (file == EFile.FileH)
                        KingRingBB[c, sq] |= KingRingBB[c, sq].WestOne();
                    else if (file == EFile.FileA)
                        KingRingBB[c, sq] |= KingRingBB[c, sq].EastOne();

                    Debug.Assert(!KingRingBB[c, sq].Empty());
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard XrayBishopAttacks(this Square square, BitBoard occupied, BitBoard blockers)
        {
            var attacks = square.BishopAttacks(occupied);
            blockers &= attacks;
            return attacks ^ square.BishopAttacks(occupied ^ blockers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard XrayRookAttacks(this Square square, BitBoard occupied, BitBoard blockers)
        {
            var attacks = square.RookAttacks(occupied);
            blockers &= attacks;
            return attacks ^ square.RookAttacks(occupied ^ blockers);
        }

        public static BitBoard XrayAttacks(this Square square, EPieceType pieceType, BitBoard occupied, BitBoard blockers)
        {
            switch (pieceType)
            {
                case EPieceType.Bishop:
                    return square.XrayBishopAttacks(occupied, blockers);
                case EPieceType.Rook:
                    return square.XrayRookAttacks(occupied, blockers);
                case EPieceType.Queen:
                    return XrayBishopAttacks(square, occupied, blockers) | XrayRookAttacks(square, occupied, blockers);
                default:
                    return EmptyBitBoard;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard KnightAttacks(this Square square) => PseudoAttacksBB[EPieceType.Knight.AsInt(), square.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard KingAttacks(this Square square) => PseudoAttacksBB[EPieceType.King.AsInt(), square.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard GetAttacks(this Square square, EPieceType pieceType, BitBoard occupied = new BitBoard())
        {
            switch (pieceType)
            {
                case EPieceType.Knight:
                case EPieceType.King:
                    return PseudoAttacksBB[pieceType.AsInt(), square.AsInt()];

                case EPieceType.Bishop:
                    return square.BishopAttacks(occupied);

                case EPieceType.Rook:
                    return square.RookAttacks(occupied);

                case EPieceType.Queen:
                    return square.QueenAttacks(occupied);

                default:
                    return EmptyBitBoard;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref BitBoard PseudoAttack(this Square @this, EPieceType pieceType) => ref PseudoAttacksBB[pieceType.AsInt(), @this.AsInt()];

        /// <summary>
        /// Attack for pawn.
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">The player side</param>
        /// <returns>ref to bitboard of attack</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref BitBoard PawnAttack(this Square @this, Player side) => ref PseudoAttacksBB[side.Side, @this.AsInt()];

        /// <summary>
        /// Returns the bitboard representation of the rank of which the square is located.
        /// </summary>
        /// <param name="sq">The square</param>
        /// <returns>The bitboard of square rank</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardRank(this Square sq) => RankBB[sq.Rank().AsInt()];

        /// <summary>
        /// Returns the bitboard representation of a rank.
        /// </summary>
        /// <param name="r">The rank</param>
        /// <returns>The bitboard of square rank</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardRank(this Rank r) => RankBB[r.AsInt()];

        /// <summary>
        /// Returns the bitboard representation of the file of which the square is located.
        /// </summary>
        /// <param name="this">The square</param>
        /// <returns>The bitboard of square file</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardFile(this Square @this) => FileBB[@this.File().AsInt()];

        /// <summary>
        /// Returns the bitboard representation of the file.
        /// </summary>
        /// <param name="this">The file</param>
        /// <returns>The bitboard of file</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardFile(this File @this) => FileBB[@this.AsInt()];

        /// <summary>
        /// Returns all squares in front of the square in the same file as bitboard
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">The side, white is north and black is south</param>
        /// <returns>The bitboard of all forward file squares</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard ForwardFile(this Square @this, Player side) => ForwardFileBB[side.Side, @this.AsInt()];

        /// <summary>
        /// Returns all squares in pawn attack pattern in front of the square.
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">White = north, Black = south</param>
        /// <returns>The bitboard representation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PawnAttackSpan(this Square @this, Player side) => PawnAttackSpanBB[side.Side, @this.AsInt()];

        /// <summary>
        /// Returns all square of both file and pawn attack pattern in front of square.
        /// This is the same as ForwardFile() | PawnAttackSpan().
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">White = north, Black = south</param>
        /// <returns>The bitboard representation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PassedPawnFrontAttackSpan(this Square @this, Player side) => PassedPawnMaskBB[side.Side, @this.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard ForwardRanks(this Square @this, Player side) => ForwardRanksBB[side.Side, @this.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitboardBetween(this Square firstSquare, Square secondSquare) => BetweenBB[firstSquare.AsInt(), secondSquare.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Get(this BitBoard bb, int pos) => (int)(bb.Value >> pos) & 0x1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet(this BitBoard bb, int pos) => (bb.Value & (One << pos)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square First(this BitBoard bb) => bb.Lsb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Last(this BitBoard bb) => bb.Msb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Line(this Square s1, Square s2) => LineBB[s1.AsInt(), s2.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Aligned(this Square s1, Square s2, Square s3) => (Line(s1, s2) & s3) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard KingRing(this Square sq, Player side) => KingRingBB[side.Side, sq.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Distance(this Square source, Square destination) => SquareDistance[source.AsInt(), destination.AsInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard DistanceRing(this Square square, int length) => DistanceRingBB[square.AsInt(), length];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PromotionRank(this Player us) => PromotionRanks[us.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToString(this BitBoard bb, TextWriter outputWriter)
        {
            try
            {
                outputWriter.WriteLine(bb);
            }
            catch (IOException ioException)
            {
                throw new IOException("Writer is not available.", ioException);
            }
        }

        public static string PrintBitBoard(BitBoard b, string title = "")
        {
            var s = new StringBuilder("+---+---+---+---+---+---+---+---+---+\n", 1024);
            if (!string.IsNullOrWhiteSpace(title))
                s.AppendLine($"| {title}");
            for (var r = ERank.Rank8; r >= ERank.Rank1; --r)
            {
                s.AppendFormat("| {0} ", (int)r + 1);
                for (var f = EFile.FileA; f <= EFile.FileH; ++f)
                    s.AppendFormat("| {0} ", (b & new Square(r, f)).Empty() ? ' ' : 'X');
                s.AppendLine("|\n+---+---+---+---+---+---+---+---+---+");
            }

            s.AppendLine("|   | A | B | C | D | E | F | G | H |");
            s.AppendLine("+---+---+---+---+---+---+---+---+---+");
            return s.ToString();
        }

        /// <summary>
        /// Retrieves the least significant bit in a ulong word.
        /// </summary>
        /// <param name="bb">The word to get lsb from</param>
        /// <returns>The index of the found bit</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Lsb(this BitBoard bb)
        {
            // @ C author Matt Taylor (2003)
            bb ^= bb - 1;
            var folded = (uint)(bb ^ (bb >> 32));
            return Lsb64Table[folded * 0x78291ACF >> 26];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Msb(this BitBoard bb)
        {
            const ulong debruijn64 = 0x03f79d71b4cb0a89UL;
            bb |= bb >> 1;
            bb |= bb >> 2;
            bb |= bb >> 4;
            bb |= bb >> 8;
            bb |= bb >> 16;
            bb |= bb >> 32;
            return Msb64Table[(bb.Value * debruijn64) >> 58];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard NorthOne(this BitBoard bb) => bb << 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard SouthOne(this BitBoard bb) => bb >> 8;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard EastOne(this BitBoard bb) => (bb & ~FILEH) << 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard WestOne(this BitBoard bb) => (bb & ~FILEA) >> 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard SouthEastOne(this BitBoard bb) => (bb & ~FILEH) >> 7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard SouthWestOne(this BitBoard bb) => (bb & ~FILEA) >> 9;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard NorthWestOne(this BitBoard bb) => (bb & ~FILEA) << 7;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard NorthEastOne(this BitBoard bb) => (bb & ~FILEH) << 9;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard NorthFill(this BitBoard bb)
        {
            bb |= bb << 8;
            bb |= bb << 16;
            bb |= bb << 32;
            return bb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard SouthFill(this BitBoard bb)
        {
            bb |= bb >> 8;
            bb |= bb >> 16;
            bb |= bb >> 32;
            return bb;
        }

        /// <summary>
        /// Shorthand method for north or south fill of bitboard depending on color
        /// </summary>
        /// <param name="bb">The bitboard to fill</param>
        /// <param name="side">The direction to fill in, white = north, black = south</param>
        /// <returns>Filled bitboard</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Fill(this BitBoard bb, Player side) => side == EPlayer.White ? bb.NorthFill() : bb.SouthFill();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Shift(this BitBoard bb, Direction direction)
        {
            if (ShiftFuncs.TryGetValue(direction.Value, out var func))
                return func(bb);

            throw new ArgumentException("Invalid shift argument.", nameof(direction));
        }

        /* non extension methods */

        /// <summary>
        /// Reset the least significant bit in-place
        /// </summary>
        /// <param name="bb">The bitboard as reference</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetLsb(ref BitBoard bb) => bb &= bb - 1;

        /// <summary>
        /// Counts bit set in a specified BitBoard
        /// </summary>
        /// <param name="bb">The ulong bit representation to count</param>
        /// <returns>The number of bits found</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(BitBoard bb)
        {
            var y = 0;
            while (bb)
            {
                y++;
                ResetLsb(ref bb);
            }

            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Rank7(this Player player) => Rank7BitBoards[player.Side];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard Rank3(this Player player) => Rank3BitBoards[player.Side];

        /// <summary>
        /// Generate a bitboard based on a variadic amount of squares.
        /// </summary>
        /// <param name="squares">The squares to generate bitboard from</param>
        /// <returns>The generated bitboard</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard MakeBitboard(params Square[] squares) => squares.Aggregate(EmptyBitBoard, (current, t) => current | t);

        /// <summary>
        /// Helper method to generate shift function dictionary for all directions.
        /// </summary>
        /// <returns>The generated shift dictionary</returns>
        private static IDictionary<Directions, Func<BitBoard, BitBoard>> MakeShiftFuncs()
        {
            var sf = new Dictionary<Directions, Func<BitBoard, BitBoard>>(13)
            {
                {Directions.NoDirection, board => board},
                {Directions.North, board => board.NorthOne()},
                {Directions.NorthDouble, board => board.NorthOne().NorthOne()},
                {Directions.NorthEast, board => board.NorthEastOne()},
                {Directions.NorthWest, board => board.NorthWestOne()},
                {Directions.NorthFill, board => board.NorthFill()},
                {Directions.South, board => board.SouthOne()},
                {Directions.SouthDouble, board => board.SouthOne().SouthOne()},
                {Directions.SouthEast, board => board.SouthEastOne()},
                {Directions.SouthWest, board => board.SouthWestOne()},
                {Directions.SouthFill, board => board.SouthFill()},
                {Directions.East, board => board.EastOne()},
                {Directions.West, board => board.WestOne()}
            };

            return sf;
        }
    }
}