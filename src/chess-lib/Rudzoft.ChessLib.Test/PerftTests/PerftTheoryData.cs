using System.Diagnostics.Contracts;

namespace Rudzoft.ChessLib.Test.PerftTests;

public sealed class PerftTheoryData : TheoryData<string, int, ulong>
{
    public PerftTheoryData(string[] fens, int[] depths, ulong[] results)
    {
        Contract.Assert(fens != null);
        Contract.Assert(depths != null);
        Contract.Assert(results != null);
        Contract.Assert(fens.Length == depths.Length);
        Contract.Assert(fens.Length == results.Length);

        for (var i = 0; i < fens.Length; i++)
            Add(fens[i], depths[i], results[i]);
    }
    
}