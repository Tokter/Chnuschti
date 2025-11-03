using Chnuschti;
using Chnuschti.Controls;
using SkiaSharp;
using Test.TestControls;

namespace Test.Layout;

public class StackPanelLayout : IClassFixture<ThemeFixture>
{
    private ThemeFixture _themeFixture;
    private const float ScreenWidth = 800;
    private const float ScreenHeight = 600;

    public StackPanelLayout(ThemeFixture themeFixture)
    {
        _themeFixture = themeFixture;
    }

    /// <summary>
    /// Tests the horizontal orientation of a <see cref="StackPanel"/> with different vertical content alignments.
    /// </summary>
    /// <remarks>This test verifies that a <see cref="StackPanel"/> with horizontal orientation correctly
    /// aligns its children at the top, center, and bottom based on the <see cref="VerticalAlignment"/> setting. It
    /// checks the layout slots of the children to ensure they match the expected positions for each alignment
    /// scenario.</remarks>
    [Fact]
    public void HorizontalOrientation()
    {
        TestBlock tb1 = null!;
        TestBlock tb2 = null!;
        TestBlock tb3 = null!;
        TestBlock tb4 = null!;

        var stackPanel = new StackPanel()
            .With(sp =>
            {
                sp.Orientation = Orientation.Horizontal;
                sp.VerticalContentAlignment = VerticalAlignment.Top;
            })
            .Children(CreateBlocks(out tb1, out tb2, out tb3, out tb4));

        // First test top alignment
        stackPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        stackPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));
        Assert.Equal(tb1.Margin.Top, tb1.LayoutSlot.Top);
        Assert.Equal(tb2.Margin.Top, tb2.LayoutSlot.Top);
        Assert.Equal(tb3.Margin.Top, tb3.LayoutSlot.Top);
        Assert.Equal(tb4.Margin.Top, tb4.LayoutSlot.Top);
        AssertContentBoundsSize(tb1, tb2, tb3, tb4);
        AssertHorizontalAlignment(tb1, tb2, tb3, tb4);

        // Now test center alignment
        stackPanel.VerticalContentAlignment = VerticalAlignment.Center;
        stackPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        stackPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));
        Assert.Equal(ScreenHeight / 2.0f - tb1.LayoutSlot.Height / 2.0f, tb1.LayoutSlot.Top);
        Assert.Equal(ScreenHeight / 2.0f - tb2.LayoutSlot.Height / 2.0f, tb2.LayoutSlot.Top);
        Assert.Equal(ScreenHeight / 2.0f - tb3.LayoutSlot.Height / 2.0f, tb3.LayoutSlot.Top);
        Assert.Equal(ScreenHeight / 2.0f - tb4.LayoutSlot.Height / 2.0f, tb4.LayoutSlot.Top);
        AssertContentBoundsSize(tb1, tb2, tb3, tb4);
        AssertHorizontalAlignment(tb1, tb2, tb3, tb4);

        // Finally test bottom alignment
        stackPanel.VerticalContentAlignment = VerticalAlignment.Bottom;
        stackPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        stackPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));
        Assert.Equal(ScreenHeight - tb1.LayoutSlot.Height - tb1.Margin.Bottom, tb1.LayoutSlot.Top);
        Assert.Equal(ScreenHeight - tb2.LayoutSlot.Height - tb2.Margin.Bottom, tb2.LayoutSlot.Top);
        Assert.Equal(ScreenHeight - tb3.LayoutSlot.Height - tb3.Margin.Bottom, tb3.LayoutSlot.Top);
        Assert.Equal(ScreenHeight - tb4.LayoutSlot.Height - tb4.Margin.Bottom, tb4.LayoutSlot.Top);
        AssertContentBoundsSize(tb1, tb2, tb3, tb4);
        AssertHorizontalAlignment(tb1, tb2, tb3, tb4);
    }

    /// <summary>
    /// Tests the vertical orientation and horizontal alignment of child elements within a <see cref="StackPanel"/>.
    /// </summary>
    /// <remarks>This test verifies that child elements are correctly aligned to the left, center, and right
    /// when the <see cref="StackPanel"/> is set to vertical orientation. It checks the layout slots of the child
    /// elements against their expected positions based on the specified horizontal alignment.</remarks>
    [Fact]
    public void VerticalOrientation()
    {
        TestBlock tb1 = null!;
        TestBlock tb2 = null!;
        TestBlock tb3 = null!;
        TestBlock tb4 = null!;

        var stackPanel = new StackPanel()
            .With(sp =>
            {
                sp.Orientation = Orientation.Vertical;
                sp.HorizontalContentAlignment = HorizontalAlignment.Left;
            })
            .Children(CreateBlocks(out tb1, out tb2, out tb3, out tb4));

        // First test left alignment
        stackPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        stackPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));
        Assert.Equal(tb1.Margin.Left, tb1.LayoutSlot.Left);
        Assert.Equal(tb2.Margin.Left, tb2.LayoutSlot.Left);
        Assert.Equal(tb3.Margin.Left, tb3.LayoutSlot.Left);
        Assert.Equal(tb4.Margin.Left, tb4.LayoutSlot.Left);
        AssertContentBoundsSize(tb1, tb2, tb3, tb4);
        AssertVerticalAlignment(tb1, tb2, tb3, tb4);

        // Now test center alignment
        stackPanel.HorizontalContentAlignment = HorizontalAlignment.Center;
        stackPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        stackPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));
        Assert.Equal(ScreenWidth / 2.0f - tb1.LayoutSlot.Width / 2.0f, tb1.LayoutSlot.Left);
        Assert.Equal(ScreenWidth / 2.0f - tb2.LayoutSlot.Width / 2.0f, tb2.LayoutSlot.Left);
        Assert.Equal(ScreenWidth / 2.0f - tb3.LayoutSlot.Width / 2.0f, tb3.LayoutSlot.Left);
        Assert.Equal(ScreenWidth / 2.0f - tb4.LayoutSlot.Width / 2.0f, tb4.LayoutSlot.Left);
        AssertContentBoundsSize(tb1, tb2, tb3, tb4);
        AssertVerticalAlignment(tb1, tb2, tb3, tb4);

        // Finally test right alignment
        stackPanel.HorizontalContentAlignment = HorizontalAlignment.Right;
        stackPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        stackPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));
        Assert.Equal(ScreenWidth - tb1.LayoutSlot.Width - tb1.Margin.Right, tb1.LayoutSlot.Left);
        Assert.Equal(ScreenWidth - tb2.LayoutSlot.Width - tb2.Margin.Right, tb2.LayoutSlot.Left);
        Assert.Equal(ScreenWidth - tb3.LayoutSlot.Width - tb3.Margin.Right, tb3.LayoutSlot.Left);
        Assert.Equal(ScreenWidth - tb4.LayoutSlot.Width - tb4.Margin.Right, tb4.LayoutSlot.Left);
        AssertContentBoundsSize(tb1, tb2, tb3, tb4);
        AssertVerticalAlignment(tb1, tb2, tb3, tb4);
    }

    private static TestBlock[] CreateBlocks(out TestBlock tb1, out TestBlock tb2, out TestBlock tb3, out TestBlock tb4)
    {
        tb1 = new TestBlock { Width = 200, Height = 100, Margin = new Thickness(10) };
        tb2 = new TestBlock { Width = 190, Height = 90, Margin = new Thickness(9) };
        tb3 = new TestBlock { Width = 180, Height = 80, Margin = new Thickness(8) };
        tb4 = new TestBlock { ContentWidth = 100, ContentHeight = 40, Margin = new Thickness(8), Padding = new Thickness(7) };
        return new[] { tb1, tb2, tb3, tb4 };
    }

    private void AssertContentBoundsSize(TestBlock tb1, TestBlock tb2, TestBlock tb3, TestBlock tb4)
    {
        Assert.Equal(new SKRect(0, 0, tb1.Width, tb1.Height), tb1.ContentBounds);
        Assert.Equal(new SKRect(0, 0, tb2.Width, tb2.Height), tb2.ContentBounds);
        Assert.Equal(new SKRect(0, 0, tb3.Width, tb3.Height), tb3.ContentBounds);
        Assert.Equal(new SKRect(0, 0, tb4.ContentWidth + tb4.Padding.Horizontal, tb4.ContentHeight + tb4.Padding.Vertical), tb4.ContentBounds);
    }

    private void AssertHorizontalAlignment(TestBlock tb1, TestBlock tb2, TestBlock tb3, TestBlock tb4)
    {
        Assert.Equal(tb1.Margin.Left, tb1.LayoutSlot.Left);
        Assert.Equal(tb1.LayoutSlot.Width + tb1.Margin.Horizontal + tb2.Margin.Left, tb2.LayoutSlot.Left);
        Assert.Equal(tb1.LayoutSlot.Width + tb1.Margin.Horizontal + tb2.LayoutSlot.Width + tb2.Margin.Horizontal + tb3.Margin.Left, tb3.LayoutSlot.Left);
        Assert.Equal(tb1.LayoutSlot.Width + tb1.Margin.Horizontal + tb2.LayoutSlot.Width + tb2.Margin.Horizontal + tb3.LayoutSlot.Width + tb3.Margin.Horizontal + tb4.Margin.Left, tb4.LayoutSlot.Left);
    }

    private void AssertVerticalAlignment(TestBlock tb1, TestBlock tb2, TestBlock tb3, TestBlock tb4)
    {
        Assert.Equal(tb1.Margin.Top, tb1.LayoutSlot.Top);
        Assert.Equal(tb1.LayoutSlot.Height + tb1.Margin.Vertical + tb2.Margin.Top, tb2.LayoutSlot.Top);
        Assert.Equal(tb1.LayoutSlot.Height + tb1.Margin.Vertical + tb2.LayoutSlot.Height + tb2.Margin.Vertical + tb3.Margin.Top, tb3.LayoutSlot.Top);
        Assert.Equal(tb1.LayoutSlot.Height + tb1.Margin.Vertical + tb2.LayoutSlot.Height + tb2.Margin.Vertical + tb3.LayoutSlot.Height + +tb3.Margin.Vertical + tb4.Margin.Top, tb4.LayoutSlot.Top);
    }
}
