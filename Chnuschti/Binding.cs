using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Chnuschti;

/// <summary>
/// Every binding can be read and notifies when its value changes.
/// </summary>
public interface IReadOnlyBinding
{
    object? Value { get; }
    event Action? ValueChanged;
}

/// <summary>
/// A binding that can also be written to (TwoWay).
/// </summary>
public interface ITwoWayBinding : IReadOnlyBinding
{
    void Write(object? value);   // throws if conversion fails
}

/// <summary>
/// Binding  – OneWay / TwoWay / OneTime between
/// a source INotifyPropertyChanged and a target DependencyObject
/// </summary>
public enum BindingMode
{
    /// <summary>
    /// Specifies a one-way communication mode where messages are sent without expecting a response.
    /// </summary>
    /// <remarks>This mode is typically used in scenarios where the sender does not require confirmation or a
    /// reply from the receiver. It is suitable for fire-and-forget operations or broadcasting messages.</remarks>
    OneWay,

    /// <summary>
    /// Represents a bidirectional communication mode or interaction pattern.
    /// </summary>
    /// <remarks>This value is typically used to indicate that data or messages can flow in both directions
    /// between two entities, such as a client and a server, or two components in a system.</remarks>
    TwoWay,

    /// <summary>
    /// Represents a one-time operation or event that occurs only once.
    /// </summary>
    /// <remarks>This type is typically used to model actions or events that are designed to execute a single
    /// time. Once the operation is completed, it cannot be repeated.</remarks>
    OneTime 
}

/// <summary>
/// Represents a binding between a source property and a target property, enabling data synchronization.
/// </summary>
/// <remarks>A <see cref="Binding{TSource, TProp}"/> can operate in one of three modes: <see
/// cref="BindingMode.OneWay"/>,  <see cref="BindingMode.TwoWay"/>, or <see cref="BindingMode.OneTime"/>. The mode
/// determines whether changes  propagate from the source to the target, or bidirectionally.  Use the static factory
/// methods <see cref="OneWay(TSource, Expression{Func{TSource, TProp}})"/>,  <see cref="TwoWay(TSource,
/// Expression{Func{TSource, TProp}})"/>, and <see cref="OneTime(TSource, Expression{Func{TSource, TProp}})"/>  to
/// create bindings with the desired mode.</remarks>
/// <typeparam name="TSource">The type of the source object, which must implement <see cref="INotifyPropertyChanged"/>.</typeparam>
/// <typeparam name="TProp">The type of the property being bound.</typeparam>
public sealed class Binding<TSource, TProp> : ITwoWayBinding where TSource : INotifyPropertyChanged
{
    private readonly Func<TSource, TProp> _getter;
    private readonly Action<TSource, TProp>? _setter;
    private readonly string _propName;

    /// <summary>Gets the source object associated with the current operation or context.</summary>
    public TSource Source { get; }

    /// <summary>Gets the binding mode that determines how data flows between the source and target of the binding.</summary>
    public BindingMode Mode { get; }

    /// <summary>Current value of the source property.</summary>
    public TProp? Value => _getter(Source);

    /// <summary>Assigns value back to the source (TwoWay mode only).</summary>
    public void SetValue(TProp v)
    {
        if (_setter is null)
            throw new InvalidOperationException("Binding is not TwoWay.");
        _setter(Source, v);
    }

    #region ITwoWayBinding implementation

    object? IReadOnlyBinding.Value => _getter(Source);

    /// <summary>Raised when the source property fires PropertyChanged.</summary>
    public event Action? ValueChanged;

    public void Write(object? v)
    {
        if (_setter is null)
            throw new InvalidOperationException("Binding is OneWay/OneTime.");
        _setter(Source, (TProp)v!);
    }

    #endregion


    // ----------------------------------------------------------------------
    //  Construction helpers
    // ----------------------------------------------------------------------
    public static Binding<TSource, TProp> OneWay(TSource source, Expression<Func<TSource, TProp>> property) => new(source, property, BindingMode.OneWay);
    public static Binding<TSource, TProp> TwoWay(TSource source, Expression<Func<TSource, TProp>> property) => new(source, property, BindingMode.TwoWay);
    public static Binding<TSource, TProp> OneTime(TSource source, Expression<Func<TSource, TProp>> property) => new(source, property, BindingMode.OneTime);

    /// <summary>
    /// Initializes a new instance of the <see cref="Binding{TSource, TProp}"/> class, creating a binding between a source
    /// object and a property.
    /// </summary>
    /// <remarks>This constructor establishes a binding between the specified property of the source object and the
    /// binding instance. - If the binding mode is <see cref="BindingMode.TwoWay"/>, a setter is compiled to allow updates
    /// to the source property. - The source object must implement <see cref="INotifyPropertyChanged"/> to enable property
    /// change notifications.</remarks>
    /// <param name="source">The source object that implements <see cref="INotifyPropertyChanged"/>. Cannot be <see langword="null"/>.</param>
    /// <param name="selector">An expression that specifies the property of the source object to bind to. Must be a property access expression.</param>
    /// <param name="mode">The binding mode, indicating whether the binding is one-way or two-way.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="selector"/> is not a property access expression.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="mode"/> is <see cref="BindingMode.TwoWay"/> and the selected property is read-only.</exception>
    private Binding(TSource source, Expression<Func<TSource, TProp>> selector, BindingMode mode)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Mode = mode;

