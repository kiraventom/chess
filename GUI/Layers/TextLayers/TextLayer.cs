using Logic;
using Topten.RichTextKit;

namespace GUI.Layers.TextLayers;

public abstract class TextLayer : RectangleLayer
{
    protected TextLayer(Position position) : base(position)
    {
    }

    protected static int MeasureFontSize(float width, float height, string text)
    {
        RichString richString;
        var fontSize = 0;
        do
        {
            ++fontSize;
            richString = new RichString
            {
                MaxHeight = height,
                MaxWidth = width,
                DefaultAlignment = TextAlignment.Center
            };

            richString.FontSize(fontSize).Add(text);
        } while (!richString.Truncated);

        return fontSize - 1;
    }
}