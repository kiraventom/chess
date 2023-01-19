using Logic.Pieces;
// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Logic;

public delegate Task<PromotionPiece> PromotionCallback(Pawn pawn);

public class Board
{
    private readonly PromotionCallback _promotionCallback;
    private readonly Dictionary<Field, Piece> _pieces = new();
    private readonly Dictionary<Position, Field> _fields = new();

    public Field this[Position pos] => _fields[pos];

    public IReadOnlyDictionary<Field, Piece> Pieces => _pieces;

    public King WhiteKing { get; private set; }
    public King BlackKing { get; private set; }

    internal Board(PromotionCallback promotionCallback)
    {
        _promotionCallback = promotionCallback;

        for (var row = 1; row <= 8; row++)
        for (var col = 1; col <= 8; col++)
            _fields[new Position(row, col)] = new Field(this, new Position(row, col));
    }

    private Board(Board board) : this(Game.DefaultPromotionCallback)
    {
        foreach (var (field, piece) in board.Pieces)
        {
            var newPiece = Piece.Clone(piece, _fields[field.Position]);
            AddPiece(newPiece);
        }
    }

    public static HashSet<Position> GetMoves(Piece piece)
    {
        var moves = piece.GetEmptyBoardMoves().Where(piece.CanMove);
        var attacks = piece.GetEmptyBoardAttacks().Where(piece.CanTake);
        return new HashSet<Position>(moves.Concat(attacks));
    }

    internal IEnumerable<Field> GetAttacked(Piece piece)
    {
        return piece.GetEmptyBoardAttacks().Where(piece.IsAttacking).Select(p => _fields[p]);
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

    internal Board IfMakeMove(Move move)
    {
        var copy = new Board(this);
        copy.MovePiece(move).ConfigureAwait(false).GetAwaiter().GetResult();
        return copy;
    }

    internal async Task MovePiece(Move move)
    {
        if (!IsValidMove(move))
            throw new InvalidOperationException();

        var piece = _pieces[_fields[move.From]];
        _pieces.Remove(_fields[move.From]);

        var newField = _fields[move.To];
        var takenPiece = newField.Piece;
        if (takenPiece is not null)
            _pieces.Remove(newField);

        _pieces.Add(newField, piece);
        piece.Field = newField;

        if (piece is Pawn pawn && move.To.Row is 1 or 8)
            await Promote(pawn);

        if (piece is ICastlePiece castlePiece)
            castlePiece.DidMove = true;

        if (piece is King king && move.AbsHorizontalChange > 1)
            Castle(king.Position.Row, Game.GetCastleDirection(move));
    }

    private void Castle(int row, CastleDirection direction)
    {
        var rookMove = Game.GetRookCastleMove(direction, row);
        MovePiece(rookMove).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task Promote(Pawn pawn)
    {
        var field = _fields[pawn.Position];
        _pieces.Remove(field);

        var promotionPiece = await _promotionCallback(pawn);
        Piece piece = promotionPiece switch
        {
            PromotionPiece.Knight => new Knight(field, pawn.Color),
            PromotionPiece.Bishop => new Bishop(field, pawn.Color),
            PromotionPiece.Rook => new Rook(field, pawn.Color) {DidMove = true},
            PromotionPiece.Queen => new Queen(field, pawn.Color),
            _ => throw new InvalidOperationException()
        };

        _pieces.Add(field, piece);
    }

    private static bool IsValidMove(Move move) => move.From.IsValid && move.To.IsValid;
}