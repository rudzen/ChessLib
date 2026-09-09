using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;

namespace Rudzoft.ChessLib.WebApi.Queries;

public sealed record MoveQuery(string Fen = FenData.StartPositionFen, MoveGenerationTypes Types = MoveGenerationTypes.Legal, ChessMode Mode = ChessMode.Normal);