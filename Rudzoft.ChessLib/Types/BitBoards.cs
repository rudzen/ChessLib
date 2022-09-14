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
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Extensions;

namespace Rudzoft.ChessLib.Types;

public static class BitBoards
{
    internal const ulong One = 0x1ul;

    public static readonly BitBoard WhiteArea = new(0x00000000FFFFFFFFUL);

    public static readonly BitBoard BlackArea = ~WhiteArea;

    private static readonly BitBoard LightSquares = new(0x55AA55AA55AA55AAUL);

    private static readonly BitBoard[] ColorsBB = { LightSquares, ~LightSquares };

    private static readonly BitBoard FileABB = new(0x0101010101010101UL);

    private static readonly BitBoard FileBBB = new(0x0202020202020202UL);

    private static readonly BitBoard FileCBB = new(0x404040404040404UL);

    private static readonly BitBoard FileDBB = new(0x808080808080808UL);

    private static readonly BitBoard FileEBB = new(0x1010101010101010);

    private static readonly BitBoard FileFBB = new(0x2020202020202020);

    private static readonly BitBoard FileGBB = new(0x4040404040404040UL);

    private static readonly BitBoard FileHBB = new(0x8080808080808080UL);

    private static readonly BitBoard[] FilesBB =
    {
        FileABB, FileBBB, FileCBB, FileDBB,
        FileEBB, FileFBB, FileGBB, FileHBB
    };

    private static readonly BitBoard Rank1BB = new(0x00000000000000ffUL);

    private static readonly BitBoard Rank2BB = new(0x000000000000ff00UL);

    private static readonly BitBoard Rank3BB = new(0x0000000000ff0000UL);

    private static readonly BitBoard Rank4BB = new(0x00000000ff000000UL);

    private static readonly BitBoard Rank5BB = new(0x000000ff00000000UL);

    private static readonly BitBoard Rank6BB = new(0x0000ff0000000000UL);

    private static readonly BitBoard Rank7BB = new(0x00ff000000000000UL);

    private static readonly BitBoard Rank8BB = new(0xff00000000000000UL);

    private static readonly BitBoard[] RanksBB =
    {
        Rank1BB, Rank2BB, Rank3BB, Rank4BB,
        Rank5BB, Rank6BB, Rank7BB, Rank8BB
    };

    public static readonly BitBoard EmptyBitBoard = new(ulong.MinValue);

    public static readonly BitBoard AllSquares = ~EmptyBitBoard;

    public static readonly BitBoard PawnSquares = AllSquares & ~(Rank1BB | Rank8BB);

    public static readonly BitBoard QueenSide = new(FileABB | FileBBB | FileCBB | FileDBB);

    public static readonly BitBoard CenterFiles = new(FileCBB | FileDBB | FileEBB | FileFBB);

    public static readonly BitBoard KingSide = new(FileEBB | FileFBB | FileGBB | FileHBB);

    public static readonly BitBoard Center = new((FileDBB | FileEBB) & (Rank4BB | Rank5BB));

    // A1..H8 | H1..A8
    public static readonly BitBoard DiagonalBB = new(0x8142241818244281UL);

    private static readonly BitBoard[] PromotionRanks = { Rank8BB, Rank1BB };

    public static readonly BitBoard PromotionRanksBB = Rank1BB | Rank8BB;

    internal static readonly BitBoard[] BbSquares =
    {
        0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000, 0x10000,
        0x20000, 0x40000, 0x80000, 0x100000, 0x200000, 0x400000, 0x800000, 0x1000000, 0x2000000, 0x4000000, 0x8000000,
        0x10000000, 0x20000000, 0x40000000, 0x80000000, 0x100000000, 0x200000000, 0x400000000, 0x800000000,
        0x1000000000, 0x2000000000, 0x4000000000, 0x8000000000, 0x10000000000, 0x20000000000, 0x40000000000,
        0x80000000000,
        0x100000000000, 0x200000000000, 0x400000000000, 0x800000000000, 0x1000000000000, 0x2000000000000,
        0x4000000000000, 0x8000000000000, 0x10000000000000, 0x20000000000000, 0x40000000000000, 0x80000000000000,
        0x100000000000000,
        0x200000000000000, 0x400000000000000, 0x800000000000000, 0x1000000000000000, 0x2000000000000000,
        0x4000000000000000, 0x8000000000000000
    };

