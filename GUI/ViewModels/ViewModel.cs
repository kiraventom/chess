using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using Logic;
using Logic.Pieces;
using GUI.Utils;
using Logic.Pieces.Engine;
using Microsoft.VisualBasic;

namespace GUI.ViewModels;

public class ViewModel : BaseNotifier
{
    private readonly ChangeTracker _changeTracker;
    private const PieceColor PlayerColor = PieceColor.White;

    private PawnPromoter _pawnPromoter;

    private Game _game;
    private Piece _selectedPiece;
    private Engine _engine;
    private bool _isEngineBusy;

    public ICommand RestartCommand { get; }

    public GameLogReader.GameLogReader GameLogReader { get; }

    public string CurrentTurn => _game.CurrentTurn.ToString();
    public string GameState => _game.GameState.ToString();

    public int EngineDepth { get; set; } = 3;

    public bool IsEngineBusy
    {
        get => _isEngineBusy;
        private set => SetAndRaise(ref _isEngineBusy, value);
    }

    public ViewModel(ChangeTracker changeTracker)
    {
        GameLogReader = new GameLogReader.GameLogReader();
        _changeTracker = changeTracker;
        RestartCommand = new Command(Init);
    }

    public void Init()
    {
        _game = new Game(GetPromotionPiece);
        _engine = new Engine(_game);
        _pawnPromoter = new PawnPromoter(_changeTracker, _game.Board);
        GameLogReader.Clear();
        _selectedPiece = null;

        RaisePropertyChanged(nameof(CurrentTurn));
        RaisePropertyChanged(nameof(GameState));

        OnForceRedraw();
    }

    public void OnForceRedraw()
    {
        HashSet<Position> moves = null;

        for (var row = 1; row <= 8; row++)
        {
            for (var col = 1; col <= 8; col++)
            {
                var position = new Position(row, col);

                var isHighlighted = false;
                var isSelected = false;

                if (_selectedPiece is not null)
                {
                    moves ??= Board.GetMoves(_selectedPiece);

                    isHighlighted = moves.Contains(position);
                    isSelected = _selectedPiece.Position == position;
                }

                _changeTracker.RegisterField(_game.Board[position], isHighlighted, isSelected);
            }
        }
    }

    public async Task OnClick(Position position)
    {
        var isValidMove = TryGetMove(position, out var move);
        if (!isValidMove)
            return;

        await MakeMove(move);

        if (_game.GameState is Logic.GameState.WaitingForMove)
        {
            IsEngineBusy = true;

            var engineMove = await _engine.GetMove(EngineDepth);
            await MakeMove(engineMove);

            IsEngineBusy = false;
        }
    }

    private async Task MakeMove(Move move)
    {
        await _game.MakeMove(move);

        RaisePropertyChanged(nameof(CurrentTurn));
        RaisePropertyChanged(nameof(GameState));

        _changeTracker.RegisterField(_game.Board[move.From]);
        _changeTracker.RegisterField(_game.Board[move.To]);

        var lastTurn = _game.GameLog.Turns[^1];
        foreach (var extraPosition in lastTurn.LastMove.Extra)
            _changeTracker.RegisterField(_game.Board[extraPosition]);

        GameLogReader.AddOrUpdateTurn(lastTurn);
    }

    private bool TryGetMove(Position position, out Move move)
    {
        move = default;

        if (!position.IsValid)
            return false;

        if (_pawnPromoter.IsPromoting)
        {
            _pawnPromoter.HandleClick(position);
            return false;
        }

        if (_selectedPiece is null)
        {
            var didSelect = TrySelectPiece(position);
            if (didSelect)
                _changeTracker.RegisterPiece(_selectedPiece, _game.Board);

            return false;
        }

        var selectedPiece = _selectedPiece;
        _selectedPiece = null;

        // deselect
        _changeTracker.RegisterPiece(selectedPiece, _game.Board, false);

        if (!Board.GetMoves(selectedPiece).Contains(position))
            return false;

        move = new Move(selectedPiece.Position, position);
        return true;
    }

    private async Task<PromotionPiece> GetPromotionPiece(Pawn pawn)
    {
        _pawnPromoter.BeginPromoting(pawn);
        var piece = await _pawnPromoter.GetPiece();
        _pawnPromoter.EndPromoting();

        OnForceRedraw();
        return piece;
    }

    private bool TrySelectPiece(Position position)
    {
        var field = _game.Board[position];
        if (!field.IsOccupied)
            return false;

        var piece = field.Piece;

        if (piece.Color == PlayerColor)
            _selectedPiece = piece;

        return _selectedPiece is not null;
    }
}