using Logic;
using SkiaSharp;

namespace GUI.Layers;

public enum HighlightType
{
    Selected,
    Move,
    Attack,
    PromotionFog
}

public class HighlightLayer : RectangleLayer
{
    private readonly SKPaint _movesPaint = new() {Color = new SKColor(0, 180, 10, 128)};
    private readonly SKPaint _attackedPaint = new() {Color = new SKColor(156, 62, 62, 128)};
    private readonly SKPaint _selectionPaint = new() {Color = new SKColor(156, 156, 62, 128)};
    private readonly SKPaint _promotionFogPaint = new() {Color = new SKColor(0, 0, 0, 128)};

    public HighlightType Type { get; }

    public HighlightLayer(Position position, HighlightType type) : base(position)
    {
        Type = type;
    }

    protected override void PaintInternal(SKCanvas canvas, SKRect renderRect)
    {
        var paint = Type switch
        {
            HighlightType.Selected => _selectionPaint,
            HighlightType.Move => _movesPaint,
            HighlightType.Attack => _attackedPaint,
            HighlightType.PromotionFog => _promotionFogPaint,
        };

        canvas.DrawRect(renderRect, paint);
    }
}