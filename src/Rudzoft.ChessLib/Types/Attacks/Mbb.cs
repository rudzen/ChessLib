using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Rudzoft.ChessLib.Types.Attacks;

public static class Mbb
{
    private static readonly ulong[] RookMagics =
    {
        0xa8002c000108020UL, 0x6c00049b0002001UL, 0x100200010090040UL, 0x2480041000800801UL,
        0x280028004000800UL, 0x900410008040022UL, 0x280020001001080UL, 0x2880002041000080UL,
        0xa000800080400034UL, 0x4808020004000UL, 0x2290802004801000UL, 0x411000d00100020UL,
        0x402800800040080UL, 0xb000401004208UL, 0x2409000100040200UL, 0x1002100004082UL,
        0x22878001e24000UL, 0x1090810021004010UL, 0x801030040200012UL, 0x500808008001000UL,
        0xa08018014000880UL, 0x8000808004000200UL, 0x201008080010200UL, 0x801020000441090UL,
        0x800080204005UL, 0x1040200040100048UL, 0x120200402082UL, 0xd14880480100080UL,
        0x12040280080080UL, 0x100040080020080UL, 0x9020010080800200UL, 0x813241200148449UL,
        0x491604000800080UL, 0x100401000402001UL, 0x4820010021001040UL, 0x400402202000812UL,
        0x209009005000802UL, 0x810800601800400UL, 0x4301083214000150UL, 0x204026458e00140UL,
        0x40204000808000UL, 0x8001008040010020UL, 0x8410820820420010UL, 0x1003001000090020UL,
        0x804040008008080UL, 0x12000810020004UL, 0x1000100200040208UL, 0x430000a044020001UL,
        0x2800080008000100UL, 0xe0100040002240UL, 0x200100401700UL, 0x2244100408008080UL,
        0x8000400801980UL, 0x200081004020100UL, 0x8010100228810400UL, 0x2000009044210200UL,
        0x4080008040102101UL, 0x1a2208080204d101UL, 0x4100010002080088UL, 0x8100042000102001UL
    };

    private static readonly ulong[] BishopMagics =
    {
        0x89a1121896040240UL, 0x2004844802002010UL, 0x2068080051921000UL, 0x62880a0220200808UL,
        0x404008080020000UL, 0x10082100100a4218UL, 0x8040002811040900UL, 0x8010100a02020058UL,
        0x20800090488c0080UL, 0x1210815040200050UL, 0x8010008820400082UL, 0x4100210040110042UL,
        0x880800040081080UL, 0x102008010402040UL, 0x211021800500412UL, 0x1000402000400180UL,
        0x1040082084200040UL, 0x1208000080008410UL, 0x840081084110800UL, 0x100020004010004UL,
        0x10004000800400UL, 0x40c0422080a000UL, 0x420804400400UL, 0x110402040081040UL,
        0x20088080080220UL, 0x810001008080UL, 0x80800080101002UL, 0x82000020100050UL,
        0x4100001020020UL, 0x4040400808010UL, 0x2010840008001UL, 0x104100002080UL,
        0x200c000200402000UL, 0x10102000088010UL, 0x80800080004000UL, 0x100008020040UL,
        0x8004808020100200UL, 0x408000404040UL, 0x208004008008UL, 0x82000082008000UL,
        0x84008202004000UL, 0x8200200400840080UL, 0x40101000400080UL, 0x401020401100UL,
        0x100810001002UL, 0x8000400080804UL, 0x800200a0010100UL, 0x10404000808200UL
    };

    private static readonly int[] RookShifts =
    {
        52, 53, 53, 53, 53, 53, 53, 52,
        53, 54, 54, 54, 54, 54, 54, 53,
        53, 54, 54, 54, 54, 54, 54, 53,
        53, 54, 54, 54, 54, 54, 54, 53,
        53, 54, 54, 54, 54, 54, 54, 53,
        53, 54, 54, 54, 54, 54, 54, 53,
        53, 54, 54, 54, 54, 54, 54, 53,
        52, 53, 53, 53, 53, 53, 53, 52
    };

