using System.Diagnostics.SymbolStore;

namespace Logic.Pieces.Engine;

public class Engine
{
    private readonly Game _game;

    public Engine(Game game)
    {
        _game = game;
    }

    public async Task<Move> GetMove()
    {
        await Task.Delay(Random.Shared.Next(700, 1300));

        var colorToMove = _game.CurrentTurn;
        var bestMove = Calculate(_game.Board, colorToMove);
        return bestMove;
    }

    private static IEnumerable<Move> GetAllMoves(Board board, PieceColor colorToMove)
    {
        return from piece in board.Pieces.Values
            where piece.Color == colorToMove
            let positions = Board.GetMoves(piece)
            where positions.Count > 0
            from pos in positions
            select new Move(piece.Position, pos);
    }

    private static Move Calculate(Board board, PieceColor colorToMove)
    {
        var ourColor = colorToMove;
        var enemyColor = Piece.InvertColor(ourColor);

        var allOurMoves = GetAllMoves(board, ourColor);
        var allOurChains = new List<EvaluatedMoveChain>();
        foreach (var move in allOurMoves)
        {
            var ourMoveChain = new EvaluatedMoveChain();

            // our turn
            var ifWeMakeTurn = board.IfMove(move);
            var evalAfterOurTurn = Evaluate(ifWeMakeTurn, enemyColor);
            ourMoveChain = ourMoveChain.AddMove(move, evalAfterOurTurn);

            // enemy answer
            var allEnemyChains = new List<EvaluatedMoveChain>();
            var allEnemyAnswers = GetAllMoves(ifWeMakeTurn, enemyColor);
            foreach (var enemyAnswer in allEnemyAnswers)
            {
                var ifEnemyAnswers = ifWeMakeTurn.IfMove(enemyAnswer);
                var evalAfterEnemyAnswer = Evaluate(ifEnemyAnswers, ourColor);
                var enemyAnswerChain = ourMoveChain.AddMove(move, evalAfterEnemyAnswer);
                allEnemyChains.Add(enemyAnswerChain);
            }

            // false if we have mate or stalemate
            if (allEnemyChains.Any())
            {
                // we pick the best possible enemy answer
                allEnemyChains.Sort((chain, otherChain) => chain.Evaluation.CompareTo(otherChain.Evaluation));
                var enemyBestChain = enemyColor == PieceColor.Black ? allEnemyChains[0] : allEnemyChains[^1];
                ourMoveChain = enemyBestChain;
            }

            allOurChains.Add(ourMoveChain);
        }

        // we pick the best move
        allOurChains.Sort((chain, otherChain) => chain.Evaluation.CompareTo(otherChain.Evaluation));
        var ourBestChain = ourColor == PieceColor.Black ? allOurChains[0] : allOurChains[^1];
        return ourBestChain.Moves[0];
    }

    private static int Evaluate(Board board, PieceColor color)
    {
        var gameState = Game.GetState(board, color);
        switch (gameState)
        {
            case GameState.Stalemate:
                return 0;
            case GameState.BlackWin:
                return int.MinValue;
            case GameState.WhiteWin:
                return int.MaxValue;
        }

        var evaluation = 0;

        var whitePieces = board.Pieces.Values.Where(p => p.Color == PieceColor.White);
        var blackPieces = board.Pieces.Values.Where(p => p.Color == PieceColor.Black);

        evaluation += whitePieces.Sum(GetPieceValue);
        evaluation -= blackPieces.Sum(GetPieceValue);

        return evaluation;
    }

    private static int GetPieceValue(Piece piece)
    {
        return piece switch
        {
            Queen => 9,
            Rook => 5,
            Bishop => 3,
            Knight => 3,
            Pawn => 1,
            King => 0
        };
    }
}

public readonly struct EvaluatedMoveChain
{
    private readonly Move[] _moves;

    public IReadOnlyList<Move> Moves => _moves;

    public int Evaluation { get; }

    public EvaluatedMoveChain()
    {
        _moves = Array.Empty<Move>();
        Evaluation = 0;
    }

    private EvaluatedMoveChain(Move[] moves, int eval)
    {
        _moves = moves;
        Evaluation = eval;
    }

    public EvaluatedMoveChain AddMove(Move move, int eval)
    {
        var moves = new Move[_moves.Length + 1];
        Array.Copy(_moves, moves, _moves.Length);
        moves[^1] = move;
        return new EvaluatedMoveChain(moves, Evaluation + eval);
    }
}