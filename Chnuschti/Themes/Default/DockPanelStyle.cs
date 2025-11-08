using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class DockPanelStyle : Style
{
    public DockPanelStyle()
    {
        Renderer = new DockPanelRenderer();
    }
}

public class DockPanelResource : RenderState
{
}

public class DockPanelRenderer : Renderer<DockPanel, DockPanelResource>
{
    public override SKSize OnMeasure(DockPanel element, DockPanelResource resource, SKSize availableContent)
    {
        float remainingW = availableContent.Width;
        float remainingH = availableContent.Height;
        float minWidth = 0;
        float minHeight = 0;
        foreach (var child in element.Children)
        {
            // Child receives remaining space
            var avail = new SKSize(remainingW, remainingH);
            child.Measure(avail);
            var need = child.DesiredSize; // includes child.Margin
            // Adjust remaining space based on docking
            switch (DockPanel.GetDock(child))
            {
                case Dock.Left:
                case Dock.Right:
                    remainingW -= need.Width;
                    minHeight = Math.Max(minHeight, need.Height);
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    remainingH -= need.Height;
                    minWidth = Math.Max(minWidth, need.Width);
                    break;
            }
        }
        
        // Final size is original available size minus any remaining space
        return new SKSize(Math.Max(minWidth, availableContent.Width - remainingW), Math.Max(minHeight, availableContent.Height - remainingH));
    }
}