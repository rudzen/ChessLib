/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2019 Rudy Alex Kohn

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

namespace Rudz.Chess.TranspositionTable
{
    using Types;

    public struct TTEntry
    {
        public uint key32;
        public Move move;
        public sbyte depth;
        public byte generation;
        public int value;
        public int staticValue;
        public Bound type;

        public TTEntry(uint k, Move m, sbyte d, byte g, int v, int sv, Bound b)
        {
            key32 = k;
            move = m;
            depth = d;
            generation = g;
            value = v;
            staticValue = sv;
            type = b;
        }

        public TTEntry(TTEntry tte)
        {
            this = tte;
        }

        public void Defaults()
        {
            key32 = 0;
            move = MoveExtensions.EmptyMove;
            depth = sbyte.MinValue;
            generation = 0;
            value = staticValue = int.MaxValue;
            type = Bound.Void;
        }

        public void Save(TTEntry tte)
        {
            key32 = tte.key32;
            if (tte.move != MoveExtensions.EmptyMove)
                move = tte.move;
            depth = tte.depth;
            generation = tte.generation;
            value = tte.value;
            staticValue = tte.staticValue;
            type = tte.type;
        }

    };
}