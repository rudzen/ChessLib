﻿using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Events;

public struct PieceRemovedEventArgs
{
    public Square EmptiedSquare { get; }
    public Piece RemovedPiece { get; }

    public PieceRemovedEventArgs(Square emptiedSquare, Piece removedPiece)
    {
        EmptiedSquare = emptiedSquare;
        RemovedPiece = removedPiece;
    }
}
