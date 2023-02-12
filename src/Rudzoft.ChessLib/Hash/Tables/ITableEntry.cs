using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables;

public interface ITableEntry
{
    HashKey Key { get; set; }

    Score Evaluate(IPosition pos);
    
    Score Evaluate(IPosition pos, in BitBoard attacks, Player own);
}