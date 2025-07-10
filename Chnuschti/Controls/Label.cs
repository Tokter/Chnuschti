// ---------------------------------------------------------------------------
//  Label – text element for the Skia-backed UI framework
// ---------------------------------------------------------------------------

using System;
using SkiaSharp;

namespace Chnuschti.Controls;

public sealed class Label : Control
{
    public Label()
    {
        IsHitTestVisible = false; // Labels are not interactive by default
    }

    // --------------------------------------------------------------------
    //  Dependency properties
    // --------------------------------------------------------------------
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(Label), new PropertyMetadata(string.Empty, OnInvalidateMeasure));

    // CLR wrappers --------------------------------------------------------
    public string Text { get => (string)GetValue(TextProperty)!; set => SetValue(TextProperty, value); }

    //Convert string to Label
    public static implicit operator Label(string text)
    {
        return new Label { Text = text };
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
