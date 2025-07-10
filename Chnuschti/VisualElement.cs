using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class VisualElement : DependencyObject, IDisposable
{
    public long Id { get; } = Interlocked.Increment(ref _idCounter);
    private static long _idCounter;

    #region Style
    private Style? _style;
    private string _styleKey = string.Empty; // key for style lookup

    public Style? Style
    {
        get
        {
            if (_style == null)
            {
                LookupStyle(); // lazy lookup
            }
            return _style;
        }
        set
        {
            if (_style != value)
            {
                LookupStyle();
            }
        }
    }

    public string StyleKey
    {
        get => _styleKey;
        set
        {
            if (_styleKey != value)
            {
                _styleKey = value;
                LookupStyle();
            }
        }
    }

    private void LookupStyle()
    {
        var thisType = this.GetType();
        Style? newStyle = null;
        do
        {
            newStyle = ThemeManager.Current.Resources.Get<Style>(thisType, _styleKey);
            if (newStyle == null)
            {
                thisType = thisType.BaseType; // walk up the inheritance chain
            }
        } while (newStyle == null && thisType != null);


        if (newStyle != null && newStyle != _style)
        {
            var oldStyle = _style;
            _style = newStyle;
            ChangeStyle(oldStyle, _style);
        }
    }

    #endregion

    #region Layout
    private bool _localDirty = true;   // local 2-D transform changed
    private bool _worldDirty = true;   // parent or local changed
    private SKMatrix _localMatrix;     // cached translation·rotation·scale→parent
    private SKMatrix _worldMatrix;     // cached cumulative matrix to root
    private SKMatrix _invWorldMatrix;  // inverse of world matrix
    private bool _isMeasureValid;
    private bool _isArrangeValid;

    public static readonly DependencyProperty MarginProperty = DependencyProperty.Register(nameof(Margin), typeof(Thickness), typeof(VisualElement), new PropertyMetadata(new Thickness()));
    public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(VisualElement), new PropertyMetadata(new Thickness()));
    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(nameof(Width), typeof(float), typeof(VisualElement), new PropertyMetadata(float.NaN));
    public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(nameof(Height), typeof(float), typeof(VisualElement), new PropertyMetadata(float.NaN));
    public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(nameof(MinWidth), typeof(float), typeof(VisualElement), new PropertyMetadata(0f));
    public static readonly DependencyProperty MinHeightProperty = DependencyProperty.Register(nameof(MinHeight), typeof(float), typeof(VisualElement), new PropertyMetadata(0f));
    public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(nameof(MaxWidth), typeof(float), typeof(VisualElement), new PropertyMetadata(float.PositiveInfinity));
    public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register(nameof(MaxHeight), typeof(float), typeof(VisualElement), new PropertyMetadata(float.PositiveInfinity));
    public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(VisualElement), new PropertyMetadata(true, OnVisibilityChanged));

    public Thickness Margin
    {
        get => (Thickness)GetValue(MarginProperty)!;
        set => SetValue(MarginProperty, value);
    }

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty)!;
        set => SetValue(PaddingProperty, value);
    }

    public float Width
    {
        get => (float)GetValue(WidthProperty)!;
        set => SetValue(WidthProperty, value);
    }

    public float Height
    {
        get => (float)GetValue(HeightProperty)!;
        set => SetValue(HeightProperty, value);
    }

    public float MinWidth
    {
        get => (float)GetValue(MinWidthProperty)!;
        set => SetValue(MinWidthProperty, value);
    }

    public float MinHeight
    {
        get => (float)GetValue(MinHeightProperty)!;
        set => SetValue(MinHeightProperty, value);
    }

    public float MaxWidth
    {
        get => (float)GetValue(MaxWidthProperty)!;
        set => SetValue(MaxWidthProperty, value);
    }

    public float MaxHeight
    {
        get => (float)GetValue(MaxHeightProperty)!;
        set => SetValue(MaxHeightProperty, value);
    }

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty)!;
        set => SetValue(IsVisibleProperty, value);
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyProperty p, object? oldValue, object? newValue)
    {
        if (d is VisualElement element) element.InvalidateMeasure(); // This will trigger layout recalculation
    }

    protected void InvalidateMeasure()
    {
        if (_isMeasureValid)
        {
            _isMeasureValid = false;
            InvalidateArrange();          // arrange depends on measure
        }
    }

    protected void InvalidateArrange()
    {
        if (_isArrangeValid)
        {
            _isArrangeValid = false;
            MatrixInvalidated();// layout moves → matrix dirty
        }
    }

    internal void MatrixInvalidated()
    {
        _worldDirty = true;             // mark my cache dirty                
        foreach (var child in _children) // keep bubbling down
        {
            child.MatrixInvalidated();
        }
    }

    /// <summary>
    /// Gets the desired size of the element, including its margin.
    /// </summary>
    public SKSize DesiredSize => _desiredSize;
    private SKSize _desiredSize;

    /// <summary>
    /// Gets the bounds of the content, excluding any padding.
    /// </summary>
    public SKRect ContentBounds => _contentBounds;
    private SKRect _contentBounds;

    /// <summary>
    /// First walk – child tells parent how much space it wants.
    /// </summary>
    /// <param name="availableSize"></param>
    public void Measure(SKSize availableSize)
    {
        if (_isMeasureValid) return;
        _isMeasureValid = true;
        UpdateDrawResources();

        //subtract margin from availableSize
        var availableContentSize = new SKSize(Math.Max(0, availableSize.Width - Margin.Horizontal), Math.Max(0, availableSize.Height - Margin.Vertical));

        //let child override to compute its own size (defaults to 0,0)
        var desiredSize = (_style?.Renderer?.Measure(this, availableContentSize) ?? SKSize.Empty) + Padding;

        //use explicit sizing if Widht/Height are set
        var w = float.IsNaN(Width) ? desiredSize.Width : Width;
        var h = float.IsNaN(Height) ? desiredSize.Height : Height;

        //clamp to min/max
        w = MathFEX.Clamp(w, MinWidth, MaxWidth);
        h = MathFEX.Clamp(h, MinHeight, MaxHeight);

        _desiredSize = new SKSize(w + Margin.Horizontal, h + Margin.Vertical);
    }

    private SKRect _layoutSlot;

    /// <summary>
    /// Arranges the layout of the control within the specified slot.
    /// </summary>
    /// <remarks>This method updates the layout slot and calculates the content bounds by removing padding
    /// from the specified slot. It then arranges the control's child elements within the adjusted bounds.</remarks>
    /// <param name="slot">The rectangular area, defined in screen coordinates, within which the control should be arranged.
    /// Slot does not include Margin
    /// </param>
    public void Arrange(SKRect slot)
    {
        if (_isArrangeValid && slot == _layoutSlot) return;

        _layoutSlot = slot;
        _isArrangeValid = true;

        // parent already removed Margin → remove Padding
        _contentBounds = ShrinkBy(slot, Padding);

        // let the control position its kids
        ArrangeContent(ToLocal(_contentBounds));
    }

    /// <summary>
    /// Override to position/size content-specific visuals.
    /// </summary>
    /// <param name="contentRect">
    /// The rectangle in which the content should be arranged, defined in local coordinates.
    /// </param>
    protected virtual void ArrangeContent(SKRect contentRect)
    {
        foreach (var c in _children)
        {
            if (!c.IsVisible) continue; // Skip invisible children

            var desired = c._desiredSize;
            var r = new SKRect(contentRect.Left, contentRect.Top, contentRect.Left + desired.Width - c.Margin.Horizontal, contentRect.Top + desired.Height - c.Margin.Vertical);

            c.Arrange(r);
        }
    }

    internal static SKRect ToLocal(in SKRect parentRect) => new(0, 0, parentRect.Width, parentRect.Height);
    internal static SKRect ShrinkBy(in SKRect rect, in Thickness m) => new(rect.Left + m.Left, rect.Top + m.Top, rect.Right - m.Right, rect.Bottom - m.Bottom);

    #endregion

    #region Scene Graph

    public static readonly DependencyProperty ScaleXProperty = DependencyProperty.Register(nameof(ScaleX), typeof(float), typeof(VisualElement), new PropertyMetadata(1f, OnLocalTransformChanged, inherits: false));
    public static readonly DependencyProperty ScaleYProperty = DependencyProperty.Register(nameof(ScaleY), typeof(float), typeof(VisualElement), new PropertyMetadata(1f, OnLocalTransformChanged, inherits: false));
    public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(nameof(Rotation), typeof(float), typeof(VisualElement), new PropertyMetadata(0f, OnLocalTransformChanged, inherits: false));

    public float ScaleX
    {
        get => (float)GetValue(ScaleXProperty)!;
        set => SetValue(ScaleXProperty, value);
    }

    public float ScaleY
    {
        get => (float)GetValue(ScaleYProperty)!;
        set => SetValue(ScaleYProperty, value);
    }

    /// <summary>Rotation in degrees (clockwise).</summary>
    public float Rotation
    {
        get => (float)GetValue(RotationProperty)!;
        set => SetValue(RotationProperty, value);
    }

    private readonly List<VisualElement> _children = new();
    public IReadOnlyList<VisualElement> Children => _children;

    public VisualElement Add(VisualElement child)
    {
        child.ParentInternal = this;
        _children.Add(child);
        this.InvalidateMeasure(); // child added, measure may change
        return this;
    }

    public void Add(params VisualElement[] children)
    {
        foreach (var child in children)
        {
            Add(child);
        }
    }

    protected void ReplaceVisualChild(VisualElement? oldChild, VisualElement? newChild)
    {
        if (oldChild != null)
        {
            _children.Remove(oldChild);
            oldChild.ParentInternal = null;
            oldChild.InvalidateMeasure(); // child removed, measure may change
        }

        if (newChild != null)
        {
            if (_children.Contains(newChild))
            {
                // If the new child is already in the list, just update its parent
                newChild.ParentInternal = this;
                return;
            }
            else
            {
                _children.Add(newChild);
                newChild.ParentInternal = this;
                this.InvalidateMeasure(); // child added, measure may change
            }
        }
    }

    private SKMatrix LocalMatrix
    {
        get
        {
            if (_localDirty)
            {
                _localMatrix = SKMatrix.CreateIdentity();
                _localMatrix = _localMatrix.PreConcat(
                SKMatrix.CreateTranslation(_contentBounds.Left, _contentBounds.Top));
                _localMatrix = _localMatrix.PreConcat(SKMatrix.CreateScale(ScaleX, ScaleY));
                _localMatrix = _localMatrix.PreConcat(SKMatrix.CreateRotationDegrees(Rotation));
                _localDirty = false;
            }
            return _localMatrix;
        }
    }

    private SKMatrix WorldMatrix
    {
        get
        {
            if (_worldDirty)
            {
                if (ParentInternal is VisualElement p)
                    _worldMatrix = SKMatrix.Concat(p.WorldMatrix, LocalMatrix);
                else
                    _worldMatrix = LocalMatrix;

                _invWorldMatrix = _worldMatrix.Invert(); // cache inverse

                _worldDirty = false;
            }
            return _worldMatrix;
        }
    }

    public SKPoint PointToScreen(SKPoint local) => _worldMatrix.MapPoint(local);
    public SKPoint PointFromScreen(SKPoint screen) => _invWorldMatrix.MapPoint(screen);

    static void OnLocalTransformChanged(DependencyObject d, DependencyProperty p, object? o, object? n)
    {
        if (d is VisualElement v)
        {
            v._localDirty = true; // local transform changed
            v.MatrixInvalidated(); // bubble to children
        }
    }

    #endregion

    #region Rendering

    public void Render(SKCanvas canvas, double deltaTime)
    {
        if (!IsVisible) return;
        UpdateDrawResources();

        canvas.Save();
        canvas.SetMatrix(WorldMatrix);         // overwrite, no Concat chain
        Style?.Renderer?.Render(this, canvas, deltaTime);

        foreach (var c in _children) c.Render(canvas, deltaTime);
#if DEBUG
        DrawLayoutDebug(canvas);
#endif
        canvas.Restore();
    }

    public static bool ShowLayoutDebug = false;

    /// <summary>Draws two rectangles around this element:<br/>
    /// • **red** = outer edge of <c>Margin</c><br/>
    /// • **blue** = outer edge of <c>Padding</c></summary>
    public void DrawLayoutDebug(SKCanvas c)
    {
        if (!ShowLayoutDebug) return;

        // margins extend *outside* the padding-boxed content bounds
        var w = ContentBounds.Width;
        var h = ContentBounds.Height;

        var marginRect = new SKRect(-Margin.Left, -Margin.Top, w + Margin.Right, h + Margin.Bottom);

        var paddingRect = new SKRect(0, 0, w, h);

        using var penMargin = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(255, 50, 50, 90) };
        using var penPadding = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(50, 150, 255, 90) };

        c.DrawRect(marginRect, penMargin);

        c.DrawRect(paddingRect, penPadding);
    }

    #endregion

    #region Hit Test

    public static readonly DependencyProperty IsHitTestVisibleProperty = DependencyProperty.Register(nameof(IsHitTestVisible), typeof(bool), typeof(VisualElement), new PropertyMetadata(true));

    public bool IsHitTestVisible
    {
        get => (bool)GetValue(IsHitTestVisibleProperty)!;
        set => SetValue(IsHitTestVisibleProperty, value);
    }

    public VisualElement? HitTest(SKPoint screenPt)
    {
        if (!IsHitTestVisible) return null;

        // 1) children first – last rendered = top-most
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            var hit = _children[i].HitTest(screenPt);
            if (hit != null) return hit;
        }

        // 2) self: map to local and hit the content rect
        var local = PointFromScreen(screenPt);
        if (IsPointInsideLocal(local)) return this;

        return null;
    }

    protected virtual bool IsPointInsideLocal(SKPoint local)
    {
        // local (0,0) .. (ContentBounds.Width, ContentBounds.Height)
        return local.X >= 0 && local.Y >= 0 &&
               local.X <= ContentBounds.Width &&
               local.Y <= ContentBounds.Height;
    }

    #endregion

    #region Draw Resource handling
    private bool _drawResourcesDirty = true;
    private bool disposedValue;

    public void InvalidateDrawResources()
    {
        _drawResourcesDirty = true; // mark paint cache dirty
        InvalidateMeasure(); // measure may depend on paint (e.g. text size)
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateDrawResources()
    {
        if (!_drawResourcesDirty) return;
        _drawResourcesDirty = false;
        _style?.Renderer?.UpdateResources(this);
    }

    private void DisposeDrawResources()
    {
        _style?.Renderer?.DeleteResources(this);
    }
    
    private void ChangeStyle(Style? oldStyle, Style? newStyle)
    {
        oldStyle?.Renderer?.DeleteResources(this);
        InvalidateDrawResources();
    }

    #endregion

    #region Disposal
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var child in _children)
                {
                    child.Dispose(); // Dispose children
                }
                DisposeDrawResources(); // Dispose paint resources
            }
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}