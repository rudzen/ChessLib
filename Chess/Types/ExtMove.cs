namespace Rudz.Chess.Types
{
    /// <summary>
    /// Extended move structure which combines Move and Score
    /// </summary>
    public struct ExtMove
    {

        public Move Move;

        public Score Score;

        public ExtMove(Move m, int s)
        {
            Move = m;
            Score = s;
        }
    }
}