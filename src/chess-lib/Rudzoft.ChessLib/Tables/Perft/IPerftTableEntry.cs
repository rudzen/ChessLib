namespace Rudzoft.ChessLib.Tables.Perft;

public interface IPerftTableEntry : ITableEntry
{
    public UInt128 Count { get; set; }
    public int Depth { get; set; }
}