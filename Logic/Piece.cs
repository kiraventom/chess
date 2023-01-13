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

    public Piece(Field field, PieceColor color)
    {
        Field = field;
        Color = color;
    }

    public bool CanMove(Position position)
    {
        if (position == Position)
            return false;

        if (Field.Board[position].IsOccupied && Field.Board[position].Piece.Color == Color)
            return false;

        // pinned, check

        return CanMoveInternal(position);
    }

    public bool CanAttack(Position position)
    {
        return CanAttackInternal(position);
    }

    public abstract IEnumerable<Position> GetValidPositions();
    protected abstract bool CanMoveInternal(Position positionMoveTo);
    protected abstract bool CanAttackInternal(Position attackedPosition);
}

public class King : Piece
{
    public King(Field field, PieceColor color) : base(field, color)
    {
    }

    public override IEnumerable<Position> GetValidPositions()
    {
        List<Position> positions = new(8);
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

    protected override bool CanMoveInternal(Position positionMoveTo)
    {
        var otherPieces = Field.Board.Pieces.Values.Where(p => p != this);
        var enemyPieces = otherPieces.Where(p => p.Color != Color);
        var allEnemiesAttacks = enemyPieces.SelectMany(ep => Field.Board.GetAttacked(ep));
        return allEnemiesAttacks.All(f => f.Position != positionMoveTo);
    }

    protected override bool CanAttackInternal(Position attackedPosition)
    {
        return true;
    }
}

public class Bishop : Piece
{
    public Bishop(Field field, PieceColor color) : base(field, color)
    {
    }

    public override IEnumerable<Position> GetValidPositions()
    {
        List<Position> validPositions = new();

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

    protected override bool CanMoveInternal(Position positionMoveTo)
    {
        return CanAttackInternal(positionMoveTo);
    }

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