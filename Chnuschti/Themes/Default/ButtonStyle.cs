using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public abstract class ButtonBasestyle : Style
{
    public ButtonBasestyle() : base()
    {
        Add(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
        Add(Button.VerticalContentAlignmentProperty, VerticalAlignment.Center);
        Add(Button.PaddingProperty, new Thickness(ThemeManager.Current.Radius, 0));
    }
}

public class ButtonStyle : ButtonBasestyle
{
    public ButtonStyle() : base()
    {
        Add(Button.MinWidthProperty, ThemeManager.Current.Width);
        Add(Button.MinHeightProperty, ThemeManager.Current.Height);
        Renderer = new ButtonRenderer(ButtonRenderFlags.Filled);
    }
}

public class ButtonFlatStyle : ButtonBasestyle
{
    public ButtonFlatStyle() : base()
    {
        Renderer = new ButtonRenderer(ButtonRenderFlags.None);
    }
}

public class ButtonOutlinedStyle : ButtonBasestyle
{
    public ButtonOutlinedStyle() : base()
    {
        Add(Button.MinWidthProperty, ThemeManager.Current.Width);
        Add(Button.MinHeightProperty, ThemeManager.Current.Height);
        Renderer = new ButtonRenderer(ButtonRenderFlags.Outlined);
    }
}

public class ButtonRenderState : RenderState
{
    public bool PreviousPressedState = false;
    public bool PreviousEnabledState = true;
    public SKPaint BackgroundPaint { get; set; } = new SKPaint();
    public SKPaint OutlinePaint { get; set; } = new SKPaint();
    public SKPaint ShadowPaint { get; set; } = new SKPaint();
    public SKPaint HoverPaint { get; } = new();

    public float Radius { get; set; } = 5;
    public float Depth { get; set; } = 2;

    public ButtonRenderState()
    {
        Animations.Add(new AnimationColor("BackgroundColor", TimeSpan.FromSeconds(0.5), (c) => { BackgroundPaint.Color = c; OutlinePaint.Color = c; }));
        Animations.Add(new AnimationNumeric<float>("Radius", TimeSpan.FromSeconds(0.2), (r) => Radius = r));
        Animations.Add(new AnimationNumeric<float>("Depth", TimeSpan.FromSeconds(0.2), (d) => Depth = d));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BackgroundPaint.Dispose();
            OutlinePaint.Dispose();
            ShadowPaint.Dispose();
            HoverPaint.Dispose();
        }
        base.Dispose(disposing);
    }
}

[Flags]
public enum ButtonRenderFlags
{
    None = 0,
    Shadow = 1 << 0,
    Filled = 1 << 1,
    Outlined = 1 << 2,
}

public class ButtonRenderer : Renderer<Button, ButtonRenderState>
{
    private ButtonRenderFlags _renderFlags;

    public ButtonRenderer(ButtonRenderFlags renderFlags)
    {
        _renderFlags = renderFlags;
    }

    public override SKSize OnMeasure(Button element, ButtonRenderState resource, SKSize availableContent)
    {
        if (element.Content == null) return SKSize.Empty;
        
        // Let child measure first
        element.Content.Measure(availableContent);
        var childNeed = element.Content.DesiredSize;

        //var w = Math.Max(ThemeManager.Current.Width, (element.Content != null ? childNeed.Width : 0));
        //var h = Math.Max(ThemeManager.Current.Height, childNeed.Height);
        //return new SKSize(w, h);

        return new SKSize((element.Content != null ? childNeed.Width : 0), (element.Content != null ? childNeed.Height : 0));
    }

    public override void OnRender(SKCanvas c, Button e, ButtonRenderState r, double deltaTime)
    {
        if (_renderFlags.HasFlag(ButtonRenderFlags.Shadow) && r.Depth > 0.0f)
        {
            // draw shadow
            c.DrawRoundRect(0 + r.Depth, 0 + r.Depth, e.ContentBounds.Width, e.ContentBounds.Height, r.Radius, r.Radius, r.ShadowPaint);
        }

        if (e.IsMouseOver)
        {
            var hoverSize = 4.0f;
            c.DrawRoundRect(0 - hoverSize, 0 - hoverSize, e.ContentBounds.Width + 2 * hoverSize, e.ContentBounds.Height + 2 * hoverSize, r.Radius + hoverSize, r.Radius + hoverSize, r.HoverPaint);
        }

        if (_renderFlags.HasFlag(ButtonRenderFlags.Filled))
        {
            c.DrawRoundRect(0, 0, e.ContentBounds.Width, e.ContentBounds.Height, r.Radius, r.Radius, r.BackgroundPaint);
        }
        else if (_renderFlags.HasFlag(ButtonRenderFlags.Outlined))
        {
            c.DrawRoundRect(0, 0, e.ContentBounds.Width, e.ContentBounds.Height, r.Radius, r.Radius, r.OutlinePaint);
        }

        // let ContentControl draw child
        e.Content?.Render(c, deltaTime);   // child draws in its own local coords
    }

    public override void OnUpdateRenderState(Button e, ButtonRenderState r)
    {
        // ---------- upfront colour decisions ----------
        var backgroundColor = e.Background != SKColor.Empty ? e.Background : ThemeManager.Current.AccentColor;

        // ---------- quick helpers ----------
        void SetEnabledColours(bool enabled)
        {
            if (enabled)
            {
                r.BackgroundPaint.Color = backgroundColor;
                r.OutlinePaint.Color = backgroundColor;
            }
            else
            {
                var disabled = ThemeManager.Current.DisabledColor;
                r.BackgroundPaint.Color = disabled.WithAlpha(80);
                r.OutlinePaint.Color = disabled;
            }
        }

        void StartPressAnimation(bool pressed)
        {
            r.Animations["Radius"].Start(
                pressed ? ThemeManager.Current.Radius : ThemeManager.Current.PressedRadius,
                pressed ? ThemeManager.Current.PressedRadius : ThemeManager.Current.Radius);

            r.Animations["Depth"].Start(
                pressed ? 3.0f : 0.0f,
                pressed ? 0.0f : 3.0f);

            r.Animations["BackgroundColor"].Start(
                pressed ? backgroundColor : Darken(backgroundColor, 20),
                pressed ? Darken(backgroundColor, 20) : backgroundColor);
        }

        void Initialize()
        {
            if (r.Initialized) return;
            InitPaint(r.BackgroundPaint, SKPaintStyle.Fill);
            InitPaint(r.OutlinePaint, SKPaintStyle.Stroke, ThemeManager.Current.BorderThickness);
            InitPaint(r.ShadowPaint, SKPaintStyle.Fill, color: ThemeManager.Current.ShadowColor);
            InitPaint(r.HoverPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);

            r.Animations["Radius"].Initialize(ThemeManager.Current.Radius);
            r.Animations["BackgroundColor"].Initialize(backgroundColor);
            r.Animations["Depth"].Initialize(3.0f);

            r.Initialized = true;
        }

        Initialize();

        // ---------- state-driven updates ----------
        if (e.IsEnabled != r.PreviousEnabledState)
        {
            r.PreviousEnabledState = e.IsEnabled;
            SetEnabledColours(e.IsEnabled);
        }

        if (e.IsPressed != r.PreviousPressedState)
        {
            r.PreviousPressedState = e.IsPressed;
            StartPressAnimation(e.IsPressed);
        }
    }
}