namespace Rudz.Chess.UCI
{
    public struct HiResTimerArgs
    {
        public HiResTimerArgs(double delay, int id)
        {
            Delay = delay;
            Id = id;
        }

        public double Delay;

        public int Id;
    }
}