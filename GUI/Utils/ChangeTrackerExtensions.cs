using System.Collections.Generic;
using Common;
using GUI.Layers;
using GUI.Layers.TextLayers;
using Logic;
using Logic.Pieces;

namespace GUI.Utils;

public static class ChangeTrackerExtensions
{
    public static void RegisterPiece(this ChangeTracker changeTracker, Piece piece, Board board,
        bool shouldHighlight = true)
    {
        var moves = Board.GetMoves(piece);

        changeTracker.RegisterField(piece.Field, shouldHighlight, true);
        foreach (var pos in moves)
            changeTracker.RegisterField(board[pos], shouldHighlight);
    }

    public static void RegisterField(this ChangeTracker changeTracker, Field field, bool isHighlighted = false,
        bool isSelectedPiece = false)
    {
        var layers = new List<IChangeLayer>();
        layers.Add(new FieldLayer(field.Position, field.Color));

        if (isHighlighted)
        {
            HighlightType highlightType;
            if (isSelectedPiece)
                highlightType = HighlightType.Selected;
            else if (field.IsOccupied)
                highlightType = HighlightType.Attack;
            else
                highlightType = HighlightType.Move;

            layers.Add(new HighlightLayer(field.Position, highlightType));
        }

        if (field.IsOccupied)
            layers.Add(new PieceLayer(field.Piece));

        if (field.Position.Row == 1)
            layers.Add(new LabelLayer(field.Position, LabelType.Columns));
        else if (field.Position.Column == 1)
            layers.Add(new LabelLayer(field.Position, LabelType.Rows));

        var change = new Change(layers);
        changeTracker.Register(change);
    }
}