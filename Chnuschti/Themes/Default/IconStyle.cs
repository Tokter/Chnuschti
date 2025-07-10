using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class IconStyle : Style
{
    public IconStyle(DefaultTheme theme)
    {
        Renderer = new IconRenderer(theme);
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
        canvas.Save();
        
        // Calculate the translation needed for alignment
        float translateX = 0;
        float translateY = 0;
        
        float contentWidth = element.ContentBounds.Width;
        float contentHeight = element.ContentBounds.Height;
        float pathWidth = resource.Bounds.Width;
        float pathHeight = resource.Bounds.Height;
        
        // Apply horizontal alignment
        switch (element.HorizontalContentAlignment)
        {
            case HorizontalAlignment.Left:
                // Already at left, no translation needed
                break;
            case HorizontalAlignment.Center:
                translateX = (contentWidth - pathWidth) / 2;
                break;
            case HorizontalAlignment.Right:
                translateX = contentWidth - pathWidth;
                break;
            case HorizontalAlignment.Stretch:
                // For stretch, we'll scale the path
                float scaleX = contentWidth / pathWidth;
                canvas.Scale(scaleX, 1);
                break;
        }
        
        // Apply vertical alignment
        switch (element.VerticalContentAlignment)
        {
            case VerticalAlignment.Top:
                // Already at top, no translation needed
                break;
            case VerticalAlignment.Center:
                translateY = (contentHeight - pathHeight) / 2;
                break;
            case VerticalAlignment.Bottom:
                translateY = contentHeight - pathHeight;
                break;
            case VerticalAlignment.Stretch:
                // For stretch, we'll scale the path
                float scaleY = contentHeight / pathHeight;
                
                // If we're already scaling horizontally for stretch, adjust our coordinates
                if (element.HorizontalContentAlignment == HorizontalAlignment.Stretch)
                {
                    canvas.Scale(1, scaleY);
                }
                else
                {
                    canvas.Scale(1, scaleY);
                }
                break;
        }
        
        // Apply the translation
        canvas.Translate(translateX, translateY);
        
        // Draw the path
        canvas.DrawPath(resource.Path, resource.Paint);
        
        canvas.Restore();
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

        // Shift the path to the top left corner (0,0)
        // This makes alignment calculations simpler in the render method
        if (r.Bounds.Width > 0 && r.Bounds.Height > 0)
        {
            var offsetX = -r.Bounds.Left;
            var offsetY = -r.Bounds.Top;
            r.Path.Offset(offsetX, offsetY);
            r.Bounds.Offset(offsetX, offsetY);
        }
    }
}
