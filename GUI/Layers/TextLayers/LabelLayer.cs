using Logic;
using SkiaSharp;
using Topten.RichTextKit;

namespace GUI.Layers.TextLayers;

public enum LabelType {Rows, Columns}

public class LabelLayer : TextLayer
{
    private readonly string _labelString;
    private readonly LabelType _type;

    public LabelLayer(Position position, LabelType type) : base(position)
    {
        _labelString = position.ToString();
        _type = type;
    }

    protected override void PaintInternal(SKCanvas canvas, SKRect renderRect)
    {
        var fontSize = MeasureFontSize(renderRect.Width, renderRect.Height / 5, "A");

        switch (_type)
        {
            case LabelType.Rows:
            {
                var richString = new RichString();
                richString.FontSize(fontSize).Add(_labelString[1].ToString());
                richString.Paint(canvas, renderRect.Location);
                break;
            }

            case LabelType.Columns:
            {
                var richString = new RichString();
                richString.FontSize(fontSize).Add(_labelString[0].ToString().ToUpper());
                var x = renderRect.Right - richString.MeasuredWidth;
                var y = renderRect.Bottom - richString.MeasuredHeight;
                richString.Paint(canvas, new SKPoint(x, y));
                break;
            }
        }
    }
}