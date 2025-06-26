// ---------------------------------------------------------------------------
//  ContentControl – hosts exactly one child VisualElement
// ---------------------------------------------------------------------------

using System;
using SkiaSharp;

namespace Chnuschti.Controls;

public class ContentControl : Control
{
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
    private Action? _worldMatrixBridge;   // keeps reference to detach later

    private void AttachContent(VisualElement? oldC, VisualElement? newC)
    {
        _content = newC;
        ReplaceVisualChild(oldC, newC);

        // Layout must re-run because size may have changed
        InvalidateMeasure();
    }

    // --------------------------------------------------------------------
    //  Layout overrides
    // --------------------------------------------------------------------
    protected override SKSize MeasureContent(SKSize availContent)
    {
        if (_content == null) return SKSize.Empty;


        // Let child measure first
        _content.Measure(availContent);
        return _content.DesiredSize;
    }

    protected override void ArrangeContent(SKRect contentRect)
    {
        if (_content == null) return; // nothing to layout

        var childRect = ShrinkBy(ToLocal(contentRect), _content.Margin);
        _content.Arrange(childRect);
    }

    // --------------------------------------------------------------------
    //  Render – draw child in local space
    // --------------------------------------------------------------------
    protected override void RenderSelf(SKCanvas canvas)
    {
        _content?.Render(canvas);   // child draws in its own local coords
    }
}
