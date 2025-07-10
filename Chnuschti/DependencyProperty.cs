using System;
using System.Collections.Concurrent;

namespace Chnuschti;

public delegate void PropertyChangedCallback(
    DependencyObject d,
    DependencyProperty property,
    object? oldValue,
    object? newValue);

public sealed class PropertyMetadata
{
    public object? DefaultValue { get; init; }
    public PropertyChangedCallback? PropertyChangedCallback { get; init; }
    public bool Inherits { get; init; }

    public PropertyMetadata(object? defaultValue = null, PropertyChangedCallback? cb = null, bool inherits = false)
    {
        DefaultValue = defaultValue;
        PropertyChangedCallback = cb;
        Inherits = inherits;
    }
}

public sealed class DependencyProperty
{
    private static readonly ConcurrentDictionary<(Type owner, string name), DependencyProperty> _registry = new();

    private DependencyProperty(string name, Type propertyType, Type ownerType, PropertyMetadata baseMetadata, bool attached)
    {
        Name = name;
        PropertyType = propertyType;
        OwnerType = ownerType;
        PropertyChangedCallback = baseMetadata.PropertyChangedCallback;
        IsAttached = attached;
        Inherits = baseMetadata.Inherits;
        DefaultValue = baseMetadata.DefaultValue ?? GetDefault(propertyType);
    }

    public string Name { get; }
    public Type PropertyType { get; }
    public Type OwnerType { get; }
    public object? DefaultValue { get; }
    public bool Inherits { get; }
    public bool IsAttached { get; }
    public PropertyChangedCallback? PropertyChangedCallback { get; }

    // -------- Registration helpers --------

    public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata? metadata = null)
        => RegisterCore(name, propertyType, ownerType, metadata, attached: false);

    public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata? metadata = null)
        => RegisterCore(name, propertyType, ownerType, metadata, attached: true);

    private static DependencyProperty RegisterCore(string name, Type pType, Type oType, PropertyMetadata? md, bool attached)
    {
        var key = (oType, name);
        if (!_registry.TryAdd(key,
                new DependencyProperty(
                    name, pType, oType,
                    md ?? new PropertyMetadata(GetDefault(pType)),
                    attached)))
            throw new InvalidOperationException(
                $"Property '{name}' already registered on {oType.Name}.");

        return _registry[key];
    }

    private static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
}