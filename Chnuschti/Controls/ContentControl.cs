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
        //Get the default style from the current theme
        Style = ThemeManager.Current.Resources.Get<ContentControl, Style>();
    }

    // --------------------------------------------------------------------
    //  Dependency-property : Content
    // --------------------------------------------------------------------
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(Control), typeof(ContentControl), new PropertyMetadata(null, OnContentChanged, inherits: false));

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
        _content.Arrange(childRect);
    }
}
