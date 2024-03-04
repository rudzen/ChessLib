using System.Numerics;
using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Types;

public enum PieceTypes : byte
{
    NoPieceType = 0,
    Pawn = 1,
    Knight = 2,
    Bishop = 3,
    Rook = 4,
    Queen = 5,
    King = 6,
    PieceTypeNb = 7,
    AllPieces = NoPieceType
}

public readonly record struct PieceType(PieceTypes Value) : IMinMaxValue<PieceType>
{
    public PieceType(int pt) : this((PieceTypes)pt) { }

    private PieceType(PieceType pt) : this(pt.Value) { }

    public static int Count => (int)PieceTypes.PieceTypeNb + 1;

    public static PieceType NoPieceType => new(PieceTypes.NoPieceType);
    public static PieceType Pawn => new(PieceTypes.Pawn);
    public static PieceType Knight => new(PieceTypes.Knight);
    public static PieceType Bishop => new(PieceTypes.Bishop);
    public static PieceType Rook => new(PieceTypes.Rook);
    public static PieceType Queen => new(PieceTypes.Queen);
    public static PieceType King => new(PieceTypes.King);
    public static PieceType AllPieces => new(PieceTypes.AllPieces);

    public static PieceType[] AllPieceTypes =>
    [
        Pawn, Knight, Bishop, Rook, Queen, King
    ];

    public static PieceType MaxValue => King;

    public static PieceType MinValue => Pawn;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator PieceType(int value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator PieceType(PieceTypes pt) => new(pt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(PieceType left, PieceTypes right) => left.Value == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(PieceType left, PieceTypes right) => left.Value != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(PieceType left, PieceTypes right) => left.Value <= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(PieceType left, PieceTypes right) => left.Value >= right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(PieceType left, PieceTypes right) => left.Value < right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(PieceType left, PieceTypes right) => left.Value > right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator ++(PieceType left) => new(left.Value + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator --(PieceType left) => new(left.Value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator true(PieceType pc) => pc != NoPieceType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator false(PieceType pc) => pc == NoPieceType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(PieceType pc) => (int)pc.Value;

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public int AsInt() => (int)Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Piece MakePiece(Player side) => (int)Value | (side << 3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSlider() => InBetween(PieceTypes.Bishop, PieceTypes.Queen);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBetween(PieceTypes min, PieceTypes max) =>
        (uint)Value - (uint)min <= (uint)max - (uint)min;
}