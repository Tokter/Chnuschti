using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Themes.Default;

public class TabControlStyle : Style
{
    public TabControlStyle()
    {
        Renderer = new TabControlRenderer();
    }
}

public class TabControlResource : RenderState { }

/// <summary>Measures header and body taking strip placement into account, and
/// renders the header background / indicator in Material Design 3 style.</summary>
public class TabControlRenderer : Renderer<TabControl, TabControlResource>
{
    // Material Design 3 reference colors (light scheme)
    //private static readonly SKColor HeaderBackground = SKColor.Parse("#F1F3F4"); // Surface Container highest
    //private static readonly SKColor Divider = SKColor.Parse("#E0E0E0");
    //private static readonly SKColor Indicator = SKColor.Parse("#6750A4"); // Primary

    // Material Design 3 reference colors (dark scheme)
     private static readonly SKColor HeaderBackground = SKColor.Parse("#121212"); // Surface Container highest
     private static readonly SKColor Divider = SKColor.Parse("#373737");
     private static readonly SKColor Indicator = SKColor.Parse("#BB86FC"); // Primary

    public override SKSize OnMeasure(TabControl tc, TabControlResource r, SKSize avail)
    {
        var placement = tc.StripPlacement;

        if (placement is TabStripPlacement.Top or TabStripPlacement.Bottom)
        {
            tc._headerPanel.Measure(new SKSize(avail.Width, float.PositiveInfinity));
            var h = tc._headerPanel.DesiredSize;
            tc._contentHost.Measure(new SKSize(avail.Width, Math.Max(0, avail.Height - h.Height)));
            var b = tc._contentHost.DesiredSize;
            return new SKSize(Math.Max(h.Width, b.Width), h.Height + b.Height);
        }
        else // Left / Right
        {
            tc._headerPanel.Measure(new SKSize(float.PositiveInfinity, avail.Height));
            var h = tc._headerPanel.DesiredSize;
            tc._contentHost.Measure(new SKSize(Math.Max(0, avail.Width - h.Width), avail.Height));
            var b = tc._contentHost.DesiredSize;
            return new SKSize(h.Width + b.Width, Math.Max(h.Height, b.Height));
        }
    }

    public override void OnRender(SKCanvas canvas, TabControl tc, TabControlResource r, double deltaTime)
    {
        // Draw header background, divider and selection indicator.
        var hdrPanel = tc._headerPanel;
        if (hdrPanel == null || hdrPanel.Children.Count == 0) return;

        // We assume the framework gives each control its layout rect via Bounds
        // (or ArrangedRect). If not, adjust accordingly.
        var headerRect = hdrPanel.ContentBounds; // SKRect of arranged header panel

        // Background
        using var bgPaint = new SKPaint { Color = HeaderBackground, IsAntialias = false };
        canvas.DrawRect(headerRect, bgPaint);

        // Divider between header and content (1dp)
        using var divPaint = new SKPaint { Color = Divider, StrokeWidth = 1, IsAntialias = false };
        switch (tc.StripPlacement)
        {
            case TabStripPlacement.Top:
                canvas.DrawLine(headerRect.Left, headerRect.Bottom, headerRect.Right, headerRect.Bottom, divPaint);
                break;
            case TabStripPlacement.Bottom:
                canvas.DrawLine(headerRect.Left, headerRect.Top, headerRect.Right, headerRect.Top, divPaint);
                break;
            case TabStripPlacement.Left:
                canvas.DrawLine(headerRect.Right, headerRect.Top, headerRect.Right, headerRect.Bottom, divPaint);
                break;
            case TabStripPlacement.Right:
                canvas.DrawLine(headerRect.Left, headerRect.Top, headerRect.Left, headerRect.Bottom, divPaint);
                break;
        }

        // Selection indicator (3dp height or width)
        int selIdx = tc.SelectedIndex;
        if (selIdx >= 0 && selIdx < hdrPanel.Children.Count && hdrPanel.Children[selIdx] is Control selCtrl)
        {
            var selRect = selCtrl.ContentBounds;
            using var indPaint = new SKPaint { Color = Indicator, IsAntialias = true };
            const float thickness = 3f; // 3dp
            switch (tc.StripPlacement)
            {
                case TabStripPlacement.Top:
                    canvas.DrawRect(new SKRect(selRect.Left, headerRect.Bottom - thickness, selRect.Right, headerRect.Bottom), indPaint);
                    break;
                case TabStripPlacement.Bottom:
                    canvas.DrawRect(new SKRect(selRect.Left, headerRect.Top, selRect.Right, headerRect.Top + thickness), indPaint);
                    break;
                case TabStripPlacement.Left:
                    canvas.DrawRect(new SKRect(headerRect.Right - thickness, selRect.Top, headerRect.Right, selRect.Bottom), indPaint);
                    break;
                case TabStripPlacement.Right:
                    canvas.DrawRect(new SKRect(headerRect.Left, selRect.Top, headerRect.Left + thickness, selRect.Bottom), indPaint);
                    break;
            }
        }

        // Children (headerPanel & contentHost) are rendered by their own renderers
    }
}