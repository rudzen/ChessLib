namespace Rudzoft.ChessLib.Tables.Perft;

public interface IPerftTableEntry : ITableEntry
{
    public ulong Count { get; set; }
    public int Depth { get; set; }
}