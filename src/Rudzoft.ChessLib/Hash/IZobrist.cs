using Rudzoft.ChessLib.Types;
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
    ref HashKey Psq(Square square, Piece piece);
    ref HashKey Psq(int pieceCount, Piece piece);
    ref HashKey Castleling(CastleRights index);
    ref HashKey Castleling(CastleRight index);
    HashKey Side();
    ref HashKey EnPassant(File file);
    HashKey EnPassant(Square sq);
}