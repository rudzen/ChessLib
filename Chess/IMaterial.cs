namespace Rudz.Chess
{
    using Enums;
    using Types;

    public interface IMaterial
    {
        int MaterialValueTotal { get; }
        int MaterialValueWhite { get; }
        int MaterialValueBlack { get; }

        void Add(Piece piece);

        void UpdateKey(Player side, EPieceType pieceType, int delta);

        void MakeMove(Move move);

        int Count(Player side, EPieceType pieceType);

        void Clear();
    }
}