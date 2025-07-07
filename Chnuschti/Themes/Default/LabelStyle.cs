using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public static class LabelStyle
{
    public static Style CreateStyle()
    {
        var s = new Style();
        //s.Add(Chnuschti.Button.BackgroundProperty, SKColors.DodgerBlue);
        //s.Add(Chnuschti.Button.PaddingProperty, new Thickness(12, 6, 12, 6));
        //s.Add(Chnuschti.Button.ForegroundProperty, SKColors.Black);

        s.Renderer = new LabelRenderer();

        return s;
    }
}

public class LabelRenderState : RenderState
{
}

public class LabelRenderer : Renderer<Label,LabelRenderState>
{
    public override void OnRender(SKCanvas canvas, Label element, LabelRenderState resource, double deltaTime)
    {
        var txt = element.Text ?? string.Empty;

        // find baseline so that glyphs aren’t clipped on top
        resource.Font.GetFontMetrics(out var fm);
        float baseline = -fm.Ascent;   // ascent is negative

        // Draw at local origin (0,0) – contentBounds already excluded padding
        canvas.DrawText(txt, 0, baseline, resource.Font, resource.Paint);
    }

    public override SKSize OnMeasure(Label element, LabelRenderState resource, SKSize availableContent)
    {
        float w = resource.Font.MeasureText(element.Text);

        // height (baseline + descent)
        resource.Font.GetFontMetrics(out var fm);
        float h = fm.Descent - fm.Ascent;  // ascent is negative

        return new SKSize(w, h);
    }

    public override void OnUpdateRenderState(Label element, LabelRenderState resource)
    {
        resource.Font.Size = element.FontSize;

        if (!element.IsEnabled)
        {
            resource.Paint.Color = element.Foreground.WithAlpha(80);
        }
        else
        {
            resource.Paint.Color = element.Foreground;
        }

        resource.Font.Typeface = string.IsNullOrEmpty(element.FontFamily)
                              ? SKTypeface.Default
                              : SKTypeface.FromFamilyName(element.FontFamily);
        resource.Font.Embolden = element.Bold;
        resource.Font.Subpixel = true; // enable subpixel rendering for better text quality
        resource.Paint.IsAntialias = true; // enable antialiasing for better text rendering
    }
}