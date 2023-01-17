using SkiaSharp;

namespace GUI.Utils;

public static class SkRectExtensions
{
    public static SKRect Multiply(this SKRect rect, float mod) =>
        new(rect.Left * mod, rect.Top * mod, rect.Right * mod, rect.Bottom * mod);
}