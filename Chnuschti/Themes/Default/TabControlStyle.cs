using Chnuschti.Controls;
using SkiaSharp;
using System.Numerics;

namespace Chnuschti.Themes.Default;

public class TabControlStyle : Style
{
    public TabControlStyle()
    {
        Renderer = new TabControlRenderer();
    }
}

public class TabControlResource : RenderState
{
    public int PreviousSelectedIndex = -1;
    public SKPaint BackgroundPaint { get; set; } = new SKPaint();
    public SKPaint DividerPaint { get; set; } = new SKPaint();
    public SKPaint IndicatorPaint { get; set; } = new SKPaint();
    public Vector2 IndicatorStart { get; set; }
    public Vector2 IndicatorEnd { get; set; }

    public TabControlResource()
    {
        Animations.Add(new AnimationVector2("IndicatorStart", TimeSpan.FromSeconds(0.5), (v) => IndicatorStart = v, AnimationType.EaseInOut));
        Animations.Add(new AnimationVector2("IndicatorEnd", TimeSpan.FromSeconds(0.5), (v) => IndicatorEnd = v, AnimationType.EaseInOut));
    }

    public override void OnInitialize()
    {
        InitPaint(BackgroundPaint, SKPaintStyle.Fill, color: ThemeManager.Current.ShadowColor);
        InitPaint(DividerPaint, SKPaintStyle.Fill, color: ThemeManager.Current.AccentDark);
        InitPaint(IndicatorPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.AccentColor);

        Animations["IndicatorStart"].Initialize(Vector2.Zero);
        Animations["IndicatorEnd"].Initialize(Vector2.Zero);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BackgroundPaint.Dispose();
            DividerPaint.Dispose();
            IndicatorPaint.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>Measures header and body taking strip placement into account, and
/// renders the header background / indicator in Material Design 3 style.</summary>
public class TabControlRenderer : Renderer<TabControl, TabControlResource>
{
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
        //OnUpdateRenderState(tc, r); // Ensure selection indicator is up to date

        // Draw header background, divider and selection indicator.
        var hdrPanel = tc._headerPanel;
        if (hdrPanel == null || hdrPanel.Children.Count == 0) return;

        // Use LayoutSlot (panel-relative to TabControl's local coords) instead of ContentBounds
        var headerRect = hdrPanel.LayoutSlot;

        // Background
        canvas.DrawRect(headerRect, r.BackgroundPaint);

        // Divider between header and content (1dp)
        switch (tc.StripPlacement)
        {
            case TabStripPlacement.Top:
                canvas.DrawLine(headerRect.Left, headerRect.Bottom, headerRect.Right, headerRect.Bottom, r.DividerPaint);
                break;
            case TabStripPlacement.Bottom:
                canvas.DrawLine(headerRect.Left, headerRect.Top, headerRect.Right, headerRect.Top, r.DividerPaint);
                break;
            case TabStripPlacement.Left:
                canvas.DrawLine(headerRect.Right, headerRect.Top, headerRect.Right, headerRect.Bottom, r.DividerPaint);
                break;
            case TabStripPlacement.Right:
                canvas.DrawLine(headerRect.Left, headerRect.Top, headerRect.Left, headerRect.Bottom, r.DividerPaint);
                break;
        }

        // Selection indicator (3dp height or width)
        canvas.DrawRect(new SKRect(r.IndicatorStart.X, r.IndicatorStart.Y, r.IndicatorEnd.X, r.IndicatorEnd.Y), r.IndicatorPaint);

        // Children (headerPanel & contentHost) are rendered by their own renderers
    }

    public override void OnUpdateRenderState(TabControl e, TabControlResource r)
    {
        if (e.SelectedIndex != r.PreviousSelectedIndex)
        {
            int selIdx = e.SelectedIndex;
            if (selIdx >= 0 && selIdx < e._headerPanel.Children.Count)
            {
                if (SetSelectedIndex(e, r)) r.PreviousSelectedIndex = e.SelectedIndex;
            }
        }
    }

    private bool SetSelectedIndex(TabControl e, TabControlResource r)
    {
        var hdrPanel = e._headerPanel;
        if (hdrPanel == null || hdrPanel.Children.Count == 0) return false;
        var headerRect = hdrPanel.ContentBounds; // SKRect of arranged header panel


        var toCtrl = (Control)hdrPanel.Children[e.SelectedIndex];
        var toRect = toCtrl.LayoutSlot;

        if (toRect.Width<= 0 || toRect.Height <= 0) return false;

        if (r.PreviousSelectedIndex <= 0) r.PreviousSelectedIndex = 0;
        if (r.PreviousSelectedIndex >= hdrPanel.Children.Count) r.PreviousSelectedIndex = hdrPanel.Children.Count - 1;
        var fromCtrl = (Control)hdrPanel.Children[r.PreviousSelectedIndex];
        var fromRect = fromCtrl.LayoutSlot;

        const float thickness = 3f; // 3dp
        switch (e.StripPlacement)
        {
            case TabStripPlacement.Top:
                r.Animations["IndicatorStart"].Start(new Vector2(fromRect.Left, headerRect.Bottom - thickness), new Vector2(toRect.Left, headerRect.Bottom - thickness));
                r.Animations["IndicatorEnd"].Start(new Vector2(fromRect.Right, headerRect.Bottom), new Vector2(toRect.Right, headerRect.Bottom));
                //canvas.DrawRect(new SKRect(selRect.Left, headerRect.Bottom - thickness, selRect.Right, headerRect.Bottom), r.IndicatorPaint);
                break;

            case TabStripPlacement.Bottom:
                r.Animations["IndicatorStart"].Start(new Vector2(fromRect.Left, headerRect.Top), new Vector2(toRect.Left, headerRect.Top));
                r.Animations["IndicatorEnd"].Start(new Vector2(fromRect.Right, headerRect.Top + thickness), new Vector2(toRect.Right, headerRect.Top + thickness));
                //canvas.DrawRect(new SKRect(selRect.Left, headerRect.Top, selRect.Right, headerRect.Top + thickness), r.IndicatorPaint);
                break;

            case TabStripPlacement.Left:
                r.Animations["IndicatorStart"].Start(new Vector2(headerRect.Right - thickness, fromRect.Top), new Vector2(headerRect.Right - thickness, toRect.Top));
                r.Animations["IndicatorEnd"].Start(new Vector2(headerRect.Right, fromRect.Bottom), new Vector2(headerRect.Right, toRect.Bottom));
                //canvas.DrawRect(new SKRect(headerRect.Right - thickness, selRect.Top, headerRect.Right, selRect.Bottom), r.IndicatorPaint);
                break;

            case TabStripPlacement.Right:
                r.Animations["IndicatorStart"].Start(new Vector2(headerRect.Left, fromRect.Top), new Vector2(headerRect.Left, toRect.Top));
                r.Animations["IndicatorEnd"].Start(new Vector2(headerRect.Left + thickness, fromRect.Bottom), new Vector2(headerRect.Left + thickness, toRect.Bottom));
                //canvas.DrawRect(new SKRect(headerRect.Left, selRect.Top, headerRect.Left + thickness, selRect.Bottom), r.IndicatorPaint);
                break;
        }

        return true;
    }
}