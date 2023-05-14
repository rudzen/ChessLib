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

using System;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash;

/// <summary>
/// Class responsible for keeping random values for use with Zobrist hashing.
/// The zobrist key for which this class helps generate, is a "unique" representation of
/// a complete chess board, including its status.
///
/// ------------ |----------:|----------:|----------:|
///       Method |      Mean |     Error |    StdDev |
/// ------------ |----------:|----------:|----------:|
///     ULongRnd | 77.335 ns | 1.0416 ns | 0.9743 ns |
///  RKissRandom |  4.369 ns | 0.0665 ns | 0.0622 ns |
/// ------------ |----------:|----------:|----------:|
///
/// Collisions:
/// Key collisions or type-1 errors are inherent in using Zobrist keys with far less bits than required to encode all reachable chess positions.
/// This representation is using a 64 bit key, thus it could be expected to get a collision after about 2 ^ 32 or 4 billion positions.
///
/// Collision information from
/// https://chessprogramming.wikispaces.com/Zobrist+Hashing
/// </summary>
public sealed class Zobrist : IZobrist
{
    /// <summary>
    /// The default value for random seed for improved consistency
    /// </summary>
    private const ulong DefaultRandomSeed = 1070372;

    /// <summary>
    /// Represents the piece index (as in EPieces), with each a square of the board value to match.
    /// </summary>
    private readonly HashKey[][] ZobristPst = new HashKey[Square.Count][];

    /// <summary>
    /// Represents the castleling rights.
    /// </summary>
    private readonly HashKey[] ZobristCastling = new HashKey[CastleRight.Count];

    /// <summary>
    /// En-Passant is only required to have 8 entries, one for each possible file where the En-Passant square can occur.
    /// </summary>
    private readonly HashKey[] ZobristEpFile = new HashKey[File.Count];

    /// <summary>
    /// This is used if the side to move is black, if the side is white, no hashing will occur.
    /// </summary>
    private readonly HashKey ZobristSide;

    /// <summary>
    /// To use as base for pawn hash table
    /// </summary>
    public HashKey ZobristNoPawn { get; }

    public Zobrist()
    {
        var rnd = RKiss.Create(DefaultRandomSeed);
        InitializePst(in rnd);
        InitializeRandomArray(ZobristEpFile, rnd);
        InitializeRandomArray(ZobristCastling, rnd);
        ZobristSide = rnd.Rand();
        ZobristNoPawn = rnd.Rand();
    }

    private void InitializePst(in IRKiss rnd)
    {
        for (var sq = Squares.a1; sq <= Squares.h8; sq++)
        {
            var s = sq.AsInt();
            ZobristPst[s] = new HashKey[Piece.Count];
            foreach (var pc in Piece.All.AsSpan())
                ZobristPst[s][pc.AsInt()] = rnd.Rand();
        }
    }

    private static void InitializeRandomArray(Span<HashKey> array, IRKiss rnd)
    {
        for (var i = 0; i < array.Length; i++)
            array[i] = rnd.Rand();
    }

    public HashKey ComputeMaterialKey(IPosition pos)
    {
        var key = HashKey.Empty;
        foreach (var pc in Piece.All.AsSpan())
            for (var count = 0; count < pos.Board.PieceCount(pc); count++)
                key ^= GetZobristPst(count, pc);
        
        return key;
    }

    public HashKey ComputePawnKey(IPosition pos)
    {
        var key = ZobristNoPawn;

        var pawns = pos.Pieces(Piece.WhitePawn);

        while (pawns)
            key ^= GetZobristPst(BitBoards.PopLsb(ref pawns), Piece.WhitePawn);

        pawns = pos.Pieces(Piece.BlackPawn);
        
        while (pawns)
            key ^= GetZobristPst(BitBoards.PopLsb(ref pawns), Piece.BlackPawn);
        
        return key; 
    }

    public HashKey ComputePositionKey(IPosition pos)
    {
        var key = HashKey.Empty;

        foreach (var pc in Piece.All.AsSpan())
        {
            var bb = pos.Pieces(pc);
            while (bb)
                key ^= GetZobristPst(BitBoards.PopLsb(ref bb), pc);
        }

        if (pos.SideToMove.IsWhite)
            key ^= ZobristSide;

        key ^= GetZobristCastleling(pos.State.CastlelingRights.Rights);
        key ^= GetZobristEnPassant(pos.EnPassantSquare.File);
        
        return key;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey GetZobristPst(Square square, Piece piece) => ref ZobristPst[square.AsInt()][piece.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey GetZobristCastleling(CastleRights index) => ref ZobristCastling[index.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey GetZobristCastleling(CastleRight index) => ref GetZobristCastleling(index.Rights);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey GetZobristSide() => ZobristSide;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey GetZobristEnPassant(File file) => ref ZobristEpFile[file.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey GetZobristEnPassant(Square sq)
    {
        return sq == Square.None ? HashKey.Empty : GetZobristEnPassant(sq.File);
    }
}