    private static readonly int[] BishopShifts =
    {
        58, 59, 59, 59, 59, 59, 59, 58,
        59, 59, 59, 59, 59, 59, 59, 59,
        59, 59, 57, 57, 57, 57, 59, 59,
        59, 59, 57, 55, 55, 57, 59, 59,
        59, 59, 57, 55, 55, 57, 59, 59,
        59, 59, 57, 57, 57, 57, 59, 59,
        59, 59, 59, 59, 59, 59, 59, 59,
        58, 59, 59, 59, 59, 59, 59, 58
    };

    private static readonly ulong[][] RookAttacks = new ulong[64][];
    private static readonly ulong[][] BishopAttacks = new ulong[64][];

    static Mbb()
    {
        for (int square = 0; square < 64; square++)
        {
            RookAttacks[square] = GenerateRookAttacks(square, RookMagics[square], RookShifts[square]);
            BishopAttacks[square] = GenerateBishopAttacks(square, BishopMagics[square], BishopShifts[square]);
        }
    }

    public static BitBoard GetRookAttacks(Square square, in BitBoard occupancy)
    {
        return GetAttackSet(square.AsInt(), in occupancy, RookMagics[square.AsInt()], RookShifts[square.AsInt()], RookAttacks);
    }

    public static BitBoard GetBishopAttacks(Square square, in BitBoard occupancy)
    {
        return GetAttackSet(square.AsInt(), in occupancy, BishopMagics[square.AsInt()], BishopShifts[square.AsInt()], BishopAttacks);
    }

    public static BitBoard GetQueenAttacks(Square square, in BitBoard occupancy)
    {
        return GetRookAttacks(square, in occupancy) | GetBishopAttacks(square, in occupancy);
    }

    private static ulong[] GenerateRookAttacks(int square, ulong magic, int shift)
    {
        int indexBits = 1 << shift;
        ulong[] attacks = new ulong[indexBits];
        ulong[] occupancy = new ulong[indexBits];

        int rank = square / 8;
        int file = square % 8;

        for (int i = 0; i < indexBits; i++)
        {
            occupancy[i] = SetRookOccupancy(i, rank, file);
            ulong index = PEXT(occupancy[i], magic) >> (64 - shift);
            attacks[index] = CalculateRookAttacks(square, occupancy[i]);
        }

        return attacks;
    }

    private static ulong[] GenerateBishopAttacks(int square, ulong magic, int shift)
    {
        int indexBits = 1 << shift;
        ulong[] attacks = new ulong[indexBits];
        ulong[] occupancy = new ulong[indexBits];

        int rank = square / 8;
        int file = square % 8;

        for (int i = 0; i < indexBits; i++)
        {
            occupancy[i] = SetBishopOccupancy(i, rank, file);
            ulong index = PEXT(occupancy[i], magic) >> (64 - shift);
            attacks[index] = CalculateBishopAttacks(square, occupancy[i]);
        }

        return attacks;
    }

    private static ulong GetAttackSet(int square, in BitBoard occupancy, ulong magic, int shift, ulong[][] attacks)
    {
        ulong index = PEXT(occupancy, magic) >> (64 - shift);
        int maskedIndex = (int)(index & (ulong)(attacks[square].Length - 1));
        return attacks[square][maskedIndex];
    }

