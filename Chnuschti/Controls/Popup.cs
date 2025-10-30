using SkiaSharp;
using System;

namespace Chnuschti.Controls;

public enum PopupKind
{
    Menu,
    Dropdown,
    Tooltip,
    Dialog
}

public class Popup : ContentControl
{
    public VisualElement? Owner { get; private set; }

    public bool IsOpen { get; private set; }
    public bool IsModal { get; set; }
    public PopupKind Kind { get; set; }

    // If true, clicking outside closes (ignored when IsModal = true unless you choose otherwise).
    public bool CloseOnOutsideClick { get; set; } = true;

    // Optional: shade background for modal dialogs.
    public bool ShowModalOverlay { get; set; } = true;

    // Screen-space cached placement rect (used before arrange).
    private SKRect _targetScreenRect;

    public Popup()
    {
        Focusable = false;
        IsHitTestVisible = true;
        Padding = new Thickness(4);
    }

    public void Open(VisualElement owner, SKRect? anchorOverride = null)
    {
        if (IsOpen) return;
        Owner = owner;
        IsOpen = true;
        this.ParentInternal = owner;

        // Determine anchor rect in screen space.
        if (anchorOverride.HasValue)
            _targetScreenRect = anchorOverride.Value;
        else
        {
            var tl = owner.PointToScreen(new SKPoint(-owner.Padding.Left, -owner.Padding.Top));
            var br = owner.PointToScreen(new SKPoint(owner.ContentBounds.Width, owner.ContentBounds.Height));
            _targetScreenRect = new SKRect(tl.X, tl.Y, br.X, br.Y);
            //_targetScreenRect = owner.ContentBounds;
        }

        PopupManager.Register(this);
        InvalidateMeasure();
    }

    private void PrintTranslationChain(VisualElement ve)
    {
        while (ve != null)
        {
            var t = ve.LayoutSlot;
            System.Diagnostics.Debug.WriteLine($"Element {ve.GetType().Name} (Id={ve.Id}) Translation: ({t.Left}, {t.Top})");
            ve = ve.ParentInternal;
        }
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        PopupManager.Unregister(this);
        Owner = null;
    }

    internal void PerformLayout(VisualElement root)
    {
        if (!IsOpen) return;

        // Measure unconstrained (could clamp later).
        Measure(new SKSize(float.PositiveInfinity, float.PositiveInfinity));
        var size = DesiredSize;

        // Default placement: below & left-aligned with anchor.
        float left = _targetScreenRect.Left;
        float top = _targetScreenRect.Bottom;

        // Map screen → root local for arrange.
        var localTL = Owner.PointFromScreen(new SKPoint(left, top));
        var rect = new SKRect(localTL.X, localTL.Y, localTL.X + size.Width, localTL.Y + size.Height);
        Arrange(rect);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && IsOpen)
        {
            PopupManager.Unregister(this);
        }
        base.Dispose(disposing);
    }

    protected override void ArrangeContent(SKRect contentRect)
    {
        if (Content is VisualElement ve)
        {
            var inner = new SKRect(
                contentRect.Left + Padding.Left,
                contentRect.Top + Padding.Top,
                contentRect.Right - Padding.Right,
                contentRect.Bottom - Padding.Bottom);
            ve.Arrange(inner);
        }
    }
}