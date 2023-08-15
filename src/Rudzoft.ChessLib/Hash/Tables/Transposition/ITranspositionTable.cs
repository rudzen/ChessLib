using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

public interface ITranspositionTable
{
    ulong Hits { get; }
    
    /// TranspositionTable::set_size() sets the size of the transposition table,
    /// measured in megabytes.
    void SetSize(uint mbSize);

    /// TranspositionTable::clear() overwrites the entire transposition table
    /// with zeroes. It is called whenever the table is resized, or when the
    /// user asks the program to clear the table (from the UCI interface).
    void Clear();

    /// TranspositionTable::store() writes a new entry containing position key and
    /// valuable information of current position. The lowest order bits of position
    /// key are used to decide on which cluster the position will be placed.
    /// When a new entry is written and there are no empty entries available in cluster,
    /// it replaces the least valuable of entries. A TTEntry t1 is considered to be
    /// more valuable than a TTEntry t2 if t1 is from the current search and t2 is from
    /// a previous search, or if the depth of t1 is bigger than the depth of t2.
    void Store(in HashKey posKey, Value v, Bound t, Depth d, Move m, Value statV, Value kingD);

    /// TranspositionTable::probe() looks up the current position in the
    /// transposition table. Returns a pointer to the TTEntry or NULL if
    /// position is not found.
    bool Probe(in HashKey posKey, ref uint ttePos, out TTEntry entry);

    /// TranspositionTable::new_search() is called at the beginning of every new
    /// search. It increments the "generation" variable, which is used to
    /// distinguish transposition table entries from previous searches from
    /// entries from the current search.
    void NewSearch();
}