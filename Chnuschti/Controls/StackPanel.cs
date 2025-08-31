using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Controls;

public enum Orientation { Vertical, Horizontal }

public sealed class StackPanel : Control
{
    public StackPanel()
    {
        IsHitTestVisible = false; // StackPanels are not interactive by default
    }

    // -------- Orientation dependency-property ----------------------------
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(StackPanel), new PropertyMetadata(Orientation.Vertical, OnOriented));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty)!;
        set => SetValue(OrientationProperty, value);
    }

    private static void OnOriented(DependencyObject d,
                               DependencyProperty p,
                               object? o, object? n)
    {
        if (d is StackPanel sp)
            sp.InvalidateMeasure();   // this also invalidates arrange & matrices
    }

    // -------- ARRANGE  ----------------------------------------------------
    // positions each child's layout slot one after another
    protected override void ArrangeContent(SKRect content)
    {
        float x = content.Left;
        float y = content.Top;

        foreach (var child in Children)
        {
            if (!child.IsVisible)
                continue;

            var desired = child.DesiredSize; // includes child's Margin
            var m = child.Margin;

            if (Orientation == Orientation.Vertical)
            {
                // Leading edges include margin; trailing edges exclude margin
                float top = y + m.Top;
                float left = content.Left + m.Left;
                float right = content.Right - m.Right;
                float height = Math.Max(0, desired.Height - m.Vertical);

                var childRect = new SKRect(left, top, right, top + height);

                // Arrange using desired size without margins for alignment
                var alignedRect = ApplyAlignment(
                    childRect,
                    Math.Max(0, desired.Width - m.Horizontal),
                    height);

                child.Arrange(alignedRect);

                // Advance by full desired size (content + margins)
                y += desired.Height;
            }
            else
            {
                float left = x + m.Left;
                float top = content.Top + m.Top;
                float bottom = content.Bottom - m.Bottom;
                float width = Math.Max(0, desired.Width - m.Horizontal);

                var childRect = new SKRect(left, top, left + width, bottom);

                var alignedRect = ApplyAlignment(
                    childRect,
                    width,
                    Math.Max(0, desired.Height - m.Vertical));

                child.Arrange(alignedRect);

                x += desired.Width;
            }
        }
    }
}
