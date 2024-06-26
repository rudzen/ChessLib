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

using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib.Hash;

/// <summary>
/// Class responsible for keeping random values for use with Zobrist hashing.
/// The zobrist key for which this class helps generate, is a "unique" representation of
/// a complete chess board, including its status.
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
    /// Represents the piece index (as in EPieces), with each a square of the board value to match.
    /// </summary>
    private readonly HashKey[][] _zobristPst;

    /// <summary>
    /// Represents the castleling rights.
    /// </summary>
    private readonly HashKey[] _zobristCastling;

    /// <summary>
    /// En-Passant is only required to have 8 entries, one for each possible file where the En-Passant square can occur.
    /// </summary>
    private readonly HashKey[] _zobristEpFile;

    /// <summary>
    /// This is used if the side to move is black, if the side is white, no hashing will occur.
    /// </summary>
    private readonly HashKey _zobristSide;

    /// <summary>
    /// To use as base for pawn hash table
    /// </summary>
    public HashKey ZobristNoPawn { get; }

    public Zobrist(IRKiss rKiss)
    {
        _zobristPst = new HashKey[Square.Count][];

        foreach (var sq in Square.All)
        {
            _zobristPst[sq] = new HashKey[Piece.Count];
            foreach (var pc in Piece.All.AsSpan())
                _zobristPst[sq][pc] = rKiss.Rand();
        }

        _zobristCastling = new HashKey[CastleRight.Count];

        for (var cr = CastleRights.None; cr <= CastleRights.Any; cr++)
        {
            var v = cr.AsInt();
            _zobristCastling[v] = HashKey.Empty;
            var bb = BitBoard.Create((uint)v);
            while (bb)
            {
                var key = _zobristCastling[1UL << BitBoards.PopLsb(ref bb)];
                key = !key.IsEmpty ? key : rKiss.Rand();
                _zobristCastling[v] ^= key.Key;
            }
        }

        _zobristEpFile = new HashKey[File.Count];
        for (var i = 0; i < _zobristEpFile.Length; i++)
            _zobristEpFile[i] = rKiss.Rand();

        _zobristSide = rKiss.Rand();
        ZobristNoPawn = rKiss.Rand();
    }

    public HashKey ComputeMaterialKey(IPosition pos)
    {
        var key = HashKey.Empty;
        foreach (var pc in Piece.All.AsSpan())
            for (var count = 0; count < pos.Board.PieceCount(pc); count++)
                key ^= Psq(count, pc);

        return key;
    }

    public HashKey ComputePawnKey(IPosition pos)
    {
        var key = ZobristNoPawn;

        var pawns = pos.Pieces(Piece.WhitePawn);

        while (pawns)
            key ^= Psq(BitBoards.PopLsb(ref pawns), Piece.WhitePawn);

        pawns = pos.Pieces(Piece.BlackPawn);

        while (pawns)
            key ^= Psq(BitBoards.PopLsb(ref pawns), Piece.BlackPawn);

        return key;
    }

    public HashKey ComputePositionKey(IPosition pos)
    {
        var key = HashKey.Empty;

        foreach (var pc in Piece.All.AsSpan())
        {
            var bb = pos.Pieces(pc);
            while (bb)
                key ^= Psq(BitBoards.PopLsb(ref bb), pc);
        }

        if (pos.SideToMove.IsWhite)
            key ^= _zobristSide;

        key ^= Castle(pos.State.CastleRights.Rights);
        key ^= EnPassant(pos.EnPassantSquare.File);

        return key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey Psq(Square square, Piece pc) => ref _zobristPst[square][pc];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey Psq(int pieceCount, Piece pc) => ref _zobristPst[pieceCount][pc];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey Castle(CastleRights index) => ref _zobristCastling[index.AsInt()];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey Castle(CastleRight index) => ref Castle(index.Rights);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey Side() => _zobristSide;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref HashKey EnPassant(File f) => ref _zobristEpFile[f];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HashKey EnPassant(Square sq)
    {
        return sq == Square.None ? HashKey.Empty : EnPassant(sq.File);
    }
}