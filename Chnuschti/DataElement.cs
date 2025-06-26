using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public abstract class DataElement : VisualElement
{
    // DataContext (inherits so the whole subtree can “see” it)
    public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(nameof(DataContext), typeof(object), typeof(DataElement), new PropertyMetadata(null, OnDataContextChanged, inherits: true));

    public object? DataContext
    {
        get => GetValue(DataContextProperty);
        set => SetValue(DataContextProperty, value);
    }

    public event EventHandler? DataContextChanged;

    private static void OnDataContextChanged(
        DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        ((DataElement)d).DataContextChanged?.Invoke(d, EventArgs.Empty);
    }
}
