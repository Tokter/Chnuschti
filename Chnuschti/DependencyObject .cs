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
        // ------------------------------------------------------------------
        // 0)  Early-exit only if **a local entry already exists** *and* the
        //     caller is trying to overwrite it with the same value.
        //     (The very first assignment coming from SetBinding must *not*
        //     short-circuit because we still need to cache the value locally.)
        // ------------------------------------------------------------------
        if (_values.TryGetValue(dp, out var localOld) &&
            Equals(localOld, value))
            return;

        // ------------------------------------------------------------------
        // 1)  Snapshot the *effective* old value **before** we touch anything
        //     so the property-changed callback receives the right “old”.
        // ------------------------------------------------------------------
        var old = GetValue(dp);

        // ------------------------------------------------------------------
        // 2)  Store the new local value – this makes future GetValue() calls
        //     independent of the binding’s current state.
        // ------------------------------------------------------------------
        _values[dp] = value;

        // ------------------------------------------------------------------
        // 3)  If the target property is the *sink* of a TwoWay binding, now
        //     push the change back to the source.  (Do this *after* step 2 so
        //     a ValueChanged bounce-back does not clobber the local cache.)
        // ------------------------------------------------------------------
        if (_bindings.TryGetValue(dp, out var bind) &&
            bind is ITwoWayBinding tw && tw.CanWrite &&
            !Equals(bind.Value, value))          // avoid infinite echo
        {
            tw.Write(value);
        }

        // ------------------------------------------------------------------
        // 4)  Notify listeners and the control itself.
        // ------------------------------------------------------------------
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
        binding.ValueChanged += () => SetValue(target, binding.Value);

        // Initial transfer for OneWay / TwoWay / OneTime
        SetValue(target, binding.Value);
    }
}
