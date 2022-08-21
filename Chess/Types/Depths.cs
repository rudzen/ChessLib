namespace Rudz.Chess.Types;

public enum Depths
{
    ZERO = 0,
    QS_CHECK = 0,
    QS_NO_CHECK = -1,
    QS_RECAP = -5,
    NONE = -6,
    OFFSET = NONE - 1,
    MAX_PLY = 256 + OFFSET - 4, // Used only for TT entry occupancy check
}

public readonly struct Depth
{
    private readonly int _value;

    public Depth(int depth)
    {
        _value = depth;
    }

    public Depth(Depths depth)
    {
        _value = (int)depth;
    }

    public int Value => _value;
}