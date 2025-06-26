using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public sealed record Setter(DependencyProperty Property, object? Value);

public class Style
{
    private readonly List<Setter> _setters = new();
    public Style? BasedOn { get; init; }

    public Style Add(DependencyProperty p, object? v)
    {
        _setters.Add(new Setter(p, v)); 
        return this;
    }

    public bool TryGetValue(DependencyProperty p, out object? v)
    {
        foreach (var s in _setters)
            if (s.Property == p) { v = s.Value; return true; }

        if (BasedOn is not null) return BasedOn.TryGetValue(p, out v);
        v = null; return false;
    }

    public IRenderer? Renderer { get; set; } = null;
}