using Rudz.Chess;

namespace Chess.DemoEngine
{
    /// <summary>
    /// Demo engine search
    /// </summary>
    public sealed class Searcher : ISearch
    {
        public void PickNextMove(int move_num, IMoveList moveList)
        {
            throw new System.NotImplementedException();
        }

        public void ClearSearchInfo(SearchInfo searchInfo)
        {
            throw new System.NotImplementedException();
        }

        public int Quiescence(int alpha, int beta, SearchInfo searchinfo)
        {
            throw new System.NotImplementedException();
        }

        public int alphaBeta(int alpha, int beta, int depth, SearchInfo searchinfo, bool includeNull)
        {
            throw new System.NotImplementedException();
        }

        public void Search(SearchInfo searchInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}