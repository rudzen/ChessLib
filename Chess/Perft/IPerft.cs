using System.Threading.Tasks;

namespace Rudz.Chess.Perft
{
    public interface IPerft
    {
        Task<ulong> DoPerft();

        void ClearPositions();

        void AddPosition(PerftPosition pp);

        void AddStartPosition();

        ulong GetPositionCount(int index, int positionIndex);
    }
}