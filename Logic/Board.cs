namespace Logic;

public class Board
{
    private readonly Dictionary<Field, Piece> _pieces;
    private readonly Dictionary<Position, Field> _fields;

    public Field this[Position pos] => _fields[pos];

    public IReadOnlyDictionary<Field, Piece> Pieces => _pieces;

    public Board()
    {
        _pieces = new Dictionary<Field, Piece>();
        _fields = new Dictionary<Position, Field>();
        for (int row = 1; row <= 8; row++)
        {
            for (int col = 1; col <= 8; col++)
            {
                _fields[new Position(row, col)] = new Field(this, new Position(row, col));
            }
        }
    }

    public void AddPiece(Piece piece)
    {
        _pieces.Add(piece.Field, piece);
    }

    public IEnumerable<Field> GetMoves(Piece piece)
    {
        return piece.GetValidPositions().Where(piece.CanMove).Select(p => _fields[p]);
    }

    public IEnumerable<Field> GetAttacked(Piece piece)
    {
        return piece.GetValidPositions().Where(piece.CanAttack).Select(p => _fields[p]);
    }

    public void MovePiece(Position from, Position to)
    {
        var piece = _fields[from].Piece;

        if (!piece.CanMove(to))
            throw new InvalidOperationException();

        _pieces.Remove(_fields[from]);

        var newField = _fields[to];
        _pieces.Add(newField, piece);
        piece.Field = newField;
    }
}