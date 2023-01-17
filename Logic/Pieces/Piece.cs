namespace Logic.Pieces;

public enum PieceColor
{
    White,
    Black
}

public interface ICastlePiece
{
    public bool DidMove { get; set; }
}

public abstract class Piece
{
    public Position Position => Field.Position;
    public Field Field { get; set; }
    public PieceColor Color { get; }

    protected Piece(Field field, PieceColor color)
    {
        Field = field;
        Color = color;
    }

    public bool CanMove(Position newPosition)
    {
        if (Field.Board[newPosition].IsOccupied)
            return false;

        var canMove = CanMoveInternal(newPosition);
        if (!canMove)
            return false;

        var ifMove = Field.Board.IfMove(new Move(Position, newPosition));
        var allyKing = Color == PieceColor.White ? ifMove.WhiteKing : ifMove.BlackKing;
        return !allyKing.IsInCheck();
    }

    public bool CanTake(Position newPosition)
    {
        // TODO override for en passant
        if (!Field.Board[newPosition].IsOccupied || Field.Board[newPosition].Piece.Color == Color)
            return false;

        var canEat = CanTakeInternal(newPosition);
        if (!canEat)
            return false;

        var ifMove = Field.Board.IfMove(new Move(Position, newPosition));
        var allyKing = Color == PieceColor.White ? ifMove.WhiteKing : ifMove.BlackKing;
        return !allyKing.IsInCheck();
    }

    public bool IsAttacking(Position position)
    {
        if (!GetEmptyBoardAttacks().Contains(position))
            return false;

        return IsAttackingInternal(position);
    }

    public static Piece Clone<T>(T oldPiece, Field newField) where T : Piece
    {
        return oldPiece switch
        {
            King => new King(newField, oldPiece.Color),
            Queen => new Queen(newField, oldPiece.Color),
            Bishop => new Bishop(newField, oldPiece.Color),
            Rook => new Rook(newField, oldPiece.Color),
            Knight => new Knight(newField, oldPiece.Color),
            Pawn => new Pawn(newField, oldPiece.Color),
            _ => throw new InvalidOperationException()
        };
    }

    public static PieceColor InvertColor(PieceColor color) => color == PieceColor.White ? PieceColor.Black : PieceColor.White;

    public abstract HashSet<Position> GetEmptyBoardMoves();
    public virtual HashSet<Position> GetEmptyBoardAttacks() => GetEmptyBoardMoves();
    protected abstract bool CanMoveInternal(Position positionToMoveTo);
    protected abstract bool IsAttackingInternal(Position attackedPosition);
    protected virtual bool CanTakeInternal(Position positionToEatAt) => CanMoveInternal(positionToEatAt);
}