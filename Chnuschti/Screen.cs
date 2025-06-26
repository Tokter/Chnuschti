using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class Screen : ContentControl
{
    private readonly Random rand = new();

    public void SetSize(int width, int height)
    {
        Measure(new SKSize(width, height));
        Arrange(new SKRect(0, 0, width, height));
    }

    protected override void RenderSelf(SKCanvas canvas)
    {
        canvas.Clear(SKColors.White);

        if (Content == null) return;
        Content.Render(canvas);
    }
}
