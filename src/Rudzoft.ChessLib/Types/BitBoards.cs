/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

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

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Types;

public static class BitBoards
{
    internal const ulong One = 0x1ul;

    public static readonly BitBoard WhiteArea = new(0x00000000FFFFFFFFUL);

    public static readonly BitBoard BlackArea = ~WhiteArea;

    private static readonly BitBoard LightSquares = new(0x55AA55AA55AA55AAUL);

    private static readonly BitBoard[] ColorsBB = [LightSquares, ~LightSquares];

    private static readonly BitBoard FileABB = new(0x0101010101010101UL);

    private static readonly BitBoard FileBBB = new(0x0202020202020202UL);

    private static readonly BitBoard FileCBB = new(0x404040404040404UL);

    private static readonly BitBoard FileDBB = new(0x808080808080808UL);

    private static readonly BitBoard FileEBB = new(0x1010101010101010);

    private static readonly BitBoard FileFBB = new(0x2020202020202020);

    private static readonly BitBoard FileGBB = new(0x4040404040404040UL);

    private static readonly BitBoard FileHBB = new(0x8080808080808080UL);

    private static readonly BitBoard[] FilesBB =
    [
        FileABB, FileBBB, FileCBB, FileDBB,
        FileEBB, FileFBB, FileGBB, FileHBB
    ];

    private static readonly BitBoard Rank1BB = new(0x00000000000000ffUL);

    private static readonly BitBoard Rank2BB = new(0x000000000000ff00UL);

    private static readonly BitBoard Rank3BB = new(0x0000000000ff0000UL);

    private static readonly BitBoard Rank4BB = new(0x00000000ff000000UL);

    private static readonly BitBoard Rank5BB = new(0x000000ff00000000UL);

    private static readonly BitBoard Rank6BB = new(0x0000ff0000000000UL);

    private static readonly BitBoard Rank7BB = new(0x00ff000000000000UL);

    private static readonly BitBoard Rank8BB = new(0xff00000000000000UL);

    private static readonly BitBoard[] RanksBB =
    [
        Rank1BB, Rank2BB, Rank3BB, Rank4BB,
        Rank5BB, Rank6BB, Rank7BB, Rank8BB
    ];

    public static readonly BitBoard EmptyBitBoard = new(ulong.MinValue);

    public static readonly BitBoard AllSquares = ~EmptyBitBoard;

    public static readonly BitBoard PawnSquares = AllSquares & ~(Rank1BB | Rank8BB);

    public static readonly BitBoard QueenSide = new(FileABB | FileBBB | FileCBB | FileDBB);

    public static readonly BitBoard CenterFiles = new(FileCBB | FileDBB | FileEBB | FileFBB);

    public static readonly BitBoard KingSide = new(FileEBB | FileFBB | FileGBB | FileHBB);

    public static readonly BitBoard Center = new((FileDBB | FileEBB) & (Rank4BB | Rank5BB));

    // A1..H8 | H1..A8
    public static readonly BitBoard DiagonalBB = new(0x8142241818244281UL);

    private static readonly BitBoard[] PromotionRanks = [Rank8BB, Rank1BB];

    public static readonly BitBoard PromotionRanksBB = Rank1BB | Rank8BB;

    internal static readonly BitBoard[] BbSquares =
    [
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
    ];

    public static readonly BitBoard CornerA1 = Square.A1 | Square.B1 | Square.A2 | Square.B2;

    public static readonly BitBoard CornerA8 = Square.A8 | Square.B8 | Square.A7 | Square.B7;

    public static readonly BitBoard CornerH1 = Square.H1 | Square.G1 | Square.H2 | Square.G2;

    public static readonly BitBoard CornerH8 = Square.H8 | Square.G8 | Square.H7 | Square.G7;

    private static readonly BitBoard[] Ranks1 = [Rank1BB, Rank8BB];

    private static readonly BitBoard[] Ranks6And7BB = [Rank6BB | Rank7BB, Rank2BB | Rank3BB];

    private static readonly BitBoard[] Ranks7And8BB = [Rank7BB | Rank8BB, Rank1BB | Rank2BB];

    private static readonly BitBoard[] Ranks3BB = [Rank3BB, Rank6BB];

    private static readonly BitBoard[] Ranks7BB = [Rank7BB, Rank2BB];

    /// <summary>
    /// PseudoAttacks are just that, full attack range for all squares for all pieces. The pawns
    /// are a special case, as index range 0,sq are for White and 1,sq are for Black. This is
    /// possible because index 0 is NoPiece type.
    /// </summary>
    private static readonly BitBoard[][] PseudoAttacksBB = new BitBoard[PieceType.Count][];

