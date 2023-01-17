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

    protected override void PaintInternal(SKCanvas canvas, float renderLeft, float renderTop, float renderRight, float renderBottom)
    {
        // TODO MeasureText
        // _whiteFieldPaint.MeasureText()

        var color = _pieceColor == PieceColor.White ? SKColors.White : SKColors.Black;

        var width = renderRight - renderLeft;
        var height = renderBottom - renderTop;
        RichString richString = new()
        {
            MaxWidth = width,
            MaxHeight = height,
            DefaultAlignment = TextAlignment.Center
        };

        var fontSize = MeasureFontSize(width, height, _pieceString);

        richString.FontSize(fontSize).FontFamily("FreeSerif");

        richString.TextColor(color);
        if (_pieceColor == PieceColor.White)
            richString.HaloWidth(width / 20).HaloColor(SKColors.Black);

        richString.Add(_pieceString);

        var point = new SKPoint(renderLeft, renderTop);
        richString.Paint(canvas, point);
    }

    private static string GetPieceStr(Logic.Pieces.Piece piece)
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
}