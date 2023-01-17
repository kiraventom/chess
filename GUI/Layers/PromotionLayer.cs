using Logic;
using SkiaSharp;

namespace GUI.Layers;

public class PromotionLayer : RectangleLayer
{
    private readonly SKPaint _backgroundPaint = new() {Color = new SKColor(50, 0, 0)};

    public PromotionLayer(Position position) : base(position)
    {
    }

    protected override void PaintInternal(SKCanvas canvas, SKRect renderRect)
    {
        canvas.DrawRect(renderRect, _backgroundPaint);
    }
}