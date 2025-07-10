using Chnuschti.Controls;
using System;

namespace Chnuschti;

/// <summary>
/// Defines a template that specifies the visual appearance of data objects.
/// </summary>
public abstract class DataTemplate : IViewLocator
{
    /// <summary>
    /// Creates a control for the specified data item.
    /// </summary>
    public abstract Control? Build(object? data);

    /// <summary>
    /// Determines if this template can be used for the specified data item.
    /// </summary>
    public abstract bool Match(object? data);
}

/// <summary>
/// A strongly typed data template for creating visuals for specific data types.
/// </summary>
/// <typeparam name="T">The type of data object this template handles.</typeparam>
public class DataTemplate<T> : DataTemplate
{
    private readonly Func<T, Control> _factory;

    /// <summary>
    /// Creates a new data template with the specified factory function.
    /// </summary>
    /// <param name="factory">Function that creates a control for a data item.</param>
    public DataTemplate(Func<T, Control> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Determines if this template can handle the specified data item.
    /// </summary>
    public override bool Match(object? data) => data is T;

    /// <summary>
    /// Creates a control for the specified data item.
    /// </summary>
    public override Control? Build(object? data)
    {
        if (data is T typedData)
        {
            return _factory(typedData);
        }
        return null;
    }
}