    private static readonly BitBoard[][] PawnAttackSpanBB = new BitBoard[Color.Count][];

    private static readonly BitBoard[][] PassedPawnMaskBB = new BitBoard[Color.Count][];

    private static readonly BitBoard[][] ForwardRanksBB = new BitBoard[Color.Count][];

    private static readonly BitBoard[][] ForwardFileBB = new BitBoard[Color.Count][];

    private static readonly BitBoard[][] KingRingBB = new BitBoard[Color.Count][];

    private static readonly BitBoard[][] BetweenBB = new BitBoard[Square.Count][];

    private static readonly BitBoard[][] LineBB = new BitBoard[Square.Count][];

    private static readonly BitBoard[] AdjacentFilesBB =
    [
        FileBBB, FileABB | FileCBB, FileBBB | FileDBB, FileCBB | FileEBB, FileDBB | FileFBB, FileEBB | FileGBB,
        FileFBB | FileHBB, FileGBB
    ];

    private static readonly int[][] SquareDistance = new int[Square.Count][]; // chebyshev distance

    private static readonly BitBoard[][] DistanceRingBB = new BitBoard[Square.Count][];

    private static readonly BitBoard[] SlotFileBB =
    [
        FileEBB | FileFBB | FileGBB | FileHBB, // King
        FileABB | FileBBB | FileCBB | FileDBB, // Queen
        FileCBB | FileDBB | FileEBB | FileFBB  // Center
    ];

    private static readonly Direction[] PawnPushDirections = [Direction.North, Direction.South];

    private static readonly Func<BitBoard, BitBoard>[] FillFuncs = MakeFillFuncs();

    static BitBoards()
    {
        for (var i = 0; i < PseudoAttacksBB.Length; i++)
            PseudoAttacksBB[i] = new BitBoard[Square.Count];

        PawnAttackSpanBB[0] = new BitBoard[Square.Count];
        PawnAttackSpanBB[1] = new BitBoard[Square.Count];

        PassedPawnMaskBB[0] = new BitBoard[Square.Count];
        PassedPawnMaskBB[1] = new BitBoard[Square.Count];

        ForwardRanksBB[0] = new BitBoard[Square.Count];
        ForwardRanksBB[1] = new BitBoard[Square.Count];

        ForwardFileBB[0] = new BitBoard[Square.Count];
        ForwardFileBB[1] = new BitBoard[Square.Count];

        KingRingBB[0] = new BitBoard[Square.Count];
        KingRingBB[1] = new BitBoard[Square.Count];

        for (var i = 0; i < BetweenBB.Length; i++)
        {
            BetweenBB[i] = new BitBoard[Square.Count];
            LineBB[i] = new BitBoard[Square.Count];
        }

        // ForwardRanksBB population loop idea from sf
        foreach (var r in Rank.All)
            ForwardRanksBB[0][r] = ~(ForwardRanksBB[1][r + 1] = ForwardRanksBB[1][r] | r.BitBoardRank());

        foreach (var c in Color.AllColors.AsSpan())
        {
            foreach (var sq in Square.All.AsSpan())
            {
                var file = sq.File;
                ForwardFileBB[c][sq] = ForwardRanksBB[c][sq.Rank] & file.BitBoardFile();
                PawnAttackSpanBB[c][sq] = ForwardRanksBB[c][sq.Rank] & AdjacentFilesBB[file];
                PassedPawnMaskBB[c][sq] = ForwardFileBB[c][sq] | PawnAttackSpanBB[c][sq];
            }
        }

        // have to compute here before we access the BitBoards
        foreach (var sq1 in Square.All.AsSpan())
        {
            SquareDistance[sq1] = new int[Square.Count];
            DistanceRingBB[sq1] = new BitBoard[8];
            foreach (var sq2 in Square.All.AsSpan())
            {
                var dist = Math.Max(distanceFile(sq1, sq2), distanceRank(sq1, sq2));
                SquareDistance[sq1][sq2] = dist;
                DistanceRingBB[sq1][dist] |= sq2;
            }
        }

        // mini local helpers
        Span<PieceType> validMagicPieces = stackalloc PieceType[] { PieceType.Bishop, PieceType.Rook };

        // Pseudo attacks for all pieces
        foreach (var sq1 in Square.All)
        {
            InitializePseudoAttacks(sq1);

            // Compute lines and betweens
            foreach (var pt in validMagicPieces)
            {
                foreach (var sq2 in Square.All)
                {
                    if ((PseudoAttacksBB[pt][sq1] & sq2).IsEmpty)
                        continue;

                    LineBB[sq1][sq2] = (GetAttacks(sq1, pt, EmptyBitBoard) &
                                      GetAttacks(sq2, pt, EmptyBitBoard)) | sq1 | sq2;
                    BetweenBB[sq1][sq2] = GetAttacks(sq1, pt, BbSquares[sq2]) &
                                        GetAttacks(sq2, pt, BbSquares[sq1]);
                }
            }

            // Compute KingRings
            InitializeKingRing(sq1);
        }

        return;

        static int distanceRank(Square x, Square y) => distance(x.Rank, y.Rank);
        static int distanceFile(Square x, Square y) => distance(x.File, y.File);

        // local helper functions to calculate distance
        static int distance(int x, int y) => Math.Abs(x - y);
    }

