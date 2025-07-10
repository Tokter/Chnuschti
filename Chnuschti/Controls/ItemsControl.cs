using Chnuschti.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Chnuschti.Controls;

/// <summary>
/// A control that can be used to present a collection of items.
/// </summary>
public class ItemsControl : Control
{
    // --------------------------------------------------------------------
    //  Dependency-properties
    // --------------------------------------------------------------------
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsControl), new PropertyMetadata(null, OnItemsSourceChanged));
    public static readonly DependencyProperty ItemsPanelProperty = DependencyProperty.Register(nameof(ItemsPanel), typeof(Control), typeof(ItemsControl), new PropertyMetadata(null, OnItemsPanelChanged));
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(IViewLocator), typeof(ItemsControl), new PropertyMetadata(null, OnItemTemplateChanged));

    // --------------------------------------------------------------------
    //  Public properties
    // --------------------------------------------------------------------

    /// <summary>
    /// Gets or sets the collection of items to be displayed by this ItemsControl.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the panel used to layout the items.
    /// Default is a vertical StackPanel.
    /// </summary>
    public Control? ItemsPanel
    {
        get => (Control?)GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display each item.
    /// </summary>
    public IViewLocator? ItemTemplate
    {
        get => (IViewLocator?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    // --------------------------------------------------------------------
    //  Private fields
    // --------------------------------------------------------------------
    private Control? _itemsPanel;
    private readonly Dictionary<object, Control> _itemsControls = new();
    private INotifyCollectionChanged? _observableCollection;

    // --------------------------------------------------------------------
    //  Constructors
    // --------------------------------------------------------------------
    public ItemsControl()
    {
        // Default to a vertical StackPanel
        ItemsPanel = new StackPanel();
    }

    // --------------------------------------------------------------------
    //  Overrides
    // --------------------------------------------------------------------
    protected override void ArrangeContent(SkiaSharp.SKRect contentRect)
    {
        if (_itemsPanel != null)
        {
            // Let the panel handle the layout of the items
            var childRect = ShrinkBy(ToLocal(contentRect), _itemsPanel.Margin);
            _itemsPanel.Arrange(childRect);
        }
    }

    // --------------------------------------------------------------------
    //  Property change handlers
    // --------------------------------------------------------------------
    private static void OnItemsSourceChanged(DependencyObject d, DependencyProperty p, object? oldValue, object? newValue)
    {
        if (d is ItemsControl itemsControl)
        {
            itemsControl.OnItemsSourceChanged((IEnumerable?)oldValue, (IEnumerable?)newValue);
        }
    }

    private static void OnItemsPanelChanged(DependencyObject d, DependencyProperty p, object? oldValue, object? newValue)
    {
        if (d is ItemsControl itemsControl)
        {
            itemsControl.OnItemsPanelChanged((Control?)oldValue, (Control?)newValue);
        }
    }

    private static void OnItemTemplateChanged(DependencyObject d, DependencyProperty p, object? oldValue, object? newValue)
    {
        if (d is ItemsControl itemsControl)
        {
            itemsControl.OnItemTemplateChanged((IViewLocator?)oldValue, (IViewLocator?)newValue);
        }
    }

    // --------------------------------------------------------------------
    //  Implementation
    // --------------------------------------------------------------------
    private void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
        // Unsubscribe from old collection's change events
        if (_observableCollection != null)
        {
            _observableCollection.CollectionChanged -= OnCollectionChanged;
            _observableCollection = null;
        }

        // Clear existing items
        ClearItems();

        // Add items from new collection
        if (newValue != null)
        {
            // Subscribe to collection changes if supported
            if (newValue is INotifyCollectionChanged observableCollection)
            {
                _observableCollection = observableCollection;
                _observableCollection.CollectionChanged += OnCollectionChanged;
            }

            // Add all items from new collection
            foreach (var item in newValue)
            {
                AddItem(item);
            }
        }

        InvalidateMeasure();
    }

    private void OnItemsPanelChanged(Control? oldValue, Control? newValue)
    {
        // Remove old panel
        if (oldValue != null)
        {
            ReplaceVisualChild(oldValue, null);
            _itemsPanel = null;
        }

        // Add new panel
        if (newValue != null)
        {
            _itemsPanel = newValue;
            ReplaceVisualChild(null, _itemsPanel);

            // Transfer existing items to new panel
            if (_itemsPanel != null)
            {
                foreach (var control in _itemsControls.Values)
                {
                    _itemsPanel.Add(control);
                }
            }
        }

        InvalidateMeasure();
    }

    private void OnItemTemplateChanged(IViewLocator? oldValue, IViewLocator? newValue)
    {
        // Refresh all items with new template
        if (ItemsSource != null)
        {
            ClearItems();

            foreach (var item in ItemsSource)
            {
                AddItem(item);
            }
        }

        InvalidateMeasure();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        RemoveItem(item);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddItem(item);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                ClearItems();
                if (ItemsSource != null)
                {
                    foreach (var item in ItemsSource)
                    {
                        AddItem(item);
                    }
                }
                break;
        }

        InvalidateMeasure();
    }

    private void AddItem(object item)
    {
        if (_itemsPanel == null || item == null)
            return;

        Control? control = null;

        // Use ItemTemplate if provided
        if (ItemTemplate != null && ItemTemplate.Match(item))
        {
            control = ItemTemplate.Build(item);
        }

        // Fall back to default rendering
        if (control == null)
        {
            // Default presentation - just convert to string
            control = new TextBlock { Text = item.ToString() ?? string.Empty };
        }

        // Set item as DataContext for the control
        control.DataContext = item;

        // Add to collection and panel
        _itemsControls[item] = control;
        _itemsPanel.Add(control);
    }

    private void RemoveItem(object item)
    {
        if (_itemsPanel == null || item == null)
            return;

        if (_itemsControls.TryGetValue(item, out var control))
        {
            _itemsPanel.Children.FirstOrDefault(c => c == control)?.Dispose();
            _itemsControls.Remove(item);
        }
    }

    private void ClearItems()
    {
        if (_itemsPanel != null)
        {
            foreach (var control in _itemsControls.Values)
            {
                control.Dispose();
            }
        }

        _itemsControls.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from collection changes
            if (_observableCollection != null)
            {
                _observableCollection.CollectionChanged -= OnCollectionChanged;
                _observableCollection = null;
            }

            // Dispose all item controls
            ClearItems();
        }

        base.Dispose(disposing);
    }
}
