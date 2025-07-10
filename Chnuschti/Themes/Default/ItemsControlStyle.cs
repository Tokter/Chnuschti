using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Themes.Default;

public static class ItemsControlStyle
{
    public static Style CreateStyle()
    {
        var s = new Style();
        s.Renderer = new ItemsControlRenderer();
        return s;
    }
}

public class ItemsControlResource : RenderState
{
}

public class ItemsControlRenderer : Renderer<ItemsControl, ItemsControlResource>
{
    public override SKSize OnMeasure(ItemsControl element, ItemsControlResource resource, SKSize availableContent)
    {
        if (element.Children.FirstOrDefault() is VisualElement child)
        {
            child.Measure(availableContent);
            return child.DesiredSize;
        }

        return SKSize.Empty;
    }
}