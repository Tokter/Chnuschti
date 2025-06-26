using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

// Strongly typed helper for attached properties
public sealed record Attached<T>(DependencyProperty Property)
{
    public static Attached<T> Register(string name, Type owner, T? defaultValue = default!)
        => new(DependencyProperty.RegisterAttached(name, typeof(T), owner, new PropertyMetadata(defaultValue)));
}
