using Logic.Pieces;

namespace Logic;

public enum GameState
{
    WaitingForMove,
    WhiteWin,
    BlackWin,
    Stalemate
}

public enum CastleDirection
{
    Short,
    Long
}

public enum PromotionPiece
{
    Queen,
    Rook,
    Bishop,
    Knight,
}

public class Game
{
    public Board Board { get; }

    public PieceColor CurrentTurn { get; private set; }
    public GameState GameState { get; private set; }

    public GameLog.GameLog GameLog { get; }

    public Game(PromotionCallback promotionCallback)
    {
        Board = new Board(promotionCallback);
        AddDefaultPieces(Board);

        CurrentTurn = PieceColor.White;
        GameState = GameState.WaitingForMove;
        GameLog = new GameLog.GameLog();
    }

    public async Task MakeMove(Move move)
    {
        if (GameState != GameState.WaitingForMove)
            throw new InvalidOperationException();

        List<Position> extraPositions = new();

        var takenPiece = Board[move.To].Piece;
        await Board.MovePiece(move);

        CurrentTurn = Piece.InvertColor(CurrentTurn);
        GameState = GetState(Board, CurrentTurn);

        var movedPiece = Board[move.To].Piece;
        var isCastle = movedPiece is King && move.AbsHorizontalChange > 1;
        if (isCastle)
        {
            var direction = GetCastleDirection(move);
            var rookMove = GetRookCastleMove(direction, move.From.Row);
            extraPositions.Add(rookMove.From);
            extraPositions.Add(rookMove.To);
        }

        // TODO en passant
        // if (enPassant)
        // {
        //     extraPositions.Add(takenPawnPosition);
        //     takenPiece = takenPawn;
        // }

        var isTake = takenPiece is not null;

        var king = CurrentTurn == PieceColor.White ? Board.WhiteKing : Board.BlackKing;
        var isCheck = king.IsInCheck();

        var isMate = GameState is GameState.WhiteWin or GameState.BlackWin;

        PromotionPiece? promotionPiece = null;
        var isPromotion = movedPiece != Board[move.To].Piece;
        if (isPromotion)
        {
            promotionPiece = Board[move.To].Piece switch
            {
                Queen => PromotionPiece.Queen,
                Rook => PromotionPiece.Rook,
                Bishop => PromotionPiece.Bishop,
                Knight => PromotionPiece.Knight
            };
        }

        GameLog.AddMove(movedPiece, move, isTake, isCheck, isMate, isCastle, extraPositions, promotionPiece);
    }

    internal static CastleDirection GetCastleDirection(Move kingMove)
    {
        return kingMove.HorizontalChange > 0 ? CastleDirection.Short : CastleDirection.Long;
    }

    internal static Move GetRookCastleMove(CastleDirection direction, int row)
    {
        return direction switch
        {
            CastleDirection.Short => new Move($"H{row}", $"F{row}"),
            CastleDirection.Long => new Move($"A{row}", $"D{row}"),
            _ => throw new InvalidOperationException()
        };
    }

    internal static Task<PromotionPiece> DefaultPromotionCallback(Pawn pawn) => Task.FromResult(PromotionPiece.Queen);

    internal static GameState GetState(Board board, PieceColor currentTurn)
    {
        var allyPieces = board.Pieces.Values.Where(p => p.Color == currentTurn);
        bool anyPieceHasAnyMove = allyPieces.Any(p => Board.GetMoves(p).Any());
        if (anyPieceHasAnyMove)
            return GameState.WaitingForMove;

        var king = currentTurn == PieceColor.White ? board.WhiteKing : board.BlackKing;
        if (!king.IsInCheck())
            return GameState.Stalemate;

        return currentTurn == PieceColor.White ? GameState.BlackWin : GameState.WhiteWin;
    }

    private static void AddDefaultPieces(Board board)
    {
        board.AddPiece(new King(board["E1"], PieceColor.White));
        board.AddPiece(new Queen(board["D1"], PieceColor.White));
        board.AddPiece(new Bishop(board["C1"], PieceColor.White));
        board.AddPiece(new Bishop(board["F1"], PieceColor.White));
        board.AddPiece(new Knight(board["B1"], PieceColor.White));
        board.AddPiece(new Knight(board["G1"], PieceColor.White));
        board.AddPiece(new Rook(board["A1"], PieceColor.White));
        board.AddPiece(new Rook(board["H1"], PieceColor.White));
        for (int col = 1; col <= 8; col++)
            board.AddPiece(new Pawn(board[new Position(2, col)], PieceColor.White));

        board.AddPiece(new King(board["E8"], PieceColor.Black));
        board.AddPiece(new Queen(board["D8"], PieceColor.Black));
        board.AddPiece(new Bishop(board["C8"], PieceColor.Black));
        board.AddPiece(new Bishop(board["F8"], PieceColor.Black));
        board.AddPiece(new Knight(board["B8"], PieceColor.Black));
        board.AddPiece(new Knight(board["G8"], PieceColor.Black));
        board.AddPiece(new Rook(board["A8"], PieceColor.Black));
        board.AddPiece(new Rook(board["H8"], PieceColor.Black));
        for (int col = 1; col <= 8; col++)
            board.AddPiece(new Pawn(board[new Position(7, col)], PieceColor.Black));
    }
}