    public static readonly BitBoard CornerA1 = Square.A1 | Square.B1 | Square.A2 | Square.B2;

    public static readonly BitBoard CornerA8 = Square.A8 | Square.B8 | Square.A7 | Square.B7;

    public static readonly BitBoard CornerH1 = Square.H1 | Square.G1 | Square.H2 | Square.G2;

    public static readonly BitBoard CornerH8 = Square.H8 | Square.G8 | Square.H7 | Square.G7;

    private static readonly BitBoard[] Ranks1 = { Rank1BB, Rank8BB };

    private static readonly BitBoard[] Ranks6And7BB = { Rank6BB | Rank7BB, Rank2BB | Rank3BB };

    private static readonly BitBoard[] Ranks7And8BB = { Rank7BB | Rank8BB, Rank1BB | Rank2BB };

    private static readonly BitBoard[] Ranks3BB = { Rank3BB, Rank6BB };

    private static readonly BitBoard[] Ranks7BB = { Rank7BB, Rank2BB };

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

    private static readonly BitBoard[] AdjacentFilesBB =
    {
        FileBBB, FileABB | FileCBB, FileBBB | FileDBB, FileCBB | FileEBB, FileDBB | FileFBB, FileEBB | FileGBB,
        FileFBB | FileHBB, FileGBB
    };

    private static readonly int[][] SquareDistance; // chebyshev distance

    private static readonly BitBoard[][] DistanceRingBB;

    private static readonly Direction[] PawnPushDirections = { Direction.North, Direction.South };

    private static readonly IDictionary<Direction, Func<BitBoard, BitBoard>> ShiftFuncs = MakeShiftFuncs();

    private static readonly Func<BitBoard, BitBoard>[] FillFuncs = MakeFillFuncs();

