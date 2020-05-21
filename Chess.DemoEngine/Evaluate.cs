using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;

namespace Chess.DemoEngine
{
    /// <summary>
    /// Demo engine evaluation
    /// </summary>
    public sealed class Evaluate
    {
        private static readonly int[] PIECE_VALUES = {0, 100, 350, 350, 550, 1000, 50000, 100, 350, 350, 550, 1000, 50000};

        // condition for end game material detection.. (should be tweaked?)
        // private static readonly int ENDGAME_MAT = 1 * PIECE_VALUES[wR] + 2 * PIECE_VALUES[wN] + 2 * PIECE_VALUES[wP] + PIECE_VALUES[wK];
        // private static readonly int ENDGAME_MAT_DELTA = PIECE_VALUES[wP] * (1 / 2);

        private static readonly int[] WP_EVAL = {
            0	,	0	,	0	,	0	,	0	,	0	,	0	,	0	,
            10	,	10	,	0	,	-10	,	-10	,	0	,	10	,	10	,
            5	,	0	,	0	,	5	,	5	,	0	,	0	,	5	,
            0	,	0	,	10	,	20	,	20	,	10	,	0	,	0	,
            5	,	5	,	5	,	10	,	10	,	5	,	5	,	5	,
            10	,	10	,	10	,	20	,	20	,	10	,	10	,	10	,
            20	,	20	,	20	,	30	,	30	,	20	,	20	,	20	,
            0	,	0	,	0	,	0	,	0	,	0	,	0	,	0
        };
    
        // basic evaluation points
        private const int PAWN_ISOLATED = -10;
        private static int[] PAWN_PASSED = {0, 5, 10, 20, 35, 60, 100, 200};

        private const int ROOK_OPEN_FILE = 10;
        private const int ROOK_OPEN_FILE_SEMI = 5;

        private const int QUEEN_OPEN_FILE = 10;
        private const int QUEEN_OPEN_FILE_SEMI = 5;

        private const int BISHOP_PAIR = 30;

        private const int KNIGHT_TRAPPED = 4;

        private const int CASTLE_KING = 10;
        private const int CASTLE_QUEEN = 5;

        private const int KING_GUARD = 30;
        private const int KING_GUARD_SEMI = 20;

        private const int W_INFINITE = 60000;
        private const int IS_MATE = (W_INFINITE - 64);

        private readonly IPosition _pos;

        public Evaluate(IPosition pos)
        {
            _pos = pos;
        }
        
        public Score Eval()
        {
            return new Score();
        }

        private Score Eval(PieceTypes pt, Player us)
        {
            var result = EvaluatePawns(Player.White) - EvaluatePawns(Player.Black);
            


        }

        private Score EvaluatePawns(Player us)
        {
            var result = new Score();
            var them = ~us;

            var squares = _pos.Squares(PieceTypes.Pawn, us);

            if (squares.IsEmpty)
                return result;

            foreach (var sq in squares)
            {
                if (sq == Square.None)
                    break;

                result += WP_EVAL[sq.Relative(us).AsInt()];

                if (_pos.PawnIsolated(sq, us))
                    result += PAWN_ISOLATED;

                if (_pos.PassedPawn(sq))
                    result += PAWN_PASSED[sq.Rank().AsInt()];
            }

            return result;
        }

        private Score EvaluateKnights(Player us)
        {
            return IS_MATE;
        }

        private Score EvaluateBishops(Player us)
        {
            return IS_MATE;
        }

        private Score EvaluateRooks(Player us)
        {
            
        }

        private Score EvaluateQueens(Player us)
        {
            
        }

        private Score EvaluatePieces(PieceTypes pt, Player us)
        {
            var them = ~us;

            var pieces = _pos.Squares(pt, us);

            if (pieces.IsEmpty)
                return Score.Zero;
            
            var result = Score.Zero;

            foreach (var square in pieces)
            {
                
            }
            




        }
        
    }
}