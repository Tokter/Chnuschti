using SkiaSharp;

namespace Chnuschti.Controls;

public class ScrollBar : Control
{
    public ScrollBar()
    {
        // Defaults
        Orientation = Orientation.Vertical;
        SmallChange = 16f;
        LargeChange = 64f;
        Minimum = 0f;
        Maximum = 100f;
        Viewport = 0f;
        IsHitTestVisible = true;
    }

    // ---------------- Dependency Properties ----------------
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ScrollBar),
            new PropertyMetadata(Orientation.Vertical, OnLayoutAffecting));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(float), typeof(ScrollBar),
            new PropertyMetadata(0f, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(float), typeof(ScrollBar),
            new PropertyMetadata(100f, OnRangeChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(float), typeof(ScrollBar),
            new PropertyMetadata(0f, OnValueChanged));

    public static readonly DependencyProperty ViewportProperty =
        DependencyProperty.Register(nameof(Viewport), typeof(float), typeof(ScrollBar),
            new PropertyMetadata(0f, OnLayoutAffecting)); // affects handle size

    public static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(nameof(SmallChange), typeof(float), typeof(ScrollBar),
            new PropertyMetadata(16f));

    public static readonly DependencyProperty LargeChangeProperty =
        DependencyProperty.Register(nameof(LargeChange), typeof(float), typeof(ScrollBar),
            new PropertyMetadata(64f));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty)!;
        set => SetValue(OrientationProperty, value);
    }

    public float Minimum
    {
        get => (float)GetValue(MinimumProperty)!;
        set => SetValue(MinimumProperty, value);
    }

    public float Maximum
    {
        get => (float)GetValue(MaximumProperty)!;
        set => SetValue(MaximumProperty, value);
    }

    public float Value
    {
        get => (float)GetValue(ValueProperty)!;
        set => SetValue(ValueProperty, ClampValue(value));
    }

    public float Viewport
    {
        get => (float)GetValue(ViewportProperty)!;
        set => SetValue(ViewportProperty, Math.Max(0, value));
    }

    public float SmallChange
    {
        get => (float)GetValue(SmallChangeProperty)!;
        set => SetValue(SmallChangeProperty, Math.Max(0, value));
    }

    public float LargeChange
    {
        get => (float)GetValue(LargeChangeProperty)!;
        set => SetValue(LargeChangeProperty, Math.Max(0, value));
    }

    // ------------- Events ---------------
    public event EventHandler<float>? ValueChanged;

    // ------------- Internal State -------------
    private bool _isDragging;
    private float _dragStartValue;
    private float _dragStartCoord; // Y or X depending on orientation

    internal float GetRange() => Math.Max(0f, Maximum - Minimum);
    private float ClampValue(float v)
    {
        var range = GetRange();
        if (range <= 0) return Minimum;
        return MathFEX.Clamp(v, Minimum, Maximum);
    }

    private static void OnValueChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var sb = (ScrollBar)d;
        sb.InvalidateDrawResources();
        sb.ValueChanged?.Invoke(sb, sb.Value);
    }

    private static void OnRangeChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        var sb = (ScrollBar)d;
        // Adjust value if out of range
        sb.Value = sb.ClampValue(sb.Value);
        sb.InvalidateDrawResources();
    }

    private static void OnLayoutAffecting(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        if (d is ScrollBar sb)
        {
            sb.InvalidateMeasure();
        }
    }

    // ---------- Interaction (drag + track click) ----------
    public override void MouseDown(SKPoint screenPt)
    {
        base.MouseDown(screenPt);
        _isDragging = true;
        var local = PointFromScreen(screenPt);
        _dragStartCoord = Orientation == Orientation.Vertical ? local.Y : local.X;
        _dragStartValue = Value;
    }

    public override void MouseMove(SKPoint screenPt)
    {
        if (!_isDragging) return;
        var local = PointFromScreen(screenPt);

        var (trackStart, trackLen, handleLen) = GetLayoutMetrics();
        var coord = Orientation == Orientation.Vertical ? local.Y : local.X;

        var delta = coord - _dragStartCoord;
        var range = GetRange();
        if (range <= 0 || trackLen <= handleLen) return;

        var movable = trackLen - handleLen;
        var valueDelta = (delta / movable) * range;
        Value = ClampValue(_dragStartValue + valueDelta);
    }

    public override void MouseUp(SKPoint screenPt)
    {
        if (_isDragging)
        {
            _isDragging = false;
        }
        base.MouseUp(screenPt);
    }

    protected override void OnClick(object? sender, EventArgs e)
    {
        // Clicking on track (not dragging handle) does page jump
        if (_isDragging) return;
        //var local = PointFromScreen((SKPoint)sender!); // sender is screen point from base? If not skip
    }

    protected override bool IsPointInsideLocal(SKPoint local)
        => base.IsPointInsideLocal(local); // full rect hit area

    // ----- Metrics exposed to Renderer -----
    internal (float trackStart, float trackLength, float handleLength, float handleOffset) GetRenderMetrics()
    {
        var (trackStart, trackLen, handleLen) = GetLayoutMetrics();
        var range = GetRange();
        float handleOffset = 0f;
        if (range > 0 && trackLen > handleLen)
        {
            var movable = trackLen - handleLen;
            handleOffset = ((Value - Minimum) / range) * movable;
        }
        return (trackStart, trackLen, handleLen, handleOffset);
    }

    private (float trackStart, float trackLength, float handleLength) GetLayoutMetrics()
    {
        // Inside ContentBounds
        float cross = Orientation == Orientation.Vertical ? ContentBounds.Width : ContentBounds.Height;
        float main = Orientation == Orientation.Vertical ? ContentBounds.Height : ContentBounds.Width;

        // Track spans entire main-axis
        float trackStart = 0f;
        float trackLen = main;

        // Handle size based on viewport (if supplied)
        var range = GetRange();
        float handleLen;
        if (Viewport > 0 && range > 0)
        {
            var fraction = Math.Clamp(Viewport / (range + Viewport), 0f, 1f);
            handleLen = Math.Max(ThemeManager.Current.Height * 0.75f, trackLen * fraction); // min handle size
        }
        else
        {
            handleLen = Math.Min(trackLen, ThemeManager.Current.Height); // fallback
        }

        return (trackStart, trackLen, handleLen);
    }
}