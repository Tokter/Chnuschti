using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

public class TestBox : Control
{
    public TestBox()
    {
        IsHitTestVisible = false; // TestBox is not interactive by default
    }

    protected override SKSize MeasureContent(SKSize availableContentSize)
    {
        return new SKSize(20, 20);
    }

    protected override void RenderSelf(SKCanvas canvas)
    {
        // Draw a simple border for the TestBox
        using var paint = new SKPaint
        {
            Color = SKColors.Purple,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        var rect = new SKRect(0, 0, 20, 20);
        canvas.DrawRect(rect, paint);

        // Optionally fill the background
        using var fillPaint = new SKPaint { Color = SKColors.LightGray };
        canvas.DrawRect(rect, fillPaint);
    }
}
