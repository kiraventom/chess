using System;
using Logic;
using SkiaSharp;

namespace GUI.Layers;

public class HighlightLayer : RectangleLayer
{
    private readonly SKPaint _movesPaint = new() {Color = new SKColor(0, 180, 10, 128)};
    private readonly SKPaint _attackedPaint = new() {Color = new SKColor(156, 62, 62, 128)};
    private readonly SKPaint _selectionPaint = new() {Color = new SKColor(156, 156, 62, 128)};

    public HighlightType Type { get; }

    public HighlightLayer(Position position, HighlightType type) : base(position)
    {
        Type = type;
    }

    protected override void PaintInternal(SKCanvas canvas, float renderLeft, float renderTop, float renderRight, float renderBottom)
    {
        var skRect = new SKRect()
        {
            Left = renderLeft,
            Top = renderTop,
            Right = renderRight,
            Bottom = renderBottom
        };

        var paint = Type switch
        {
            HighlightType.None => throw new InvalidOperationException(),
            HighlightType.Selected => _selectionPaint,
            HighlightType.Move => _movesPaint,
            HighlightType.Attack => _attackedPaint,
        };

        canvas.DrawRect(skRect, paint);
    }
}