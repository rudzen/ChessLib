/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Rudz.Chess.Types
{
    using Enums;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Move struct. Contains a single int for move related information.
    /// Also includes set and get functions for the relevant data stored in the int bits.
    /// </summary>
    public struct Move : ICloneable
    {
        // offsets for bit positions in move data
        private const int MoveSideOffset = 29;

        private const int MovePieceOffset = 26;

        private const int CapturePieceOffset = 22;

        private const int PromotePieceOffset = 18;

        private const int MoveTypeOffset = 12;

        private const int MoveFromSquareOffset = 6;

        /// <summary>
        /// Contains ALL relevant move information.
        /// See the constant offsets for details about which bits contains what data.
        /// </summary>
        private int _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> struct.
        /// Necessary extra constructor as it can sometimes be required to only created a basic move.
        /// </summary>
        /// <param name="from">The from square/// </param>
        /// <param name="to">The to square</param>
        public Move(Square from, Square to)
            : this()
        {
            SetFromSquare(from);
            SetToSquare(to);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> struct.
        /// Constructor for capture moves
        /// </summary>
        /// <param name="piece">The moving piece</param>
        /// <param name="captured">The captured piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="type">The move type</param>
        public Move(Piece piece, Piece captured, Square from, Square to, EMoveType type)
            : this(from, to)
        {
            SetMovingPiece(piece);
            SetCapturedPiece(captured);
            SetMoveType(type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> struct.
        /// Constructor mainly for use with UI quiet move generation.
        /// It contains implicit no capture piece or promotion piece.
        /// Type is implicit Quiet.
        /// </summary>
        /// <param name="piece">The piece to move</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        public Move(Piece piece, Square from, Square to)
            : this(from, to)
        {
            SetMovingPiece(piece);
            SetMoveType(EMoveType.Quiet);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> struct.
        /// Constructor for capture+promotion moves
        /// </summary>
        /// <param name="piece">The moving piece</param>
        /// <param name="captured">The captured piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="type">The move type</param>
        /// <param name="promotedEPiece">The promotion piece</param>
        public Move(Piece piece, Piece captured, Square from, Square to, EMoveType type, Piece promotedEPiece)
            : this(piece, captured, from, to, type) => SetPromotedPiece(promotedEPiece);

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> struct.
        /// Constructor for quiet promotion moves
        /// </summary>
        /// <param name="piece">The moving piece</param>
        /// <param name="from">The from square</param>
        /// <param name="to">The to square</param>
        /// <param name="type">The move type</param>
        /// <param name="promoted">The promotion piece</param>
        public Move(Piece piece, Square from, Square to, EMoveType type, Piece promoted)
            : this(from, to)
        {
            SetMovingPiece(piece);
            SetMoveType(type);
            SetPromotedPiece(promoted);
        }

        private Move(int data) => _data = data;

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public int Data
        {
            get => _data;
            set => _data = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Move(string value) => new Move(new Square(value[1] - '1', value[0] - 'a'), new Square(value[3] - '1', value[2] - 'a'));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Move left, Move right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Move left, Move right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetData() => _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetFromSquare() => new Square((_data >> MoveFromSquareOffset) & 0x3F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFromSquare(Square square) => _data |= square.ToInt() << MoveFromSquareOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetToSquare() => new Square(_data & 0x3F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetToSquare(Square square) => _data |= square.ToInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetMovingPiece() => (_data >> MovePieceOffset) & 0xF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMovingPiece(Piece piece) => _data |= piece.ToInt() << MovePieceOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EPieceType GetMovingPieceType() => GetMovingPiece().Type();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetCapturedPiece() => (_data >> CapturePieceOffset) & 0xF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCapturedPieceType(EPieceType pieceType) => GetCapturedPiece().Type() == pieceType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapturedPiece(Piece piece) => _data |= piece.ToInt() << CapturePieceOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPromotedPiece() => (_data >> PromotePieceOffset) & 0xF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPromotedPiece(Piece piece) => _data |= piece.ToInt() << PromotePieceOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsQueenPromotion() => IsPromotionMove() && GetPromotedPiece().Type() == EPieceType.Queen;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Player GetMovingSide() => new Player((_data >> MoveSideOffset) & 0x1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSideMask() => GetMovingSide() << 0x3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EMoveType GetMoveType() => (EMoveType)((_data >> MoveTypeOffset) & 0x3F);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMoveType(EMoveType moveType) => _data |= (int)moveType << MoveTypeOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsType(EMoveType moveType) => GetMoveType() == moveType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCaptureMove() => GetMoveType().HasFlagFast(EMoveType.Epcapture | EMoveType.Capture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnPassantMove() => GetMoveType().HasFlagFast(EMoveType.Epcapture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCastlelingMove() => GetMoveType().HasFlagFast(EMoveType.Castle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPromotionMove() => GetMoveType().HasFlagFast(EMoveType.Promotion);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDoublePush() => GetMoveType().HasFlagFast(EMoveType.Doublepush);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullMove() => _data == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidMove() => GetFromSquare().ToInt() != GetToSquare().ToInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Clone() => new Move(_data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Move other) => _data == other._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is Move move && Equals(move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() =>
            IsNullMove()
                ? ".."
                : !IsPromotionMove()
                    ? $"{GetFromSquare().GetSquareString()}{GetToSquare().GetSquareString()}"
                    : $"{GetFromSquare().GetSquareString()}{GetToSquare().GetSquareString()}{GetPromotedPiece().GetPromotionChar()}";
    }
}