using Rudzoft.ChessLib.Enums;

namespace Rudzoft.ChessLib.WebApi.Queries;

public sealed record MoveQuery(string Fen = Fen.Fen.StartPositionFen, MoveGenerationTypes Types = MoveGenerationTypes.Legal, ChessMode Mode = ChessMode.Normal);
