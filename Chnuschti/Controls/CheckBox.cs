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
            // Pick up the default style from the active theme
            Style = ThemeManager.Current.Resources.Get<CheckBox, Style>();
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

        // ---- custom layout (reserve room for the check box) ------------
        private const float BOX_SIZE = 16f;
        private const float SPACING = 0f;

        protected override void ArrangeContent(SKRect contentRect)
        {
            if (Content == null) return;

            var childRect = new SKRect(
                contentRect.Left + BOX_SIZE + SPACING,
                contentRect.Top,
                contentRect.Right,
                contentRect.Bottom);

            childRect = ShrinkBy(childRect, Content.Margin);
            Content.Arrange(childRect);
        }
    }
}
