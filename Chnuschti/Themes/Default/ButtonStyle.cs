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
    public SKColor  BackgroundColor { get; set; }
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

    public override void OnInitialize()
    {
        InitPaint(BackgroundPaint, SKPaintStyle.Fill);
        InitPaint(OutlinePaint, SKPaintStyle.Stroke, ThemeManager.Current.BorderThickness);
        InitPaint(ShadowPaint, SKPaintStyle.Fill, color: ThemeManager.Current.ShadowColor);
        InitPaint(HoverPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);

        Animations["Radius"].Initialize(ThemeManager.Current.Radius);
        Animations["BackgroundColor"].Initialize(BackgroundColor);
        Animations["Depth"].Initialize(3.0f);
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
            c.DrawRoundRect(0 + r.Depth, 0 + r.Depth, e.ContentBounds.Width, e.ContentBounds.Height, r.Radius, r.Radius, r.ShadowPaint);
        }

        if (e.IsMouseOver)
        {
            var hoverSize = 4.0f;
            c.DrawRoundRect(0 - hoverSize, 0 - hoverSize, e.ContentBounds.Width + 2 * hoverSize, e.ContentBounds.Height + 2 * hoverSize, r.Radius + hoverSize, r.Radius + hoverSize, r.HoverPaint);
        }

        if (_renderFlags.HasFlag(ButtonRenderFlags.Filled))
        {
            c.DrawRoundRect(e.ContentBounds, r.Radius, r.Radius, r.BackgroundPaint);
        }
        else if (_renderFlags.HasFlag(ButtonRenderFlags.Outlined))
        {
            c.DrawRoundRect(e.ContentBounds, r.Radius, r.Radius, r.OutlinePaint);
        }

        // let ContentControl draw child
        e.Content?.Render(c, deltaTime);   // child draws in its own local coords
    }


    public override void OnInitialize(Button element, ButtonRenderState resource)
    {
        resource.BackgroundColor = element.Background != SKColor.Empty ? element.Background : ThemeManager.Current.AccentColor;
    }

    public override void OnUpdateRenderState(Button e, ButtonRenderState r)
    {
        if (e.IsEnabled != r.PreviousEnabledState)
        {
            r.PreviousEnabledState = e.IsEnabled;
            SetEnabledColours(e, r, e.IsEnabled);
        }

        if (e.IsPressed != r.PreviousPressedState)
        {
            r.PreviousPressedState = e.IsPressed;
            StartPressAnimation(e, r, e.IsPressed);
        }
    }

    private void SetEnabledColours(Button e, ButtonRenderState r, bool enabled)
    {
        if (enabled)
        {
            r.BackgroundPaint.Color = r.BackgroundColor;
            r.OutlinePaint.Color = r.BackgroundColor;
        }
        else
        {
            var disabled = ThemeManager.Current.DisabledColor;
            r.BackgroundPaint.Color = disabled.WithAlpha(80);
            r.OutlinePaint.Color = disabled;
        }
    }

    private void StartPressAnimation(Button e, ButtonRenderState r, bool pressed)
    {
        r.Animations["Radius"].Start(
            pressed ? ThemeManager.Current.Radius : ThemeManager.Current.PressedRadius,
            pressed ? ThemeManager.Current.PressedRadius : ThemeManager.Current.Radius);

        r.Animations["Depth"].Start(
            pressed ? 3.0f : 0.0f,
            pressed ? 0.0f : 3.0f);

        r.Animations["BackgroundColor"].Start(
            pressed ? r.BackgroundColor : Darken(r.BackgroundColor, 20),
            pressed ? Darken(r.BackgroundColor, 20) : r.BackgroundColor);
    }
}