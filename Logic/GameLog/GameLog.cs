using Logic.Pieces;

namespace Logic.GameLog;

public class GameLog
{
    private readonly List<GameLogTurn> _turns = new();

    public IReadOnlyList<GameLogTurn> Turns => _turns;

    public void AddMove(Piece piece, Move move, bool isTake, bool isCheck, bool isMate, bool isCastle,
        IEnumerable<Position> extra, PromotionPiece? promotionPiece)
    {
        var logMove = new GameLogMove(move, piece, isTake, isCheck, isMate, isCastle, extra, promotionPiece);
        if (logMove.Piece.Color == PieceColor.White)
            _turns.Add(new GameLogTurn(_turns.Count + 1) {WhiteMove = logMove});
        else
            _turns[^1].BlackMove = logMove;
    }
}