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
using System.Linq;
using Rudzoft.ChessLib.Extensions;
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
    public string ErrorMsg { get; private set; }
    public bool IsOk { get; private set; }

    public IPositionValidator Validate(in IPosition pos, PositionValidationTypes type = PositionValidationTypes.All)
    {
        var error = string.Empty;

        if (type.HasFlagFast(PositionValidationTypes.Basic))
            error = ValidateBasic(in pos);

        if (type.HasFlagFast(PositionValidationTypes.Castle))
            error = ValidateCastleling(in pos, error);

        if (type.HasFlagFast(PositionValidationTypes.Kings))
            error = ValidateKings(in pos, error);

        if (type.HasFlagFast(PositionValidationTypes.Pawns))
            error = ValidatePawns(in pos, error);

        if (type.HasFlagFast(PositionValidationTypes.PieceConsistency))
            error = ValidatePieceConsistency(in pos, error);

        if (type.HasFlagFast(PositionValidationTypes.PieceCount))
            error = ValidatePieceCount(pos, error);

        if (type.HasFlagFast(PositionValidationTypes.PieceTypes))
            error = ValidatePieceTypes(in pos, error);

        if (type.HasFlagFast(PositionValidationTypes.State))
            error = ValidateState(in pos, error);

        IsOk = error.IsNullOrEmpty();
        ErrorMsg = error;
        return this;
    }

    private static string ValidateBasic(in IPosition pos)
    {
        var error = string.Empty;
        if (pos.SideToMove != Player.White && pos.SideToMove != Player.Black)
            error = AddError(error, $"{nameof(pos.SideToMove)} is not a valid");

        if (pos.GetPiece(pos.GetKingSquare(Player.White)) != Piece.WhiteKing)
            error = AddError(error, "white king position is not a white king");

        if (pos.GetPiece(pos.GetKingSquare(Player.Black)) != Piece.BlackKing)
            error = AddError(error, "black king position is not a black king");

        if (pos.EnPassantSquare != Square.None && pos.EnPassantSquare.RelativeRank(pos.SideToMove) != Ranks.Rank6)
            error = AddError(error, $"{nameof(pos.EnPassantSquare)} square is not on rank 6");

        return error;
    }

    private string ValidateCastleling(in IPosition pos, string error)
    {
        Span<Player> players = stackalloc Player[] { Player.White, Player.Black };
        Span<CastleRight> crs = stackalloc CastleRight[] { CastleRight.None, CastleRight.None };

        foreach (var c in players)
        {
            crs[0] = CastleRights.King.MakeCastleRights(c);
            crs[1] = CastleRights.Queen.MakeCastleRights(c);

            var ourRook = PieceTypes.Rook.MakePiece(c);
            foreach (var cr in crs)
            {
                if (!pos.CanCastle(cr))
                    continue;

                var rookSq = pos.CastlingRookSquare(cr);

                if (pos.GetPiece(rookSq) != ourRook)
                    error = AddError(error, $"rook does not appear on its position for {c}");

                if (pos.GetCastleRightsMask(rookSq) != cr)
                    error = AddError(error, $"castleling rights mask at {rookSq} does not match for player {c}");

                if ((pos.GetCastleRightsMask(pos.GetKingSquare(c)) & cr) != cr)
                    error = AddError(error,
                        $"castleling rights mask at {pos.GetKingSquare(c)} does not match for player {c}");
            }
        }

        return error;
    }

    private static string ValidateKings(in IPosition pos, string error)
    {
        Span<Player> players = stackalloc Player[] { Player.White, Player.Black };

        foreach (var player in players)
        {
            var count = pos.PieceCount(PieceTypes.King, player);
            if (count != 1)
                error = AddError(error, $"king count for player {player} was {count}");
        }

        if (!(pos.AttacksTo(pos.GetKingSquare(~pos.SideToMove)) & pos.Pieces(pos.SideToMove)).IsEmpty)
            error = AddError(error, "kings appear to attack each other");

        return error;
    }

    private static string ValidatePawns(in IPosition pos, string error)
    {
        if (!(pos.Pieces(PieceTypes.Pawn) & (Rank.Rank1.RankBB() | Rank.Rank8.RankBB())).IsEmpty)
            error = AddError(error, "pawns exists on rank 1 or rank 8");

        if (pos.PieceCount(PieceTypes.Pawn, Player.White) > 8)
            error = AddError(error, "white side has more than 8 pawns");

        if (pos.PieceCount(PieceTypes.Pawn, Player.Black) > 8)
            error = AddError(error, "black side has more than 8 pawns");

        return error;
    }

    private static string ValidatePieceConsistency(in IPosition pos, string error)
    {
        if (!(pos.Pieces(Player.White) & pos.Pieces(Player.Black)).IsEmpty)
            error = AddError(error, "white and black pieces overlap");

        if ((pos.Pieces(Player.White) | pos.Pieces(Player.Black)) != pos.Pieces())
            error = AddError(error, "white and black pieces do not match all pieces");

        if (pos.Pieces(Player.White).Count > 16)
            error = AddError(error, "white side has more than 16 pieces");

        if (pos.Pieces(Player.Black).Count > 16)
            error = AddError(error, "black side has more than 16 pieces");

        return error;
    }

    private static string ValidatePieceCount(IPosition pos, string error) =>
        Piece.AllPieces
            .Select(static pc => new { pc, pt = pc.Type() })
            .Select(static t => new { t, c = t.pc.ColorOf() })
            .Where(t => pos.PieceCount(t.t.pt, t.c) != pos.Pieces(t.t.pt, t.c).Count)
            .Select(static t => t.t.pc)
            .Aggregate(error, static (current, pc) => AddError(current, $"piece count does not match for piece {pc}"));

    private static string ValidatePieceTypes(in IPosition pos, string error)
    {
        Span<PieceTypes> pts = stackalloc PieceTypes[]
        {
            PieceTypes.Pawn,
            PieceTypes.Knight,
            PieceTypes.Bishop,
            PieceTypes.Rook,
            PieceTypes.Queen,
            PieceTypes.King
        };

        foreach (var p1 in pts)
        {
            foreach (var p2 in pts)
            {
                if (p1 == p2 || (pos.Pieces(p1) & pos.Pieces(p2)).IsEmpty)
                    continue;

                error = AddError(error, $"piece types {p1} and {p2} doesn't align");
            }
        }

        return error;
    }

    private string ValidateState(in IPosition pos, string error)
    {
        var state = pos.State;

        if (state.Key.Key == 0 && !pos.Pieces().IsEmpty)
            error = AddError(error, "state key is invalid");

        if (pos.Pieces(PieceTypes.Pawn, pos.SideToMove).IsEmpty &&
            state.PawnKey.Key != Zobrist.ZobristNoPawn)
            error = AddError(error, "empty pawn key is invalid");

        if (state.Repetition < 0)
            error = AddError(error, $"{nameof(state.Repetition)} is negative");

        if (state.Rule50 < 0)
            error = AddError(error, $"{nameof(state.Rule50)} is negative");

        if (state.Equals(state.Previous))
            error = AddError(error, "state has itself as previous state");

        return error;
    }

    private static string AddError(string error, string message)
        => error.IsNullOrEmpty()
            ? message
            : $"{error}, {message}";
}