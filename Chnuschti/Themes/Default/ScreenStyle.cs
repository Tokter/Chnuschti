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
    public ScreenStyle()
    {
        Renderer = new ScreenRenderer();
    }
}

public class ScreenResource : RenderState
{
}

public class ScreenRenderer : Renderer<Screen, ScreenResource>
{
    public override void OnRender(SKCanvas canvas, Screen element, ScreenResource resource, double deltaTime)
    {
        canvas.Clear(ThemeManager.Current.BackgroundColor);
        element.Content?.Render(canvas, deltaTime);
    }
}
