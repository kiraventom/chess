namespace Logic.Pieces;
public class King : Piece, ICastlePiece
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

            var move = new Move(Position, positionToMoveTo);
            if (move.AbsHorizontalChange > 1)
            {
                var castleDirection = Game.GetCastleDirection(move);
                var rookMove = Game.GetRookCastleMove(castleDirection, move.From.Row);
                var rook = Field.Board[rookMove.From].Piece;
                if (!rook.CanMove(rookMove.To))
                {
                    _checkingForCastle = false;
                    return false;
                }

                var step = castleDirection == CastleDirection.Short ? 1 : -1;
                var offset = new Offset(0, step);
                var positionToStepThrough = Position + offset;

                while (positionToStepThrough != positionToMoveTo)
                {
                    if (!CanMove(positionToStepThrough))
                    {
                        _checkingForCastle = false;
                        return false;
                    }

                    positionToStepThrough += offset;
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