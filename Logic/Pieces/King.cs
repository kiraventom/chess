namespace Logic.Pieces;

public class King : Piece
{
    // Флаг нужен для того, чтобы при проверке длинной рокировки не перепроверять поле D два раза -- при проверке поля D и при проверке поля C
    private bool _checkingForCastle;

    public bool DidMove { get; set; }

    internal King(Field field, PieceColor color) : base(field, color)
    {
    }

    public bool IsInCheck()
    {
        var enemyPieces = Field.Board.Pieces.Values.Where(p => p.Color != Color);
        return enemyPieces.Any(ep => ep.IsAttacking(Position));
    }

    public override HashSet<Position> GetEmptyBoardMoves()
    {
        HashSet<Position> validMoves = new(8);
        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                var offset = new Offset(rowOffset, columnOffset);
                if (offset.IsZero)
                    continue;

                var validPosition = Position + offset;
                if (validPosition.IsValid)
                    validMoves.Add(validPosition);
            }
        }

        if (!DidMove)
        {
            var rightRook = Field.Board[$"H{Position.Row}"].Piece;
            if (rightRook is Rook {DidMove: false})
                validMoves.Add($"G{Position.Row}");

            var leftRook = Field.Board[$"A{Position.Row}"].Piece;
            if (leftRook is Rook {DidMove: false})
                validMoves.Add($"C{Position.Row}");
        }

        return validMoves;
    }

    protected override bool CanMoveInternal(Position positionToMoveTo)
    {
        if (!_checkingForCastle)
        {
            _checkingForCastle = true;

            var columnOffset = positionToMoveTo.Column - Position.Column;
            if (Math.Abs(columnOffset) > 1)
            {
                var step = columnOffset > 0 ? 1 : -1;
                var positionToStepThrough = Position + new Offset(0, step);
                while (positionToStepThrough != positionToMoveTo)
                {
                    if (!CanMove(positionToStepThrough))
                        return false;

                    positionToStepThrough += new Offset(0, step);
                }
            }

            _checkingForCastle = false;
        }

        var enemyPieces = Field.Board.Pieces.Values.Where(p => p.Color != Color);
        var attackedFields = enemyPieces.SelectMany(ep => Field.Board.GetAttacked(ep));
        return attackedFields.All(f => f.Position != positionToMoveTo);
    }

    protected override bool IsAttackingInternal(Position attackedPosition) => true;
}