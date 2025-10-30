using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class MenuStyle : Style
{
    public MenuStyle() { Renderer = new MenuRenderer(); }
}

public class MenuRenderState : RenderState
{
    public SKPaint MenuBackground { get; } = new();
    public override void OnInitialize() => InitPaint(MenuBackground, SKPaintStyle.Fill, 0, ThemeManager.Current.ShadowColor);
    protected override void Dispose(bool d) { if (d) MenuBackground.Dispose(); base.Dispose(d); }
}

public class MenuRenderer : Renderer<Menu, MenuRenderState>
{
    public override SKSize OnMeasure(Menu e, MenuRenderState r, SKSize avail)
    {
        e._panel.Measure(avail);
        return e._panel.DesiredSize;
    }

    public override void OnRender(SKCanvas c, Menu e, MenuRenderState r, double dt)
    {
        var b = e.ContentBounds;
        c.DrawRoundRect(b, ThemeManager.Current.Radius, ThemeManager.Current.Radius, r.MenuBackground);
        // children render themselves
    }
}
