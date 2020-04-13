namespace Rudz.Chess
{
    using Enums;
    using Types;

    public interface IMoveAmbiguity
    {
        string ToNotation(Move move, MoveNotations notation = MoveNotations.Fan);
    }
}