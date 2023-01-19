using System.Collections.Concurrent;

namespace Logic.Pieces.Engine;

public class Engine
{
    private readonly Game _game;

    public EvaluatedMoveChain LastChain { get; private set; }

    public Engine(Game game)
    {
        _game = game;
    }

    public async Task<Move> GetMove(int depth)
    {
        var colorToMove = _game.CurrentTurn;
        var bestChain =
            await Task.Run(() => GetBestChain(depth, _game.Board, colorToMove, new EvaluatedMoveChain()));

        LastChain = bestChain;
        return bestChain.Moves[0];
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

    private static EvaluatedMoveChain GetBestChain(int depth, Board board, PieceColor color, EvaluatedMoveChain prevChain)
    {
        if (depth == 0)
            return prevChain;

        var otherColor = Piece.InvertColor(color);

        // make all moves
        var allChains = new ConcurrentBag<EvaluatedMoveChain>();
        var allMoves = GetAllMoves(board, color);
        Parallel.ForEach(allMoves, move =>
        {
            var ifMakeMove = board.IfMakeMove(move);
            var evalAfterMove = Evaluate(ifMakeMove, otherColor);
            var chain = prevChain.AddMove(move, evalAfterMove);

            // get other color best move
            chain = GetBestChain(depth - 1, ifMakeMove, otherColor, chain);

            allChains.Add(chain);
        });

        // no moves - mate or stalemate
        if (!allChains.Any())
            return prevChain;

        // pick the best
        var allChainsList = allChains.ToList();
        allChainsList.Sort((chain, otherChain) => chain.Evaluation.CompareTo(otherChain.Evaluation));
        var bestChain = color == PieceColor.Black ? allChainsList[0] : allChainsList[^1];
        return bestChain;
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