using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class ButtonStyle : Style
{
    public ButtonStyle() : base()
    {
        Add(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
        Add(Button.VerticalContentAlignmentProperty, VerticalAlignment.Center);
        Add(Button.PaddingProperty, new Thickness(ThemeManager.Current.Radius, 0));
        Renderer = new ButtonRenderer();
    }
}

public class ButtonRenderState : RenderState
{
    public bool PreviousPressedState = false;
    public bool PreviousEnabledState = true;
    public SKPaint BackgroundPaint { get; set; } = new SKPaint();
    public float Radius { get; set; } = 5;
    public SKImageFilter? Shadow { get; set; } = null;

    public ButtonRenderState()
    {
        Animations.Add(new AnimationColor("BackgroundColor", TimeSpan.FromSeconds(0.5), (c) => BackgroundPaint.Color = c));
        Animations.Add(new AnimationNumeric<float>("Radius", TimeSpan.FromSeconds(0.2), (r) => Radius = r));
        Animations.Add(new AnimationNumeric<float>("Depth", TimeSpan.FromSeconds(0.2), (d) => Shadow = SKImageFilter.CreateDropShadow(0, d, 2, 2, SKColors.Black.WithAlpha(100))));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BackgroundPaint.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class ButtonRenderer : Renderer<Button, ButtonRenderState>
{
    public override SKSize OnMeasure(Button element, ButtonRenderState resource, SKSize availableContent)
    {
        if (element.Content == null) return SKSize.Empty;
        
        // Let child measure first
        element.Content.Measure(availableContent);
        var childNeed = element.Content.DesiredSize;

        var w = Math.Max(ThemeManager.Current.Width, (element.Content != null ? childNeed.Width : 0));
        var h = Math.Max(ThemeManager.Current.Height, childNeed.Height);

        return new SKSize(w, h);
    }

    public override void OnRender(SKCanvas canvas, Button element, ButtonRenderState r, double deltaTime)
    {
        if (r.Shadow != null) r.BackgroundPaint.ImageFilter = r.Shadow;
        canvas.DrawRoundRect(0, 0, element.ContentBounds.Width, element.ContentBounds.Height, r.Radius, r.Radius, r.BackgroundPaint);

        // let ContentControl draw child
        element.Content?.Render(canvas, deltaTime);   // child draws in its own local coords
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
            }
            else
            {
                var disabled = ThemeManager.Current.DisabledColor;
                r.BackgroundPaint.Color = disabled.WithAlpha(80);
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