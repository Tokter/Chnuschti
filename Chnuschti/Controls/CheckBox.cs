// -------------------------------------------------------------------------
//  CheckBox – a two-state toggle with optional content
// -------------------------------------------------------------------------
using System;
using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Controls
{
    public sealed class CheckBox : ContentControl
    {
        public CheckBox()
        {
            VerticalContentAlignment = VerticalAlignment.Center; // Center the content vertically
            InvalidateDrawResources();
        }

        // ---- dependency-properties -------------------------------------
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(CheckBox), new PropertyMetadata(false, OnCheckedChanged));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty)!;
            set => SetValue(IsCheckedProperty, value);
        }

        private static void OnCheckedChanged(DependencyObject d, DependencyProperty p, object? oldVal, object? newVal)
        {
            var cb = (CheckBox)d;
            cb.InvalidateDrawResources();    // redraw tick
        }

        // ---- click handling --------------------------------------------
        protected override void OnClick(object? sender, EventArgs e)
        {
            // toggle first, then bubble the event
            IsChecked = !IsChecked;
            base.OnClick(sender, e);
        }

        protected override void ArrangeContent(SKRect contentRect)
        {
            if (Content == null) return;

            var childRect = new SKRect(
                contentRect.Left + ThemeManager.Current.Width,
                contentRect.Top,
                contentRect.Right,
                contentRect.Bottom);

            childRect = ShrinkBy(childRect, Content.Margin);

            // Use the desiredSize for the content (minus margins)
            var desiredWidth = Content.DesiredSize.Width - Content.Margin.Horizontal;
            var desiredHeight = Content.DesiredSize.Height - Content.Margin.Vertical;

            // Apply alignment
            var alignedRect = ApplyAlignment(childRect, desiredWidth, desiredHeight);

            Content.Arrange(alignedRect);
        }
    }
}
