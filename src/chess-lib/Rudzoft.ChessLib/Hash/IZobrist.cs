﻿using Rudzoft.ChessLib.Types;
using File = Rudzoft.ChessLib.Types.File;

namespace Rudzoft.ChessLib.Hash;

public interface IZobrist
{
    /// <summary>
    /// To use as base for pawn hash table
    /// </summary>
    HashKey ZobristNoPawn { get; }

    HashKey ComputeMaterialKey(IPosition pos);
    HashKey ComputePawnKey(IPosition pos);
    HashKey ComputePositionKey(IPosition pos);
    ref HashKey Psq(Square square, Piece pc);
    ref HashKey Psq(int pieceCount, Piece pc);
    ref HashKey Castle(CastleRights index);
    ref HashKey Castle(CastleRight index);
    HashKey Side();
    ref HashKey EnPassant(File f);
    HashKey EnPassant(Square sq);
}