    static BitBoards()
    {
        PseudoAttacksBB = new BitBoard[PieceTypes.PieceTypeNb.AsInt()][];
        for (var i = 0; i < PseudoAttacksBB.Length; i++)
            PseudoAttacksBB[i] = new BitBoard[Square.Count];

        PawnAttackSpanBB = new BitBoard[Player.Count][];
        PawnAttackSpanBB[0] = new BitBoard[Square.Count];
        PawnAttackSpanBB[1] = new BitBoard[Square.Count];

        PassedPawnMaskBB = new BitBoard[Player.Count][];
        PassedPawnMaskBB[0] = new BitBoard[Square.Count];
        PassedPawnMaskBB[1] = new BitBoard[Square.Count];

        ForwardRanksBB = new BitBoard[Player.Count][];
        ForwardRanksBB[0] = new BitBoard[Square.Count];
        ForwardRanksBB[1] = new BitBoard[Square.Count];

        ForwardFileBB = new BitBoard[Player.Count][];
        ForwardFileBB[0] = new BitBoard[Square.Count];
        ForwardFileBB[1] = new BitBoard[Square.Count];

        KingRingBB = new BitBoard[Player.Count][];
        KingRingBB[0] = new BitBoard[Square.Count];
        KingRingBB[1] = new BitBoard[Square.Count];

        BetweenBB = new BitBoard[Square.Count][];
        for (var i = 0; i < BetweenBB.Length; i++)
            BetweenBB[i] = new BitBoard[Square.Count];

        LineBB = new BitBoard[Square.Count][];
        for (var i = 0; i < LineBB.Length; i++)
            LineBB[i] = new BitBoard[Square.Count];

        SquareDistance = new int[Square.Count][];
        for (var i = 0; i < SquareDistance.Length; i++)
            SquareDistance[i] = new int[Square.Count];

        DistanceRingBB = new BitBoard[Square.Count][];
        for (var i = 0; i < SquareDistance.Length; i++)
            DistanceRingBB[i] = new BitBoard[8];

        // local helper functions to calculate distance
        static int distance(int x, int y) => Math.Abs(x - y);
        static int distanceFile(Square x, Square y) => distance(x.File.AsInt(), y.File.AsInt());
        static int distanceRank(Square x, Square y) => distance(x.Rank.AsInt(), y.Rank.AsInt());

        // ForwardRanksBB population loop idea from sf
        foreach (var r in Rank.All)
        {
            var rank = r.AsInt();
            ForwardRanksBB[0][rank] = ~(ForwardRanksBB[1][rank + 1] = ForwardRanksBB[1][rank] | r.BitBoardRank());
        }

        BitBoard bb;

        foreach (var player in Player.AllPlayers)
        {
            bb = AllSquares;
            while (bb)
            {
                var square = PopLsb(ref bb);
                var s = square.AsInt();
                var file = square.File;
                var rank = square.Rank.AsInt();
                ForwardFileBB[player.Side][s] = ForwardRanksBB[player.Side][rank] & file.BitBoardFile();
                PawnAttackSpanBB[player.Side][s] = ForwardRanksBB[player.Side][rank] & AdjacentFilesBB[file.AsInt()];
                PassedPawnMaskBB[player.Side][s] = ForwardFileBB[player.Side][s] | PawnAttackSpanBB[player.Side][s];
            }
        }

        // mini local helpers
        static BitBoard ComputeKnightAttack(in BitBoard bb)
        {
            var res = (bb & ~(FileABB | FileBBB)) << 6;
            res |= (bb & ~FileABB) << 15;
            res |= (bb & ~FileHBB) << 17;
            res |= (bb & ~(FileGBB | FileHBB)) << 10;
            res |= (bb & ~(FileGBB | FileHBB)) >> 6;
            res |= (bb & ~FileHBB) >> 15;
            res |= (bb & ~FileABB) >> 17;
            res |= (bb & ~(FileABB | FileBBB)) >> 10;
            return res;
        }

        Span<PieceTypes> validMagicPieces = stackalloc PieceTypes[] { PieceTypes.Bishop, PieceTypes.Rook };

        bb = AllSquares;
        // Pseudo attacks for all pieces
        while (bb)
        {
            var s1 = PopLsb(ref bb);
            var sq = s1.AsInt();
            var b = s1.AsBb();

            var file = s1.File;

            var bb2 = AllSquares & ~s1;
            // distance computation
            while (bb2)
            {
                var s2 = PopLsb(ref bb2);
                var dist = (byte)distanceFile(s1, s2).Max(distanceRank(s1, s2));
                SquareDistance[sq][s2.AsInt()] = dist;
                DistanceRingBB[sq][dist] |= s2;
            }

            PseudoAttacksBB[0][sq] = b.NorthEastOne() | b.NorthWestOne();
            PseudoAttacksBB[1][sq] = b.SouthWestOne() | b.SouthEastOne();

            var pt = PieceTypes.Knight.AsInt();
            PseudoAttacksBB[pt][sq] = ComputeKnightAttack(in b);

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
                var bb3 = AllSquares;
                while (bb3)
                {
                    var s2 = PopLsb(ref bb3);
                    if ((PseudoAttacksBB[pt][sq] & s2).IsEmpty)
                        continue;

                    var sq2 = s2.AsInt();

                    LineBB[sq][sq2] = (GetAttacks(s1, validMagicPiece, EmptyBitBoard) & GetAttacks(s2, validMagicPiece, EmptyBitBoard)) | s1 | s2;
                    BetweenBB[sq][sq2] = GetAttacks(s1, validMagicPiece, BbSquares[sq2]) & GetAttacks(s2, validMagicPiece, BbSquares[sq]);
                }
            }

            // Compute KingRings
            InitializeKingRing(s1, sq, file);
        }
    }

    private static void InitializeKingRing(Square s1, int sq, File file)
    {
        const int pt = (int)PieceTypes.King;
        foreach (var player in Player.AllPlayers)
        {
            KingRingBB[player.Side][sq] = PseudoAttacksBB[pt][sq];
            if (s1.RelativeRank(player) == Ranks.Rank1)
                KingRingBB[player.Side][sq] |= KingRingBB[player.Side][sq].Shift(PawnPushDirections[player.Side]);

            if (file == Files.FileH)
                KingRingBB[player.Side][sq] |= KingRingBB[player.Side][sq].WestOne();
            else if (file == Files.FileA)
                KingRingBB[player.Side][sq] |= KingRingBB[player.Side][sq].EastOne();

            Debug.Assert(!KingRingBB[player.Side][sq].IsEmpty);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FileBB(this File f)
        => FilesBB[f.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard RankBB(this Rank r)
        => RanksBB[r.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ColorBB(this Player p)
        => ColorsBB[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FirstRank(Player p)
        => Ranks1[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ThirdRank(Player p)
        => Ranks3BB[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SeventhRank(Player p)
        => Ranks7BB[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SixthAndSeventhRank(Player p)
        => Ranks6And7BB[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SeventhAndEightsRank(Player p)
        => Ranks7And8BB[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PseudoAttacks(this PieceTypes pt, Square sq)
        => PseudoAttacksBB[pt.AsInt()][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KnightAttacks(this Square sq)
        => PseudoAttacksBB[PieceTypes.Knight.AsInt()][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KingAttacks(this Square sq)
        => PseudoAttacksBB[PieceTypes.King.AsInt()][sq.AsInt()];

    /// <summary>
    /// Attack for pawn.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <param name="p">The player side</param>
    /// <returns>ref to bitboard of attack</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttack(this Square sq, Player p)
        => PseudoAttacksBB[p.Side][sq.AsInt()];

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
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard BitBoardRank(this Rank r)
        => Rank1BB << (8 * r.AsInt());

    /// <summary>
    /// Returns the bitboard representation of the file of which the square is located.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <returns>The bitboard of square file</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardFile(this Square sq)
        => sq.File.BitBoardFile();

    /// <summary>
    /// Returns the bitboard representation of the file.
    /// </summary>
    /// <param name="f">The file</param>
    /// <returns>The bitboard of file</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard BitBoardFile(this File f)
        => FileABB << f.AsInt();

    /// <summary>
    /// Returns all squares in front of the square in the same file as bitboard
    /// </summary>
    /// <param name="sq">The square</param>
    /// <param name="p">The side, white is north and black is south</param>
    /// <returns>The bitboard of all forward file squares</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ForwardFile(this Square sq, Player p)
        => ForwardFileBB[p.Side][sq.AsInt()];

    /// <summary>
    /// Returns all squares in pawn attack pattern in front of the square.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <param name="p">White = north, Black = south</param>
    /// <returns>The bitboard representation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttackSpan(this Square sq, Player p)
        => PawnAttackSpanBB[p.Side][sq.AsInt()];

    /// <summary>
    /// Returns all square of both file and pawn attack pattern in front of square. This is the
    /// same as ForwardFile() | PawnAttackSpan().
    /// </summary>
    /// <param name="sq">The square</param>
    /// <param name="p">White = north, Black = south</param>
    /// <returns>The bitboard representation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PassedPawnFrontAttackSpan(this Square sq, Player p)
        => PassedPawnMaskBB[p.Side][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ForwardRanks(this Square sq, Player p)
        => ForwardRanksBB[p.Side][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitboardBetween(this Square sq1, Square sq2)
        => BetweenBB[sq1.AsInt()][sq2.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Get(this in BitBoard bb, int pos)
        => (int)(bb.Value >> pos) & 0x1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSet(this in BitBoard bb, int pos)
        => (bb.Value & (One << pos)) != ulong.MinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square First(this in BitBoard bb)
        => bb.Lsb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Last(this in BitBoard bb)
        => bb.Msb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Line(this Square sq1, Square sq2)
        => LineBB[sq1.AsInt()][sq2.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard AdjacentFiles(File f)
        => AdjacentFilesBB[f.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool Aligned(this Square sq1, Square sq2, Square sq3)
        => !(Line(sq1, sq2) & sq3).IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FrontSquares(this Player p, Square sq)
        => ForwardRanksBB[p.Side][sq.AsInt()] & sq.BitBoardFile();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KingRing(this Square sq, Player p)
        => KingRingBB[p.Side][sq.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance(this Square sq1, Square sq2)
        => SquareDistance[sq1.AsInt()][sq2.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard DistanceRing(this Square sq, int length)
        => DistanceRingBB[sq.AsInt()][length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PromotionRank(this Player p)
        => PromotionRanks[p.Side];

    public static string PrintBitBoard(in BitBoard bb, string title = "")
    {
        const string line = "+---+---+---+---+---+---+---+---+---+";
        const string buttom = "|   | A | B | C | D | E | F | G | H |";
        Span<char> span = stackalloc char[768];
        var idx = 0;
        foreach (var c in line)
            span[idx++] = c;
        span[idx++] = '\n';
        foreach (var c in title.Length > 64 ? title.AsSpan()[..64] : title.AsSpan())
            span[idx++] = c;

        for (var r = Ranks.Rank8; r >= Ranks.Rank1; --r)
        {
            span[idx++] = '|';
            span[idx++] = ' ';
            span[idx++] = (char)('0' + (int)r + 1);
            span[idx++] = ' ';
            for (var f = Files.FileA; f <= Files.FileH; ++f)
            {
                span[idx++] = '|';
                span[idx++] = ' ';
                span[idx++] = (bb & new Square(r, f)).IsEmpty ? ' ' : 'X';;
                span[idx++] = ' ';
            }

            span[idx++] = '|';
            span[idx++] = '\n';
            foreach (var c in line)
                span[idx++] = c;
            span[idx++] = '\n';
        }

        foreach (var c in buttom)
            span[idx++] = c;

        span[idx++] = '\n';

        foreach (var c in line)
            span[idx++] = c;

        span[idx] = '\n';

        return new string(span[..idx]);
    }

    /// <summary>
    /// Retrieves the least significant bit in an ulong word.
    /// </summary>
    /// <param name="bb">The word to get lsb from</param>
    /// <returns>The index of the found bit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Lsb(this in BitBoard bb)
        => new(BitOperations.TrailingZeroCount(bb.Value));

    /// <summary>
    /// Retrieves the least significant bit in an int word.
    /// </summary>
    /// <param name="v">The word to get lsb from</param>
    /// <returns>The index of the found bit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Lsb(this int v)
        => BitOperations.TrailingZeroCount(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Msb(this in BitBoard bb)
        => new(63 - BitOperations.LeadingZeroCount(bb.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square FrontMostSquare(in BitBoard bb, Player p)
        => p.IsWhite ? Lsb(in bb) : Msb(in bb);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard NorthOne(this BitBoard bb)
        => bb << 8;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard SouthOne(this BitBoard bb)
        => bb >> 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard EastOne(this in BitBoard bb)
        => (bb & ~FileHBB) << 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard WestOne(this in BitBoard bb)
        => (bb & ~FileABB) >> 1;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard SouthEastOne(this in BitBoard bb)
        => (bb & ~FileHBB) >> 7;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard SouthWestOne(this in BitBoard bb)
        => (bb & ~FileABB) >> 9;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard NorthWestOne(this in BitBoard bb)
        => (bb & ~FileABB) << 7;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard NorthEastOne(this in BitBoard bb)
        => (bb & ~FileHBB) << 9;

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
    /// <param name="p">The direction to fill in, white = north, black = south</param>
    /// <returns>Filled bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Fill(this in BitBoard bb, Player p)
        => FillFuncs[p.Side](bb);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard Shift(this in BitBoard bb, Direction d)
    {
        if (ShiftFuncs.TryGetValue(d, out var func))
            return func(bb);

        throw new ArgumentException("Invalid shift argument.", nameof(d));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnEastAttack(this in BitBoard bb, Player p)
        => Shift(in bb, p.PawnEastAttackDistance());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnWestAttack(this in BitBoard bb, Player p)
        => Shift(in bb, p.PawnWestAttackDistance());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttacks(this in BitBoard bb, Player p)
        => PawnEastAttack(in bb, p) | PawnWestAttack(in bb, p);

    /// <summary>
    /// Compute all attack squares where two pawns attack the square
    /// </summary>
    /// <param name="bb">The squares to compute attacks from</param>
    /// <param name="p">The side</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnDoubleAttacks(this in BitBoard bb, Player p)
        => PawnEastAttack(in bb, p) & PawnWestAttack(in bb, p);

    /// <summary>
    /// Reset the least significant bit in-place
    /// </summary>
    /// <param name="bb">The bitboard as reference</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void ResetLsb(ref BitBoard bb)
        => bb &= bb - 1;

    /// <summary>
    /// Reset the least significant bit in-place
    /// </summary>
    /// <param name="v">The integer as reference</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetLsb(ref int v)
        => v &= v - 1;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Square PopLsb(ref BitBoard bb)
    {
        var sq = bb.Lsb();
        ResetLsb(ref bb);
        return sq;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int PopLsb(ref int v)
    {
        var i = v.Lsb();
        ResetLsb(ref v);
        return i;
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
    public static BitBoard Rank7(this Player p)
        => Ranks7BB[p.Side];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Rank3(this Player p)
        => Ranks3BB[p.Side];

    /// <summary>
    /// Generate a bitboard based on a square.
    /// </summary>
    /// <param name="sq">The square to generate bitboard from</param>
    /// <returns>The generated bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard MakeBitboard(Square sq)
        => sq.AsBb();

    /// <summary>
    /// Generate a bitboard based on two squares.
    /// </summary>
    /// <param name="sq">The square to generate bitboard from</param>
    /// <param name="sq2">The second square to generate bitboard from</param>
    /// <returns>The generated bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard MakeBitboard(Square sq, Square sq2)
        => sq.AsBb() | sq2.AsBb();

    /// <summary>
    /// Generate a bitboard based on a variadic amount of squares.
    /// </summary>
    /// <param name="sqs">The squares to generate bitboard from</param>
    /// <returns>The generated bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard MakeBitboard(params Square[] sqs)
        => sqs.Aggregate(EmptyBitBoard, static (current, t) => current | t);

    /// <summary>
    /// Tests if a bitboard has more than one bit set
    /// </summary>
    /// <param name="bb">The bitboard to set</param>
    /// <returns>true if more than one bit set, otherwise false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MoreThanOne(in BitBoard bb)
        => (bb.Value & (bb.Value - 1)) != 0;

    /// <summary>
    /// Helper method to generate shift function dictionary for all directions.
    /// </summary>
    /// <returns>The generated shift dictionary</returns>
    private static IDictionary<Direction, Func<BitBoard, BitBoard>> MakeShiftFuncs()
        => new Dictionary<Direction, Func<BitBoard, BitBoard>>(13)
        {
            { Direction.None, static board => board },
            { Direction.North, static board => board.NorthOne() },
            { Direction.NorthDouble, static board => board.NorthOne().NorthOne() },
            { Direction.NorthEast, static board => board.NorthEastOne() },
            { Direction.NorthWest, static board => board.NorthWestOne() },
            { Direction.NorthFill, static board => board.NorthFill() },
            { Direction.South, static board => board.SouthOne() },
            { Direction.SouthDouble, static board => board.SouthOne().SouthOne() },
            { Direction.SouthEast, static board => board.SouthEastOne() },
            { Direction.SouthWest, static board => board.SouthWestOne() },
            { Direction.SouthFill, static board => board.SouthFill() },
            { Direction.East, static board => board.EastOne() },
            { Direction.West, static board => board.WestOne() }
        };

    private static Func<BitBoard, BitBoard>[] MakeFillFuncs()
        => new Func<BitBoard, BitBoard>[] { NorthFill, SouthFill };

    private static BitBoard GetAttacks(this in Square sq, PieceTypes pt, in BitBoard occ = default)
    {
        return pt switch
        {
            PieceTypes.Knight => PseudoAttacksBB[pt.AsInt()][sq.AsInt()],
            PieceTypes.King => PseudoAttacksBB[pt.AsInt()][sq.AsInt()],
            PieceTypes.Bishop => sq.BishopAttacks(occ),
            PieceTypes.Rook => sq.RookAttacks(occ),
            PieceTypes.Queen => sq.QueenAttacks(occ),
            _ => EmptyBitBoard
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard PseudoAttack(this in Square sq, PieceTypes pt)
        => PseudoAttacksBB[pt.AsInt()][sq.AsInt()];
}
