using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Rudzoft.ChessLib.Types;

public sealed class RootMove : List<Move>
{
    public RootMove(Move m)
    {
        Add(m);
    }
    
    public Value OldValue { get; set; }
    
    public Value NewValue { get; set; }
    
    public Depth SelDepth { get; set; }
    
    public int TbRank { get; set; }
    
    public Value TbValue { get; set; }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator RootMove(Move m)
        => new(m);

    public static bool operator !=(RootMove left, Move right)
        => left.FirstOrDefault() != right;

    public static bool operator ==(RootMove left, Move right)
        => left.FirstOrDefault() == right;
    
    public static bool operator <(RootMove left, RootMove right)
        => left.NewValue > right.NewValue || left.NewValue == right.NewValue && left.OldValue > right.OldValue;

    public static bool operator >(RootMove left, RootMove right)
        => left.NewValue < right.NewValue || left.NewValue == right.NewValue && left.OldValue < right.OldValue;
}
