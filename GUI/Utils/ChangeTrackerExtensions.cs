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
        bool isSelectedPiece = false, bool isPromotionFog = false)
    {
        var layers = new List<IChangeLayer>();
        layers.Add(new FieldLayer(field.Position, field.Color));

        if (field.IsOccupied)
            layers.Add(new PieceLayer(field.Piece));

        if (isHighlighted)
        {
            HighlightType highlightType;
            if (isPromotionFog)
                highlightType = HighlightType.PromotionFog;
            else if (isSelectedPiece)
                highlightType = HighlightType.Selected;
            else if (field.IsOccupied)
                highlightType = HighlightType.Attack;
            else
                highlightType = HighlightType.Move;

            layers.Add(new HighlightLayer(field.Position, highlightType));
        }

        if (field.Position.Row == 1)
            layers.Add(new LabelLayer(field.Position, LabelType.Columns));

        if (field.Position.Column == 1)
            layers.Add(new LabelLayer(field.Position, LabelType.Rows));

        var change = new Change(layers);
        changeTracker.Register(change);
    }

    public static void RegisterPromotion(this ChangeTracker changeTracker, Position position, PromotionPiece promotionPiece, PieceColor pieceColor)
    {
        var layers = new List<IChangeLayer>
        {
            new PromotionLayer(position),
            new PieceLayer(promotionPiece, pieceColor, position)
        };

        changeTracker.Register(new Change(layers));
    }
}