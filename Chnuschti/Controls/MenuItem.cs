using SkiaSharp;
using System;
using System.Windows.Input;

namespace Chnuschti.Controls;

public class MenuItem : Control
{
    // visual children (persisted; not recreated each arrange)
    private readonly Control _iconHost = new ContentControl { IsHitTestVisible = false, Margin = new Thickness(8, 0, 8, 0) };
    private readonly Label _textLabel = new() { VerticalContentAlignment = VerticalAlignment.Center };
    private readonly Label _shortcutLabel = new() { HorizontalContentAlignment = HorizontalAlignment.Right, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 8, 0) };
    private readonly Label _submenuChevron = new() { Text = "▶", HorizontalContentAlignment = HorizontalAlignment.Right, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 6, 0) };
    private Popup? _submenuPopup;

    internal Menu? ParentMenu { get; set; }

    public MenuItem()
    {
        IsHitTestVisible = true;
        Padding = new Thickness(8, 4, 8, 4);
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Center;

        // child order: icon | text | spacer | shortcut | chevron (popup lives outside normal tree)
        AddChild(_iconHost);
        AddChild(_textLabel);
        AddChild(_shortcutLabel);
        AddChild(_submenuChevron);

        UpdateVisuals();
        UpdateChevronVisibility();
    }

    // ---------------- Dependency Properties ----------------
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Control), typeof(MenuItem), new PropertyMetadata(null, OnDPChanged_Visual));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(MenuItem), new PropertyMetadata(string.Empty, OnDPChanged_Visual));
    public static readonly DependencyProperty ShortcutProperty = DependencyProperty.Register(nameof(Shortcut), typeof(string), typeof(MenuItem), new PropertyMetadata(string.Empty, OnDPChanged_Visual));
    public static readonly DependencyProperty SubmenuProperty = DependencyProperty.Register(nameof(Submenu), typeof(Menu), typeof(MenuItem), new PropertyMetadata(null, OnSubmenuChanged));
    public static readonly DependencyProperty IsSubmenuOpenProperty = DependencyProperty.Register(nameof(IsSubmenuOpen), typeof(bool), typeof(MenuItem), new PropertyMetadata(false, OnLayoutAffecting));
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(MenuItem), new PropertyMetadata(null, OnCommandChanged));
    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(MenuItem), new PropertyMetadata(null, OnCommandCanExecuteMaybe));

    // ---------------- CLR wrappers ----------------
    public Control? Icon { get => (Control?)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public string Text { get => (string)GetValue(TextProperty)!; set => SetValue(TextProperty, value); }
    public string Shortcut { get => (string)GetValue(ShortcutProperty)!; set => SetValue(ShortcutProperty, value); }
    public Menu? Submenu { get => (Menu?)GetValue(SubmenuProperty); set => SetValue(SubmenuProperty, value); }
    public bool IsSubmenuOpen { get => (bool)GetValue(IsSubmenuOpenProperty)!; set => SetValue(IsSubmenuOpenProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    // ---------------- DP Callbacks ----------------
    private static void OnDPChanged_Visual(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var mi = (MenuItem)d;
        mi.UpdateVisuals();
        mi.InvalidateMeasure();
    }

    private static void OnSubmenuChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var mi = (MenuItem)d;

        // IMPORTANT: previous implementation cleared items and copied, wiping submenu items
        // Here we just ensure popup wrapper exists for the new menu.
        if (n is Menu newMenu)
        {
            if (mi._submenuPopup == null || mi._submenuPopup.Content != newMenu)
            {
                mi._submenuPopup = new Popup();
                mi._submenuPopup.Kind = PopupKind.Menu;
                mi._submenuPopup.Content = newMenu;
            }
        }

        mi.UpdateChevronVisibility();
        mi.ApplySubmenuVisibility();
        mi.InvalidateMeasure();
    }

    private static void OnLayoutAffecting(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var mi = (MenuItem)d;
        if (p == IsSubmenuOpenProperty)
            mi.ApplySubmenuVisibility();
        mi.InvalidateMeasure();
    }

    private static void OnCommandChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var mi = (MenuItem)d;
        if (o is ICommand oldCmd) oldCmd.CanExecuteChanged -= mi.OnCanExecuteChanged;
        if (n is ICommand newCmd) newCmd.CanExecuteChanged += mi.OnCanExecuteChanged;
        mi.SyncIsEnabledWithCommand();
    }

    private static void OnCommandCanExecuteMaybe(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        ((MenuItem)d).SyncIsEnabledWithCommand();
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => SyncIsEnabledWithCommand();

    private void SyncIsEnabledWithCommand()
    {
        if (Command is null) return;

        try { IsEnabled = Command.CanExecute(CommandParameter); }
        catch { IsEnabled = false; }
        InvalidateDrawResources();
    }

    // ---------------- Public API ----------------
    public MenuItem AddItem(MenuItem item)
    {
        item.ParentMenu = this.ParentMenu;
        EnsureSubmenu();
        Submenu!.AddItem(item);
        UpdateChevronVisibility();
        return this;
    }

    public MenuItem Add(params MenuItem[] items)
    {
        foreach (var item in items) AddItem(item);
        return this;
    }

    // ---------------- Input ----------------
    public override void MouseEnter(SKPoint screenPt)
    {
        base.MouseEnter(screenPt);

        if (ParentMenu?.IsMenuTracking == true || IsInOpenChain())
        {
            OpenSubmenu();                    // open new submenu right away
            ParentMenu?.CloseSiblingSubmenus(this); // close the previously open sibling submenu
        }
    }

    public override void MouseLeave(SKPoint screenPt)
    {
        base.MouseLeave(screenPt);

        // Close only if pointer is not over the submenu
        if (IsSubmenuOpen && _submenuPopup != null)
        {
            var hit = _submenuPopup.HitTest(screenPt);
            if (hit is null) CloseSubmenu();
        }
    }

    protected override void OnClick(object? sender, EventArgs e)
    {
        if (HasSubmenu)
        {
            (ParentMenu as Menu)?.BeginTracking();

            OpenSubmenu(); // your existing logic to show the Popup
            (ParentMenu as Menu)?.CloseSiblingSubmenus(this);
            return;
        }

        // leaf → execute command if available
        if (IsEnabled && Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }

        Click?.Invoke(this, EventArgs.Empty);

        // Close all ancestor menus by asking root to collapse (optional – basic noop here)
        (ParentInternal as Menu)?.RequestCloseAll?.Invoke();
    }

    public event EventHandler? Click;

    private bool HasSubmenu => (Submenu?.ItemCount ?? 0) > 0;

    private void OpenSubmenu()
    {
        EnsureSubmenu();
        _submenuPopup!.Open(this);
        // Close siblings
        if (ParentInternal is Menu parent) parent.CloseAllSubmenusExcept(this);
        IsSubmenuOpen = true;
        InvalidateArrange();
    }

    internal void CloseSubmenu(bool recursive = false)
    {
        _submenuPopup?.Close();
        IsSubmenuOpen = false;
        InvalidateArrange();

        if (recursive)
        {
            // Close all child submenus
            foreach (var child in Submenu?.ItemsInternal ?? Enumerable.Empty<MenuItem>())
            {
                child.CloseSubmenu(recursive: true);
            }
        }
    }

    // ---------------- Layout ----------------
    protected override void ArrangeContent(SKRect contentRect)
    {
        UpdateChevronVisibility();

        // inner padding
        var inner = ShrinkBy(ToLocal(contentRect), Padding);

        float x = inner.Left;
        float h = inner.Height;

        // icon
        if (Icon != null)
        {
            if (_iconHost is ContentControl cc) cc.Content = Icon;
        }
        else if (_iconHost is ContentControl clearCc) clearCc.Content = null;

        // Measure children with unconstrained cross-axis
        _iconHost.Measure(new SKSize(float.PositiveInfinity, h));
        var iw = _iconHost.DesiredSize.Width;
        var iconRect = new SKRect(x, inner.Top, x + iw, inner.Bottom);
        _iconHost.Arrange(iconRect);
        x += iw;

        // text
        _textLabel.Text = Text ?? string.Empty;
        _textLabel.Measure(new SKSize(float.PositiveInfinity, h));
        var tw = _textLabel.DesiredSize.Width;
        var textRect = new SKRect(x, inner.Top, x + tw, inner.Bottom);
        _textLabel.Arrange(textRect);
        x += tw;

        // reserve room at right for shortcut + chevron
        float rightX = inner.Right;

        // chevron
        if (HasSubmenu)
        {
            _submenuChevron.Measure(new SKSize(float.PositiveInfinity, h));
            var cw = _submenuChevron.DesiredSize.Width;
            var cRect = new SKRect(rightX - cw, inner.Top, rightX, inner.Bottom);
            _submenuChevron.Arrange(cRect);
            rightX -= cw;
        }
        else
        {
            _submenuChevron.Arrange(new SKRect(0, 0, 0, 0)); // collapse
        }

        // shortcut
        if (!string.IsNullOrEmpty(Shortcut))
        {
            _shortcutLabel.Text = Shortcut ?? string.Empty;
            _shortcutLabel.Measure(new SKSize(float.PositiveInfinity, h));
            var sw = _shortcutLabel.DesiredSize.Width;
            var sRect = new SKRect(rightX - sw, inner.Top, rightX, inner.Bottom);
            _shortcutLabel.Arrange(sRect);
            rightX -= sw;
        }

        // Popup layout handled by PopupManager; no need to arrange submenu here.
    }

    private void UpdateVisuals()
    {
        if (_iconHost is ContentControl cc) cc.Content = Icon;
        _textLabel.Text = Text ?? string.Empty;
        _shortcutLabel.Text = Shortcut ?? string.Empty;
        UpdateChevronVisibility();
    }

    private void UpdateChevronVisibility()
    {
        var parentMenu = ParentInternal?.ParentInternal as Menu;
        var parentIsHorizontal = parentMenu?.Orientation == Orientation.Horizontal;

        // Show chevron only if there *is* a submenu AND the parent menu is vertical.
        _submenuChevron.IsVisible = HasSubmenu && !parentIsHorizontal;
    }

    private void ApplySubmenuVisibility()
    {
        if (_submenuPopup == null) return;
        _submenuPopup.IsVisible = IsSubmenuOpen;
        _submenuPopup.IsHitTestVisible = IsSubmenuOpen;
    }

    private bool IsInOpenChain()
    {
        // true if any ancestor menu item is already open (common for nested submenus)
        var p = ParentInternal as MenuItem;
        while (p != null)
        {
            if (p.IsSubmenuOpen) return true;
            p = p.ParentInternal as MenuItem;
        }
        return false;
    }

    internal bool DescendantHasOpenSubmenu()
    => Submenu?.ItemsInternal?.Any(mi => mi.IsSubmenuOpen || mi.DescendantHasOpenSubmenu()) == true;


    private void EnsureSubmenu()
    {
        if (Submenu != null)
        {
            if (_submenuPopup == null)
            {
                _submenuPopup = new Popup();
                _submenuPopup.Kind = PopupKind.Menu;
                _submenuPopup.Content = Submenu;
            }
            return;
        }

        var submenu = new Menu()
        {
            Margin = new Thickness(0),
            Padding = new Thickness(4),
            Orientation = Orientation.Vertical,
        };
        Submenu = submenu; // triggers OnSubmenuChanged which sets _popupSubmenu
        if (_submenuPopup == null)
        {
            _submenuPopup = new Popup();
            _submenuPopup.Kind = PopupKind.Menu;
            _submenuPopup.Content = Submenu;
        }
    }
}
