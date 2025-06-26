using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public interface IRenderer
{
    void Render(VisualElement element, SKCanvas canvas);
    void UpdateResources(VisualElement element);
    void DeleteResources(VisualElement element);
    RenderResource GetResource(VisualElement element);
}
