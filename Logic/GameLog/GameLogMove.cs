using Logic.Pieces;

namespace Logic.GameLog;

public class GameLogMove
{
    public Move Move { get; }
    public Piece Piece { get; }
    public bool IsTake { get; }
    public bool IsCastle { get; }
    public bool IsCheck { get; }
    public bool IsMate { get; }
    public IEnumerable<Position> Extra { get; }
    public bool IsPromotion { get; }
    public PromotionPiece? PromotionPiece { get; }

    public GameLogMove(Move move, Piece piece, bool isTake, bool isCheck, bool isMate, bool isCastle,
        IEnumerable<Position> extra, PromotionPiece? promotionPiece)
    {
        Move = move;
        Piece = piece;
        IsTake = isTake;

        if (isMate && !isCheck)
            throw new InvalidOperationException();

        IsCheck = isCheck;
        IsMate = isMate;
        IsCastle = isCastle;
        Extra = extra;
        IsPromotion = promotionPiece is not null;
        PromotionPiece = promotionPiece;
    }
}