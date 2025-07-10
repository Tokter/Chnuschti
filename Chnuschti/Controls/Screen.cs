using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

public class Screen : ContentControl
{
    private readonly Random rand = new();

    public Screen()
    {
    }

    public void SetSize(float width, float height)
    {
        Measure(new SKSize(width, height));
        Arrange(new SKRect(0, 0, width, height));
    }
}
