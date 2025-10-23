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

    public float AnimatedOffset;
    public float AnimatedSize;

    public ScrollBarRenderState()
    {
        Animations.Add(new AnimationNumeric<float>("HandleOffset", TimeSpan.FromSeconds(0.05), v => AnimatedOffset = v, AnimationType.EaseInOut));
        Animations.Add(new AnimationNumeric<float>("HandleSize", TimeSpan.FromSeconds(0.25), v => AnimatedSize = v, AnimationType.EaseInOut));
    }

    public override void OnInitialize()
    {
        InitPaint(TrackPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.ShadowColor.WithAlpha(60));
        InitPaint(HandlePaint, SKPaintStyle.Fill, 0, ThemeManager.Current.AccentColor);
        InitPaint(HoverPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);
        InitPaint(DisabledPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.DisabledColor.WithAlpha(100));

        Animations["HandleOffset"].Initialize(0f);
        Animations["HandleSize"].Initialize(ThemeManager.Current.Height);
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

    public override void OnInitialize(ScrollBar e, ScrollBarRenderState r)
    {
    }

    public override void OnUpdateRenderState(ScrollBar e, ScrollBarRenderState r)
    {
        var (trackStart, trackLen, handleLen, handleOffset) = e.GetRenderMetrics();

        // Animate handle movement & size
        r.Animations["HandleOffset"].Start(r.AnimatedOffset, handleOffset);
        r.Animations["HandleSize"].Start(r.AnimatedSize, handleLen);

        // Colour adjustments
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

        // Draw track
        c.DrawRect(0, 0, bounds.Width, bounds.Height, e.IsEnabled ? r.TrackPaint : r.DisabledPaint);

        var thickness = e.Orientation == Orientation.Vertical ? bounds.Width : bounds.Height;

        // Handle rect
        SKRect handleRect;
        if (e.Orientation == Orientation.Vertical)
        {
            handleRect = new SKRect(0, r.AnimatedOffset, thickness, r.AnimatedOffset + r.AnimatedSize);
        }
        else
        {
            handleRect = new SKRect(r.AnimatedOffset, 0, r.AnimatedOffset + r.AnimatedSize, thickness);
        }

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