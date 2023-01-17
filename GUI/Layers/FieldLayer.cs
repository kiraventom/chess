using Logic;
using SkiaSharp;

namespace GUI.Layers;

public class FieldLayer : RectangleLayer
{
    private readonly FieldColor _color;
    private readonly SKPaint _whiteFieldPaint = new() {Color = SKColors.LightGray};
    private readonly SKPaint _blackFieldPaint = new() {Color = SKColors.Gray};

    public FieldLayer(Position position, FieldColor color) : base(position)
    {
        _color = color;
    }

    protected override void PaintInternal(SKCanvas canvas, float renderLeft, float renderTop, float renderRight, float renderBottom)
    {
        var paint = _color == FieldColor.White ? _whiteFieldPaint : _blackFieldPaint;
        var rect = new SKRect
        {
            Left = renderLeft,
            Top = renderTop,
            Right = renderRight,
            Bottom = renderBottom,
        };

        canvas.DrawRect(rect, paint);
    }
}