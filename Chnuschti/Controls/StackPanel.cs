using Chnuschti.Controls;
using SkiaSharp;

namespace Chnuschti.Controls;

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
    // positions each child's layout slot one after another
    protected override void ArrangeContent(SKRect content)
    {
        float curX = content.Left;
        float curY = content.Top;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;
            
            var need = child.DesiredSize;             // includes Margin
            float childDesiredWidth = need.Width - child.Margin.Horizontal;
            float childDesiredHeight = need.Height - child.Margin.Vertical;
            
            if (Orientation == Orientation.Vertical)
            {
                // Calculate full available width for this item
                float availableWidth = Math.Max(0, content.Width);
                
                // Determine horizontal position based on alignment
                float x = curX;
                float width = childDesiredWidth;
                
                // Apply horizontal alignment
                switch (HorizontalContentAlignment)
                {
                    case HorizontalAlignment.Center:
                        if (width < availableWidth)
                            x = curX + (availableWidth - width) / 2;
                        break;
                    case HorizontalAlignment.Right:
                        if (width < availableWidth)
                            x = curX + availableWidth - width;
                        break;
                    case HorizontalAlignment.Stretch:
                        width = availableWidth;
                        break;
                }
                
                // Create rectangle with the appropriate alignment applied
                var r = new SKRect(
                    x,
                    curY + child.Margin.Top,
                    x + width,
                    curY + childDesiredHeight + child.Margin.Top);
                
                child.Arrange(r);
                curY += need.Height;                  // next row
            }
            else // Horizontal
            {
                // Calculate full available height for this item
                float availableHeight = Math.Max(0, content.Height);
                
                // Determine vertical position based on alignment
                float y = curY;
                float height = childDesiredHeight;
                
                // Apply vertical alignment
                switch (VerticalContentAlignment)
                {
                    case VerticalAlignment.Center:
                        if (height < availableHeight)
                            y = curY + (availableHeight - height) / 2;
                        break;
                    case VerticalAlignment.Bottom:
                        if (height < availableHeight)
                            y = curY + availableHeight - height;
                        break;
                    case VerticalAlignment.Stretch:
                        height = availableHeight;
                        break;
                }
                
                // Create rectangle with the appropriate alignment applied
                var r = new SKRect(
                    curX + child.Margin.Left,
                    y,
                    curX + childDesiredWidth + child.Margin.Left,
                    y + height);
                
                child.Arrange(r);
                curX += need.Width;                   // next column
            }
        }
    }
}
