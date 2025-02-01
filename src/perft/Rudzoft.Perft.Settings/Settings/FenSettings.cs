namespace Rudzoft.Perft.Settings.Settings;

public sealed class FenSettings
{
    public const string RootName = "Fen";

    public FenEntry[] Entries { get; init; } = [];
}

public sealed class FenEntry
{
    public string Fen { get; init; } = null!;

    public DepthValues[] Depths { get; init; } = [];
}

public sealed class DepthValues
{
    public int Depth { get; init; }

    public ulong ExpectedMoveCount { get; init; }
}