using Logic;
using Logic.Pieces;
using SkiaSharp;
using Topten.RichTextKit;

namespace GUI.Layers.TextLayers;

public class PieceLayer : TextLayer
{
    private readonly string _pieceString;
    private readonly PieceColor _pieceColor;

    public PieceLayer(Piece piece) : base(piece.Position)
    {
        _pieceString = GetPieceStr(piece);
        _pieceColor = piece.Color;
    }

    public PieceLayer(PromotionPiece piece, PieceColor color, Position position) : base(position)
    {
        _pieceString = GetPieceStr(piece);
        _pieceColor = color;
    }

    protected override void PaintInternal(SKCanvas canvas, SKRect renderRect)
    {
        var color = _pieceColor == PieceColor.White ? SKColors.White : SKColors.Black;

        RichString richString = new()
        {
            MaxWidth = renderRect.Width,
            MaxHeight = renderRect.Height,
            DefaultAlignment = TextAlignment.Center
        };

        var fontSize = MeasureFontSize(renderRect.Width, renderRect.Height, _pieceString);

        richString.FontSize(fontSize).FontFamily("FreeSerif");

        richString.TextColor(color);
        if (_pieceColor == PieceColor.White)
            richString.HaloWidth(renderRect.Width / 20).HaloColor(SKColors.Black);

        richString.Add(_pieceString);

        richString.Paint(canvas, renderRect.Location);
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

    private static string GetPieceStr(PromotionPiece piece)
    {
        return piece switch
        {
            PromotionPiece.Queen => "♛",
            PromotionPiece.Rook => "♜",
            PromotionPiece.Bishop => "♝",
            PromotionPiece.Knight => "♞",
        };
    }
}