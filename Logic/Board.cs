using System.Net.NetworkInformation;
using System.Reflection;

namespace Logic;

public class Board
{
    private readonly Dictionary<Field, Piece> _pieces = new();
    private readonly Dictionary<Position, Field> _fields = new();

    public Field this[Position pos] => _fields[pos];

    public IReadOnlyDictionary<Field, Piece> Pieces => _pieces;

    public King WhiteKing { get; private set; }
    public King BlackKing { get; private set; }

    public Board()
    {
        for (int row = 1; row <= 8; row++)
        {
            for (int col = 1; col <= 8; col++)
            {
                _fields[new Position(row, col)] = new Field(this, new Position(row, col));
            }
        }
    }

    private Board(Board board) : this()
    {
        foreach (var (field, piece) in board.Pieces)
        {
            var newPiece = Piece.Clone(piece, _fields[field.Position]);
            AddPiece(newPiece);
        }
    }

    public void AddPiece(Piece piece)
    {
        _pieces.Add(piece.Field, piece);

        if (piece is King {Color: PieceColor.White} whiteKing)
        {
            if (WhiteKing is not null)
                throw new InvalidOperationException();

            WhiteKing = whiteKing;
        }

        if (piece is King {Color: PieceColor.Black} blackKing)
        {
            if (BlackKing is not null)
                throw new InvalidOperationException();

            BlackKing = blackKing;
        }
    }

    public Board IfMove(Move move)
    {
        if (!IsValidMove(move))
            throw new InvalidOperationException();

        var copy = new Board(this);
        copy.MovePiece(move);
        return copy;
    }

    public Board IfAdd(Piece newPiece)
    {
        var copy = new Board(this);
        var pieceCopy = Piece.Clone(newPiece, copy[newPiece.Position]);
        copy.AddPiece(pieceCopy);
        return copy;
    }

    public Board IfRemove(Piece oldPiece)
    {
        if (_fields[oldPiece.Position].Piece != oldPiece)
            throw new InvalidOperationException();

        var copy = new Board(this);
        copy._pieces.Remove(copy[oldPiece.Position]);
        return copy;
    }

    public IEnumerable<Field> GetMoves(Piece piece)
    {
        return piece.GetEmptyBoardMoves().Where(piece.CanMove).Select(p => _fields[p]);
    }

    public IEnumerable<Field> GetAttacked(Piece piece)
    {
        return piece.GetEmptyBoardMoves().Where(piece.CanAttack).Select(p => _fields[p]);
    }

    public void MovePiece(Move move)
    {
        if (!IsValidMove(move))
            throw new InvalidOperationException();

        _pieces.Remove(_fields[move.From], out var piece);

        var newField = _fields[move.To];
        var eatenPiece = newField.Piece;
        if (eatenPiece is not null)
            _pieces.Remove(newField);

        _pieces.Add(newField, piece);
        piece.Field = newField;
    }

    private bool IsValidMove(Move move)
    {
        return move.From.IsValid && move.To.IsValid;
    }
}