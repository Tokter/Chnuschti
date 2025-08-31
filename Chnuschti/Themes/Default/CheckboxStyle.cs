using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class CheckboxStyle : Style
{
    public CheckboxStyle(DefaultTheme theme)
    {
        Renderer = new CheckBoxRenderer(theme);
    }
}

internal sealed class CheckBoxRenderState : RenderState
{
    public bool PreviousCheckedState = false;
    public bool PreviousPressedState = false;
    public bool PreviousEnabledState = true;
    public SKPaint BorderPaint { get; } = new();
    public SKPaint BackgroundPaint { get; } = new();
    public SKPaint HandlePaint { get; } = new();
    public SKPaint HoverPaint { get; } = new();
    public float HandlePosition { get; set; }
    public float HandleSize { get; set; }

    public CheckBoxRenderState()
    {
        Animations.Add(new AnimationColor("HandleColor", TimeSpan.FromSeconds(0.5), (c) => HandlePaint.Color = c));
        Animations.Add(new AnimationColor("BackgroundColor", TimeSpan.FromSeconds(0.5), (c) => BackgroundPaint.Color = c));
        Animations.Add(new AnimationNumeric<float>("HandlePosition", TimeSpan.FromSeconds(0.5), (p) => HandlePosition = p, AnimationType.EaseInOut));
        Animations.Add(new AnimationNumeric<float>("HandleSize", TimeSpan.FromSeconds(0.2), (p) => HandleSize = p));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BorderPaint.Dispose();
            BackgroundPaint.Dispose();
            HandlePaint.Dispose();
            HoverPaint.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal sealed class CheckBoxRenderer : Renderer<CheckBox, CheckBoxRenderState>
{
    private readonly DefaultTheme _theme;


    public CheckBoxRenderer(DefaultTheme theme) => _theme = theme;

    // 1) measure -----------------------------------------------------
    public override SKSize OnMeasure(CheckBox elem, CheckBoxRenderState res, SKSize avail)
    {
        // Reserve box + spacing, remainder is for content
        var remain = new SKSize(
            Math.Max(0, avail.Width - CheckBox.WIDTH),
            avail.Height);

        var childNeed = SKSize.Empty;
        if (elem.Content != null)
        {
            elem.Content.Measure(remain);
            childNeed = elem.Content.DesiredSize;
        }

        var w = CheckBox.WIDTH + (elem.Content != null ? childNeed.Width : 0);
        var h = Math.Max(CheckBox.HEIGHT, childNeed.Height);

        return new SKSize(w, h);
    }

    // 2) render ------------------------------------------------------
    public override void OnRender(SKCanvas c, CheckBox e, CheckBoxRenderState r, double deltaTime)
    {
        // Position box vertically centred
        var y = (e.ContentBounds.Height - CheckBox.HEIGHT) / 2f;

        // Draw box
        var radius = CheckBox.HEIGHT / 2.0f;
        c.DrawRoundRect(0, y, CheckBox.WIDTH, CheckBox.HEIGHT, radius, radius, r.BackgroundPaint);
        c.DrawRoundRect(0, y, CheckBox.WIDTH, CheckBox.HEIGHT, radius, radius, r.BorderPaint);

        if (e.IsMouseOver)
        {
            c.DrawCircle(r.HandlePosition, y + CheckBox.HEIGHT / 2f, CheckBox.HANDLE_HOVER / 2f, r.HoverPaint);
        }
        c.DrawCircle(r.HandlePosition, y + CheckBox.HEIGHT / 2f, r.HandleSize / 2f, r.HandlePaint);
    }

    // 3) update / theme reaction ------------------------------------
    public override void OnUpdateRenderState(CheckBox e, CheckBoxRenderState r)
    {
        // ---------- upfront colour decisions ----------
        var handleOn = e.Foreground != SKColor.Empty ? e.Foreground : _theme.AccentBright;
        var backOn = e.Background != SKColor.Empty ? e.Background : _theme.AccentColor;

        // ---------- quick helpers ----------
        void InitPaint(SKPaint p, SKPaintStyle style, float stroke = 0, SKColor? color = null)
        {
            p.Style = style;
            p.IsAntialias = true;
            if (stroke > 0) p.StrokeWidth = stroke;
            if (color.HasValue) p.Color = color.Value;
        }

        void SetEnabledColours(bool enabled)
        {
            if (enabled)
            {
                var on = handleOn;      // cache captured locals
                var off = _theme.OffColor;

                r.BorderPaint.Color = backOn;
                r.HandlePaint.Color = e.IsChecked ? on : off;
                r.BackgroundPaint.Color = e.IsChecked ? backOn : off.WithAlpha(80);
            }
            else
            {
                var disabled = _theme.DisabledColor;
                r.BorderPaint.Color = disabled;
                r.HandlePaint.Color = disabled;
                r.BackgroundPaint.Color = disabled.WithAlpha(80);
            }
        }

        void StartCheckAnimations(bool isChecked)
        {
            r.Animations["HandlePosition"].Start(
                isChecked ? CheckBox.HEIGHT / 2f : CheckBox.WIDTH - CheckBox.HEIGHT / 2f,
                isChecked ? CheckBox.WIDTH - CheckBox.HEIGHT / 2f : CheckBox.HEIGHT / 2f);

            r.Animations["HandleColor"].Start(
                isChecked ? _theme.OffColor : handleOn,
                isChecked ? handleOn : _theme.OffColor);

            r.Animations["BackgroundColor"].Start(
                isChecked ? _theme.OffColor : backOn,
                isChecked ? backOn : _theme.OffColor.WithAlpha(80));

            r.Animations["HandleSize"].Start(
                r.HandleSize,
                isChecked ? CheckBox.HANDLE_ON : CheckBox.HANDLE_OFF);
        }

        void Initialize()
        {
            if (r.Initialized) return;

            r.Animations["HandlePosition"].Initialize(CheckBox.HEIGHT / 2f);
            r.Animations["HandleColor"].Initialize(_theme.OffColor);
            r.Animations["BackgroundColor"].Initialize(_theme.OffColor.WithAlpha(80));
            r.Animations["HandleSize"].Initialize(CheckBox.HANDLE_OFF);

            r.Initialized = true;
        }

        void StartPressAnimation(bool pressed) =>
            r.Animations["HandleSize"].Start(
                r.HandleSize,
                pressed ? CheckBox.HANDLE_PRESSED
                        : (e.IsChecked ? CheckBox.HANDLE_ON : CheckBox.HANDLE_OFF));

        // ---------- one-time paint setup (safe to run every call) ----------
        InitPaint(r.BorderPaint, SKPaintStyle.Stroke, _theme.Thickness, backOn);
        InitPaint(r.BackgroundPaint, SKPaintStyle.Fill);
        InitPaint(r.HandlePaint, SKPaintStyle.Fill);
        InitPaint(r.HoverPaint, SKPaintStyle.Fill, 0, _theme.HoverColor);

        Initialize();

        // ---------- state-driven updates ----------
        if (e.IsEnabled != r.PreviousEnabledState)
        {
            r.PreviousEnabledState = e.IsEnabled;
            SetEnabledColours(e.IsEnabled);
        }

        if (e.IsChecked != r.PreviousCheckedState)
        {
            r.PreviousCheckedState = e.IsChecked;
            StartCheckAnimations(e.IsChecked);
        }
        
        if (e.IsPressed != r.PreviousPressedState)
        {
            r.PreviousPressedState = e.IsPressed;
            StartPressAnimation(e.IsPressed);
        }
    }



}