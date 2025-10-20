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

public class CheckBoxRenderState : RenderState
{
    public bool PreviousCheckedState = false;
    public bool PreviousPressedState = false;
    public bool PreviousEnabledState = true;
    public SKColor HandleOnColor { get; set; }
    public SKColor BackgroundOnColor { get; set; }
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

    public override void OnInitialize()
    {
        InitPaint(BorderPaint, SKPaintStyle.Stroke, ThemeManager.Current.BorderThickness, BackgroundOnColor);
        InitPaint(BackgroundPaint, SKPaintStyle.Fill);
        InitPaint(HandlePaint, SKPaintStyle.Fill);
        InitPaint(HoverPaint, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);

        Animations["HandlePosition"].Initialize(ThemeManager.Current.Height / 2f);
        Animations["HandleColor"].Initialize(ThemeManager.Current.OffColor);
        Animations["BackgroundColor"].Initialize(ThemeManager.Current.OffColor.WithAlpha(80));
        Animations["HandleSize"].Initialize(ThemeManager.Current.HandleOff);
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

public class CheckBoxRenderer : Renderer<CheckBox, CheckBoxRenderState>
{
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

    public override void OnInitialize(CheckBox e, CheckBoxRenderState r)
    {
        r.BackgroundOnColor = e.Foreground != SKColor.Empty ? e.Foreground : ThemeManager.Current.AccentBright;
        r.HandleOnColor = e.Background != SKColor.Empty ? e.Background : ThemeManager.Current.AccentColor;
    }

    public override void OnUpdateRenderState(CheckBox e, CheckBoxRenderState r)
    {
        if (e.IsEnabled != r.PreviousEnabledState)
        {
            r.PreviousEnabledState = e.IsEnabled;
            SetEnabledColours(e, r, e.IsEnabled);
        }

        if (e.IsChecked != r.PreviousCheckedState)
        {
            r.PreviousCheckedState = e.IsChecked;
            StartCheckAnimations(e, r, e.IsChecked);
        }

        if (e.IsPressed != r.PreviousPressedState)
        {
            r.PreviousPressedState = e.IsPressed;
            StartPressAnimation(e, r, e.IsPressed);
        }
    }

    public void SetEnabledColours(CheckBox e, CheckBoxRenderState r, bool enabled)
    {
        if (enabled)
        {
            var on = r.HandleOnColor;      // cache captured locals
            var off = ThemeManager.Current.OffColor;

            r.BorderPaint.Color = r.BackgroundOnColor;
            r.HandlePaint.Color = e.IsChecked ? on : off;
            r.BackgroundPaint.Color = e.IsChecked ? r.BackgroundOnColor : off.WithAlpha(80);
        }
        else
        {
            var disabled = ThemeManager.Current.DisabledColor;
            r.BorderPaint.Color = disabled;
            r.HandlePaint.Color = disabled;
            r.BackgroundPaint.Color = disabled.WithAlpha(80);
        }
    }

    public void StartCheckAnimations(CheckBox e, CheckBoxRenderState r, bool isChecked)
    {
        r.Animations["HandlePosition"].Start(
            isChecked ? ThemeManager.Current.Height / 2f : ThemeManager.Current.Width - ThemeManager.Current.Height / 2f,
            isChecked ? ThemeManager.Current.Width - ThemeManager.Current.Height / 2f : ThemeManager.Current.Height / 2f);

        r.Animations["HandleColor"].Start(
            isChecked ? ThemeManager.Current.OffColor : r.HandleOnColor,
            isChecked ? r.HandleOnColor : ThemeManager.Current.OffColor);

        r.Animations["BackgroundColor"].Start(
            isChecked ? ThemeManager.Current.OffColor : r.BackgroundOnColor,
            isChecked ? r.BackgroundOnColor : ThemeManager.Current.OffColor.WithAlpha(80));

        r.Animations["HandleSize"].Start(
            r.HandleSize,
            isChecked ? ThemeManager.Current.HandleOn : ThemeManager.Current.HandleOff);
    }

    public void StartPressAnimation(CheckBox e, CheckBoxRenderState r, bool pressed)
    {
        r.Animations["HandleSize"].Start(
            r.HandleSize,
            pressed ? ThemeManager.Current.HandlePressed
                    : (e.IsChecked ? ThemeManager.Current.HandleOn : ThemeManager.Current.HandleOff));
    }   

}