    private static ulong SetRookOccupancy(int index, int rank, int file)
    {
        ulong occupancy = 0UL;

        for (int i = 0; i < 6; i++)
        {
            int bit = (index >> i) & 1;
            int targetRank = rank;
            int targetFile = file + i + 1;
            occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        for (int i = 0; i < 6; i++)
        {
            int bit = (index >> (i + 6)) & 1;
            int targetRank = rank;
            int targetFile = file - i - 1;
            occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        for (int i = 0; i < 5; i++)
        {
            int bit = (index >> (i + 12)) & 1;
            int targetRank = rank + i + 1;
            int targetFile = file;
            occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        for (int i = 0; i < 5; i++)
        {
            int bit = (index >> (i + 17)) & 1;
            int targetRank = rank - i - 1;
            int targetFile = file;
            occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        return occupancy;
    }

    private static ulong SetBishopOccupancy(int index, int rank, int file)
    {
        ulong occupancy = 0UL;

        for (int i = 0; i < 4; i++)
        {
            int bit = (index >> i) & 1;
            int targetRank = rank + i + 1;
            int targetFile = file + i + 1;
            if (targetRank < 8 && targetFile < 8)
                occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        for (int i = 0; i < 4; i++)
        {
            int bit = (index >> (i + 4)) & 1;
            int targetRank = rank - i - 1;
            int targetFile = file + i + 1;
            if (targetRank >= 0 && targetFile < 8)
                occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        for (int i = 0; i < 4; i++)
        {
            int bit = (index >> (i + 8)) & 1;
            int targetRank = rank + i + 1;
            int targetFile = file - i - 1;
            if (targetRank < 8 && targetFile >= 0)
                occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        for (int i = 0; i < 4; i++)
        {
            int bit = (index >> (i + 12)) & 1;
            int targetRank = rank - i - 1;
            int targetFile = file - i - 1;
            if (targetRank >= 0 && targetFile >= 0)
                occupancy |= (ulong)bit << (targetRank * 8 + targetFile);
        }

        return occupancy;
    }

    private static ulong CalculateRookAttacks(int square, ulong occupancy)
    {
        int rank = square / 8;
        int file = square % 8;
        ulong attacks = 0UL;

        for (int targetRank = rank + 1; targetRank < 8; targetRank++)
        {
            attacks |= 1UL << (targetRank * 8 + file);
            if ((occupancy & (1UL << (targetRank * 8 + file))) != 0) break;
        }

        for (int targetRank = rank - 1; targetRank >= 0; targetRank--)
        {
            attacks |= 1UL << (targetRank * 8 + file);
            if ((occupancy & (1UL << (targetRank * 8 + file))) != 0) break;
        }

        for (int targetFile = file + 1; targetFile < 8; targetFile++)
        {
            attacks |= 1UL << (rank * 8 + targetFile);
            if ((occupancy & (1UL << (rank * 8 + targetFile))) != 0) break;
        }

        for (int targetFile = file - 1; targetFile >= 0; targetFile--)
        {
            attacks |= 1UL << (rank * 8 + targetFile);
            if ((occupancy & (1UL << (rank * 8 + targetFile))) != 0) break;
        }

        return attacks;
    }

    private static ulong CalculateBishopAttacks(int square, ulong occupancy)
    {
        int rank = square / 8;
        int file = square % 8;
        ulong attacks = 0UL;

        for (int r = rank + 1, f = file + 1; r < 8 && f < 8; r++, f++)
        {
            attacks |= 1UL << (r * 8 + f);
            if ((occupancy & (1UL << (r * 8 + f))) != 0) break;
        }

        for (int r = rank - 1, f = file + 1; r >= 0 && f < 8; r--, f++)
        {
            attacks |= 1UL << (r * 8 + f);
            if ((occupancy & (1UL << (r * 8 + f))) != 0) break;
        }

        for (int r = rank + 1, f = file - 1; r < 8 && f >= 0; r++, f--)
        {
            attacks |= 1UL << (r * 8 + f);
            if ((occupancy & (1UL << (r * 8 + f))) != 0) break;
        }

        for (int r = rank - 1, f = file - 1; r >= 0 && f >= 0; r--, f--)
        {
            attacks |= 1UL << (r * 8 + f);
            if ((occupancy & (1UL << (r * 8 + f))) != 0) break;
        }

        return attacks;
    }
    
    private static ulong PEXT(in BitBoard source, in ulong mask)
    {
        if (Bmi2.X64.IsSupported)
        {
            return Bmi2.X64.ParallelBitExtract(source.Value, mask);
        }

        // Fallback implementation for when the PEXT instruction is not available
        ulong result = 0;
        ulong one = 1;
        for (ulong bb = mask; bb != 0; bb &= (bb - 1))
        {
            int index = BitScanForward(bb);
            if ((source & (one << index)) != 0)
            {
                result |= (one << BitScanForward(bb & ~(bb - 1)));
            }
        }
        return result;
    }
    
    private static int BitScanForward(ulong bb)
    {
        if (bb == 0) return -1;

        int index = 0;
        while ((bb & 1) == 0)
        {
            bb >>= 1;
            index++;
        }

        return index;
    }
}