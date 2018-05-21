# ChessLib
A C# chess data library with complete move generation and all needed custom types.

## What is this for?

This library contains all the data, types and structures for which to create a piece of
chess software. It does not contain any heuristics or search algorithms as these
are ment to be implemented separatly.

## But why?

This library was developed during a course "Advanced C# and object oriented programming E17" at DTU (Danish Technical University) as part of a chess user interface, which will be released at a later stage when it is ready.
Since the UI needs to handle legality, mate check etc etc, all these features are contained within the library.

## Can i use this as a starting point for my chess software?

Yes you can, it is designed with that in mind.


## Is it fast?

Yes. As far as C# goes it should be able to hold its own.

## Features

 * Complete move generation with legality check.
 * Custom compact types for Bitboard, Square, Piece, Move (a single int) and Player with tons of operators and helper functionality.
 * 100% Bitboard support with piece attacks for all types, including lots of helper functions.
 * Lots of pre-calculated bitboard arrays.
 * FEN handling with optional legality check.
 * Magic bitboard implementation Copyright (C) 2007 Pradyumna Kannan. Converted to C#.
 * FEN input and output supported.
 * Chess960 support.
 * Zobrist key support.
 * Basic UCI structure.
 * HiRes timer.
 * Draw by repetition detection.
 * Mate validation.
 * Plenty of unit tests to see how it works.
 * Notation generation for the following notation types: Fan, San, Lan, Ran, Uci.
 * Very basic perft project included, performs depth 6 (119060324 moves) on a AMD-FX 8350 in ~12.5 seconds.
 
## What exactly is not included?

 * Evaluation
 * Search
 * Transposition table
 * Communication using e.i. UCI (base parameter struct supplied though)

## Planned

 * Transposition table
 * Better move generation with staged enumerator
 * Better state structure
 