using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Events;

public struct PieceMovedEventArgs
{
    public Square From { get; }
    public Square To { get; }
    public Piece MovedPiece { get; }

    public PieceMovedEventArgs(Square from, Square to, Piece movedPiece)
    {
        From = from;
        To = to;
        MovedPiece = movedPiece;
    }
}
