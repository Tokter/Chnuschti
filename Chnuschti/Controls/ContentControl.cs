// ---------------------------------------------------------------------------
//  ContentControl – hosts exactly one child VisualElement
// ---------------------------------------------------------------------------

using System;
using SkiaSharp;

namespace Chnuschti.Controls;

public class ContentControl : Control
{
    public ContentControl()
    {
    }

    // --------------------------------------------------------------------
    //  Dependency-property : Content
    // --------------------------------------------------------------------
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(Control), typeof(ContentControl), new PropertyMetadata(null, OnContentChanged));

    /// <summary>The single child element. May be null.</summary>
    public Control? Content
    {
        get => (Control?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private static void OnContentChanged(DependencyObject d, DependencyProperty p, object? oldVal, object? newVal)
    {
        var cc = (ContentControl)d;
        cc.AttachContent((Control?)oldVal, (Control?)newVal);
    }

    // --------------------------------------------------------------------
    //  Attach / detach helpers
    // --------------------------------------------------------------------
    private VisualElement? _content;      // private backing field

    private void AttachContent(VisualElement? oldC, VisualElement? newC)
    {
        _content = newC;
        ReplaceVisualChild(oldC, newC);

        // Layout must re-run because size may have changed
        InvalidateMeasure();
    }

    protected override void ArrangeContent(SKRect contentRect)
    {
        if (_content == null) return; // nothing to layout

        var childRect = ShrinkBy(ToLocal(contentRect), _content.Margin);

        // Use the desiredSize for the content (minus margins)
        var desiredWidth = _content.DesiredSize.Width - _content.Margin.Horizontal;
        var desiredHeight = _content.DesiredSize.Height - _content.Margin.Vertical;

        // Apply alignment
        var alignedRect = ApplyAlignment(childRect, desiredWidth, desiredHeight);

        _content.Arrange(alignedRect);
    }
}
