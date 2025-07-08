using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public static class ScreenStyle
{
    public static Style CreateStyle(DefaultTheme theme)
    {
        var s = new Style();
        //s.Add(Screen.ForegroundProperty, SKColors.White);
        //s.Add(Chnuschti.Button.FontFamilyProperty, "Arial");
        s.Renderer = new ScreenRenderer(theme);
        return s;
    }
}

public class ScreenResource : RenderState
{
}

public class ScreenRenderer : Renderer<Screen, ScreenResource>
{
    private DefaultTheme _theme;

    public ScreenRenderer(DefaultTheme theme)
    {
        _theme = theme;
    }

    public override void OnRender(SKCanvas canvas, Screen element, ScreenResource resource, double deltaTime)
    {
        canvas.Clear(_theme.BackgroundColor);
        element.Content?.Render(canvas, deltaTime);
    }
}
