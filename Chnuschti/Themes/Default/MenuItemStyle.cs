using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Themes.Default;

public class MenuItemStyle : Style
{
    public MenuItemStyle() { Renderer = new MenuItemRenderer(); }
}

public class MenuItemRS : RenderState
{
    public SKPaint Hover { get; } = new();
    public SKPaint Press { get; } = new();
    public override void OnInitialize()
    {
        InitPaint(Hover, SKPaintStyle.Fill, 0, ThemeManager.Current.HoverColor);
        InitPaint(Press, SKPaintStyle.Fill, 0, ThemeManager.Current.AccentDark);
    }
    protected override void Dispose(bool d)
    { if (d) { Hover.Dispose(); Press.Dispose(); } base.Dispose(d); }
}

public class MenuItemRenderer : Renderer<MenuItem, MenuItemRS>
{
    public override SKSize OnMeasure(MenuItem e, MenuItemRS r, SKSize avail)
    {
        // Defer to children: icon + text + shortcut + chevron; add padding
        foreach (var ch in e.Children)
        {
            // Skip overlay submenu host (Menu) for intrinsic width
            if (ch is Menu) continue;
            ch.Measure(new SKSize(float.PositiveInfinity, float.PositiveInfinity));
        }
        var w = 0f; var h = 0f;
        foreach (var ch in e.Children)
        {
            if (ch is Menu) continue;                 // overlay: not part of base width
            if (!ch.IsVisible) continue;              // invisible (e.g. chevron in main menu)

            var s = ch.DesiredSize;
            w += s.Width;
            h = Math.Max(h, s.Height);
        }
        //w += e.Padding.Horizontal;
        //h += e.Padding.Vertical;
        //return new SKSize(w, Math.Max(h, ThemeManager.Current.Height * 1.2f));
        return new SKSize(w, h);
    }

    public override void OnRender(SKCanvas c, MenuItem e, MenuItemRS r, double dt)
    {
        if (!e.IsEnabled) return;
        var b = e.ContentBounds;
        if (e.IsPressed) c.DrawRect(b, r.Press);
        else if (e.IsMouseOver) c.DrawRect(b, r.Hover);
    }
}
