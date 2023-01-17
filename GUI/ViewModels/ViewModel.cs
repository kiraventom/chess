using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using Logic;
using Logic.Pieces;
using GUI.Utils;

namespace GUI.ViewModels;

public class ViewModel : BaseNotifier
{
    private readonly ChangeTracker _changeTracker;

    private PawnPromoter _pawnPromoter;

    private Game _game;
    private Piece _selectedPiece;

    public ICommand RestartCommand { get; }

    public GameLogReader.GameLogReader GameLogReader { get; }

    public string CurrentTurn => _game.CurrentTurn.ToString();
    public string GameState => _game.GameState.ToString();

    public ViewModel(ChangeTracker changeTracker)
    {
        GameLogReader = new GameLogReader.GameLogReader();
        _changeTracker = changeTracker;

        RestartCommand = new Command(Init);
    }

    public void Init()
    {
        _game = new Game(GetPromotionPiece);
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
        if (!position.IsValid)
            return;

        if (_pawnPromoter.IsPromoting)
        {
            _pawnPromoter.HandleClick(position);
            return;
        }

        if (_selectedPiece is null)
        {
            bool didSelect = TrySelectPiece(position);
            if (didSelect)
                _changeTracker.RegisterPiece(_selectedPiece, _game.Board);

            return;
        }

        var selectedPiece = _selectedPiece;
        _selectedPiece = null;

        // deselect
        _changeTracker.RegisterPiece(selectedPiece, _game.Board, false);

        if (!Board.GetMoves(selectedPiece).Contains(position))
            return;

        var move = new Move(selectedPiece.Position, position);
        await _game.MakeMove(move);

        RaisePropertyChanged(nameof(CurrentTurn));
        RaisePropertyChanged(nameof(GameState));

        _changeTracker.RegisterField(_game.Board[move.From]);
        _changeTracker.RegisterField(_game.Board[move.To]);

        var lastTurn = _game.GameLog.Turns[^1];
        if (lastTurn.LastMove.IsCastle)
            foreach (var extraPosition in lastTurn.LastMove.Extra)
                _changeTracker.RegisterField(_game.Board[extraPosition]);

        GameLogReader.AddOrUpdateTurn(lastTurn);
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

        if (piece.Color == _game.CurrentTurn)
            _selectedPiece = piece;

        return _selectedPiece is not null;
    }
}