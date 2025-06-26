using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti;

public enum Orientation { Vertical, Horizontal }


public sealed class StackPanel : Control
{
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


    // -------- MEASURE  ----------------------------------------------------
    // asks each child for its desired size, then sums or maxes as needed
    protected override SKSize MeasureContent(SKSize availableContent)
    {
        float totalW = 0, totalH = 0;
        float maxW = 0, maxH = 0;

        foreach (var child in Children)
        {
            // Child receives infinite space in the stacking direction
            var avail = Orientation == Orientation.Vertical
                ? new SKSize(availableContent.Width, float.PositiveInfinity)
                : new SKSize(float.PositiveInfinity, availableContent.Height);

            child.Measure(avail);

            var need = child.DesiredSize; // includes child.Margin
            if (Orientation == Orientation.Vertical)
            {
                totalH += need.Height;
                maxW = Math.Max(maxW, need.Width);
            }
            else
            {
                totalW += need.Width;
                maxH = Math.Max(maxH, need.Height);
            }
        }

        if (Orientation == Orientation.Vertical)
            return new SKSize(maxW, totalH);
        else
            return new SKSize(totalW, maxH);
    }

    // -------- ARRANGE  ----------------------------------------------------
    // positions each child’s layout slot one after another
    protected override void ArrangeContent(SKRect content)
    {
        if (_templateRoot != null) { base.ArrangeContent(content); return; }

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
