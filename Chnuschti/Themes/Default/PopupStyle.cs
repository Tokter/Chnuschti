using Chnuschti.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Themes.Default
{
    internal class PopupStyle : Style
    {
        public PopupStyle()
        {
            Renderer = new PopupRenderer();
        }
    }

    public class PopupResourceResource : RenderState
    {
    }

    public class PopupRenderer : Renderer<Popup, PopupResourceResource>
    {
        public override SKSize OnMeasure(Popup element, PopupResourceResource resource, SKSize availableContent)
        {
            if (element.Content == null) return SKSize.Empty;

            // Let child measure first
            element.Content.Measure(availableContent);
            return element.Content.DesiredSize;
        }
    }
}