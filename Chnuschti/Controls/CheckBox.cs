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
        public const float WIDTH = 52;
        public const float HEIGHT = 32;
        public const float HANDLE_OFF = 16f;
        public const float HANDLE_ON = 24f;
        public const float HANDLE_PRESSED = 28f;
        public const float HANDLE_HOVER = 40f;

        public CheckBox()
        {
            // Pick up the default style from the active theme
            Style = ThemeManager.Current.Resources.Get<CheckBox, Style>();
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
                contentRect.Left + CheckBox.WIDTH,
                contentRect.Top,
                contentRect.Right,
                contentRect.Bottom);

            childRect = ShrinkBy(childRect, Content.Margin);
            Content.Arrange(childRect);
        }
    }
}
