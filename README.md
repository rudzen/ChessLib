# ChessLib
A C# chess data library with complete move generation and all needed custom types.

[![Build status](https://ci.appveyor.com/api/projects/status/6dksl8dsq5s1n2uv/branch/master?svg=true)](https://ci.appveyor.com/project/rudzen/chesslib/branch/master)

## Requirements

* .NET 6.0+

## What is this for?

This library contains all the data, types and structures for which to create a piece of
chess software. It does not contain any heuristics or search algorithms as these
are meant to be implemented separately.

## But why?

This library was developed during a course "Advanced C# and object oriented programming E17" at DTU (Danish Technical University) as part of a chess user interface, which will be released at a later stage when it is ready.
Since the UI needs to handle legality, mate check etc., all these features are contained within the library.

## Can I use this as a starting point for my chess software?

Yes you can, it is designed with that in mind.

## Features

* Custom perft application which uses the library to calculate and compare results from custom positions
* Transposition Table
* Complete move generation with legality check
* Custom compact and very efficient types for Bitboard, Square, Piece, Move, Player, File, Rank and Direction with tons of operators and helper functionality
* 100% Bitboard support with piece attacks for all types, including lots of helper functions
* Lots of pre-calculated bitboard arrays to aid in the creation of an evaluation functionality
* FEN handling with optional legality check
* Magic bitboard implementation Copyright (C) 2007 Pradyumna Kannan. Converted to C#
* FEN input and output supported
* Chess960 support
* Zobrist key support
* Basic UCI structure
* HiRes timer
* Draw by repetition detection
* Mate validation
* Plenty of unit tests to see how it works
* Notation generation for the following notation types: Fan, San, Lan, Ran, Uci
* Benchmark project for perft
* Custom MoveList data structure
* Pawn blockage algorithm

## Is it fast?

Yes. As far as C# goes it should be able to hold its own.

Perft console test program approximate timings to depth 6 for normal start position

* AMD-FX 8350 = ~12.5 seconds. (without TT) (earlier version)
* Intel i7-8086k = ~3.5 seconds

## What is not included?

* Evaluation
* Search
* Communication using e.i. UCI (base parameter struct supplied though)

## Planned

* Basic chess engine (search + evaluation) w. UCI support
* As nuget package
