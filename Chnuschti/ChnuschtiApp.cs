using Chnuschti.Controls;
using Microsoft.VisualBasic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class ChnuschtiApp
{
    private float MousePosX = 0.0f;
    private float MousePosY = 0.0f;

    public IViewLocator? ViewLocator { get; set; }
    public Screen? Screen { get; set; }

    public void Configure()
    {
        HotReloadManager.RegisterApp(this);
        OnStartup();
    }

    protected virtual void OnStartup()
    {
        // Override this method to add startup logic
    }

    private Control? _mouseOver;     // who the cursor is currently over (hover)
    private Control? _mouseCapture;  // who owns the mouse during a drag

    public float Scale { get; set; } = 1.0f;
    public float ScreenWidth { get; set; }
    public float ScreenHeight { get; set; }

    public void SetSize(float width, float height)
    {
        ScreenWidth = width;
        ScreenHeight = height;

        if (Screen == null) return; // Ensure Screen is initialized
        Screen.ScaleX = Scale;
        Screen.ScaleY = Scale;
    }

    private readonly FrameTimer _timer = new();

    public void Render(SKCanvas canvas)
    {
        _timer.Tick();

        if (Screen == null) return; // Render the screen content

        //Layout the screen
        Screen.Measure(new SKSize(ScreenWidth / Scale, ScreenHeight / Scale));
        Screen.Arrange(new SKRect(0, 0, ScreenWidth / Scale, ScreenHeight / Scale));

        Screen.Render(canvas, _timer.DeltaTime);

        //Draw mouse position for debugging at the bottom left corner
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
        };
        using var font = new SKFont
        {
            Size = 16 * Screen.ScaleY,
        };
        canvas.DrawText($"Mouse: {MousePosX / Scale:F2}, {MousePosY / Scale:F2}", 10 * Scale, ScreenHeight - 10 * Scale, font, paint);

        //Draw FPS at the top right corner
        canvas.DrawText($"FPS: {_timer.Fps:F2}", ScreenWidth - 80 * Scale, 15 * Scale, font, paint);

    }

    /// <summary>
    /// Processes the specified input event and determines whether it was handled successfully.
    /// </summary>
    /// <param name="inputEvent">The input event to process. Cannot be null.</param>
    /// <returns><see langword="true"/> if the input event was handled successfully; otherwise, <see langword="false"/>.</returns>
    public bool ProcessInputEvent(InputEvent inputEvent)
    {
        if (Screen == null) return false;

        switch (inputEvent.InputEventType)
        {
            case InputEventType.MouseMove:
                {
                    MousePosX = inputEvent.MousePos.X;
                    MousePosY = inputEvent.MousePos.Y;

                    if (_mouseCapture != null)
                    {
                        // While captured → no hit-test, just forward moves
                        if (_mouseCapture.IsEnabled) _mouseCapture.MouseMove(inputEvent.MousePos);
                        return true;
                    }

                    // No capture → maintain hover enter/leave and forward move to hovered control
                    var hit = VisualTreeHelper.HitTest(Screen, inputEvent.MousePos) as Control;

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
                    var hit = VisualTreeHelper.HitTest(Screen, inputEvent.MousePos) as Control;

                    // Update hover state to the one we clicked, for consistency
                    if (!ReferenceEquals(hit, _mouseOver))
                    {
                        _mouseOver?.MouseLeave(inputEvent.MousePos);
                        _mouseOver = hit;
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
                    if (_mouseCapture != null)
                    {
                        var cap = _mouseCapture;
                        _mouseCapture = null;
                        if (cap.IsEnabled) cap.MouseUp(inputEvent.MousePos);

                        // After releasing, re-evaluate hover under current cursor
                        var hit = VisualTreeHelper.HitTest(Screen, inputEvent.MousePos) as Control;
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
                    break;
                }

            case InputEventType.KeyDown:
                {
                    // Tab navigation is app-level
                    if (inputEvent.Key == Key.Tab)
                    {
                        if (inputEvent.Shift) FocusManager.Instance.MoveFocusPrev(Screen);
                        else FocusManager.Instance.MoveFocusNext(Screen);
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
                    // This event should carry characters (already layout/IME-processed)
                    //FocusManager.Instance.DispatchTextInput(new TextInputEvent(inputEvent.Text));
                    break;
                }

        }

        return true;
    }

}
