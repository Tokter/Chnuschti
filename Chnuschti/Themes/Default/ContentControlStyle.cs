using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default;

public class ContentControlStyle : Style
{
    public ContentControlStyle()
    {
        Renderer = new ContentControlRenderer();
    }
}

public class ContentControlResource : RenderState
{
}

public class ContentControlRenderer : Renderer<ContentControl, ContentControlResource>
{
    public override SKSize OnMeasure(ContentControl element, ContentControlResource resource, SKSize availableContent)
    {
        if (element.Content == null) return SKSize.Empty;

        // Let child measure first
        element.Content.Measure(availableContent);
        return element.Content.DesiredSize;
    }

    //public override void OnRender(SKCanvas canvas, ContentControl element, ContentControlResource resource)
    //{
    //    element.Content?.Render(canvas);   // child draws in its own local coords
    //}
}