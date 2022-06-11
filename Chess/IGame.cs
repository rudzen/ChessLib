/*
ChessLib, a chess data structure library

MIT License

Copyright (c) 2017-2022 Rudy Alex Kohn

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

namespace Rudz.Chess;

using Enums;
using Fen;
using System;
using System.Collections.Generic;
using Types;

public interface IGame : IEnumerable<Piece>
{
    Action<Piece, Square> PieceUpdated { get; }
    BitBoard Occupied { get; }
    IPosition Pos { get; }
    GameEndTypes GameEndType { get; set; }

    bool IsRepetition { get; }

    void NewGame(string fen = Fen.Fen.StartPositionFen);

    FenData GetFen();

    void UpdateDrawTypes();

    string ToString();

    BitBoard OccupiedBySide(Player c);

    Player CurrentPlayer();

    ulong Perft(int depth, bool root);
}
