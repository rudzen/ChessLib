/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2023 Rudy Alex Kohn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.ObjectPool;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Notation.Notations;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Notation;

public sealed class SanToMove : ISanToMove
{
    private readonly ObjectPool<IMoveList> _moveListPool;

    public SanToMove(ObjectPool<IMoveList> moveListPool)
    {
        _moveListPool = moveListPool;
    }

    public IReadOnlyList<Move> FromNotation(IPosition pos, IEnumerable<string> notationalMoves, INotation notation)
    {
        var moveList = _moveListPool.Get();
        var result = new List<Move>();

        var validNotationalMoves = notationalMoves
            .Where(static notationalMove => !string.IsNullOrWhiteSpace(notationalMove));

        var state = new State();

        foreach (var notationalMove in validNotationalMoves)
        {
            moveList.Generate(pos);
            
            var moves = moveList.Get();
            
            if (moves.IsEmpty)
                break;

            foreach (var move in moves)
            {
                var notatedMove = notation.Convert(move);
                if (string.IsNullOrWhiteSpace(notatedMove))
                    continue;

                if (notationalMove.Equals(notatedMove))
                {
                    pos.MakeMove(move.Move, in state);
                    result.Add(move);
                    break;
                }
            }
        }

        _moveListPool.Return(moveList);

        return result;
    }

    public Move FromNotation(IPosition pos, ReadOnlySpan<char> sanMove, INotation notation)
    {
        if (sanMove.IsEmpty || sanMove[0] == '*')
            return Move.EmptyMove;

        var moveList = _moveListPool.Get();
        moveList.Generate(in pos);
        var moves = moveList.Get();

        var m = Move.EmptyMove;
        
        foreach (var move in moves)
        {
            var san = notation.Convert(move);

            if (string.IsNullOrWhiteSpace(san))
                continue;
            
            if (!sanMove.Equals(san.AsSpan(), StringComparison.InvariantCulture))
                continue;
            
            m = move;
            break;
        }

        _moveListPool.Return(moveList);

        return m;
    }
}
