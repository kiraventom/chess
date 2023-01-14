namespace Logic.Pieces;

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

    protected override bool CanMoveInternal(Position positionToMoveTo) => IsAttackingInternal(positionToMoveTo);

    protected override bool IsAttackingInternal(Position attackedPosition)
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