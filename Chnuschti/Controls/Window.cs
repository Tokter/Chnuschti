using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Chnuschti.InputEvent;

namespace Chnuschti.Controls;

public enum WindowState
{
    Normal,
    Minimized,
    Maximized
}

public class Window : ContentControl
{
    private readonly Random rand = new();

    public Window()
    {
        InitializeComponent();
        IsRoot = true;
    }

    protected virtual void InitializeComponent()
    {
    }

    public void Show()
    {
        ChnuschtiApp.Current.Platform?.CreateWindow(this);
    }

    public void Close()
    {
        ChnuschtiApp.Current.Platform?.CloseWindow(this);
    }

    public WindowState WindowState
    {
        get => ChnuschtiApp.Current.Platform?.GetPlatformWindow(this)?.WindowState ?? WindowState.Normal;
        set
        {
            var platformWindow = ChnuschtiApp.Current.Platform?.GetPlatformWindow(this);
            if (platformWindow != null)
            {
                platformWindow.WindowState = value;
            }
        }
    }


    #region Rendering

    private readonly FrameTimer _timer = new();

    public void Render(SKCanvas canvas)
    {
        _timer.Tick();

        //Layout the screen
        Measure(new SKSize(Width, Height));
        Arrange(new SKRect(0, 0, Width, Height));

        Render(canvas, _timer.DeltaTime);

        //Draw mouse position for debugging at the bottom left corner
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
        };
        using var font = new SKFont
        {
            Size = 16 * Scale,
        };

        canvas.DrawText($"Mouse: {_mousePosX / Scale:F2}, {_mousePosY / Scale:F2}", 10 * Scale, Height - 10 * Scale, font, paint);

        //Draw FPS at the top right corner
        canvas.DrawText($"FPS: {_timer.Fps:F2}", Width - 80 * Scale, 15 * Scale, font, paint);

    }

    public void SetSize(float width, float height)
    {
        Width = width / Scale;
        Height = height / Scale;
    }

    #endregion

    #region Input handling

    private float _mousePosX = 0.0f;
    private float _mousePosY = 0.0f;
    private Control? _mouseOver;     // who the cursor is currently over (hover)
    private Control? _mouseCapture;  // who owns the mouse during a drag
    private Control? _pressed;       // who is currently pressed (for visual state)

    /// <summary>
    /// Processes the specified input event and determines whether it was handled successfully.
    /// </summary>
    /// <param name="inputEvent">The input event to process. Cannot be null.</param>
    /// <returns><see langword="true"/> if the input event was handled successfully; otherwise, <see langword="false"/>.</returns>
    public bool ProcessInputEvent(InputEvent inputEvent)
    {
        switch (inputEvent.InputEventType)
        {
            case InputEventType.MouseMove:
                {
                    _mousePosX = inputEvent.MousePos.X;
                    _mousePosY = inputEvent.MousePos.Y;

                    if (_mouseCapture != null)
                    {
                        // While captured → no hit-test, just forward moves
                        if (_mouseCapture.IsEnabled) _mouseCapture.MouseMove(inputEvent.MousePos);
                        return true;
                    }

                    // No capture → maintain hover enter/leave and forward move to hovered control
                    var hit = VisualTreeHelper.HitTest(this, inputEvent.MousePos) as Control;

                    if (!ReferenceEquals(hit, _mouseOver))
                    {
                        _mouseOver?.MouseLeave(inputEvent.MousePos);
                        _mouseOver = hit;
                        if (_mouseOver?.IsEnabled == true) _mouseOver.MouseEnter(inputEvent.MousePos);
                    }

                    if (_mouseOver?.IsEnabled == true) _mouseOver.MouseMove(inputEvent.MousePos);
                    break;
                }

            case InputEventType.MouseDown:
                {
                    // Find control under cursor and capture it immediately
                    _pressed = VisualTreeHelper.HitTest(this, inputEvent.MousePos) as Control;

                    // Update hover state to the one we clicked, for consistency
                    if (!ReferenceEquals(_pressed, _mouseOver))
                    {
                        _mouseOver?.MouseLeave(inputEvent.MousePos);
                        _mouseOver = _pressed;
                        if (_mouseOver?.IsEnabled == true) _mouseOver.MouseEnter(inputEvent.MousePos);
                    }

                    if (_mouseOver?.IsEnabled == true)
                    {
                        _mouseCapture = _mouseOver;           // <- capture starts here
                        _mouseCapture.MouseDown(inputEvent.MousePos);
                    }
                    else
                    {
                        _mouseCapture = null;
                    }
                    break;
                }

            case InputEventType.MouseUp:
                {
                    var wasPressed = _pressed;
                    _pressed = null;

                    if (_mouseCapture != null)
                    {
                        var cap = _mouseCapture;
                        _mouseCapture = null;
                        if (cap.IsEnabled) cap.MouseUp(inputEvent.MousePos);

                        // After releasing, re-evaluate hover under current cursor
                        var hit = VisualTreeHelper.HitTest(this, inputEvent.MousePos) as Control;
                        if (!ReferenceEquals(hit, _mouseOver))
                        {
                            _mouseOver?.MouseLeave(inputEvent.MousePos);
                            _mouseOver = hit;
                            if (_mouseOver?.IsEnabled == true) _mouseOver.MouseEnter(inputEvent.MousePos);
                        }
                    }
                    else
                    {
                        // No capture → normal mouse up on hovered control
                        if (_mouseOver?.IsEnabled == true) _mouseOver.MouseUp(inputEvent.MousePos);
                    }

                    // If this up created/shows a submenu, don't treat it as an "outside click".
                    if (wasPressed is MenuItem mi && mi.IsSubmenuOpen) break;

                    if (PopupManager.TryHandleOutsideClick(inputEvent.MousePos))
                    {
                        foreach (var menu in this.DescendantsOfType<Menu>())
                            menu.CloseAllSubmenus();
                    }
                    break;
                }

            case InputEventType.KeyDown:
                {
                    // Tab navigation is app-level
                    if (inputEvent.Key == Key.Tab)
                    {
                        if (inputEvent.Shift) FocusManager.Instance.MoveFocusPrev(this);
                        else FocusManager.Instance.MoveFocusNext(this);
                        return true; // handled
                    }

                    // Esc to clear focus? (optional)
                    // if (e.Key == Key.Escape) { FocusManager.Instance.ClearFocus(); return true; }

                    FocusManager.Instance.DispatchKeyDown(inputEvent);
                    break;
                }

            case InputEventType.KeyUp:
                {
                    FocusManager.Instance.DispatchKeyUp(inputEvent);
                    break;
                }

            case InputEventType.TextInput:
                {
                    if (inputEvent.Text != null) FocusManager.Instance.DispatchTextInput(new TextInputEvent(inputEvent.Text));
                    break;
                }

        }

        return true;
    }

    #endregion
}
