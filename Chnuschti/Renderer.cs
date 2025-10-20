using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class Renderer<VisEl,Res> : IRenderer 
    where VisEl : VisualElement 
    where Res : RenderState
{
    private Dictionary<long, RenderState> _resources = new();

    public void DeleteResources(VisualElement element)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            resource.Dispose();
            _resources.Remove(element.Id);
        }
    }

    private double _animationTime = 0;
    private const double animationTimeStep = 1.0 / 60.0;

    public void Render(VisualElement element, SKCanvas canvas, double deltaTime)
    {
        if (_resources.TryGetValue(element.Id, out var resource))
        {
            _animationTime += deltaTime;
            if (_animationTime >= animationTimeStep)
            {
                resource.Animations.UpdateAll(_animationTime);
                _animationTime = 0;
            }

            // Use the resource to render the element
            OnRender(canvas, (VisEl)element, (Res)resource, deltaTime);
        }
    }

    public virtual void OnInitialize(VisEl element, Res resource)
    {
    }

    public virtual void OnRender(SKCanvas canvas, VisEl element, Res resource, double deltaTime)
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
            OnUpdateRenderState((VisEl)element, (Res)resource);
        }
        else
        {
            // Create a new render state if it doesn't exist
            var newRenderState = Activator.CreateInstance<Res>();
            _resources[element.Id] = newRenderState;

            OnInitialize((VisEl)element, (Res)newRenderState);
            newRenderState.OnInitialize();

            OnUpdateRenderState((VisEl)element, newRenderState);
        }
    }

    public virtual void OnUpdateRenderState(VisEl element, Res resource)
    {
    }

    public RenderState GetResource(VisualElement element)
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

public abstract class RenderState : IDisposable
{
    private bool _disposedValue;

    public AnimationGroup Animations { get; } = new();

    public SKPaint Paint { get; set; } = new SKPaint();
    public SKFont Font { get; set; } = new SKFont();

    public virtual void OnInitialize()
    {
    }

    protected void InitPaint(SKPaint p, SKPaintStyle style, float stroke = 0, SKColor? color = null)
    {
        p.Style = style;
        p.IsAntialias = true;
        if (stroke > 0) p.StrokeWidth = stroke;
        if (color.HasValue) p.Color = color.Value;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Paint.Dispose();
                Font.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}