        // ------------------------------------------------------------------
        // Compile strongly-typed getter  (no reflection at run-time)
        // ------------------------------------------------------------------
        _getter = selector.Compile();

        // ------------------------------------------------------------------
        // Get the property name from the expression – one-off light reflection
        // ------------------------------------------------------------------
        if (selector.Body is MemberExpression me)
            _propName = me.Member.Name;
        else
            throw new ArgumentException("Selector must be a property access");

        // ------------------------------------------------------------------
        // Compile setter only if needed
        // ------------------------------------------------------------------
        if (mode == BindingMode.TwoWay)
        {
            if (me.Member is not System.Reflection.PropertyInfo pi || !pi.CanWrite)
                throw new InvalidOperationException("Property is read-only.");

            // Build lambda: (TSource s, TProp v) => s.Property = v;
            var src = Expression.Parameter(typeof(TSource), "s");
            var val = Expression.Parameter(typeof(TProp), "v");
            var body = Expression.Assign(Expression.Property(src, pi), val);
            _setter = Expression.Lambda<Action<TSource, TProp>>(body, src, val).Compile();
        }

        // ------------------------------------------------------------------
        // Subscribe to PropertyChanged
        // ------------------------------------------------------------------
        Source.PropertyChanged += SourceOnPropertyChanged;
    }

    private void SourceOnPropertyChanged(object? s, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propName || string.IsNullOrEmpty(e.PropertyName))
        {
            // Invoke the event directly instead of casting to IReadOnlyBinding
            ValueChanged?.Invoke();
        }
    }
}

/// <summary>
/// Represents a binding that computes a value dynamically based on dependencies (zero or more INotifyPropertyChanged sources)
/// and raises notifications when the value changes.
/// </summary>
/// <remarks>The <see cref="LiveBinding"/> class allows you to define a computed value that is updated whenever
/// one of its dependencies changes. Dependencies are specified as pairs of <see cref="INotifyPropertyChanged"/> sources
/// and property names. The computed value is determined by the provided delegate, which is invoked on demand.</remarks>
public sealed class LiveBinding : IReadOnlyBinding
{
    /// <summary>Compute the value on demand.</summary>
    public object? Value => _compute();

    /// <summary>Raised when ANY of the dependencies fires PropertyChanged.</summary>
    public event Action? ValueChanged;

    // ----------------------------------------------------------------------
    //  Construction helper
    // ----------------------------------------------------------------------
    public LiveBinding(
        Func<object?> compute,
        params (INotifyPropertyChanged source, string propertyName)[] deps)
    {
        _compute = compute ?? throw new ArgumentNullException(nameof(compute));

        foreach (var (src, propName) in deps)
        {
            if (src == null) continue;
            src.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propName || string.IsNullOrEmpty(e.PropertyName))
                    ValueChanged?.Invoke();
            };
        }
    }

    private readonly Func<object?> _compute;
}

/// <summary>
/// Represents a binding between a <see cref="DataElement"/> and a property of its <see cref="DataContext"/>.
/// </summary>
/// <remarks>This class facilitates two-way or one-way binding between a <see cref="DataElement"/> and a
/// property of its <see cref="DataContext"/>. The binding is automatically updated when the <see
/// cref="DataElement.DataContext"/> changes.</remarks>
/// <typeparam name="TSource">The type of the <see cref="DataContext"/> object, which must implement <see cref="INotifyPropertyChanged"/>.</typeparam>
/// <typeparam name="TProp">The type of the property being bound.</typeparam>
internal sealed class DataContextBinding<TSource, TProp> : ITwoWayBinding
    where TSource : class, INotifyPropertyChanged
{
    private readonly DataElement _fe;
    private readonly Expression<Func<TSource, TProp>> _selector;
    private readonly BindingMode _mode;

    private Binding<TSource, TProp>? _inner;   // the real binding we proxy

    public DataContextBinding(DataElement fe, Expression<Func<TSource, TProp>> selector, BindingMode mode)
    {
        _fe = fe;
        _selector = selector;
        _mode = mode;

        fe.DataContextChanged += (_, __) => Rewire();
        Rewire();
    }

    #region IReadOnlyBinding / ITwoWayBinding implementation
    public object? Value => _inner is null ? null : _inner.Value;

    public void Write(object? v)
    {
        if (_inner is null) return;
        if (_inner is ITwoWayBinding tw) tw.Write(v);
        else throw new InvalidOperationException("Binding is not TwoWay.");
    }

    public event Action? ValueChanged;

    #endregion

    private void Rewire()
    {
        if (_inner != null)                        // detach old
            _inner.ValueChanged -= OnInnerChanged;

        _inner = null;
        if (_fe.DataContext is TSource src)
        {
            _inner = _mode switch
            {
                BindingMode.OneWay => Binding<TSource, TProp>.OneWay(src, _selector),
                BindingMode.TwoWay => Binding<TSource, TProp>.TwoWay(src, _selector),
                _ => Binding<TSource, TProp>.OneTime(src, _selector)
            };
            _inner.ValueChanged += OnInnerChanged;
        }
        // Notify target that value may have changed
        ValueChanged?.Invoke();
    }

    private void OnInnerChanged() => ValueChanged?.Invoke();
}