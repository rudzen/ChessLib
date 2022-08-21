﻿/*
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

using System;
using System.Runtime.CompilerServices;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;

namespace Rudz.Chess.Hash;

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
public static class Zobrist
{
    /// <summary>
    /// The default value for random seed for improved consistency
    /// </summary>
    private const ulong DefaultRandomSeed = 1070372;

    /// <summary>
    /// Represents the piece index (as in EPieces), with each a square of the board value to match.
    /// </summary>
    private static readonly ulong[][] ZobristPst = new ulong[16][];

    /// <summary>
    /// Represents the castleling rights.
    /// </summary>
    private static readonly ulong[] ZobristCastling = new ulong[16];

    /// <summary>
    /// En-Passant is only required to have 8 entries, one for each possible file where the En-Passant square can occur.
    /// </summary>
    private static readonly ulong[] ZobristEpFile = new ulong[8];

    /// <summary>
    /// This is used if the side to move is black, if the side is white, no hashing will occur.
    /// </summary>
    private static readonly ulong ZobristSide;

    /// <summary>
    /// To use as base for pawn hash table
    /// </summary>
    public static readonly HashKey ZobristNoPawn;

    static Zobrist()
    {
        var rnd = RKiss.Create(DefaultRandomSeed);
        InitializePst(rnd);
        InitializeRandomArray(ZobristEpFile, rnd);
        InitializeRandomArray(ZobristCastling, rnd);
        ZobristSide = rnd.Rand();
        ZobristNoPawn = rnd.Rand();
    }

    private static void InitializePst(IRKiss rnd)
    {
        for (var i = 0; i < ZobristPst.Length; i++)
            ZobristPst[i] = new ulong[64];

        Span<Piece> pieces = stackalloc Piece[]
        {
            Pieces.WhitePawn, Pieces.WhiteKnight, Pieces.WhiteBishop,
            Pieces.WhiteRook, Pieces.WhiteQueen,  Pieces.WhiteKing,
            Pieces.BlackPawn, Pieces.BlackKnight, Pieces.BlackBishop,
            Pieces.BlackRook, Pieces.BlackQueen,  Pieces.BlackKing
        };

        foreach (var pc in pieces)
        foreach (var sq in BitBoards.AllSquares)
            ZobristPst[pc.AsInt()][sq.AsInt()] = rnd.Rand();
    }

    private static void InitializeRandomArray(Span<ulong> array, IRKiss rnd)
    {
        for (var i = 0; i < array.Length; i++)
            array[i] = rnd.Rand();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetZobristPst(this Piece piece, Square square) => ZobristPst[piece.AsInt()][square.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetZobristCastleling(this CastlelingRights index) => ZobristCastling[index.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetZobristSide() => ZobristSide;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetZobristEnPessant(this File file) => ZobristEpFile[file.AsInt()];
}