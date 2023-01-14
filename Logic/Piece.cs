namespace Logic;

public enum PieceColor
{
    White,
    Black
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
        if (newPosition == Position)
            return false;

        if (!GetEmptyBoardMoves().Contains(newPosition))
            return false;

        if (Field.Board[newPosition].IsOccupied && Field.Board[newPosition].Piece.Color == Color)
            return false;

        var canMove = CanMoveInternal(newPosition);
        if (!canMove)
            return false;

        var ifMove = Field.Board.IfMove(new Move(Position, newPosition));
        var allyKing = Color == PieceColor.White ? ifMove.WhiteKing : ifMove.BlackKing;
        return !allyKing.IsInCheck();

    }

    public bool CanAttack(Position position)
    {
        if (!GetEmptyBoardAttacks().Contains(position))
            return false;

        return CanAttackInternal(position);
    }

    public static Piece Clone<T>(T oldPiece, Field newField) where T : Piece
    {
        return oldPiece switch
        {
            King => new King(newField, oldPiece.Color),
            Bishop => new Bishop(newField, oldPiece.Color),
            _ => throw new NotImplementedException()
        };
    }

    public abstract HashSet<Position> GetEmptyBoardMoves();
    public virtual HashSet<Position> GetEmptyBoardAttacks() => GetEmptyBoardMoves();
    protected abstract bool CanMoveInternal(Position positionToMoveTo);
    protected abstract bool CanAttackInternal(Position attackedPosition);
}

public class King : Piece
{
    public King(Field field, PieceColor color) : base(field, color)
    {
    }

    public override HashSet<Position> GetEmptyBoardMoves()
    {
        HashSet<Position> positions = new(8);
        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                var offset = new Offset(rowOffset, columnOffset);
                if (offset.IsZero)
                    continue;

                var validPosition = Position + offset;
                if (validPosition.IsValid)
                    positions.Add(validPosition);
            }
        }

        return positions;
    }

    protected override bool CanMoveInternal(Position positionToMoveTo)
    {
        var enemyPieces = Field.Board.Pieces.Values.Where(p => p.Color != Color);
        var attackedFields = enemyPieces.SelectMany(ep => Field.Board.GetAttacked(ep));
        return attackedFields.All(f => f.Position != positionToMoveTo);
    }

    protected override bool CanAttackInternal(Position attackedPosition) => true;

    public bool IsInCheck()
    {
        var enemyPieces = Field.Board.Pieces.Values.Where(p => p.Color != Color);
        return enemyPieces.Any(ep => ep.CanAttack(Position));
    }
}

public class Bishop : Piece
{
    public Bishop(Field field, PieceColor color) : base(field, color)
    {
    }

    public override HashSet<Position> GetEmptyBoardMoves()
    {
        HashSet<Position> validPositions = new();

        var offsets = new[]
        {
            new Offset(-1, -1),
            new Offset(-1, 1),
            new Offset(1, -1),
            new Offset(1, 1),
        };

        foreach (var offset in offsets)
        {
            var validPosition = Position;
            while (validPosition.IsValid)
            {
                if (validPosition != Position)
                    validPositions.Add(validPosition);

                validPosition += offset;
            }
        }

        return validPositions;
    }

    protected override bool CanMoveInternal(Position positionToMoveTo) => CanAttackInternal(positionToMoveTo);

    protected override bool CanAttackInternal(Position attackedPosition)
    {
        var rowOffset = Position.Row > attackedPosition.Row ? -1 : 1;
        var colOffset = Position.Column > attackedPosition.Column ? -1 : 1;
        var offset = new Offset(rowOffset, colOffset);

        var possibleMove = Position;

        while (true)
        {
            possibleMove += offset;

            var field = Field.Board[possibleMove];
            if (possibleMove == attackedPosition)
                return !field.IsOccupied || field.Piece.Color != Color;

            if (field.IsOccupied)
                return false;
        }
    }
}