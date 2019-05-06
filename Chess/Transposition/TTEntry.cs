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

namespace Rudz.Chess.Transposition
{
    using Types;

    public struct TTEntry
    {
        public uint Key32;
        public Move Move;
        public sbyte Depth;
        public sbyte Generation;
        public int Value;
        public int StaticValue;
        public Bound Type;

        public TTEntry(uint k, Move m, sbyte d, sbyte g, int v, int sv, Bound b)
        {
            Key32 = k;
            Move = m;
            Depth = d;
            Generation = g;
            Value = v;
            StaticValue = sv;
            Type = b;
        }

        public TTEntry(TTEntry tte)
        {
            this = tte;
        }

        public void Defaults()
        {
            Key32 = 0;
            Move = MoveExtensions.EmptyMove;
            Depth = sbyte.MinValue;
            Generation = 1;
            Value = StaticValue = int.MaxValue;
            Type = Bound.Void;
        }

        public void Save(TTEntry tte)
        {
            Key32 = tte.Key32;
            if (tte.Move != MoveExtensions.EmptyMove)
                Move = tte.Move;
            Depth = tte.Depth;
            Generation = tte.Generation;
            Value = tte.Value;
            StaticValue = tte.StaticValue;
            Type = tte.Type;
        }
    };
}