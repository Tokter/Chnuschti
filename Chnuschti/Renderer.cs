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
    }

    public SKSize Measure(VisualElement element, SKSize availableContent)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            // Use the resource to measure the element
            return OnMeasure((VisEl)element, (Res)resource, availableContent);
        }
        else return SKSize.Empty;
    }

    public virtual SKSize OnMeasure(VisEl element, Res resource, SKSize availableContent)
    {
        var maxW = 0f;
        var maxH = 0f;
        foreach (var c in element.Children)
        {
            if (!c.IsVisible) continue; // Skip invisible children

            c.Measure(availableContent);
            var d = c.DesiredSize;
            maxW = Math.Max(maxW, d.Width);
            maxH = Math.Max(maxH, d.Height);
        }
        return new SKSize(maxW, maxH);
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


    protected SKColor Brighten(SKColor color, byte amount)
    {
        return new SKColor(
            (byte)Math.Min(color.Red + amount, 255),
            (byte)Math.Min(color.Green + amount, 255),
            (byte)Math.Min(color.Blue + amount, 255),
            color.Alpha);
    }

    protected SKColor Darken(SKColor color, byte amount)
    {
        return new SKColor(
            (byte)Math.Max(color.Red - amount, 0),
            (byte)Math.Max(color.Green - amount, 0),
            (byte)Math.Max(color.Blue - amount, 0),
            color.Alpha);
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

public class FontRenderResource : RenderResource
{
    public SKPaint Paint { get; set; } = new SKPaint();
    public SKFont Font { get; set; } = new SKFont();

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