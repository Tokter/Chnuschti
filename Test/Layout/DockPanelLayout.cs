using Chnuschti;
using Chnuschti.Controls;
using SkiaSharp;
using Test.TestControls;

namespace Test.Layout;

public class DockPanelLayout : IClassFixture<ThemeFixture>
{
    private ThemeFixture _themeFixture;
    private const float ScreenWidth = 800;
    private const float ScreenHeight = 600;

    public DockPanelLayout(ThemeFixture themeFixture)
    {
        _themeFixture = themeFixture;
    }

    /// <summary>
    /// Tests the layout behavior of a <see cref="DockPanel"/> by arranging its child elements and verifying their
    /// content bounds and layout slots.
    /// </summary>
    /// <remarks>This method creates a <see cref="DockPanel"/> with five <see cref="TestBlock"/> elements,
    /// measures and arranges them within a specified screen size, and asserts that each block's content bounds and
    /// layout slots are correctly set according to their dock positions.</remarks>
    [Fact]
    public void DockLayout()
    {
        TestBlock tb1 = null!;
        TestBlock tb2 = null!;
        TestBlock tb3 = null!;
        TestBlock tb4 = null!;
        TestBlock tb5 = null!;

        var dockPanel = new DockPanel()
            .Children(CreateBlocks(out tb1, out tb2, out tb3, out tb4, out tb5));
        dockPanel.Measure(new SKSize(ScreenWidth, ScreenHeight));
        dockPanel.Arrange(new SKRect(0, 0, ScreenWidth, ScreenHeight));

        // Assert ContentBounds
        Assert.Equal(tb1.ContentBounds, new SKRect(0, 0, tb1.Width, ScreenHeight - tb1.Margin.Vertical));
        Assert.Equal(tb2.ContentBounds, new SKRect(0, 0, tb2.Width, ScreenHeight - tb2.Margin.Vertical));
        Assert.Equal(tb3.ContentBounds, new SKRect(0, 0, ScreenWidth - tb1.Width - tb1.Margin.Horizontal - tb2.Width - tb2.Margin.Horizontal - tb3.Margin.Horizontal, tb3.Height));
        Assert.Equal(tb4.ContentBounds, new SKRect(0, 0, ScreenWidth - tb1.Width - tb1.Margin.Horizontal - tb2.Width - tb2.Margin.Horizontal - tb4.Margin.Horizontal, tb4.ContentHeight + tb4.Padding.Vertical));
        Assert.Equal(tb5.ContentBounds, new SKRect(0, 0, 
            ScreenWidth - tb1.Width - tb1.Margin.Horizontal - tb2.Width - tb2.Margin.Horizontal - tb5.Margin.Horizontal, 
            ScreenHeight - tb3.Height - tb3.Margin.Vertical - tb4.ContentHeight - tb4.Padding.Vertical - tb4.Margin.Vertical - tb5.Margin.Vertical
        ));

        // Assert LayoutSlot
        Assert.Equal(tb1.LayoutSlot, new SKRect(tb1.Margin.Left, tb1.Margin.Top, tb1.Width + tb1.Margin.Right, ScreenHeight - tb1.Margin.Bottom)); // Left
        Assert.Equal(tb2.LayoutSlot, new SKRect(ScreenWidth - tb2.Width - tb2.Margin.Right, tb2.Margin.Top, ScreenWidth - tb2.Margin.Right, ScreenHeight - tb2.Margin.Bottom)); // Right
        Assert.Equal(tb3.LayoutSlot, new SKRect(tb1.Width + tb1.Margin.Horizontal + tb3.Margin.Left, tb3.Margin.Top, ScreenWidth - tb2.Width - tb2.Margin.Horizontal - tb3.Margin.Right, tb3.Margin.Top + tb3.Height)); // Top
        Assert.Equal(tb4.LayoutSlot, new SKRect(tb1.Width + tb1.Margin.Horizontal + tb4.Margin.Left, ScreenHeight - tb4.ContentHeight - tb4.Padding.Vertical - tb4.Margin.Bottom, ScreenWidth - tb2.Width - tb2.Margin.Horizontal - tb4.Margin.Right, ScreenHeight - tb4.Margin.Bottom)); // Bottom
        Assert.Equal(tb5.LayoutSlot, new SKRect(tb1.Width + tb1.Margin.Horizontal + tb5.Margin.Left, tb3.Margin.Vertical + tb3.Height + tb5.Margin.Top, ScreenWidth - tb2.Width - tb2.Margin.Horizontal - tb5.Margin.Right, ScreenHeight - tb4.ContentHeight - tb4.Padding.Vertical - tb4.Margin.Vertical - tb5.Margin.Bottom)); // Center
    }

    private static TestBlock[] CreateBlocks(out TestBlock tb1, out TestBlock tb2, out TestBlock tb3, out TestBlock tb4, out TestBlock tb5)
    {
        tb1 = new TestBlock { Width = 200, Height = 100, Margin = new Thickness(10) }.Dock(Dock.Left);
        tb2 = new TestBlock { Width = 190, Height = 90, Margin = new Thickness(9) }.Dock(Dock.Right);
        tb3 = new TestBlock { Width = 180, Height = 80, Margin = new Thickness(8) }.Dock(Dock.Top);
        tb4 = new TestBlock { ContentWidth = 100, ContentHeight = 40, Margin = new Thickness(7), Padding = new Thickness(5) }.Dock(Dock.Bottom);
        tb5 = new TestBlock { ContentWidth = 120, ContentHeight = 50, Margin = new Thickness(6), Padding = new Thickness(4) }; //Center

        return new[] { tb1, tb2, tb3, tb4, tb5 };
    }
}