    private static BitBoard ComputeKnightAttack(in BitBoard bb)
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

    private static void InitializePseudoAttacks(Square sq)
    {
        var b = sq.AsBb();
        var bishopAttacks = sq.BishopAttacks(EmptyBitBoard);
        var rookAttacks = sq.RookAttacks(EmptyBitBoard);

        // Pawns
        PseudoAttacksBB[0][sq] = b.NorthEastOne() | b.NorthWestOne();
        PseudoAttacksBB[1][sq] = b.SouthWestOne() | b.SouthEastOne();

        PseudoAttacksBB[PieceType.Knight][sq] = ComputeKnightAttack(in b);
        PseudoAttacksBB[PieceType.Bishop][sq] = bishopAttacks;
        PseudoAttacksBB[PieceType.Rook][sq] = rookAttacks;
        PseudoAttacksBB[PieceType.Queen][sq] = bishopAttacks | rookAttacks;
        PseudoAttacksBB[PieceType.King][sq] = b.NorthOne() | b.SouthOne() | b.EastOne() | b.WestOne() |
                                              PseudoAttacksBB[0][sq] | PseudoAttacksBB[1][sq];
    }

    private static void InitializeKingRing(Square sq)
    {
        var file = sq.File;

        foreach (var c in Color.AllColors)
        {
            KingRingBB[c][sq] = PseudoAttacksBB[PieceType.King][sq];
            if (sq.RelativeRank(c) == Ranks.Rank1)
                KingRingBB[c][sq] |= KingRingBB[c][sq].Shift(PawnPushDirections[c]);

            if (file == Files.FileH)
                KingRingBB[c][sq] |= KingRingBB[c][sq].WestOne();
            else if (file == Files.FileA)
                KingRingBB[c][sq] |= KingRingBB[c][sq].EastOne();

            Debug.Assert(!KingRingBB[c][sq].IsEmpty);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FileBB(this File f) => FilesBB[f.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FileBB(File first, File last)
    {
        var b = EmptyBitBoard;
        for (var f = first; f <= last; ++f)
            b |= f.BitBoardFile();
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard RankBB(this Rank r) => RanksBB[r];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ColorBB(this Color c) => ColorsBB[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FirstRank(Color c) => Ranks1[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ThirdRank(Color c) => Ranks3BB[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SeventhRank(Color c) => Ranks7BB[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SixthAndSeventhRank(Color c) => Ranks6And7BB[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SeventhAndEightsRank(Color c) => Ranks7And8BB[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PseudoAttacks(this PieceType pt, Square sq) => PseudoAttacksBB[pt][sq];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KnightAttacks(this Square sq) => PseudoAttacksBB[PieceType.Knight][sq];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KingAttacks(this Square sq) => PseudoAttacksBB[PieceType.King][sq];

    /// <summary>
    /// Attack for pawn.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <param name="c">The player side</param>
    /// <returns>ref to bitboard of attack</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttack(this Square sq, Color c) => PseudoAttacksBB[c][sq];

    /// <summary>
    /// Returns the bitboard representation of the rank of which the square is located.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <returns>The bitboard of square rank</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardRank(this Square sq) => sq.Rank.BitBoardRank();

    /// <summary>
    /// Returns the bitboard representation of a rank.
    /// </summary>
    /// <param name="r">The rank</param>
    /// <returns>The bitboard of square rank</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard BitBoardRank(this Rank r) => Rank1BB << (8 * r);

    /// <summary>
    /// Returns the bitboard representation of the file of which the square is located.
    /// </summary>
    /// <param name="sq">The square</param>
    /// <returns>The bitboard of square file</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitBoardFile(this Square sq) => sq.File.BitBoardFile();

    /// <summary>
    /// Returns the bitboard representation of the file.
    /// </summary>
    /// <param name="f">The file</param>
    /// <returns>The bitboard of file</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard BitBoardFile(this File f) => FileABB << f;

    /// <summary>
    /// Returns all squares in front of the square in the same file as bitboard
    /// </summary>
    /// <param name="s">The square</param>
    /// <param name="c">The side, white is north and black is south</param>
    /// <returns>The bitboard of all forward file squares</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ForwardFile(this Square s, Color c) => ForwardFileBB[c][s];

    /// <summary>
    /// Returns all squares in pawn attack pattern in front of the square.
    /// </summary>
    /// <param name="s">The square</param>
    /// <param name="c">White = north, Black = south</param>
    /// <returns>The bitboard representation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttackSpan(this Square s, Color c) => PawnAttackSpanBB[c][s];

    /// <summary>
    /// Returns all square of both file and pawn attack pattern in front of square. This is the
    /// same as ForwardFile() | PawnAttackSpan().
    /// </summary>
    /// <param name="s">The square</param>
    /// <param name="c">White = north, Black = south</param>
    /// <returns>The bitboard representation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PassedPawnFrontAttackSpan(this Square s, Color c) => PassedPawnMaskBB[c][s];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard ForwardRanks(this Square s, Color c) => ForwardRanksBB[c][s];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard BitboardBetween(this Square s1, Square s2) => BetweenBB[s1][s2];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Get(this in BitBoard bb, int pos) => (int)(bb.Value >> pos) & 0x1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSet(this in BitBoard bb, int pos) => (bb.Value & (One << pos)) != ulong.MinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square First(this in BitBoard bb) => bb.Lsb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Last(this in BitBoard bb) => bb.Msb();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Line(this Square sq1, Square sq2) => LineBB[sq1][sq2];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard SlotFile(CastleSide cs) => SlotFileBB[cs];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard AdjacentFiles(File f) => AdjacentFilesBB[f];

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool Aligned(this Square sq1, Square sq2, Square sq3) => (Line(sq1, sq2) & sq3).IsNotEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard FrontSquares(this Color c, Square sq)
        => ForwardRanksBB[c][sq] & sq.BitBoardFile();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard KingRing(this Square sq, Color c) => KingRingBB[c][sq];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance(this Square sq1, Square sq2) => SquareDistance[sq1][sq2];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard DistanceRing(this Square sq, int length) => DistanceRingBB[sq][length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PromotionRank(this Color c) => PromotionRanks[c];

    public static string StringifyRaw(in ulong bb, string title = "") => Stringify(BitBoard.Create(bb), title);

    [SkipLocalsInit]
    public static string Stringify(in BitBoard bb, string title = "")
    {
        const string line = "+---+---+---+---+---+---+---+---+---+";
        const string bottom = "|   | A | B | C | D | E | F | G | H |";

        Span<char> span = stackalloc char[768];

        line.CopyTo(span);
        var idx = line.Length;
        span[idx++] = '\n';

        var t = title.Length > 64 ? title.AsSpan()[..64] : title.AsSpan();
        t.CopyTo(span[idx..]);

        Span<char> rank = stackalloc char[4] { '|', ' ', ' ', ' ' };
        for (var r = Ranks.Rank8; r >= Ranks.Rank1; --r)
        {
            rank[2] = (char)('0' + (int)r + 1);
            rank.CopyTo(span[idx..]);
            idx += rank.Length;
            for (var f = Files.FileA; f <= Files.FileH; ++f)
            {
                rank[2] = (bb & new Square(r, f)).IsEmpty ? ' ' : 'X';
                rank.CopyTo(span[idx..]);
                idx += rank.Length;
            }

            span[idx++] = '|';
            span[idx++] = '\n';
            line.CopyTo(span[idx..]);
            idx += line.Length;
            span[idx++] = '\n';
        }

        bottom.CopyTo(span[idx..]);
        idx += bottom.Length;

        span[idx++] = '\n';

        line.CopyTo(span[idx..]);
        idx += line.Length;

        span[idx] = '\n';

        return new(span[..idx]);
    }

    /// <summary>
    /// Retrieves the least significant bit in an ulong word.
    /// </summary>
    /// <param name="bb">The word to get lsb from</param>
    /// <returns>The index of the found bit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Lsb(this in BitBoard bb) => new(BitOperations.TrailingZeroCount(bb.Value));

    /// <summary>
    /// Retrieves the least significant bit in an int word.
    /// </summary>
    /// <param name="v">The word to get lsb from</param>
    /// <returns>The index of the found bit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Lsb(this int v) => BitOperations.TrailingZeroCount(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square Msb(this in BitBoard bb) => new(63 ^ BitOperations.LeadingZeroCount(bb.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square FrontMostSquare(in BitBoard bb, Color c) => c.IsWhite ? Lsb(in bb) : Msb(in bb);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard NorthOne(this BitBoard bb) => bb << 8;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard SouthOne(this BitBoard bb) => bb >> 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard EastOne(this in BitBoard bb) => (bb & ~FileHBB) << 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard WestOne(this in BitBoard bb) => (bb & ~FileABB) >> 1;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard SouthEastOne(this in BitBoard bb) => (bb & ~FileHBB) >> 7;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard SouthWestOne(this in BitBoard bb) => (bb & ~FileABB) >> 9;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard NorthWestOne(this in BitBoard bb) => (bb & ~FileABB) << 7;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard NorthEastOne(this in BitBoard bb) => (bb & ~FileHBB) << 9;

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
    /// <param name="c">The direction to fill in, white = north, black = south</param>
    /// <returns>Filled bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Fill(this in BitBoard bb, Color c) => FillFuncs[c](bb);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BitBoard Shift(this in BitBoard bb, Direction d)
    {
        if (d == Direction.North)
            return bb.NorthOne();
        if (d == Direction.South)
            return bb.SouthOne();
        if (d == Direction.SouthEast)
            return bb.SouthEastOne();
        if (d == Direction.SouthWest)
            return bb.SouthWestOne();
        if (d == Direction.NorthEast)
            return bb.NorthEastOne();
        if (d == Direction.NorthWest)
            return bb.NorthWestOne();
        if (d == Direction.NorthDouble)
            return bb.NorthOne().NorthOne();
        if (d == Direction.SouthDouble)
            return bb.SouthOne().SouthOne();
        if (d == Direction.East)
            return bb.EastOne();
        if (d == Direction.West)
            return bb.WestOne();
        if (d == Direction.NorthFill)
            return bb.NorthFill();
        if (d == Direction.SouthFill)
            return bb.SouthFill();
        if (d == Direction.None)
            return bb;
        throw new ArgumentException("Invalid shift argument.", nameof(d));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnEastAttack(this in BitBoard bb, Color c) => Shift(in bb, c.PawnEastAttackDistance());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnWestAttack(this in BitBoard bb, Color c) => Shift(in bb, c.PawnWestAttackDistance());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnAttacks(this in BitBoard bb, Color c)
        => PawnEastAttack(in bb, c) | PawnWestAttack(in bb, c);

    /// <summary>
    /// Compute all attack squares where two pawns attack the square
    /// </summary>
    /// <param name="bb">The squares to compute attacks from</param>
    /// <param name="c">The side</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard PawnDoubleAttacks(this in BitBoard bb, Color c)
        => PawnEastAttack(in bb, c) & PawnWestAttack(in bb, c);

    /// <summary>
    /// Reset the least significant bit in-place
    /// </summary>
    /// <param name="bb">The bitboard as reference</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void ResetLsb(ref BitBoard bb) => bb &= bb - 1;

    /// <summary>
    /// Reset the least significant bit in-place
    /// </summary>
    /// <param name="v">The integer as reference</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ResetLsb(ref int v) => v &= v - 1;

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
    public static int PopCount(in BitBoard bb) => BitOperations.PopCount(bb.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Rank7(this Color c) => Ranks7BB[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard Rank3(this Color c) => Ranks3BB[c];

    /// <summary>
    /// Generate a bitboard based on a square.
    /// </summary>
    /// <param name="sq">The square to generate bitboard from</param>
    /// <returns>The generated bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard MakeBitboard(Square sq) => sq.AsBb();

    /// <summary>
    /// Generate a bitboard based on two squares.
    /// </summary>
    /// <param name="sq">The square to generate bitboard from</param>
    /// <param name="sq2">The second square to generate bitboard from</param>
    /// <returns>The generated bitboard</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard MakeBitboard(Square sq, Square sq2) => sq.AsBb() | sq2;

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
    public static bool MoreThanOne(in BitBoard bb) => (bb.Value & (bb.Value - 1)) != 0;

    private static Func<BitBoard, BitBoard>[] MakeFillFuncs() => [NorthFill, SouthFill];

    private static BitBoard GetAttacks(this in Square sq, PieceType pt, in BitBoard occ = default)
    {
        if (pt == PieceType.Knight || pt == PieceType.King)
            return sq.PseudoAttack(pt);

        if (pt == PieceType.Bishop)
            return sq.BishopAttacks(in occ);

        if (pt == PieceType.Rook)
            return sq.RookAttacks(in occ);

        if (pt == PieceType.Queen)
            return sq.QueenAttacks(in occ);

        return EmptyBitBoard;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BitBoard PseudoAttack(this in Square sq, PieceType pt) => PseudoAttacksBB[pt][sq];
}