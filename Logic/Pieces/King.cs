namespace Logic.Pieces;

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

    protected override bool IsAttackingInternal(Position attackedPosition) => true;

    public bool IsInCheck()
    {
        var enemyPieces = Field.Board.Pieces.Values.Where(p => p.Color != Color);
        return enemyPieces.Any(ep => ep.IsAttacking(Position));
    }
}