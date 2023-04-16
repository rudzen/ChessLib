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
using Rudzoft.ChessLib.Hash;
using Rudzoft.ChessLib.Types;

namespace Rudzoft.ChessLib.Validation;

[Flags]
public enum PositionValidationTypes
{
    None = 0,
    Basic = 1,
    Castle = 1 << 1,
    Kings = 1 << 2,
    Pawns = 1 << 3,
    PieceConsistency = 1 << 4,
    PieceCount = 1 << 5,
    PieceTypes = 1 << 6,
    State = 1 << 7,
    All = Basic | Castle | Kings | Pawns | PieceConsistency | PieceCount | PieceTypes | State
}

public static class PositionValidationTypesExtensions
{
    public static bool HasFlagFast(this PositionValidationTypes @this, PositionValidationTypes flag)
        => (@this & flag) != PositionValidationTypes.None;
}

public sealed class PositionValidator : IPositionValidator
{
    public PositionValidationResult Validate(in IPosition pos, PositionValidationTypes type = PositionValidationTypes.All)
    {
        var errors = new List<string>();

        if (type.HasFlagFast(PositionValidationTypes.Basic))
            errors.AddRange(ValidateBasic(pos));

        if (type.HasFlagFast(PositionValidationTypes.Castle))
            errors.AddRange(ValidateCastleling(pos));

        if (type.HasFlagFast(PositionValidationTypes.Kings))
            errors.AddRange(ValidateKings(pos));

        if (type.HasFlagFast(PositionValidationTypes.Pawns))
            errors.AddRange(ValidatePawns(pos));

        if (type.HasFlagFast(PositionValidationTypes.PieceConsistency))
            errors.AddRange(ValidatePieceConsistency(pos));

        if (type.HasFlagFast(PositionValidationTypes.PieceCount))
            errors.AddRange(ValidatePieceCount(pos));

        if (type.HasFlagFast(PositionValidationTypes.PieceTypes))
            errors.AddRange(ValidatePieceTypes(pos));

        if (type.HasFlagFast(PositionValidationTypes.State))
            errors.AddRange(ValidateState(pos));

        var ok = errors.Count == 0;
        return new(ok, ok ? string.Empty : string.Join('\n', errors));
    }

    private static IEnumerable<string> ValidateBasic(IPosition pos)
    {
        if (pos.SideToMove != Player.White && pos.SideToMove != Player.Black)
            yield return $"{nameof(pos.SideToMove)} is not a valid";

        if (pos.GetPiece(pos.GetKingSquare(Player.White)) != Piece.WhiteKing)
            yield return "white king position is not a white king";

        if (pos.GetPiece(pos.GetKingSquare(Player.Black)) != Piece.BlackKing)
            yield return "black king position is not a black king";

        if (pos.EnPassantSquare != Square.None && pos.EnPassantSquare.RelativeRank(pos.SideToMove) != Ranks.Rank6)
            yield return $"{nameof(pos.EnPassantSquare)} square is not on relative rank 6";
    }

    private IEnumerable<string> ValidateCastleling(IPosition pos)
    {
        var crs = new[] { CastleRight.None, CastleRight.None };

        foreach (var p in Player.AllPlayers)
        {
            crs[0] = CastleRights.King.MakeCastleRights(p);
            crs[1] = CastleRights.Queen.MakeCastleRights(p);

            var ourRook = PieceTypes.Rook.MakePiece(p);
            foreach (var cr in crs)
            {
                if (!pos.CanCastle(cr))
                    continue;

                var rookSq = pos.CastlingRookSquare(cr);

                if (pos.GetPiece(rookSq) != ourRook)
                    yield return $"rook does not appear on its position for {p}";

                if (pos.GetCastleRightsMask(rookSq) != cr)
                    yield return $"castleling rights mask at {rookSq} does not match for player {p}";

                if ((pos.GetCastleRightsMask(pos.GetKingSquare(p)) & cr) != cr)
                    yield return $"castleling rights mask at {pos.GetKingSquare(p)} does not match for player {p}";
            }
        }
    }

    private static IEnumerable<string> ValidateKings(IPosition pos)
    {
        foreach (var player in Player.AllPlayers)
        {
            var count = pos.PieceCount(PieceTypes.King, player);
            if (count != 1)
                yield return $"king count for player {player} was {count}";
        }

        if ((pos.AttacksTo(pos.GetKingSquare(~pos.SideToMove)) & pos.Pieces(pos.SideToMove)).IsNotEmpty)
            yield return "kings appear to attack each other";
    }

    private static IEnumerable<string> ValidatePawns(IPosition pos)
    {
        if ((pos.Pieces(PieceTypes.Pawn) & (Rank.Rank1.RankBB() | Rank.Rank8.RankBB())).IsNotEmpty)
            yield return "pawns exists on rank 1 or rank 8";

        if (pos.PieceCount(PieceTypes.Pawn, Player.White) > 8)
            yield return "white side has more than 8 pawns";

        if (pos.PieceCount(PieceTypes.Pawn, Player.Black) > 8)
            yield return "black side has more than 8 pawns";
    }

    private static IEnumerable<string> ValidatePieceConsistency(IPosition pos)
    {
        if ((pos.Pieces(Player.White) & pos.Pieces(Player.Black)).IsNotEmpty)
            yield return "white and black pieces overlap";

        if ((pos.Pieces(Player.White) | pos.Pieces(Player.Black)) != pos.Pieces())
            yield return "white and black pieces do not match all pieces";

        if (pos.Pieces(Player.White).Count > 16)
            yield return "white side has more than 16 pieces";

        if (pos.Pieces(Player.Black).Count > 16)
            yield return "black side has more than 16 pieces";
    }

    private static IEnumerable<string> ValidatePieceCount(IPosition pos)
        => Piece.All
            .Where(pc => pos.PieceCount(pc) != pos.Pieces(pc).Count)
            .Select(static pc => $"piece count does not match for piece {pc}");

    private static IEnumerable<string> ValidatePieceTypes(IPosition pos)
        => Piece.AllTypes.SelectMany(p1 => Piece.AllTypes
                .Where(p2 => p1 != p2 && (pos.Pieces(p1) & pos.Pieces(p2)).IsNotEmpty),
            static (p1, p2) => $"piece types {p1} and {p2} doesn't align");

    private static IEnumerable<string> ValidateState(IPosition pos)
    {
        var state = pos.State;

        if (state.Key.Key == 0 && pos.PieceCount() != 0)
            yield return "state key is invalid";

        if (pos.PieceCount(PieceTypes.Pawn) == 0 && state.PawnKey != Zobrist.ZobristNoPawn)
            yield return "empty pawn key is invalid";

        if (state.Repetition < 0)
            yield return $"{nameof(state.Repetition)} is negative";

        if (state.Rule50 < 0)
            yield return $"{nameof(state.Rule50)} is negative";

        if (state.Equals(state.Previous))
            yield return "state has itself as previous state";
    }
}