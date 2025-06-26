using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

public sealed class Border : ContentControl
{
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(float), typeof(Border), new PropertyMetadata(0f));

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty)!;
        set => SetValue(CornerRadiusProperty, value);
    }

    protected override void RenderSelf(SKCanvas c)
    {
        using var p = new SKPaint { Color = Background, IsAntialias = true };

        var rect = new SKRect(0, 0, ContentBounds.Width, ContentBounds.Height);

        if (CornerRadius > 0)
            c.DrawRoundRect(rect, CornerRadius, CornerRadius, p);
        else
            c.DrawRect(rect, p);

        base.RenderSelf(c);        // draw child (e.g. a Label) inside
    }
}
