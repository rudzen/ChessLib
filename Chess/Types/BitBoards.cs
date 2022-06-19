/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

namespace Rudz.Chess.Types;

using Enums;
using Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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

    public static readonly BitBoard EmptyBitBoard = new(0UL);

    public static readonly BitBoard AllSquares = ~EmptyBitBoard;

    public static readonly BitBoard PawnSquares = AllSquares & ~(RANK1 | RANK8);

    public static readonly BitBoard CornerA1;

    public static readonly BitBoard CornerA8;

    public static readonly BitBoard CornerH1;

    public static readonly BitBoard CornerH8;

    public static readonly BitBoard QueenSide = new(FILEA | FILEB | FILEC | FILED);

    public static readonly BitBoard CenterFiles = new(FILEC | FILED | FILEE | FILEF);

    public static readonly BitBoard KingSide = new(FILEE | FILEF | FILEG | FILEH);

    public static readonly BitBoard Center = new((FILED | FILEE) & (RANK4 | RANK5));

    public static readonly BitBoard[] PromotionRanks = { RANK8, RANK1 };

    public static readonly BitBoard PromotionRanksBB = RANK1 | RANK8;

    internal static readonly BitBoard[] BbSquares =
        {
                0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000, 0x20000, 0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000, 0x8000000,
                0x10000000, 0x20000000, 0x40000000, 0x80000000, 0x100000000, 0x200000000, 0x400000000, 0x800000000, 0x1000000000, 0x2000000000, 0x4000000000, 0x8000000000, 0x10000000000, 0x20000000000, 0x40000000000, 0x80000000000,
                0x100000000000, 0x200000000000, 0x400000000000, 0x800000000000, 0x1000000000000, 0x2000000000000, 0x4000000000000, 0x8000000000000, 0x10000000000000, 0x20000000000000, 0x40000000000000, 0x80000000000000, 0x100000000000000,
                0x200000000000000, 0x400000000000000, 0x800000000000000, 0x1000000000000000, 0x2000000000000000, 0x4000000000000000, 0x8000000000000000
            };

    private static readonly BitBoard[] Rank1 = { RANK1, RANK8 };

    private static readonly BitBoard[] Rank3BitBoards = { RANK3, RANK6 };

    private static readonly BitBoard[] Rank7BitBoards = { RANK7, RANK2 };

    private static readonly BitBoard[] Rank6And7 = { RANK6 | RANK7, RANK2 | RANK3 };

    private static readonly BitBoard[] Rank7And8 = { RANK7 | RANK8, RANK1 | RANK2 };

    /// <summary>
    /// PseudoAttacks are just that, full attack range for all squares for all pieces. The pawns
    /// are a special case, as index range 0,sq are for White and 1,sq are for Black. This is
    /// possible because index 0 is NoPiece type.
    /// </summary>
    private static readonly BitBoard[][] PseudoAttacksBB;

    private static readonly BitBoard[][] PawnAttackSpanBB;

    private static readonly BitBoard[][] PassedPawnMaskBB;

    private static readonly BitBoard[][] ForwardRanksBB;

    private static readonly BitBoard[][] ForwardFileBB;

    private static readonly BitBoard[][] KingRingBB;

    private static readonly BitBoard[][] BetweenBB;

    private static readonly BitBoard[][] LineBB;

    private static readonly int[][] SquareDistance; // chebyshev distance

    private static readonly BitBoard[][] DistanceRingBB;

    private static readonly IDictionary<Direction, Func<BitBoard, BitBoard>> ShiftFuncs = MakeShiftFuncs();

    private static readonly Func<BitBoard, BitBoard>[] FillFuncs = MakeFillFuncs();

    static BitBoards()
    {
        PseudoAttacksBB = new BitBoard[PieceTypes.PieceTypeNb.AsInt()][];
        for (var i = 0; i < PseudoAttacksBB.Length; i++)
            PseudoAttacksBB[i] = new BitBoard[64];

        PawnAttackSpanBB = new BitBoard[2][];
        PawnAttackSpanBB[0] = new BitBoard[64];
        PawnAttackSpanBB[1] = new BitBoard[64];

        PassedPawnMaskBB = new BitBoard[2][];
        PassedPawnMaskBB[0] = new BitBoard[64];
        PassedPawnMaskBB[1] = new BitBoard[64];

        ForwardRanksBB = new BitBoard[2][];
        ForwardRanksBB[0] = new BitBoard[64];
        ForwardRanksBB[1] = new BitBoard[64];

        ForwardFileBB = new BitBoard[2][];
        ForwardFileBB[0] = new BitBoard[64];
        ForwardFileBB[1] = new BitBoard[64];

        KingRingBB = new BitBoard[2][];
        KingRingBB[0] = new BitBoard[64];
        KingRingBB[1] = new BitBoard[64];

        BetweenBB = new BitBoard[64][];
        for (var i = 0; i < BetweenBB.Length; i++)
            BetweenBB[i] = new BitBoard[64];

        LineBB = new BitBoard[64][];
        for (var i = 0; i < LineBB.Length; i++)
            LineBB[i] = new BitBoard[64];

        SquareDistance = new int[64][];
        for (var i = 0; i < SquareDistance.Length; i++)
            SquareDistance[i] = new int[64];

        DistanceRingBB = new BitBoard[64][];
        for (var i = 0; i < SquareDistance.Length; i++)
            DistanceRingBB[i] = new BitBoard[8];

        CornerA1 = MakeBitboard(Squares.a1, Squares.b1, Squares.a2, Squares.b2);
        CornerA8 = MakeBitboard(Squares.a8, Squares.b8, Squares.a7, Squares.b7);
        CornerH1 = MakeBitboard(Squares.h1, Squares.g1, Squares.h2, Squares.g2);
        CornerH8 = MakeBitboard(Squares.h8, Squares.g8, Squares.h7, Squares.g7);

        // local helper functions to calculate distance
        static int distance(int x, int y) => Math.Abs(x - y);
        static int distanceFile(Square x, Square y) => distance(x.File.AsInt(), y.File.AsInt());
        static int distanceRank(Square x, Square y) => distance(x.Rank.AsInt(), y.Rank.AsInt());

        // ForwardRanksBB population loop idea from sf
        for (var r = Ranks.Rank1; r < Ranks.RankNb; ++r)
        {
            var rank = (int)r;
            ForwardRanksBB[0][rank] = ~(ForwardRanksBB[1][rank + 1] = ForwardRanksBB[1][rank] | BitBoardRank(r));
        }

        Span<BitBoard> adjacentFilesBb = stackalloc BitBoard[] { FILEB, FILEA | FILEC, FILEB | FILED, FILEC | FILEE, FILED | FILEF, FILEE | FILEG, FILEF | FILEH, FILEG };
        Span<Player> players = stackalloc Player[] { Player.White, Player.Black };

        foreach (var player in players)
        {
            foreach (var square in AllSquares)
            {
                var s = square.AsInt();
                var file = square.File;
                var rank = square.Rank.AsInt();
                ForwardFileBB[player.Side][s] = ForwardRanksBB[player.Side][rank] & file.BitBoardFile();
                PawnAttackSpanBB[player.Side][s] = ForwardRanksBB[player.Side][rank] & adjacentFilesBb[file.AsInt()];
                PassedPawnMaskBB[player.Side][s] = ForwardFileBB[player.Side][s] | PawnAttackSpanBB[player.Side][s];
            }
        }

        // mini local helpers
        static BitBoard ComputeKnightAttack(BitBoard b)
        {
            var res = (b & ~(FILEA | FILEB)) << 6;
            res |= (b & ~FILEA) << 15;
            res |= (b & ~FILEH) << 17;
            res |= (b & ~(FILEG | FILEH)) << 10;
            res |= (b & ~(FILEG | FILEH)) >> 6;
            res |= (b & ~FILEH) >> 15;
            res |= (b & ~FILEA) >> 17;
            res |= (b & ~(FILEA | FILEB)) >> 10;
            return res;
        }

        Span<PieceTypes> validMagicPieces = stackalloc PieceTypes[] { PieceTypes.Bishop, PieceTypes.Rook };

        // Pseudo attacks for all pieces
        foreach (var s1 in AllSquares)
        {
            var sq = s1.AsInt();
            var b = s1.AsBb();

            var file = s1.File;

            // distance computation
            foreach (var s2 in AllSquares)
            {
                if (s1 == s2)
                    continue;
                var dist = (byte)distanceFile(s1, s2).Max(distanceRank(s1, s2));
                SquareDistance[sq][s2.AsInt()] = dist;
                DistanceRingBB[sq][dist] |= s2;
            }

            PseudoAttacksBB[0][sq] = b.NorthEastOne() | b.NorthWestOne();
            PseudoAttacksBB[1][sq] = b.SouthWestOne() | b.SouthEastOne();

            var pt = PieceTypes.Knight.AsInt();
            PseudoAttacksBB[pt][sq] = ComputeKnightAttack(b);

            var bishopAttacks = s1.BishopAttacks(EmptyBitBoard);
            var rookAttacks = s1.RookAttacks(EmptyBitBoard);

            pt = PieceTypes.Bishop.AsInt();
            PseudoAttacksBB[pt][sq] = bishopAttacks;

            pt = PieceTypes.Rook.AsInt();
            PseudoAttacksBB[pt][sq] = rookAttacks;

            pt = PieceTypes.Queen.AsInt();
            PseudoAttacksBB[pt][sq] = bishopAttacks | rookAttacks;

            pt = PieceTypes.King.AsInt();
            PseudoAttacksBB[pt][sq] = b.NorthOne() | b.SouthOne() | b.EastOne() | b.WestOne()
                                    | b.NorthEastOne() | b.NorthWestOne() | b.SouthEastOne() | b.SouthWestOne();

            // Compute lines and betweens
            foreach (var validMagicPiece in validMagicPieces)
            {
                pt = validMagicPiece.AsInt();
                foreach (var s2 in AllSquares)
                {
                    if ((PseudoAttacksBB[pt][sq] & s2).IsEmpty)
                        continue;

                    var sq2 = s2.AsInt();

                    LineBB[sq][sq2] = GetAttacks(s1, validMagicPiece, EmptyBitBoard) & GetAttacks(s2, validMagicPiece, EmptyBitBoard) | s1 | s2;
                    BetweenBB[sq][sq2] = GetAttacks(s1, validMagicPiece, BbSquares[sq2]) & GetAttacks(s2, validMagicPiece, BbSquares[sq]);
                }
            }

            // Compute KingRings
            pt = PieceTypes.King.AsInt();

            foreach (var player in players)
            {
                KingRingBB[player.Side][sq] = PseudoAttacksBB[pt][sq];
                if (s1.RelativeRank(player) == Ranks.Rank1)
                    KingRingBB[player.Side][sq] |= KingRingBB[player.Side][sq].Shift(player.IsWhite ? Direction.North : Direction.South);

                if (file == Files.FileH)
                    KingRingBB[player.Side][sq] |= KingRingBB[player.Side][sq].WestOne();
                else if (file == Files.FileA)
                    KingRingBB[player.Side][sq] |= KingRingBB[player.Side][sq].EastOne();

                Debug.Assert(!KingRingBB[player.Side][sq].IsEmpty);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PseudoAttacks(this PieceTypes pt, Square sq)
        => PseudoAttacksBB[pt.AsInt()][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard XrayBishopAttacks(this Square square, in BitBoard occupied, BitBoard blockers)
    {
        var attacks = square.BishopAttacks(occupied);
        blockers &= attacks;
        return attacks ^ square.BishopAttacks(occupied ^ blockers);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard XrayRookAttacks(this Square square, in BitBoard occupied, BitBoard blockers)
    {
        var attacks = square.RookAttacks(occupied);
        blockers &= attacks;
        return attacks ^ square.RookAttacks(occupied ^ blockers);
    }

    public static BitBoard XrayAttacks(this in Square square, PieceTypes pieceType, in BitBoard occupied, in BitBoard blockers)
    {
        return pieceType switch
        {
            PieceTypes.Bishop => square.XrayBishopAttacks(occupied, blockers),
            PieceTypes.Rook => square.XrayRookAttacks(occupied, blockers),
            PieceTypes.Queen => XrayBishopAttacks(square, occupied, blockers) |
                                XrayRookAttacks(square, occupied, blockers),
            _ => EmptyBitBoard
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KnightAttacks(this Square square)
        => PseudoAttacksBB[PieceTypes.Knight.AsInt()][square.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KingAttacks(this Square square)
        => PseudoAttacksBB[PieceTypes.King.AsInt()][square.AsInt()];

    /// <summary>
    /// Attack for pawn.
    /// </summary>
    /// <param name="this">The square</param>
    /// <param name="side">The player side</param>
    /// <returns>ref to bitboard of attack</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttack(this in Square @this, in Player side)
        => PseudoAttacksBB[side.Side][@this.AsInt()];

    /// <summary>
    /// Returns the bitboard representation of the rank of which the square is located.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <returns>The bitboard of square rank</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardRank(this Square sq)
        => sq.Rank.BitBoardRank();

    /// <summary>
    /// Returns the bitboard representation of a rank.
    /// </summary>
    /// <param name="r">The rank</param>
    /// <returns>The bitboard of square rank</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardRank(this Rank r)
        => RANK1 << (8 * r.AsInt());

    /// <summary>
    /// Returns the bitboard representation of the file of which the square is located.
    /// </summary>
    /// <param name="this">The square</param>
    /// <returns>The bitboard of square file</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardFile(this Square @this)
        => @this.File.BitBoardFile();

    /// <summary>
    /// Returns the bitboard representation of the file.
    /// </summary>
    /// <param name="this">The file</param>
    /// <returns>The bitboard of file</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardFile(this File @this)
        => FILEA << @this.AsInt();

    /// <summary>
    /// Returns all squares in front of the square in the same file as bitboard
    /// </summary>
    /// <param name="this">The square</param>
    /// <param name="side">The side, white is north and black is south</param>
    /// <returns>The bitboard of all forward file squares</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ForwardFile(this Square @this, Player side)
        => ForwardFileBB[side.Side][@this.AsInt()];

    /// <summary>
    /// Returns all squares in pawn attack pattern in front of the square.
    /// </summary>
    /// <param name="this">The square</param>
    /// <param name="side">White = north, Black = south</param>
    /// <returns>The bitboard representation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttackSpan(this Square @this, Player side)
        => PawnAttackSpanBB[side.Side][@this.AsInt()];

    /// <summary>
    /// Returns all square of both file and pawn attack pattern in front of square. This is the
    /// same as ForwardFile() | PawnAttackSpan().
    /// </summary>
    /// <param name="this">The square</param>
    /// <param name="side">White = north, Black = south</param>
    /// <returns>The bitboard representation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PassedPawnFrontAttackSpan(this Square @this, Player side)
        => PassedPawnMaskBB[side.Side][@this.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ForwardRanks(this Square @this, Player side)
        => ForwardRanksBB[side.Side][@this.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitboardBetween(this Square firstSquare, Square secondSquare)
        => BetweenBB[firstSquare.AsInt()][secondSquare.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Get(this in BitBoard bb, int pos)
        => (int)(bb.Value >> pos) & 0x1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSet(this in BitBoard bb, int pos)
        => (bb.Value & (One << pos)) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square First(this in BitBoard bb)
        => bb.Lsb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Last(this in BitBoard bb)
        => bb.Msb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Line(this Square s1, Square s2)
        => LineBB[s1.AsInt()][s2.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Aligned(this Square s1, Square s2, Square s3)
        => (Line(s1, s2) & s3) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KingRing(this Square sq, Player side)
        => KingRingBB[side.Side][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance(this Square source, Square destination)
        => SquareDistance[source.AsInt()][destination.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard DistanceRing(this Square square, int length)
        => DistanceRingBB[square.AsInt()][length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PromotionRank(this Player us)
        => PromotionRanks[us.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToString(this in BitBoard bb, TextWriter outputWriter)
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

    public static string PrintBitBoard(in BitBoard b, string title = "")
    {
        var s = new StringBuilder("+---+---+---+---+---+---+---+---+---+\n", 1024);
        if (!title.IsNullOrWhiteSpace())
            s.AppendLine($"| {title}");
        for (var r = Ranks.Rank8; r >= Ranks.Rank1; --r)
        {
            s.AppendFormat("| {0} ", (int)r + 1);
            for (var f = Files.FileA; f <= Files.FileH; ++f)
                s.AppendFormat("| {0} ", (b & new Square(r, f)).IsEmpty ? ' ' : 'X');
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
    public static Square Lsb(this in BitBoard bb)
        => BitOperations.TrailingZeroCount(bb.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Msb(this in BitBoard bb)
        => 63 - BitOperations.LeadingZeroCount(bb.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard NorthOne(this BitBoard bb)
        => bb << 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SouthOne(this BitBoard bb)
        => bb >> 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard EastOne(this in BitBoard bb)
        => (bb & ~FILEH) << 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard WestOne(this in BitBoard bb)
        => (bb & ~FILEA) >> 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SouthEastOne(this in BitBoard bb)
        => (bb & ~FILEH) >> 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SouthWestOne(this in BitBoard bb)
        => (bb & ~FILEA) >> 9;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard NorthWestOne(this in BitBoard bb)
        => (bb & ~FILEA) << 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard NorthEastOne(this in BitBoard bb)
        => (bb & ~FILEH) << 9;

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
    public static BitBoard Fill(this in BitBoard bb, Player side)
        => FillFuncs[side.Side](bb);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Shift(this in BitBoard bb, Direction direction)
    {
        if (ShiftFuncs.TryGetValue(direction, out var func))
            return func(bb);

        throw new ArgumentException("Invalid shift argument.", nameof(direction));
    }

    /* non extension methods */

    /// <summary>
    /// Reset the least significant bit in-place
    /// </summary>
    /// <param name="bb">The bitboard as reference</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetLsb(ref BitBoard bb)
        => bb &= bb - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square PopLsb(ref BitBoard bb)
    {
        var sq = bb.Lsb();
        ResetLsb(ref bb);
        return sq;
    }

    /// <summary>
    /// Counts bit set in a specified BitBoard
    /// </summary>
    /// <param name="bb">The ulong bit representation to count</param>
    /// <returns>The number of bits found</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopCount(in BitBoard bb)
        => BitOperations.PopCount(bb.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Rank7(this in Player player)
        => Rank7BitBoards[player.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Rank3(this in Player player)
        => Rank3BitBoards[player.Side];

    /// <summary>
    /// Generate a bitboard based on a variadic amount of squares.
    /// </summary>
    /// <param name="squares">The squares to generate bitboard from</param>
    /// <returns>The generated bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard MakeBitboard(params Square[] squares)
        => squares.Aggregate(EmptyBitBoard, (current, t) => current | t);

    /// <summary>
    /// Helper method to generate shift function dictionary for all directions.
    /// </summary>
    /// <returns>The generated shift dictionary</returns>
    private static IDictionary<Direction, Func<BitBoard, BitBoard>> MakeShiftFuncs()
    {
        var sf = new Dictionary<Direction, Func<BitBoard, BitBoard>>(13)
            {
                {Direction.None, board => board},
                {Direction.North, board => board.NorthOne()},
                {Direction.NorthDouble, board => board.NorthOne().NorthOne()},
                {Direction.NorthEast, board => board.NorthEastOne()},
                {Direction.NorthWest, board => board.NorthWestOne()},
                {Direction.NorthFill, board => board.NorthFill()},
                {Direction.South, board => board.SouthOne()},
                {Direction.SouthDouble, board => board.SouthOne().SouthOne()},
                {Direction.SouthEast, board => board.SouthEastOne()},
                {Direction.SouthWest, board => board.SouthWestOne()},
                {Direction.SouthFill, board => board.SouthFill()},
                {Direction.East, board => board.EastOne()},
                {Direction.West, board => board.WestOne()}
            };

        return sf;
    }

    private static Func<BitBoard, BitBoard>[] MakeFillFuncs()
        => new Func<BitBoard, BitBoard>[] { NorthFill, SouthFill };

    private static BitBoard GetAttacks(this in Square square, PieceTypes pieceType, in BitBoard occupied = default)
    {
        return pieceType switch
        {
            PieceTypes.Knight => PseudoAttacksBB[pieceType.AsInt()][square.AsInt()],
            PieceTypes.King => PseudoAttacksBB[pieceType.AsInt()][square.AsInt()],
            PieceTypes.Bishop => square.BishopAttacks(occupied),
            PieceTypes.Rook => square.RookAttacks(occupied),
            PieceTypes.Queen => square.QueenAttacks(occupied),
            _ => EmptyBitBoard
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard PseudoAttack(this in Square @this, PieceTypes pieceType)
        => PseudoAttacksBB[pieceType.AsInt()][@this.AsInt()];
}
