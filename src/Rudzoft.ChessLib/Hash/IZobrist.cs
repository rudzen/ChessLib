using Rudzoft.ChessLib.Types;

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
    ref HashKey GetZobristPst(Square square, Piece piece);
    ref HashKey GetZobristCastleling(CastleRights index);
    ref HashKey GetZobristCastleling(CastleRight index);
    HashKey GetZobristSide();
    ref HashKey GetZobristEnPassant(File file);
    HashKey GetZobristEnPassant(Square sq);
}