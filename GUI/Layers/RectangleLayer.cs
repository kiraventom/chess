using Common;
using GUI.Utils;
using Logic;
using SkiaSharp;

namespace GUI.Layers;

public abstract class RectangleLayer : IChangeLayer
{
    private SKRect Rectangle { get; }

    protected RectangleLayer(Position position)
    {
        Rectangle = new SKRect
        {
            Left = position.Column - 1,
            Top = 8 - position.Row,
            Right = position.Column,
            Bottom = 8 - position.Row + 1,
        };
    }

    public void Paint(SKCanvas canvas, float mod)
    {
        PaintInternal(canvas, Rectangle.Multiply(mod));
    }

    protected abstract void PaintInternal(SKCanvas canvas, SKRect renderRect);
}