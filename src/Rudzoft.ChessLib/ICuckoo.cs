namespace Rudzoft.ChessLib;

public interface ICuckoo
{
    bool HashCuckooCycle(in IPosition pos, int end, int ply);
}