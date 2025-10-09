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
    public CheckboxStyle()
    {
        Renderer = new CheckBoxRenderer();
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
    // 1) measure -----------------------------------------------------
    public override SKSize OnMeasure(CheckBox elem, CheckBoxRenderState res, SKSize avail)
    {
        // Reserve box + spacing, remainder is for content
        var remain = new SKSize(
            Math.Max(0, avail.Width - ThemeManager.Current.Width),
            avail.Height);

        var childNeed = SKSize.Empty;
        if (elem.Content != null)
        {
            elem.Content.Measure(remain);
            childNeed = elem.Content.DesiredSize;
        }

        var w = ThemeManager.Current.Width + (elem.Content != null ? childNeed.Width : 0);
        var h = Math.Max(ThemeManager.Current.Height, childNeed.Height);

        return new SKSize(w, h);
    }

    // 2) render ------------------------------------------------------
    public override void OnRender(SKCanvas c, CheckBox e, CheckBoxRenderState r, double deltaTime)
    {
        // Position box vertically centred
        var y = (e.ContentBounds.Height - ThemeManager.Current.Height) / 2f;

        // Draw box
        var radius = ThemeManager.Current.Radius;
        c.DrawRoundRect(0, y, ThemeManager.Current.Width, ThemeManager.Current.Height, radius, radius, r.BackgroundPaint);
        c.DrawRoundRect(0, y, ThemeManager.Current.Width, ThemeManager.Current.Height, radius, radius, r.BorderPaint);

        if (e.IsMouseOver)
        {
            c.DrawCircle(r.HandlePosition, y + ThemeManager.Current.Height / 2f, ThemeManager.Current.HandleHover / 2f, r.HoverPaint);
        }
        c.DrawCircle(r.HandlePosition, y + ThemeManager.Current.Height / 2f, r.HandleSize / 2f, r.HandlePaint);
    }

    // 3) update / theme reaction ------------------------------------
    public override void OnUpdateRenderState(CheckBox e, CheckBoxRenderState r)
    {
        // ---------- upfront colour decisions ----------
        var handleOn = e.Foreground != SKColor.Empty ? e.Foreground : ThemeManager.Current.AccentBright;
        var backOn = e.Background != SKColor.Empty ? e.Background : ThemeManager.Current.AccentColor;

        // ---------- quick helpers ----------
        void SetEnabledColours(bool enabled)
        {
            if (enabled)
            {
                var on = handleOn;      // cache captured locals
                var off = ThemeManager.Current.OffColor;

                r.BorderPaint.Color = backOn;
                r.HandlePaint.Color = e.IsChecked ? on : off;
                r.BackgroundPaint.Color = e.IsChecked ? backOn : off.WithAlpha(80);
            }
            else
            {
                var disabled = ThemeManager.Current.DisabledColor;
                r.BorderPaint.Color = disabled;
                r.HandlePaint.Color = disabled;
                r.BackgroundPaint.Color = disabled.WithAlpha(80);
            }
        }

        void StartCheckAnimations(bool isChecked)
        {
            r.Animations["HandlePosition"].Start(
                isChecked ? ThemeManager.Current.Height / 2f : ThemeManager.Current.Width - ThemeManager.Current.Height / 2f,
                isChecked ? ThemeManager.Current.Width - ThemeManager.Current.Height / 2f : ThemeManager.Current.Height / 2f);

            r.Animations["HandleColor"].Start(
                isChecked ? ThemeManager.Current.OffColor : handleOn,
                isChecked ? handleOn : ThemeManager.Current.OffColor);

            r.Animations["BackgroundColor"].Start(
                isChecked ? ThemeManager.Current.OffColor : backOn,
                isChecked ? backOn : ThemeManager.Current.OffColor.WithAlpha(80));

            r.Animations["HandleSize"].Start(
                r.HandleSize,
                isChecked ? ThemeManager.Current.HandleOn : ThemeManager.Current.HandleOff);
        }

        void Initialize()
        {
            if (r.Initialized) return;

            InitPaint(r.BorderPaint, SKPaintStyle.Stroke, ThemeManager.Current.BorderThickness, backOn);
            InitPaint(r.BackgroundPaint, SKPaintStyle.Fill);
            InitPaint(r.HandlePaint, SKPaintStyle.Fill);
            InitPaint(r.HoverPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);

            r.Animations["HandlePosition"].Initialize(ThemeManager.Current.Height / 2f);
            r.Animations["HandleColor"].Initialize(ThemeManager.Current.OffColor);
            r.Animations["BackgroundColor"].Initialize(ThemeManager.Current.OffColor.WithAlpha(80));
            r.Animations["HandleSize"].Initialize(ThemeManager.Current.HandleOff);

            r.Initialized = true;
        }

        void StartPressAnimation(bool pressed) =>
            r.Animations["HandleSize"].Start(
                r.HandleSize,
                pressed ? ThemeManager.Current.HandlePressed
                        : (e.IsChecked ? ThemeManager.Current.HandleOn : ThemeManager.Current.HandleOff));

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