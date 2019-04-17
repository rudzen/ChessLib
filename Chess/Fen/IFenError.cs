namespace Rudz.Chess.Fen
{
    public interface IFenError
    {
        int ErrorNumber { get; set; }

        int FenIndex { get; set; }
    }
}