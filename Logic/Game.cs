using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Logic.Pieces;

namespace Logic;

public enum GameState
{
    WaitingForMove, WhiteWin, BlackWin, Stalemate
}

public readonly struct GameLogMove
{
    public Move Move { get; }
    public Piece Piece { get; }
    public bool IsTake { get; }
    public bool IsCastle { get; }
    public bool IsCheck { get; }
    public bool IsMate { get; }

    public GameLogMove(Move move, Piece piece, bool isTake, bool isCheck, bool isMate, bool isCastle)
    {
        Move = move;
        Piece = piece;
        IsTake = isTake;

        if (isMate && !isCheck)
            throw new InvalidOperationException();

        IsCheck = isCheck;
        IsMate = isMate;
        IsCastle = isCastle;
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        if (IsCastle)
        {
            sb.Append(Move.To.Column == 7 ? "0-0" : "0-0-0");
        }
        else
        {
            var pieceStr = Piece switch
            {
                King => "K",
                Queen => "Q",
                Bishop => "B",
                Knight => "N",
                Rook => "R",
                Pawn => string.Empty
            };

            sb.Append(pieceStr);

            if (IsTake)
                sb.Append(Piece is Pawn ? Move.From.ToString()[0] : 'x');

            sb.Append(Piece is Pawn && IsTake ? Move.To.ToString()[0] : Move.To.ToString());
        }

        if (IsMate)
            sb.Append('#');
        else if (IsCheck)
            sb.Append('+');

        return sb.ToString();
    }
}

public class GameLogTurn : BaseNotifier
{
    private GameLogMove _blackMove;
    public int Number { get; }
    public GameLogMove WhiteMove { get; internal set;}

    public GameLogMove BlackMove
    {
        get => _blackMove;
        internal set
        {
            _blackMove = value;
            RaisePropertyChanged(nameof(AsString));
        }
    }

    public GameLogTurn(int number)
    {
        Number = number;
    }

    public string AsString => ToString();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Number).Append('.').Append(' ');
        sb.Append(WhiteMove.ToString()).Append(' ');

        if (BlackMove.Piece is not null)
            sb.Append(BlackMove.ToString()).Append(' ');

        return sb.ToString();
    }
}

public class GameLog : BaseNotifier
{
    private readonly ObservableCollection<GameLogTurn> _log = new();

    public IReadOnlyList<GameLogTurn> Log => _log;

    internal GameLog()
    {

    }

    public void AddMove(Piece piece, Move move, bool isTake, bool isCheck, bool isMate, bool isCastle)
    {
        var logMove = new GameLogMove(move, piece, isTake, isCheck, isMate, isCastle);
        if (logMove.Piece.Color == PieceColor.White)
            _log.Add(new GameLogTurn(_log.Count + 1) {WhiteMove = logMove});
        else
            _log[^1].BlackMove = logMove;
    }
}

public class Game : BaseNotifier
{
    private PieceColor _currentTurn;
    private GameState _gameState;

    public ICommand RestartCommand { get; }

    public Board Board { get; private set; }
    public Piece SelectedPiece { get; private set; }

    public PieceColor CurrentTurn
    {
        get => _currentTurn;
        private set => SetAndRaise(ref _currentTurn, value);
    }

    public GameState GameState
    {
        get => _gameState;
        private set => SetAndRaise(ref _gameState, value);
    }

    public GameLog GameLog { get; private set; }

    public Game()
    {
        RestartCommand = new Command(Init);
        Init();
    }

    public bool TrySelectPiece(Position position)
    {
        var field = Board[position];
        if (!field.IsOccupied)
            return false;

        var piece = field.Piece;

        // unselect
        if (piece == SelectedPiece)
        {
            SelectedPiece = null;
            return false;
        }

        if (piece.Color == CurrentTurn)
            SelectedPiece = piece;

        return SelectedPiece is not null;
    }

    public void MakeMove(Move move)
    {
        if (SelectedPiece is null)
            throw new InvalidOperationException();

        if (GameState != GameState.WaitingForMove)
            throw new InvalidOperationException();

        var piecesBeforeMove = Board.Pieces.Count;
        Board.MovePiece(move);
        var piecesAfterMove = Board.Pieces.Count;

        SelectedPiece = null;
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        GameState = UpdateState();

        var movedPiece = Board[move.To].Piece;
        var king = CurrentTurn == PieceColor.White ? Board.WhiteKing : Board.BlackKing;
        GameLog.AddMove(
            movedPiece,
            move,
        piecesBeforeMove != piecesAfterMove,
            king.IsInCheck(),
            GameState is GameState.WhiteWin or GameState.BlackWin,
            movedPiece is King && Math.Abs(move.To.Column - move.From.Column) > 1);
    }

    private void Init()
    {
        Board = new Board();
        AddDefaultPieces(Board);

        CurrentTurn = PieceColor.White;
        GameState = GameState.WaitingForMove;
        GameLog = new GameLog();
    }

    private GameState UpdateState()
    {
        var allyPieces = Board.Pieces.Values.Where(p => p.Color == CurrentTurn);
        bool anyPieceHasAnyMove = allyPieces.Any(p => Board.GetMoves(p).Any());
        if (anyPieceHasAnyMove)
            return GameState.WaitingForMove;

        var king = CurrentTurn == PieceColor.White ? Board.WhiteKing : Board.BlackKing;
        if (!king.IsInCheck())
            return GameState.Stalemate;

        return CurrentTurn == PieceColor.White ? GameState.BlackWin : GameState.WhiteWin;
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

public class BaseNotifier : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetAndRaise<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value))
            return false;

        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }
}