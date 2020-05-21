namespace Rudz.Chess.Enums
{
    public enum Phases
    {
        EndGame,
        MidGame = 128,
        Mg = 0,
        Eg = 1,
        PhaseNb = 2
    }

    public static class PhasesExtensions
    {
        public static int AsInt(this Phases @this) => (int) @this;
    }
}