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

    public static void AssignVisualIndices(VisualElement root)
    {
        int idx = 0;
        void Walk(VisualElement e)
        {
            e.VisualIndex = idx++;
            foreach (var child in e.Children) Walk(child);
        }
        Walk(root);
    }
}
