using Common;
using Logic;
using SkiaSharp;

namespace GUI.Layers;

public enum HighlightType {None, Selected, Move, Attack}

public abstract class RectangleLayer : IChangeLayer
{
    private float Left { get; }
    private float Top { get; }
    private float Right { get; }
    private float Bottom { get; }

    protected RectangleLayer(Position position)
    {
        Left = position.Column - 1;
        Top = 8 - position.Row;
        Right = position.Column;
        Bottom = 8 - position.Row + 1;
    }

    public void Paint(SKCanvas canvas, float mod)
    {
        PaintInternal(canvas, Left * mod, Top * mod, Right * mod, Bottom * mod);
    }

    protected abstract void PaintInternal(SKCanvas canvas, float renderLeft, float renderTop, float renderRight, float renderBottom);
}