using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class Renderer<VisEl,Res> : IRenderer 
    where VisEl : VisualElement 
    where Res : RenderResource
{
    private Dictionary<long, RenderResource> _resources = new();

    public void DeleteResources(VisualElement element)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            resource.Dispose();
            _resources.Remove(element.Id);
        }
    }

    public void Render(VisualElement element, SKCanvas canvas)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            // Use the resource to render the element
            OnRender(canvas, (VisEl)element, (Res)resource);
        }
    }

    public virtual void OnRender(SKCanvas canvas, VisEl element, Res resource)
    {
        // Implement rendering logic here
        throw new NotImplementedException();
    }

    public void UpdateResources(VisualElement element)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            OnUpdateResources((VisEl)element, (Res)resource);
        }
        else
        {
            // Create a new resource if it doesn't exist
            var newResource = Activator.CreateInstance<Res>();
            _resources[element.Id] = newResource;
            OnUpdateResources((VisEl)element, newResource);
        }
    }

    public virtual void OnUpdateResources(VisEl element, Res resource)
    {
        // Implement resource update logic here
        throw new NotImplementedException();
    }

    public RenderResource GetResource(VisualElement element)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            return resource;
        }
        else
        {
            throw new InvalidOperationException("Resource not found for the given element.");
        }
    }
}

public abstract class RenderResource : IDisposable
{
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public interface IHaveFont
{
    float MeasureText(string test);
    SKRect GetTextBounds(string test);
    SKFontMetrics GetFontMetrics();
}

public class FontRenderResource : RenderResource, IHaveFont
{
    public SKPaint Paint { get; set; } = new SKPaint();
    public SKFont Font { get; set; } = new SKFont();

    public SKFontMetrics GetFontMetrics()
    {
        Font.GetFontMetrics(out var fm);
        return fm;
    }

    public SKRect GetTextBounds(string test)
    {
        Font.MeasureText(test, out var bounds);
        return bounds;
    }

    public float MeasureText(string test)
    {
        return Font.MeasureText(test, Paint);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Paint.Dispose();
            Font.Dispose();
        }
        base.Dispose(disposing);
    }
}