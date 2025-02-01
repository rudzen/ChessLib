namespace Rudzoft.Perft.Settings.Settings;

public sealed class TranspositionTableSettings
{
    public const string RootName = "TranspositionTable";

    public bool Use { get; init; }

    public int Size { get; init; }
}