using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti.Controls;

/// <summary>
/// A single tab page.  The <see cref="Header"/> is shown in the tab strip; the
/// <see cref="ContentControl.Content"/> becomes the page body.
/// </summary>
public class TabItem : ContentControl
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(object), typeof(TabItem), new PropertyMetadata(null));
    public static readonly DependencyProperty ParentTabControlProperty = DependencyProperty.Register(nameof(ParentTabControl), typeof(TabControl), typeof(TabItem), new PropertyMetadata(null));

    /// <summary>The text, icon, or visual shown on the tab strip.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
  
    public TabControl? ParentTabControl
    {
        get => GetValue(ParentTabControlProperty) as TabControl;
        internal set => SetValue(ParentTabControlProperty, value);
    }
}

/// <summary>Where the header panel is rendered, relative to the page body.</summary>
public enum TabStripPlacement { Top, Bottom, Left, Right }

/// <summary>
/// Displays a selectable row/column of tabs and the active <see cref="TabItem"/>
/// below / above / left / right of it, depending on <see cref="StripPlacement"/>.
/// A <see cref="HeaderTemplate"/> lets you design rich headers with icons,
/// close‑buttons, etc.
/// </summary>
public class TabControl : Control, IHasChildren<TabItem>
{
    // -------- Dependency‑properties --------------------------------------
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(TabControl), new PropertyMetadata(0, OnSelectedIndexChanged));
    public static readonly DependencyProperty StripPlacementProperty = DependencyProperty.Register(nameof(StripPlacement), typeof(TabStripPlacement), typeof(TabControl), new PropertyMetadata(TabStripPlacement.Top, OnStripPlacementChanged));
    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(HeaderTemplate), typeof(IViewLocator), typeof(TabControl), new PropertyMetadata(null, OnHeaderTemplateChanged));

    /// <summary>The index of the currently active tab.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty)!;
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Where the header strip is shown (Top, Bottom, Left, Right).</summary>
    public TabStripPlacement StripPlacement
    {
        get => (TabStripPlacement)GetValue(StripPlacementProperty)!;
        set => SetValue(StripPlacementProperty, value);
    }

    /// <summary>A template that defines the visual for each tab header.</summary>
    public IViewLocator? HeaderTemplate
    {
        get => (IViewLocator?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    // -------- internals ----------------------------------------------------
    internal readonly StackPanel _headerPanel;   // hosts the clickable tab headers
    internal readonly ContentControl _contentHost; // hosts the active page
    private readonly List<TabItem> _tabs = new();

    public IReadOnlyList<TabItem> Tabs => _tabs;

    public TabControl()
    {
        // build visual tree: [headerPanel] + [contentHost]
        _headerPanel = new StackPanel { Padding = new Thickness(2) };
        _contentHost = new ContentControl { Margin = new Thickness(0, 4, 0, 0) };
        AddChild(_headerPanel);
        AddChild(_contentHost);
        IsHitTestVisible = false; // TabControl itself is not interactive
    }

    // --------------------------- public API --------------------------------
    /// <summary>Adds a fully‑configured <see cref="TabItem"/> to the control.</summary>
    public void AddTab(TabItem tab)
    {
        if (tab == null) throw new ArgumentNullException(nameof(tab));
        _tabs.Add(tab);
        tab.ParentTabControl = this;             // set parent reference
        RebuildHeaderButtons();

        if (_tabs.Count == 1)
            SelectedIndex = 0;                   // auto‑select first tab
        else
            UpdateSelection();                   // keep IsSelected flags in sync

        InvalidateMeasure();
    }

    public TabItem AddChild(TabItem child)
    {
        AddTab(child);
        return child;
    }

    /// <summary>Removes a tab (and its header button) at runtime.</summary>
    public void RemoveTab(TabItem tab)
    {
        if (tab == null) throw new ArgumentNullException(nameof(tab));
        int idx = _tabs.IndexOf(tab);
        if (idx < 0) return;                     // not found

        _tabs.RemoveAt(idx);
        tab.ParentTabControl = null;             // clear parent reference
        RebuildHeaderButtons();                  // rebuild indices & visuals

        // Adjust selection
        if (_tabs.Count == 0)
        {
            _contentHost.Content = null;
            SelectedIndex = 0;
        }
        else
        {
            if (SelectedIndex >= _tabs.Count)
                SelectedIndex = _tabs.Count - 1;
            UpdateSelection();                   // refresh content + highlight
        }

        InvalidateMeasure();
    }

    // Helper to regenerate header buttons to keep internal indices in sync
    private void RebuildHeaderButtons()
    {
        _headerPanel.ClearChildren();

        for (int i = 0; i < _tabs.Count; i++)
        {
            var headerBtn = new TabHeaderButton(this, i)
            {
                Margin = new Thickness(2, 0),
                Padding = new Thickness(12, 8),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = BuildHeaderVisual(_tabs[i])
            };
            _headerPanel.AddChild(headerBtn);
        }
    }

    // ------------------------- selection logic -----------------------------
    private static void OnSelectedIndexChanged(DependencyObject d, DependencyProperty p, object? o, object? n) => ((TabControl)d).UpdateSelection();

    private void UpdateSelection()
    {
        if (_tabs.Count == 0) return;
        var idx = Math.Clamp(SelectedIndex, 0, _tabs.Count - 1);
        _contentHost.Content = _tabs[idx];

        // update header visual state
        for (int i = 0; i < _headerPanel.Children.Count; i++)
            if (_headerPanel.Children[i] is TabHeaderButton hb)
                hb.IsSelected = (i == idx);

        InvalidateDrawResources(); // selected header highlight
    }

    // ------------------- strip placement change handler --------------------
    private static void OnStripPlacementChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var tc = (TabControl)d;
        tc.UpdateHeaderPanelOrientation();
        tc.InvalidateMeasure();
    }

    private void UpdateHeaderPanelOrientation()
    {
        _headerPanel.Orientation = StripPlacement is TabStripPlacement.Left or TabStripPlacement.Right
            ? Orientation.Vertical
            : Orientation.Horizontal;

        _contentHost.Margin = StripPlacement switch
        {
            TabStripPlacement.Bottom => new Thickness(0, 0, 0, 4),
            TabStripPlacement.Left => new Thickness(4, 0, 0, 0),
            TabStripPlacement.Right => new Thickness(0, 0, 4, 0),
            _ => new Thickness(0, 4, 0, 0)
        };
    }

    // -------------------------- header templating --------------------------
    private static void OnHeaderTemplateChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
        => ((TabControl)d).RefreshHeaders();

    private void RefreshHeaders()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            if (_headerPanel.Children[i] is TabHeaderButton hb)
            {
                hb.Content = BuildHeaderVisual(_tabs[i]);
            }
        }
        InvalidateMeasure();
    }

    private Control BuildHeaderVisual(TabItem tab)
    {
        // 1) user‑supplied template gets first dibs
        if (HeaderTemplate != null && HeaderTemplate.Match(tab))
        {
            var c = HeaderTemplate.Build(tab);
            if (c != null)
            {
                c.DataContext = tab;
                return c;
            }
        }

        // 2) fallback – if Header is already a Control just reuse it
        if (tab.Header is Control ctrl) return ctrl;

        // 3) plain text fallback
        return new Label { Text = tab.Header?.ToString() ?? $"Tab {_tabs.IndexOf(tab) + 1}" };
    }

    // ------------------------- LAYOUT --------------------------------------
    protected override void ArrangeContent(SKRect content)
    {
        var hdr = _headerPanel.DesiredSize;

        switch (StripPlacement)
        {
            case TabStripPlacement.Top:
                _headerPanel.Arrange(new SKRect(content.Left, content.Top, content.Right, content.Top + hdr.Height));
                _contentHost.Arrange(new SKRect(content.Left, content.Top + hdr.Height, content.Right, content.Bottom));
                break;
            case TabStripPlacement.Bottom:
                _contentHost.Arrange(new SKRect(content.Left, content.Top, content.Right, content.Bottom - hdr.Height));
                _headerPanel.Arrange(new SKRect(content.Left, content.Bottom - hdr.Height, content.Right, content.Bottom));
                break;
            case TabStripPlacement.Left:
                _headerPanel.Arrange(new SKRect(content.Left, content.Top, content.Left + hdr.Width, content.Bottom));
                _contentHost.Arrange(new SKRect(content.Left + hdr.Width, content.Top, content.Right, content.Bottom));
                break;
            case TabStripPlacement.Right:
                _contentHost.Arrange(new SKRect(content.Left, content.Top, content.Right - hdr.Width, content.Bottom));
                _headerPanel.Arrange(new SKRect(content.Right - hdr.Width, content.Top, content.Right, content.Bottom));
                break;
        }
    }
}

internal sealed class TabHeaderButton : Button
{
    internal bool IsSelected;
    private readonly TabControl _parent;
    private readonly int _index;

    public TabHeaderButton(TabControl parent, int index)
    {
        _parent = parent;
        _index = index;
    }

    protected override void OnClick(object? sender, EventArgs e) => _parent.SelectedIndex = _index;
}