using Chnuschti.Controls;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class ChnuschtiApp
{
    public IViewLocator? ViewLocator { get; set; }
    public Screen? Screen { get; set; }

    public void Configure()
    {
        OnStartup();
    }

    protected virtual void OnStartup()
    {
        // Override this method to add startup logic
    }

    private Control? _capturedControl; // The control that has captured the mouse

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
            case InputEventType.KeyDown:
                break;

            case InputEventType.MouseMove:

                VisualElement? mouseOver = null;

                if (_capturedControl != null)
                {
                    mouseOver = VisualTreeHelper.HitTest(_capturedControl, inputEvent.MousePos);
                }

                if (mouseOver == null)
                {
                    mouseOver = VisualTreeHelper.HitTest(Screen, inputEvent.MousePos);
                }

                if (mouseOver is Control mouseOverControl)
                {
                    if (mouseOverControl != _capturedControl)
                    {
                        _capturedControl?.MouseLeave(inputEvent.MousePos); // Leave previous control
                        _capturedControl = mouseOverControl; // Capture the new control
                        if (_capturedControl.IsEnabled)
                        {
                            _capturedControl.MouseEnter(inputEvent.MousePos);
                        }
                    }

                    if (_capturedControl.IsEnabled)
                    {
                        _capturedControl.MouseMove(inputEvent.MousePos);
                    }
                }
                break;

            case InputEventType.MouseDown:
                if (VisualTreeHelper.HitTest(Screen, inputEvent.MousePos) is Control cdown)
                {
                    if (cdown.IsEnabled)
                    {
                        _capturedControl = cdown; // Capture the control
                        _capturedControl.MouseDown(inputEvent.MousePos);
                    }
                }
                else
                {
                    _capturedControl = null; // No control captured
                }
                break;

            case InputEventType.MouseUp:
                _capturedControl?.MouseUp(inputEvent.MousePos);
                _capturedControl = null;
                break;

            default:
                // Handle other input events if necessary
                break;
        }

        return true;
    }
}
