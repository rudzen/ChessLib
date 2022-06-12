using System.Collections;
using System.Collections.Generic;

namespace Chess.Test.Position
{
    public class EnPassantTestData : IEnumerable<object[]>
    {
        // format:
        // fen, expected square
        public IEnumerator<object[]> GetEnumerator()
        {
            char[] validFiles = new char[]
            { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

            char[] validRanks = new char[]
            { '3', '6' };

            char[] invalidRanks = new char[]
            { '1', '2', '4', '5', '7', '8'};

            string fen;

            foreach (var file in validFiles)
            {
                // create all "correct" fen en-passant square fens
                foreach (var rank in validRanks)
                {
                    fen = $"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq {file}{rank} 0 1";
                    yield return new object[]
                    {
                        fen,
                        new Rudz.Chess.Types.Square(rank - '1', file - 'a')
                    };
                }

                // create all with invalid ranks fen en-passant square fens
                foreach (var rank in invalidRanks)
                {
                    fen = $"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq {file}{rank} 0 1";
                    yield return new object[]
                    {
                        fen,
                        Rudz.Chess.Types.Square.None
                    };
                }

                // no en-passant square
                fen = $"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1";
                yield return new object[]
                {
                        fen,
                        Rudz.Chess.Types.Square.None
                };

                // incorrect single en-passant in fen
                fen = $"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq {file} 0 1";
                yield return new object[]
                {
                        fen,
                        Rudz.Chess.Types.Square.None
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
