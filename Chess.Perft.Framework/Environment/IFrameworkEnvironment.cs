namespace Chess.Perft.Environment
{
    public interface IFrameworkEnvironment
    {
        bool IsDevelopment { get; set; }

        string Configuration { get; }

        string DotNetFrameWork { get; }

        string Os { get; }

        bool IsAdmin { get; }

        bool HighresTimer { get; }
    }
}