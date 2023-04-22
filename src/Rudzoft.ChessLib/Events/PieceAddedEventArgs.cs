using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Events;

public class PieceAddedEventArgs
{
    public Square Square { get; }
    public Piece NewPiece { get; }

    public PieceAddedEventArgs(Square square, Piece newPiece)
    {
        Square = square;
        NewPiece = newPiece;
    }
}