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

using System.Runtime.InteropServices;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Hash.Tables.Transposition;

/// The TTEntry is the class of transposition table entries
///
/// A TTEntry needs 128 bits to be stored
///
/// bit  0-31: key
/// bit 32-63: data
/// bit 64-79: value
/// bit 80-95: depth
/// bit 96-111: static value
/// bit 112-127: margin of static value
///
/// the 32 bits of the data field are so defined
///
/// bit  0-15: move
/// bit 16-20: not used
/// bit 21-22: value type
/// bit 23-31: generation
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct TTEntry
{
    internal uint key;
    internal Move move16;
    internal Bound bound;
    internal byte generation8;
    internal short value16;
    internal short depth16;
    internal short staticValue;
    internal short staticMargin;

    internal void save(uint k, Value v, Bound t, Depth d, Move m, int g, Value statV, Value statM)
    {
        key = k;
        move16 = m;
        bound = t;
        generation8 = (byte)g;
        value16 = (short)v.Raw;
        depth16 = (short)d.Value;
        staticValue = (short)statV.Raw;
        staticMargin = (short)statM.Raw;
    }

    internal void set_generation(int g) { generation8 = (byte)g; }

    //internal UInt32 key() { return key32; }
    public Depth depth() { return depth16; }
    internal Move move() { return move16; }
    internal Value value() { return value16; }
    public Bound type() { return bound; }
    internal int generation() { return generation8; }
    internal Value static_value() { return staticValue; }
    internal Value static_value_margin() { return staticMargin; }
}
