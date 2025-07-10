using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class ScreenStyle : Style
{
    public ScreenStyle(DefaultTheme theme)
    {
        Renderer = new ScreenRenderer(theme);
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
