﻿using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;


public class LabelLargeStyle : Style
{
    public LabelLargeStyle(DefaultTheme theme)
    {
        Add(Screen.FontSizeProperty, 16f);
        Renderer = new LabelRenderer(theme);
    }
}

public class LabelMediumStyle : Style
{
    public LabelMediumStyle(DefaultTheme theme)
    {
        Add(Screen.FontSizeProperty, 14f);
        Renderer = new LabelRenderer(theme);
    }
}

public class LabelSmallStyle : Style
{
    public LabelSmallStyle(DefaultTheme theme)
    {
        Add(Screen.FontSizeProperty, 12f);
        Renderer = new LabelRenderer(theme);
    }
}

public class LabelRenderState : RenderState
{
}

public class LabelRenderer : Renderer<Label, LabelRenderState>
{
    private readonly DefaultTheme _theme;

    public LabelRenderer(DefaultTheme theme) => _theme = theme;

    public override void OnRender(SKCanvas canvas, Label element, LabelRenderState resource, double deltaTime)
    {
        var txt = element.Text ?? string.Empty;
        if (string.IsNullOrEmpty(txt)) return;

        // Calculate text dimensions
        float textWidth = resource.Font.MeasureText(txt);
        resource.Font.GetFontMetrics(out var fm);
        float textHeight = fm.Descent - fm.Ascent;  // ascent is negative
        float baseline = -fm.Ascent;   // baseline offset (distance from top to baseline)

        // Calculate horizontal position based on alignment
        float x = 0;
        switch (element.HorizontalContentAlignment)
        {
            case HorizontalAlignment.Center:
                x = (element.ContentBounds.Width - textWidth) / 2;
                break;
            case HorizontalAlignment.Right:
                x = element.ContentBounds.Width - textWidth;
                break;
            case HorizontalAlignment.Left:
            case HorizontalAlignment.Stretch: // For text, we treat stretch as left-aligned
                x = 0;
                break;
        }

        // Calculate vertical position based on alignment
        float y = baseline; // Default is top-aligned (baseline offset from top)
        switch (element.VerticalContentAlignment)
        {
            case VerticalAlignment.Center:
                // Center the text vertically, accounting for baseline
                y = (element.ContentBounds.Height - textHeight) / 2 - fm.Ascent;
                break;
            case VerticalAlignment.Bottom:
                // Align bottom of text with bottom of content area
                y = element.ContentBounds.Height - fm.Descent;
                break;
            case VerticalAlignment.Top:
            case VerticalAlignment.Stretch: // For text, we treat stretch as top-aligned
                y = baseline;
                break;
        }

        // Draw text at calculated position
        canvas.DrawText(txt, (float)Math.Round(x), (float)Math.Round(y), resource.Font, resource.Paint);
    }

    public override SKSize OnMeasure(Label element, LabelRenderState resource, SKSize availableContent)
    {
        float w = resource.Font.MeasureText(element.Text);

        // height (baseline + descent)
        resource.Font.GetFontMetrics(out var fm);
        float h = fm.Descent - fm.Ascent;  // ascent is negative

        return new SKSize(w, h);
    }

    public override void OnUpdateRenderState(Label e, LabelRenderState r)
    {
        // ---------- upfront colour decisions ----------
        var textColor = e.Foreground != SKColor.Empty ? e.Foreground : _theme.TextColor;

        r.Font.Size = e.FontSize;

        if (!e.IsEnabled)
        {
            r.Paint.Color = textColor.WithAlpha(80);
        }
        else
        {
            r.Paint.Color = textColor;
        }

        r.Font.Typeface = string.IsNullOrEmpty(e.FontFamily)
                              ? SKTypeface.Default
                              : SKTypeface.FromFamilyName(e.FontFamily, new SKFontStyle(400, 32, SKFontStyleSlant.Upright));
        r.Font.Embolden = e.Bold;
        r.Font.Subpixel = true; // enable subpixel rendering for better text quality
        r.Font.Edging = SKFontEdging.SubpixelAntialias;
        r.Paint.IsAntialias = true; // enable antialiasing for better text rendering
    }
}