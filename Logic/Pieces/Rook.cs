namespace Logic.Pieces;

public class Rook : Piece, ICastlePiece
{
    public bool DidMove { get; set; }

    internal Rook(Field field, PieceColor color) : base(field, color)
    {
    }

    public override HashSet<Position> GetEmptyBoardMoves()
    {
        HashSet<Position> validPositions = new();

        var offsets = new[]
        {
            new Offset(0, -1),
            new Offset(0, 1),
            new Offset(-1, 0),
            new Offset(1, 0),
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
        Offset offset;
        if (Position.Row == attackedPosition.Row)
        {
            var columnOffset = Position.Column < attackedPosition.Column ? 1 : -1;
            offset = new Offset(0, columnOffset);
        }
        else
        {
            var rowOffset = Position.Row < attackedPosition.Row ? 1 : -1;
            offset = new Offset(rowOffset, 0);
        }

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