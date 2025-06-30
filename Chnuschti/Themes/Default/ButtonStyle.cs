using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public static class ButtonStyle
{
    public static Style CreateStyle(DefaultTheme theme)
    {
        var s = new Style();
        s.Add(Chnuschti.Button.BackgroundProperty, new SKColor(128, 128, 128, 255));
        s.Add(Chnuschti.Button.ForegroundProperty, SKColors.White);

        s.Renderer = new ButtonRenderer(theme);

        return s;
    }
}

public class ButtonResource : RenderResource
{
    public SKPaint BorderPaint { get; set; } = new SKPaint();
    public SKPaint Paint { get; set; } = new SKPaint();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BorderPaint.Dispose();
            Paint.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class ButtonRenderer : Renderer<Button, ButtonResource>
{
    private DefaultTheme _theme;

    public ButtonRenderer(DefaultTheme theme)
    {
        _theme = theme;
    }

    public override SKSize OnMeasure(Button element, ButtonResource resource, SKSize availableContent)
    {
        if (element.Content == null) return SKSize.Empty;

        // Let child measure first
        element.Content.Measure(availableContent);
        return element.Content.DesiredSize;
    }

    public override void OnRender(SKCanvas canvas, Button element, ButtonResource resource)
    {
        canvas.DrawRoundRect(0, 0, element.ContentBounds.Width, element.ContentBounds.Height, 5, 5, resource.Paint);
        canvas.DrawRoundRect(0, 0, element.ContentBounds.Width, element.ContentBounds.Height, 5, 5, resource.BorderPaint);

        // let ContentControl draw child
        element.Content?.Render(canvas);   // child draws in its own local coords
    }

    public override void OnUpdateResources(Button element, ButtonResource resource)
    {
        resource.BorderPaint.Color = _theme.BorderColor;
        resource.BorderPaint.Style = SKPaintStyle.Stroke;
        resource.BorderPaint.IsAntialias = true;
        resource.BorderPaint.StrokeWidth = 1.1f;

        resource.Paint.Style = SKPaintStyle.Fill;

        if (!element.IsEnabled)
        {
            resource.Paint.Shader = null;
            resource.Paint.Color = element.Background.WithAlpha(100);
        }
        else if (element.IsPressed)
        {
            resource.Paint.Shader = SKShader.CreateLinearGradient(
                             new SKPoint(0, 0),
                             new SKPoint(0, element.ContentBounds.Height),
                             new SKColor[] { Brighten(element.Background, 0), Darken(element.Background, 40) },
                             new float[] { 0, 1 },
                             SKShaderTileMode.Repeat);
        }
        else if (element.IsMouseOver)
        {
            resource.Paint.Shader = SKShader.CreateLinearGradient(
                             new SKPoint(0, 0),
                             new SKPoint(0, element.ContentBounds.Height),
                             new SKColor[] { Brighten(element.Background,60), Brighten(element.Background, 20), Darken(element.Background, 20), Darken(element.Background, 60) },
                             new float[] { 0, 0.1f, 0.9f, 1 },
                             SKShaderTileMode.Repeat);
        }
        else
        {
            resource.Paint.Shader = SKShader.CreateLinearGradient(
                             new SKPoint(0, 0),
                             new SKPoint(0, element.ContentBounds.Height),
                             new SKColor[] { Brighten(element.Background, 20), Darken(element.Background, 20) },
                             new float[] { 0, 1 },
                             SKShaderTileMode.Repeat);
        }

        // background (darker when pressed)
        //var bg = element.Background.WithAlpha(100);

        //if (!element.IsEnabled) bg = bg.WithAlpha(100);
        //else if (element.IsPressed) bg = bg.WithAlpha(200);
        //else if (element.IsMouseOver) bg = bg.WithAlpha(150);

        //resource.Paint.Color = bg;
    }
}