namespace Logic.Pieces;

public class Pawn : Piece
{
    private int StartRow => Color == PieceColor.White ? 2 : 7;

    internal Pawn(Field field, PieceColor color) : base(field, color)
    {
    }

    public override HashSet<Position> GetEmptyBoardMoves()
    {
        HashSet<Position> validMoves = new();
        var rowOffset = Color == PieceColor.White ? 1 : -1;
        validMoves.Add(Position + new Offset(rowOffset, 0));

        if (Position.Row == StartRow)
            validMoves.Add(Position + new Offset(rowOffset * 2, 0));

        return validMoves;
    }

    public override HashSet<Position> GetEmptyBoardAttacks()
    {
        HashSet<Position> validAttacks = new();
        var rowOffset = Color == PieceColor.White ? 1 : -1;
        var leftAttack = Position + new Offset(rowOffset, -1);
        var rightAttack = Position + new Offset(rowOffset, 1);
        if (leftAttack.IsValid)
            validAttacks.Add(leftAttack);

        if (rightAttack.IsValid)
            validAttacks.Add(rightAttack);

        return validAttacks;
    }

    protected override bool CanTakeInternal(Position positionToEatAt) => true;
    protected override bool CanMoveInternal(Position positionToMoveTo) => true;
    protected override bool IsAttackingInternal(Position attackedPosition) => true;
}