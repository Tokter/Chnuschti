// ---------------------------------------------------------------------------
//  Label – text element for the Skia-backed UI framework
// ---------------------------------------------------------------------------

using System;
using SkiaSharp;

namespace Chnuschti.Controls;

public sealed class Label : Control
{
    // --------------------------------------------------------------------
    //  Dependency properties
    // --------------------------------------------------------------------
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Label), new PropertyMetadata(string.Empty, OnInvalidateMeasure));

    // CLR wrappers --------------------------------------------------------
    public string Text { get => (string)GetValue(TextProperty)!; set => SetValue(TextProperty, value); }

    public Label()
    {
        IsHitTestVisible = false; // Labels are not interactive by default
    }

    //Convert string to Label
    public static implicit operator Label(string text)
    {
        return new Label { Text = text };
    }


    // --------------------------------------------------------------------
    //  Layout – Measure / Arrange overrides
    // --------------------------------------------------------------------
    protected override SKSize MeasureContent(SKSize availContent, RenderResource? resource)
    {
        var txt = Text ?? string.Empty;

        // width
        if (resource is IHaveFont font)
        {
            float w = font.MeasureText(txt);

            // height (baseline + descent)
            var fm = font.GetFontMetrics();
            float h = fm.Descent - fm.Ascent;  // ascent is negative

            return new SKSize(w, h);

        }
        else
        {
            return SKSize.Empty;
        }
    }

    // --------------------------------------------------------------------
    //  Property-change helpers
    // --------------------------------------------------------------------
    private static void OnInvalidateMeasure(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        if (d is Label l)
        {
            l.InvalidateMeasure(); // will also invalidate arrange & matrices
        }
    }
}
