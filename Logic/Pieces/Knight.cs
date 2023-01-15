namespace Logic.Pieces;

public class Knight : Piece
{
    internal Knight(Field field, PieceColor color) : base(field, color)
    {
    }

    public override HashSet<Position> GetEmptyBoardMoves()
    {
        HashSet<Position> validPositions = new();

        var offsets = new[]
        {
            new Offset(-1, -2),
            new Offset(1, -2),
            new Offset(2, -1),
            new Offset(2, 1),
            new Offset(1, 2),
            new Offset(-1, 2),
            new Offset(-2, 1),
            new Offset(-2, -1),
        };

        foreach (var offset in offsets)
        {
            var position = Position + offset;
            if (position.IsValid)
                validPositions.Add(position);
        }

        return validPositions;
    }

    protected override bool CanMoveInternal(Position positionToMoveTo) => IsAttackingInternal(positionToMoveTo);

    protected override bool IsAttackingInternal(Position attackedPosition) => true;
}