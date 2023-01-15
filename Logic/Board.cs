using Logic.Pieces;

namespace Logic;

public class Board
{
    private readonly Dictionary<Field, Piece> _pieces = new();
    private readonly Dictionary<Position, Field> _fields = new();

    public Field this[Position pos] => _fields[pos];

    public IReadOnlyDictionary<Field, Piece> Pieces => _pieces;

    public King WhiteKing { get; private set; }
    public King BlackKing { get; private set; }

    internal Board()
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

    internal void AddPiece(Piece piece)
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

    internal Board IfMove(Move move)
    {
        var copy = new Board(this);
        copy.MovePiece(move);
        return copy;
    }

    internal Board IfAdd(Piece newPiece)
    {
        var copy = new Board(this);
        var pieceCopy = Piece.Clone(newPiece, copy[newPiece.Position]);
        copy.AddPiece(pieceCopy);
        return copy;
    }

    internal Board IfRemove(Piece oldPiece)
    {
        if (_fields[oldPiece.Position].Piece != oldPiece)
            throw new InvalidOperationException();

        var copy = new Board(this);
        copy._pieces.Remove(copy[oldPiece.Position]);
        return copy;
    }

    public static HashSet<Position> GetMoves(Piece piece)
    {
        var moves = piece.GetEmptyBoardMoves().Where(piece.CanMove);
        var attacks = piece.GetEmptyBoardAttacks().Where(piece.CanTake);
        return new HashSet<Position>(moves.Concat(attacks));
    }

    public IEnumerable<Field> GetAttacked(Piece piece)
    {
        return piece.GetEmptyBoardAttacks().Where(piece.IsAttacking).Select(p => _fields[p]);
    }

    internal void MovePiece(Move move)
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

        // TODO Pawn promotion

        if (piece is King king)
        {
            king.DidMove = true;

            // castle
            var colOffset = move.To.Column - move.From.Column;
            if (Math.Abs(colOffset) > 1)
            {
                if (colOffset < 0)
                    MovePiece(new Move($"A{king.Position.Row}", $"D{king.Position.Row}"));
                else
                    MovePiece(new Move($"H{king.Position.Row}", $"F{king.Position.Row}"));
            }
        }
        else if (piece is Rook rook)
            rook.DidMove = true;
    }

    private static bool IsValidMove(Move move)
    {
        return move.From.IsValid && move.To.IsValid;
    }
}