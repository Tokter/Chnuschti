using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public static class CheckboxStyle
{
    public static Style CreateStyle(DefaultTheme theme)
    {
        var s = new Style();
        s.Add(CheckBox.ForegroundProperty, SKColors.White);

        s.Renderer = new CheckBoxRenderer(theme);

        return s;
    }
}

internal sealed class CheckBoxRenderState : RenderState
{
    public bool PreviousCheckedState = false;

    public SKPaint BorderPaint { get; } = new();
    public SKPaint BoxPaint { get; } = new();
    public SKPaint TickPaint { get; } = new();

    public CheckBoxRenderState()
    {
        Animations.Add(new AnimationColor("ColorFader", TimeSpan.FromSeconds(0.5), (c) => TickPaint.Color = c));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BorderPaint.Dispose();
            BoxPaint.Dispose();
            TickPaint.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal sealed class CheckBoxRenderer : Renderer<CheckBox, CheckBoxRenderState>
{
    private readonly DefaultTheme _theme;
    private const float BOX_SIZE = 16f;
    private const float SPACING = 0f;

    public CheckBoxRenderer(DefaultTheme theme) => _theme = theme;

    // 1) measure -----------------------------------------------------
    public override SKSize OnMeasure(CheckBox elem, CheckBoxRenderState res, SKSize avail)
    {
        // Reserve box + spacing, remainder is for content
        var remain = new SKSize(
            Math.Max(0, avail.Width - BOX_SIZE - SPACING),
            avail.Height);

        var childNeed = SKSize.Empty;
        if (elem.Content != null)
        {
            elem.Content.Measure(remain);
            childNeed = elem.Content.DesiredSize;
        }

        var w = BOX_SIZE + (elem.Content != null ? SPACING + childNeed.Width : 0);
        var h = Math.Max(BOX_SIZE, childNeed.Height);

        return new SKSize(w, h);
    }

    // 2) render ------------------------------------------------------
    public override void OnRender(SKCanvas c, CheckBox e, CheckBoxRenderState r, double deltaTime)
    {
        // Position box vertically centred
        var y = (e.ContentBounds.Height - BOX_SIZE) / 2f;

        // Draw box
        c.DrawRoundRect(0, y, BOX_SIZE, BOX_SIZE, 3, 3, r.BoxPaint);
        c.DrawRoundRect(0, y, BOX_SIZE, BOX_SIZE, 3, 3, r.BorderPaint);

        // Tick
        if (e.IsChecked || r.Animations["ColorFader"].IsRunning)
        {
            var path = new SKPath();
            path.MoveTo(3, y + BOX_SIZE * 0.55f);
            path.LineTo(BOX_SIZE * 0.4f, y + BOX_SIZE - 3);
            path.LineTo(BOX_SIZE - 3, y + 3);
            c.DrawPath(path, r.TickPaint);
        }
    }

    // 3) update / theme reaction ------------------------------------
    public override void OnUpdateRenderState(CheckBox e, CheckBoxRenderState r)
    {
        r.BorderPaint.Color = e.Foreground;
        r.BorderPaint.IsAntialias = true;
        r.BorderPaint.Style = SKPaintStyle.Stroke;
        r.BorderPaint.StrokeWidth = 1.1f;

        r.BoxPaint.Color = e.IsEnabled ? _theme.BackgroundColor : _theme.BackgroundColor.WithAlpha(80);
        r.BoxPaint.Style = SKPaintStyle.Fill;
        r.BoxPaint.IsAntialias = true;

        r.TickPaint.Color = e.Foreground;
        r.TickPaint.Style = SKPaintStyle.Stroke;
        r.TickPaint.IsAntialias = true;
        r.TickPaint.StrokeWidth = 2.2f;
        
        if (e.IsChecked != r.PreviousCheckedState)
        {
            r.PreviousCheckedState = e.IsChecked;
            if (e.IsChecked)
            {
                r.Animations["ColorFader"].Start(r.BoxPaint.Color, e.Foreground);
            }
            else
            {
                r.Animations["ColorFader"].Start(e.Foreground, r.BoxPaint.Color);
            }
        }
    }
}