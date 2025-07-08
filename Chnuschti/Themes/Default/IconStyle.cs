using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public static class IconStyle
{
    public static Style CreateStyle(DefaultTheme theme)
    {
        var s = new Style();
        s.Renderer = new IconRenderer(theme);
        return s;
    }
}

public class IconRenderState : RenderState
{
    public SKPath Path { get; set; } = new();
    public SKRect Bounds { get; set; } = SKRect.Empty;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Path.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class IconRenderer : Renderer<Icon, IconRenderState>
{
    private readonly DefaultTheme _theme;

    public IconRenderer(DefaultTheme theme) => _theme = theme;

    public override void OnRender(SKCanvas canvas, Icon element, IconRenderState resource, double deltaTime)
    {
        canvas.DrawPath(resource.Path, resource.Paint);
    }

    public override SKSize OnMeasure(Icon e, IconRenderState r, SKSize availableContent)
    {
        var s = new SKSize(float.IsNaN(e.Width) ? r.Bounds.Width : e.Width, float.IsNaN(e.Height) ? r.Bounds.Height : e.Height);
        return s;
    }

    public override void OnUpdateRenderState(Icon e, IconRenderState r)
    {
        // ---------- upfront colour decisions ----------
        var iconColor = e.Foreground != SKColor.Empty ? e.Foreground : _theme.TextColor;
        r.Paint.Color = iconColor;
        r.Paint.IsAntialias = true; // enable antialiasing for better text rendering
        r.Path = SKPath.ParseSvgPathData(Icons.IconPaths[e.IconKind]);
        r.Bounds = r.Path.Bounds;

        //Shift the path to the top left corner
        if (r.Bounds.Width > 0 && r.Bounds.Height > 0)
        {
            var offsetX = -r.Bounds.Left;
            var offsetY = -r.Bounds.Top;
            r.Path.Offset(offsetX, offsetY);
            r.Bounds.Offset(offsetX, offsetY);
        }
    }
}
