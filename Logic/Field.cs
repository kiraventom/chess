namespace Logic;

public enum FieldColor
{
    White, Black
}

public class Field
{
    public Position Position { get; }
    public Board Board { get; }
    public FieldColor Color => (Position.Row + Position.Column) % 2 == 0 ? FieldColor.Black : FieldColor.White;
    public Piece Piece => Board.Pieces.ContainsKey(this) ? Board.Pieces[this] : null;
    public bool IsOccupied => Piece is not null;

    public Field(Board board, Position position)
    {
        Board = board;
        Position = position;
    }
}