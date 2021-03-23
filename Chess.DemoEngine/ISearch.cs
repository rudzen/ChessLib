using Rudz.Chess;
using Rudz.Chess.MoveGeneration;

namespace Chess.DemoEngine
{
    public interface ISearch
    {
        void PickNextMove(int move_num, IMoveList moveList);

        void ClearSearchInfo(SearchInfo searchInfo);

        int Quiescence(int alpha, int beta, SearchInfo searchinfo);

        int alphaBeta(int alpha, int beta, int depth, SearchInfo searchinfo, bool includeNull);

        void Search(SearchInfo searchInfo);
    }
}