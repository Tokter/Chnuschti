using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chnuschti;

public abstract class DependencyObject : INotifyPropertyChanged
{
    // local values
    private readonly Dictionary<DependencyProperty, object?> _values = new();
    // bindings (classic OR live)
    private readonly Dictionary<DependencyProperty, IReadOnlyBinding> _bindings = new();

    // ---------- INotifyPropertyChanged ----------
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ---------- Visual-tree linkage (for inheritance) ----------
    internal VisualElement? ParentInternal { get; set; }

    // ---------- Public API ----------
    public object? GetValue(DependencyProperty dp)
    {
        // (1) Classic / live bindings
        if (_bindings.TryGetValue(dp, out var b)) return b.Value;

        // (2) Local value
        if (_values.TryGetValue(dp, out var v)) return v;

        // (3) Style
        if (this is VisualElement ve && ve.Style?.TryGetValue(dp, out var sv) == true) return sv;

        // (4) Inherited value – walk visual tree
        if (dp.Inherits)
        {
            for (var p = ParentInternal; p is not null; p = p.ParentInternal)
                if (p._values.TryGetValue(dp, out var iv) ||      // local on ancestor
                    (p.Style?.TryGetValue(dp, out iv) == true))   // style on ancestor
                    return iv;
        }

        // (5) Default
        return dp.DefaultValue;
    }

    public void SetValue(DependencyProperty dp, object? value)
    {
        // Two-way binding target?
        if (_bindings.TryGetValue(dp, out var bind) && bind is ITwoWayBinding tw)
        {
            tw.Write(value);          // updates source; ValueChanged will bounce back
            return;
        }

        // Type check
        if (value != null && !dp.PropertyType.IsInstanceOfType(value))
            throw new ArgumentException($"Wrong type for {dp.Name}");

        var old = GetValue(dp);
        if (Equals(old, value)) return;

        _values[dp] = value;
        dp.PropertyChangedCallback?.Invoke(this, dp, old, value);
        OnPropertyChanged(dp.Name);
    }

    public void ClearValue(DependencyProperty dp)
    {
        if (_values.Remove(dp))
        {
            OnPropertyChanged(dp.Name);
        }
    }

    // ---------- Binding helpers ----------
    public void SetBinding(DependencyProperty target, IReadOnlyBinding binding)
    {
        _bindings[target] = binding;

        // React to source changes
        binding.ValueChanged += () => OnPropertyChanged(target.Name);

        // Initial transfer for OneWay / TwoWay / OneTime
        SetValue(target, binding.Value);
    }
}
