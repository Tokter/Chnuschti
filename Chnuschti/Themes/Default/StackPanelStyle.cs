using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class StackPanelStyle : Style
{
    public StackPanelStyle()
    {
        Renderer = new StackPanelRenderer();
    }
}

public class StackPanelResource : RenderState
{
}

public class StackPanelRenderer : Renderer<StackPanel, StackPanelResource>
{
    public override SKSize OnMeasure(StackPanel element, StackPanelResource resource, SKSize availableContent)
    {
        float totalW = 0, totalH = 0;
        float maxW = 0, maxH = 0;

        foreach (var child in element.Children)
        {
            // Child receives infinite space in the stacking direction
            var avail = element.Orientation == Orientation.Vertical
                ? new SKSize(availableContent.Width, float.PositiveInfinity)
                : new SKSize(float.PositiveInfinity, availableContent.Height);

            child.Measure(avail);

            var need = child.DesiredSize; // includes child.Margin
            if (element.Orientation == Orientation.Vertical)
            {
                totalH += need.Height;
                maxW = Math.Max(maxW, need.Width);
            }
            else
            {
                totalW += need.Width;
                maxH = Math.Max(maxH, need.Height);
            }
        }

        if (element.Orientation == Orientation.Vertical)
            return new SKSize(maxW, totalH);
        else
            return new SKSize(totalW, maxH);
    }
}