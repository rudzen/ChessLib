namespace Rudz.Chess.Perft
{
    public interface IPerft
    {
        ulong DoPerft();

        void ClearPositions();

        void AddPosition(PerftPosition pp);

        void AddStartPosition();

        ulong GetPositionCount(int index, int positionIndex);
    }
}