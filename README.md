# ChessLib
A C# chess data library with complete move generation and all needed custom types.

[![Build status](https://ci.appveyor.com/api/projects/status/6dksl8dsq5s1n2uv/branch/master?svg=true)](https://ci.appveyor.com/project/rudzen/chesslib/branch/master)
[![Build & Test](https://github.com/rudzen/ChessLib/actions/workflows/test.yml/badge.svg)](https://github.com/rudzen/ChessLib/actions/workflows/test.yml)
![Nuget](https://img.shields.io/nuget/v/Rudzoft.ChessLib)

## Requirements

* .NET 6.0+

## What is this for?

This library contains all the data, types and structures for which to create a piece of
chess software. It does not contain any heuristics or search algorithms as these
are meant to be implemented separately.

It also contains KPK bit compact data to determine endgame draw.

## Can I use this as a starting point for my chess software?

Yes you can, it is designed with that in mind.

## Features

* Custom perft application which uses the library to calculate and compare results from custom positions
* Transposition Table
* Complete move generation with several types
  * Legal
  * Captures
  * Quiets
  * NonEvasions
  * Evasions
  * QuietChecks
* Custom compact and very efficient types with tons of operators and helper functionality
  * Bitboard
  * CastleRight
  * Depth
  * Direction
  * ExtMove (move + score)
  * File
  * HashKey
  * Move
  * Piece
  * PieceSquare (for UI etc)
  * PieceValue
  * Player
  * Rank
  * Score
  * Square
  * Value
* Bitboard use with piece attacks for all types, including lots of helper functions
* Very fast FEN handling with optional legality check
* Magic bitboard implementation Copyright (C) 2007 Pradyumna Kannan. Converted to C#
* FEN input and output supported
* Chess960 support
* Zobrist key support
* Basic UCI structure
* HiRes timer
* Draw by repetition detection
* Mate validation
* Notation generation
  * Coordinate
  * FAN
  * ICCF
  * LAN
  * RAN
  * SAN
  * SMITH
  * UCI
* Benchmark project for perft
* Custom MoveList data structure
* Pawn blockage algorithm
* Cuckoo repetition algorithm
* Polyglot book support
* Plenty of unit tests to see how it works

### Perft

Perft console test program approximate timings to depth 6 for normal start position

* AMD-FX 8350 = ~12.5 seconds. (without TT) (earlier version)
* Intel i7-8086k = ~3.3 seconds

### Transposition Table

ph

### Move Generator

Example

```c#
// generate all legal moves for current position
const string fen = "rnbqkbnr/1ppQpppp/p2p4/8/8/2P5/PP1PPPPP/RNB1KBNR b KQkq - 1 6";

var game = GameFactory.Create(fen);
var moveList = game.Pos.GenerateMoves();
// ..
```

## What is not included?

* Evaluation (except KPK)
* Search
* Communication using e.i. UCI (base parameter struct supplied though)

## Planned

* Basic chess engine (search + evaluation) w. UCI support
