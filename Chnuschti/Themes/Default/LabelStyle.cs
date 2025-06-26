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
        s.Add(Chnuschti.Button.ForegroundProperty, SKColors.Black);

        s.Renderer = new LabelRenderer();

        return s;
    }
}

public class LabelResource : FontRenderResource
{
}

public class LabelRenderer : Renderer<Label,LabelResource>
{
    public override void OnRender(SKCanvas canvas, Label element, LabelResource resource)
    {
        var txt = element.Text ?? string.Empty;

        // find baseline so that glyphs aren’t clipped on top
        var fm = resource.GetFontMetrics();
        float baseline = -fm.Ascent;   // ascent is negative

        // Draw at local origin (0,0) – contentBounds already excluded padding
        canvas.DrawText(txt, 0, baseline, resource.Font, resource.Paint);
    }

    public override void OnUpdateResources(Label element, LabelResource resource)
    {
        resource.Font.Size = element.FontSize;
        resource.Paint.Color = element.Foreground;
        resource.Font.Typeface = string.IsNullOrEmpty(element.FontFamily)
                          ? SKTypeface.Default
                          : SKTypeface.FromFamilyName(element.FontFamily);
        resource.Font.Embolden = element.Bold;
    }
}