using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chnuschti.Controls;

public class Control : DataElement
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(Control), new PropertyMetadata(true, OnIsEnabledChanged, inherits: true));
    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(nameof(FontSize), typeof(float), typeof(Control), new PropertyMetadata(14f, OnInvalidateDrawResources));
    public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(nameof(FontFamily), typeof(string), typeof(Control), new PropertyMetadata(null, OnInvalidateDrawResources));
    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(SKColor), typeof(Control), new PropertyMetadata(SKColors.Black, OnInvalidateDrawResources));
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(SKColor), typeof(Control), new PropertyMetadata(SKColors.SlateGray));
    public static readonly DependencyProperty BoldProperty = DependencyProperty.Register(nameof(Bold), typeof(bool), typeof(Control), new PropertyMetadata(false, OnInvalidateDrawResources));

    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty)!;
        set => SetValue(IsEnabledProperty, value);
    }

    public float FontSize { get => (float)GetValue(FontSizeProperty)!; set => SetValue(FontSizeProperty, value); }
    public string? FontFamily { get => (string?)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public SKColor Foreground { get => (SKColor)GetValue(ForegroundProperty)!; set => SetValue(ForegroundProperty, value); }
    public SKColor Background { get => (SKColor)GetValue(BackgroundProperty)!; set => SetValue(BackgroundProperty, value); }
    public bool Bold { get => (bool)GetValue(BoldProperty)!; set => SetValue(BoldProperty, value); }

    //Convert string to Label
    public static implicit operator Control(string text)
    {
        return new Label { Text = text, Margin = new Thickness(4) };
    }

    private bool _isPressed = false;
    public bool IsPressed
    {
        get => _isPressed;
        set
        {
            if (_isPressed == value) return;
            _isPressed = value;
            InvalidateDrawResources(); // will also invalidate measure, arrange & matrices
        }
    }

    private bool _isMouseOver = false;
    public bool IsMouseOver
    {
        get => _isMouseOver;
        set
        {
            if (_isMouseOver == value) return;
            _isMouseOver = value;
            InvalidateDrawResources(); // will also invalidate measure, arrange & matrices
        }
    }

    public void MouseDown(SKPoint screenPt)
    {
        IsPressed = true;
    }

    public virtual void MouseUp(SKPoint screenPt)
    {
        if (!IsPressed) return;
        IsPressed = false;
        if (IsEnabled && IsMouseOver) OnClick(this, EventArgs.Empty);
    }

    public virtual void MouseMove(SKPoint screenPt)
    {
    }

    public virtual void MouseEnter(SKPoint screenPt)
    {
        IsMouseOver = true;
    }

    public virtual void MouseLeave(SKPoint screenPt)
    {
        IsMouseOver = false;
    }

    protected virtual void OnClick(object? sender, EventArgs e)
    {
    }

    // --------------------------------------------------------------------
    //  Property-change helpers
    // --------------------------------------------------------------------
    private static void OnInvalidateDrawResources(DependencyObject d, DependencyProperty p, object? oldV, object? newV)
    {
        if (d is Control c)
        {
            c.InvalidateDrawResources(); // will also invalidate measure, arrange & matrices
        }
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyProperty p, object? oldV, object? newV)
    {
        if (d is Control c)
        {
            c.InvalidateDrawResources();      // redraw the element itself
            PropagateInvalidate(c);           // and all its children
        }

        static void PropagateInvalidate(VisualElement root)
        {
            foreach (var child in root.Children)
            {
                if (child is Control ctrl) ctrl.InvalidateDrawResources();
                PropagateInvalidate(child);                 // depth-first
            }
        }

    }
}