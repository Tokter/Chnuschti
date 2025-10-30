using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Chnuschti.Controls;

public class Menu : Control
{
    internal readonly StackPanel _panel;
    private readonly List<MenuItem> _items = new();
    internal bool IsMenuTracking { get; private set; }

    public Menu()
    {
        //VerticalContentAlignment = VerticalAlignment.Center;
        //this.HorizontalContentAlignment = HorizontalAlignment.Left;
        _panel = new StackPanel { Orientation = Orientation.Horizontal };
        AddChild(_panel);
        IsHitTestVisible = false;      // items handle input
        IsMenuTracking = false;
        Padding = new Thickness(4);
        InvalidateMeasure();
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Menu), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty)!;
        set => SetValue(OrientationProperty, value);
    }

    // expose read-only collection for internal wiring (don’t mutate outside)
    internal IReadOnlyList<MenuItem> ItemsInternal => _items;
    internal int ItemCount => _items.Count;

    public Menu AddItem(MenuItem item)
    {
        if (item == null) return this;
        _items.Add(item);
        _panel.AddChild(item);
        item.ParentMenu = this;
        InvalidateMeasure();
        return this;
    }

    public Menu Add(params MenuItem[] items)
    {
        foreach (var item in items) AddItem(item);
        return this;
    }

    public void ClearItems()
    {
        _items.Clear();
        _panel.ClearChildren();
        InvalidateMeasure();
    }

    // submenu coordination
    internal bool HasAnySubmenuOpen()
    {
        foreach (var it in _items)
            if (it.IsSubmenuOpen) return true;
        return false;
    }

    internal void CloseAllSubmenus()
    {
        foreach (var it in _items)
            if (it.IsSubmenuOpen)
                it.IsSubmenuOpen = false;
        EndTrackingIfNoOpenSubmenus();
    }

    internal void CloseAllSubmenusExcept(MenuItem keep)
    {
        foreach (var it in _items)
            if (!ReferenceEquals(it, keep) && it.IsSubmenuOpen)
                it.IsSubmenuOpen = false;
        EndTrackingIfNoOpenSubmenus();
    }

    // consumers can hook this for “click closes menus” behavior
    internal Action? RequestCloseAll { get; set; }

    protected override void ArrangeContent(SKRect contentRect)
    {
        var childRect = ShrinkBy(ShrinkBy(ToLocal(contentRect), _panel.Margin), Padding);

        // Use the desiredSize for the content (minus margins)
        var desiredWidth = _panel.DesiredSize.Width - _panel.Margin.Horizontal;
        var desiredHeight = _panel.DesiredSize.Height - _panel.Margin.Vertical;

        // Apply alignment
        var alignedRect = ApplyAlignment(childRect, desiredWidth, desiredHeight);

        _panel.Arrange(alignedRect);
    }

    internal void BeginTracking() => IsMenuTracking = true;

    internal void EndTrackingIfNoOpenSubmenus() 
    {
        if (!AnySubmenuOpen(this)) IsMenuTracking = false;
    }

    internal void CloseSiblingSubmenus(MenuItem except)
    {
        foreach (var item in _items)
        {
            if (!ReferenceEquals(item, except)) item.CloseSubmenu(recursive: true);
        }
    }

    internal static bool AnySubmenuOpen(Menu menu)
        => menu.ItemsInternal.Any(mi => mi.IsSubmenuOpen || mi.DescendantHasOpenSubmenu());


    private static void OnOrientationChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        if (d is Menu m)
        {
            m._panel.Orientation = m.Orientation;

            if (m.Orientation == Orientation.Vertical)
            {
                m.HorizontalContentAlignment = HorizontalAlignment.Center;
            }
            else
            {
                m.HorizontalContentAlignment = HorizontalAlignment.Left;
            }

            // Refresh children chevrons
            foreach (var child in m.ItemsInternal)
                child.InvalidateMeasure(); // triggers ArrangeContent → UpdateChevronVisibility
            m.InvalidateMeasure();
        }
    }
}
