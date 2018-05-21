﻿/*
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

// ReSharper disable InconsistentNaming

namespace Rudz.Chess
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Enums;
    using Types;

    public static class BitBoards
    {
        // TODO : Redesign some of these arrays for better memory and/or cacheline use.
        
        public const int Zero = 0x0;

        public const ulong ZeroBb = 0x0u;

        public const ulong WhiteArea = 0x00000000FFFFFFFF;

        public const ulong BlackArea = ~WhiteArea;

        public const ulong DarkSquares = ~LightSquares;

        public static readonly BitBoard AllSquares = WhiteArea | BlackArea;

        public static readonly BitBoard PromotionRanks = RANK8 | RANK1;

        public static readonly BitBoard[] PawnCaptures = new BitBoard[128];

        public static readonly BitBoard[] KnightAttacks = new BitBoard[64];

        public static readonly BitBoard[] KingAttacks = new BitBoard[64];

        public static BitBoard CornerA1;

        public static BitBoard CornerA8;

        public static BitBoard CornerH1;

        public static BitBoard CornerH8;

        internal const ulong One = 0x1ul;

        internal static readonly BitBoard[] BbSquares =
            {
                0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000, 0x8000000,
                0x10000000, 0x20000000, 0x40000000, 0x80000000, 0x100000000, 0x200000000, 0x400000000, 0x800000000, 0x1000000000, 0x2000000000, 0x4000000000, 0x8000000000, 0x10000000000, 0x20000000000, 0x40000000000, 0x80000000000,
                0x100000000000, 0x200000000000, 0x400000000000, 0x800000000000, 0x1000000000000, 0x2000000000000, 0x4000000000000, 0x8000000000000, 0x10000000000000, 0x20000000000000, 0x40000000000000, 0x80000000000000, 0x100000000000000,
                0x200000000000000, 0x400000000000000, 0x800000000000000, 0x1000000000000000, 0x2000000000000000, 0x4000000000000000, 0x8000000000000000
            };

        private const ulong FILEA = 0x0101010101010101;

        private const ulong FILEB = 0x0202020202020202;

        private const ulong FILEC = 0x2020202020202020;

        private const ulong FILED = 0x1010101010101010;

        private const ulong FILEE = 0x808080808080808;

        private const ulong FILEF = 0x404040404040404;

        private const ulong FILEG = 0x4040404040404040;

        private const ulong FILEH = 0x8080808080808080;

        private const ulong RANK1 = 0x00000000000000ff;

        private const ulong RANK2 = 0x000000000000ff00;

        private const ulong RANK3 = 0x0000000000ff0000;

        private const ulong RANK4 = 0x00000000ff000000;

        private const ulong RANK5 = 0x000000ff00000000;

        private const ulong RANK6 = 0x0000ff0000000000;

        private const ulong RANK7 = 0x00ff000000000000;

        private const ulong RANK8 = 0xff00000000000000;

        private const ulong LightSquares = 0x55AA55AA55AA55AA;

        private static readonly ulong[] FileBB =
            {
                FILEA, FILEB, FILEC, FILED, FILEE, FILEF, FILEG, FILEH
            };

        private static readonly ulong[] RankBB =
            {
                RANK1, RANK2, RANK3, RANK4, RANK5, RANK6, RANK7, RANK8
            };

        private static readonly ulong[] Rank1 =
            {
                RANK1, RANK8
            };

        private static readonly ulong[] Rank3BitBoards =
            {
                RANK3, RANK6
            };

        private static readonly BitBoard[] Rank7BitBoards =
            {
                RANK7, RANK2
            };

        private static readonly ulong[] Rank6And7 =
            {
                RANK6 | RANK7, RANK2 | RANK3
            };

        private static readonly ulong[] Rank7And8 =
            {
                RANK7 | RANK8, RANK1 | RANK2
            };

        private static readonly int[] Lsb64Table =
            {
                63, 30, 3, 32, 59, 14, 11, 33, 60, 24, 50, 9, 55, 19, 21, 34, 61, 29, 2, 53, 51, 23, 41, 18, 56, 28, 1, 43, 46, 27, 0, 35, 62, 31, 58, 4, 5, 49, 54, 6, 15, 52, 12, 40, 7, 42, 45,
                16, 25, 57, 48, 13, 10, 39, 8, 44, 20, 47, 38, 22, 17, 37, 36, 26
            };

        private static readonly int[] Msb64Table =
            {
                0, 47, 1, 56, 48, 27, 2, 60, 57, 49, 41, 37, 28, 16, 3, 61, 54, 58, 35, 52, 50, 42, 21, 44, 38, 32, 29, 23, 17, 11, 4, 62, 46, 55, 26, 59, 40, 36, 15, 53, 34, 51, 20, 43, 31, 22,
                10, 45, 25, 39, 14, 33, 19, 30, 9, 24, 13, 18, 8, 12, 7, 6, 5, 63
            };

        private static readonly BitBoard[] AdjacentFilesBB = { FILEB, FILEA | FILEC, FILEB | FILED, FILEC | FILEE, FILED | FILEF, FILEE | FILEG, FILEF | FILEH, FILEG };
        
        private static readonly BitBoard[,] BetweenBB;

        private static readonly BitBoard[,] PawnAttackSpanBB;

        private static readonly BitBoard[,] PassedPawnMaskBB;

        private static readonly BitBoard[,] ForwardRanksBB;
        
        private static readonly BitBoard[,] ForwardFileBB;

        static BitBoards()
        {
            BetweenBB = new BitBoard[64, 64];
            PawnAttackSpanBB = new BitBoard[2, 64];
            PassedPawnMaskBB = new BitBoard[2, 64];
            ForwardRanksBB = new BitBoard[2, 64];
            ForwardFileBB = new BitBoard[2, 64];

            // ForwardRanksBB population loop idea from sf
            for (ERank r = ERank.Rank1; r < ERank.RankNb; ++r)
                ForwardRanksBB[0, (int)r] = ~(ForwardRanksBB[1, (int)r + 1] = ForwardRanksBB[1, (int)r] | RankBB[(int)r]);
            
            for (EPlayer side = EPlayer.White; side < EPlayer.PlayerNb; ++side) {
                int c = (int) side;
                foreach (Square square in AllSquares) {
                    int s = square.ToInt();
                    ForwardFileBB[c, s]    = ForwardRanksBB[c, (int)square.RankOf()] & FileBB[square.File()];
                    PawnAttackSpanBB[c, s] = ForwardRanksBB[c, (int)square.RankOf()] & AdjacentFilesBB[square.File()];
                    PassedPawnMaskBB[c, s] = ForwardFileBB [c, s] | PawnAttackSpanBB[c, s];
                }
            }
            
            foreach (Square sq in AllSquares) {
                foreach (Square fillSq in AllSquares)
                    BetweenBB[sq.ToInt(), fillSq.ToInt()] = 0;

                InitBetweenBitboards(sq, NorthOne, 8);
                InitBetweenBitboards(sq, NorthEastOne, 9);
                InitBetweenBitboards(sq, EastOne, 1);
                InitBetweenBitboards(sq, SouthEastOne, -7);
                InitBetweenBitboards(sq, SouthOne, -8);
                InitBetweenBitboards(sq, SouthWestOne, -9);
                InitBetweenBitboards(sq, WestOne, -1);
                InitBetweenBitboards(sq, NorthWestOne, 7);

                PawnCaptures[sq.ToInt()] = (sq.BitBoardSquare() & ~FILEH) << 9;
                PawnCaptures[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEA) << 7;
                PawnCaptures[sq.ToInt() + 64] = (sq.BitBoardSquare() & ~FILEA) >> 9;
                PawnCaptures[sq.ToInt() + 64] |= (sq.BitBoardSquare() & ~FILEH) >> 7;

                KnightAttacks[sq.ToInt()] = (sq.BitBoardSquare() & ~(FILEA | FILEB)) << 6;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEA) << 15;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEH) << 17;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~(FILEG | FILEH)) << 10;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~(FILEG | FILEH)) >> 6;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEH) >> 15;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEA) >> 17;
                KnightAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~(FILEA | FILEB)) >> 10;

                KingAttacks[sq.ToInt()] = (sq.BitBoardSquare() & ~FILEA) >> 1;
                KingAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEA) << 7;
                KingAttacks[sq.ToInt()] |= sq.BitBoardSquare() << 8;
                KingAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEH) << 9;
                KingAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEH) << 1;
                KingAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEH) >> 7;
                KingAttacks[sq.ToInt()] |= sq.BitBoardSquare() >> 8;
                KingAttacks[sq.ToInt()] |= (sq.BitBoardSquare() & ~FILEA) >> 9;
            }

            CornerA1 = MakeBitboard(ESquare.a1, ESquare.b1, ESquare.a2, ESquare.b2);
            CornerA8 = MakeBitboard(ESquare.a8, ESquare.b8, ESquare.a7, ESquare.b7);
            CornerH1 = MakeBitboard(ESquare.h1, ESquare.g1, ESquare.h2, ESquare.g2);
            CornerH8 = MakeBitboard(ESquare.h8, ESquare.g8, ESquare.h7, ESquare.g7);
        }

        /// <summary>
        /// Returns the bitboard representation of the rank of which the square is located.
        /// </summary>
        /// <param name="sq">The square</param>
        /// <returns>The bitboard of square rank</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardRank(this Square sq) => RankBB[(int)sq.RankOf()];

        /// <summary>
        /// Returns the bitboard representaion of the file of which the square is located.
        /// </summary>
        /// <param name="this">The square</param>
        /// <returns>The bitboard of square file</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitBoardFile(this Square @this) => FileBB[@this.File()];

        /// <summary>
        /// Returns all squares in front of the square in the same file as bitboard
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">The side, white is north and black is south</param>
        /// <returns>The bitboard of all forward file squares</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard ForwardFile(this Square @this, Player side) => ForwardFileBB[side.Side, @this.ToInt()];

        /// <summary>
        /// Returns all squares in pawn attack pattern in front of the square.
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">White = north, Black = south</param>
        /// <returns>The bitboard representation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PawnAttackSpan(this Square @this, Player side) => PawnAttackSpanBB[side.Side, @this.ToInt()];

        /// <summary>
        /// Returns all square of both file and pawn attack pattern in front of square.
        /// This is the same as ForwardFile() | PawnAttackSpan().
        /// </summary>
        /// <param name="this">The square</param>
        /// <param name="side">White = north, Black = south</param>
        /// <returns>The bitboard representation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard PassedPawnFrontAttackSpan(this Square @this, Player side) => PassedPawnMaskBB[side.Side, @this.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard ForwardRanks(this Square @this, Player side) => ForwardRanksBB[side.Side, @this.ToInt()];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard BitboardBetween(this Square firstSquare, Square secondSquare) => BetweenBB[firstSquare.ToInt(), secondSquare.ToInt()];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Get(this BitBoard bb, int pos) => (int)(bb.Value >> pos) & 0x1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Empty(this BitBoard bb) => bb.Value == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet(this BitBoard bb, int pos) => (bb.Value & (One << pos)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square First(this BitBoard bb) => bb.Lsb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Square Last(this BitBoard bb) => bb.Msb();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToString(this BitBoard bb, TextWriter outputWriter)
        {
            try {
                outputWriter.WriteLine(bb);
            } catch (IOException ioException) {
                throw new IOException("Writer is not available.", ioException);
            }
        }

        public static string PrintBitBoard(BitBoard b, string title = "")
        {
            StringBuilder s = new StringBuilder(title, 128);
            s.Append("\n+---+---+---+---+---+---+---+---+\n");
            for (int r = 7; r >= 0; r--) {
                s.Append(r + 1);
                s.Append(' ');
                for (int f = 0; f <= 7; f++) {
                    s.Append(b & BbSquares[(r << 3) + f] ? " 1 " : " . ");
                    s.Append(' ');
                }

                s.Append('\n');
            }

            s.Append("|+---+---+---+---+---+---+---+---+\n");
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
            uint folded = (uint)(bb ^ (bb >> 32));
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

        /* non extension methods */

        /// <summary>
        /// Reset the least significant bit in-place
        /// </summary>
        /// <param name="bb">The bitboard as reference</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetLsb(ref BitBoard bb) => bb &= bb - 1;

        /// <summary>
        /// Counts bit set in a specificed ulong
        /// </summary>
        /// <param name="bb">The ulong bit representation to count</param>
        /// <returns>The number of bits found</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(BitBoard bb)
        {
            int y = 0;
            while (bb) {
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
        public static BitBoard MakeBitboard(params Square[] squares)
        {
            BitBoard b = squares[0];
            for (int i = 1; i < squares.Length; ++i)
                b |= squares[i];
            return b;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitBetweenBitboards(Square from, Func<BitBoard, BitBoard> stepFunc, int step)
        {
            BitBoard bb = stepFunc(from.BitBoardSquare());
            Square to = from + step;
            BitBoard between = 0;
            while (bb)
            {
                if (!from.IsValid() || !to.IsValid())
                    continue;

                BetweenBB[from.ToInt(), to.ToInt()] = between;
                between |= bb;
                bb = stepFunc(bb);
                to += step;
            }
        }
    }
}