using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Themes.Default;

public class ScrollBarStyle : Style
{
    public ScrollBarStyle()
    {
        Add(ScrollBar.MinWidthProperty, ThemeManager.Current.Height);   // thickness if vertical
        Add(ScrollBar.MinHeightProperty, ThemeManager.Current.Height);  // thickness if horizontal
        Renderer = new ScrollBarRenderer();
    }
}

public class ScrollBarRenderState : RenderState
{
    public SKPaint TrackPaint { get; } = new();
    public SKPaint HandlePaint { get; } = new();
    public SKPaint HoverPaint { get; } = new();
    public SKPaint DisabledPaint { get; } = new();

    public override void OnInitialize()
    {
        InitPaint(TrackPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.ShadowColor.WithAlpha(60));
        InitPaint(HandlePaint, SKPaintStyle.Fill, 0, ThemeManager.Current.AccentColor);
        InitPaint(HoverPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);
        InitPaint(DisabledPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.DisabledColor.WithAlpha(100));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            TrackPaint.Dispose();
            HandlePaint.Dispose();
            HoverPaint.Dispose();
            DisabledPaint.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class ScrollBarRenderer : Renderer<ScrollBar, ScrollBarRenderState>
{
    public override SKSize OnMeasure(ScrollBar e, ScrollBarRenderState r, SKSize availableContent)
    {
        // Thickness is theme Height; length is available along orientation.
        if (e.Orientation == Orientation.Vertical)
        {
            return new SKSize(Math.Max(ThemeManager.Current.Height, e.MinWidth), availableContent.Height);
        }
        else
        {
            return new SKSize(availableContent.Width, Math.Max(ThemeManager.Current.Height, e.MinHeight));
        }
    }

    public override void OnUpdateRenderState(ScrollBar e, ScrollBarRenderState r)
    {
        // Colour adjustments only; no position/size animation
        if (!e.IsEnabled)
        {
            r.HandlePaint.Color = ThemeManager.Current.DisabledColor;
        }
        else if (e.IsPressed)
        {
            r.HandlePaint.Color = ThemeManager.Current.AccentDark;
        }
        else if (e.IsMouseOver)
        {
            r.HandlePaint.Color = ThemeManager.Current.AccentBright;
        }
        else
        {
            r.HandlePaint.Color = ThemeManager.Current.AccentColor;
        }
    }

    public override void OnRender(SKCanvas c, ScrollBar e, ScrollBarRenderState r, double deltaTime)
    {
        var bounds = e.ContentBounds;

        // Track
        c.DrawRect(0, 0, bounds.Width, bounds.Height, e.IsEnabled ? r.TrackPaint : r.DisabledPaint);

        // Handle (direct from current value/metrics)
        var (_, _, handleLen, handleOffset) = e.GetRenderMetrics();
        var thickness = e.Orientation == Orientation.Vertical ? bounds.Width : bounds.Height;

        SKRect handleRect = e.Orientation == Orientation.Vertical
            ? new SKRect(0, handleOffset, thickness, handleOffset + handleLen)
            : new SKRect(handleOffset, 0, handleOffset + handleLen, thickness);

        // Hover aura
        if (e.IsMouseOver && e.IsEnabled)
        {
            float inflate = 4f;
            var hoverRect = new SKRect(handleRect.Left - inflate, handleRect.Top - inflate, handleRect.Right + inflate, handleRect.Bottom + inflate);
            c.DrawRoundRect(hoverRect, ThemeManager.Current.Radius, ThemeManager.Current.Radius, r.HoverPaint);
        }

        // Handle
        c.DrawRoundRect(handleRect, ThemeManager.Current.Radius, ThemeManager.Current.Radius, r.HandlePaint);
    }
}
