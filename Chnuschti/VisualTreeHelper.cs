using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public static class VisualTreeHelper
{
    public static VisualElement? HitTest(VisualElement root, SKPoint screenPt)
        => root.HitTest(screenPt);
}
