using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Logic;
using Logic.Pieces;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace GUI;

// TODO Get info on "what to update" from game

public partial class MainWindow
{
    private const float FPS = 15;

    private float BoardSide =>
        (float) (MainView.ActualWidth < MainView.ActualHeight ? MainView.ActualWidth : MainView.ActualHeight);

    private float FieldSide => BoardSide / 8;

    private readonly SKPaint _whiteFieldPaint = new() {Color = SKColors.LightGray};
    private readonly SKPaint _blackFieldPaint = new() {Color = SKColors.Gray};
    private readonly SKPaint _movesPaint = new() {Color = new SKColor(0, 180, 10, 128)};
    private readonly SKPaint _attackedPaint = new() {Color = new SKColor(156, 62, 62, 128)};
    private readonly SKPaint _selectedPaint = new() {Color = new SKColor(156, 156, 62, 128)};

    private readonly Game _game;
    private Board Board => _game.Board;

    private readonly ChangeTracker _changeTracker = new();

    private Position _hoveredPosition;

    public MainWindow()
    {
        InitializeComponent();
        MainView.PaintSurface += OnPaint;
        MainView.MouseMove += OnMouseMove;
        MainView.MouseDown += OnMouseDown;
        MainView.SizeChanged += (_, _) => Invalidate();

        var timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher.CurrentDispatcher)
        {
            Interval = TimeSpan.FromSeconds(1.0 / FPS),
            IsEnabled = false
        };

        timer.Tick += OnTick;

        _game = new Game();
        DataContext = _game;

        Invalidate();

        timer.Start();
    }

    private void Invalidate()
    {
        for (var row = 1; row <= 8; row++)
        for (var col = 1; col <= 8; col++)
            _changeTracker.Register(new Position(row, col));
    }

    private void OnTick(object sender, EventArgs e)
    {
        MainView.InvalidateVisual();
    }

    // TODO Refactor
    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_hoveredPosition.IsValid)
            return;

        HashSet<Position> moves;
        if (_game.SelectedPiece is not null && (moves = Board.GetMoves(_game.SelectedPiece)).Contains(_hoveredPosition))
        {
            _changeTracker.Register(_game.SelectedPiece.Position);
            foreach (var pos in moves)
                _changeTracker.Register(pos);

            var move = new Move(_game.SelectedPiece.Position, _hoveredPosition);
            _game.MakeMove(move);

            // TODO Temp
            if (_game.SelectedPiece is King king && Math.Abs(move.To.Column - move.From.Column) > 1)
            {
                for (int col = 1; col <= 8; col++)
                {
                    _changeTracker.Register(new Position(king.Position.Row, col));
                }
            }
        }
        else
        {
            if (_game.SelectedPiece is not null)
            {
                _changeTracker.Register(_game.SelectedPiece.Position);
                foreach (var pos in Board.GetMoves(_game.SelectedPiece))
                    _changeTracker.Register(pos);
            }

            bool didSelect = _game.TrySelectPiece(_hoveredPosition);
            if (didSelect)
            {
                _changeTracker.Register(_hoveredPosition);
                foreach (var move in Board.GetMoves(_game.SelectedPiece))
                    _changeTracker.Register(move);
            }
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var point = e.GetPosition(MainView);
        var col = (int) Math.Floor(point.X / FieldSide) + 1;
        var row = 8 - (int) Math.Floor(point.Y / FieldSide);

        _hoveredPosition = new Position(row, col);
    }

    private void OnPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        var changes = _changeTracker.GetAll().ToList();
        if (changes.Any())
        {
            HashSet<Position> movesPositions = new();
            if (_game.SelectedPiece is not null)
            {
                var moves = Board.GetMoves(_game.SelectedPiece);
                foreach (var move in moves)
                    movesPositions.Add(move);
            }

            foreach (var change in changes)
            {
                var field = Board[change];
                DrawField(canvas, field);

                // TODO Fix
                if (movesPositions.Contains(change) || _game.SelectedPiece is not null && change == _game.SelectedPiece.Position)
                {
                    SKPaint paint;
                    if (!field.IsOccupied)
                        paint = _movesPaint;
                    else if (field.Piece == _game.SelectedPiece)
                        paint = _selectedPaint;
                    else
                        paint = _attackedPaint;

                    DrawHighlight(canvas, field, paint);
                }

                if (field.IsOccupied)
                    DrawPiece(canvas, field.Piece);

                DrawLabel(canvas, field);
            }
        }
    }

    private void DrawField(SKCanvas canvas, Field field)
    {
        var fieldPaint = field.Color == FieldColor.White ? _whiteFieldPaint : _blackFieldPaint;

        var rect = new SKRect
        {
            Left = FieldSide * (field.Position.Column - 1),
            Top = FieldSide * (8 - field.Position.Row + 1),
            Right = FieldSide * field.Position.Column,
            Bottom = FieldSide * (8 - field.Position.Row),
        };

        canvas.DrawRect(rect, fieldPaint);
    }

    private void DrawLabel(SKCanvas canvas, Field field)
    {
        var fontSize = MeasureFontSize(FieldSide, FieldSide / 5, "A");

        if (field.Position.Column == 1)
        {
            var richString = new RichString();
            richString.FontSize(fontSize).Add(field.Position.Row.ToString());
            var x = 0;
            var y = (8 - field.Position.Row) * FieldSide;
            richString.Paint(canvas, new SKPoint(x, y));
        }

        if (field.Position.Row == 1)
        {
            var richString = new RichString();
            richString.FontSize(fontSize).Add(field.Position.ToString()[0].ToString());
            var x = field.Position.Column * FieldSide - richString.MeasuredWidth;
            var y = 8 * FieldSide - richString.MeasuredHeight;
            richString.Paint(canvas, new SKPoint(x, y));
        }
    }

    private void DrawPiece(SKCanvas canvas, Piece piece)
    {
        var str = GetPieceStr(piece);
        var color = piece.Color == PieceColor.White ? SKColors.White : SKColors.Black;

        RichString richString = new()
        {
            MaxHeight = FieldSide,
            MaxWidth = FieldSide,
            DefaultAlignment = TextAlignment.Center
        };

        var fontSize = MeasureFontSize(FieldSide, FieldSide, str);

        richString.FontSize(fontSize).FontFamily("FreeSerif");

        richString.TextColor(color);
        if (piece.Color == PieceColor.White)
            richString.HaloWidth((int) (FieldSide / 20)).HaloColor(SKColors.Black);

        richString.Add(str);

        var point = new SKPoint((piece.Position.Column - 1) * FieldSide, (8 - piece.Position.Row) * FieldSide);
        richString.Paint(canvas, point);
    }

    private void DrawHighlight(SKCanvas canvas, Field field, SKPaint paint)
    {
        var x = (field.Position.Column - 1) * FieldSide;
        var y = (8 - field.Position.Row) * FieldSide;
        canvas.DrawRect(x, y, FieldSide, FieldSide, paint);
    }

    private static string GetPieceStr(Piece piece)
    {
        return piece switch
        {
            King => "♚",
            Queen => "♛",
            Rook => "♜",
            Bishop => "♝",
            Knight => "♞",
            Pawn => "♟",
        };
    }

    private static int MeasureFontSize(float width, float height, string text)
    {
        RichString richString;
        var fontSize = 0;
        do
        {
            ++fontSize;
            richString = new RichString
            {
                MaxHeight = height,
                MaxWidth = width,
                DefaultAlignment = TextAlignment.Center
            };

            richString.FontSize(fontSize).Add(text);
        } while (!richString.Truncated);

        return fontSize - 1;
    }
}