using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public static class SKColorEx
{
    public static SKColor AdjBrightness(this SKColor color, float brightnessDelta)
    {
        color.ToHsl(out float h, out float s, out float l);

        l += brightnessDelta;
        if (l < 0) l = 0;
        if (l > 100) l = 100;

        // Convert back to RGB
        return SKColor.FromHsl(h, s, l);
    }
}
