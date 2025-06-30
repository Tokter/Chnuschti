using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti;

public enum Orientation { Vertical, Horizontal }


public sealed class StackPanel : Control
{
    public StackPanel()
    {
        //Get the default style from the current theme
        Style = ThemeManager.Current.Resources.Get<StackPanel, Style>();
    }

    // -------- Orientation dependency-property ----------------------------
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StackPanel),
            new PropertyMetadata(Orientation.Vertical, OnOriented));

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
    // positions each child’s layout slot one after another
    protected override void ArrangeContent(SKRect content)
    {
        float curX = content.Left + Padding.Left;
        float curY = content.Top + Padding.Top;

        foreach (var child in Children)
        {
            var need = child.DesiredSize;             // includes Margin

            if (Orientation == Orientation.Vertical)
            {
                var r = new SKRect(
                    curX,
                    curY,
                    curX + Math.Max(0, content.Width - child.Margin.Horizontal),
                    curY + need.Height - child.Margin.Vertical);

                child.Arrange(r);
                curY += need.Height;                  // next row
            }
            else // Horizontal
            {
                var r = new SKRect(
                    curX,
                    curY,
                    curX + need.Width - child.Margin.Horizontal,
                    curY + Math.Max(0, content.Height - child.Margin.Vertical));

                child.Arrange(r);
                curX += need.Width;                   // next column
            }
        }
    }
}
