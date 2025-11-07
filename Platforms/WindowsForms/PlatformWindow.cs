using Chnuschti;
using Chnuschti.Controls;
using Chnuschti.WindowsForms;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsForms
{
    public class PlatformWindow : IPlatformWindow, IDisposable
    {
        private Window _chnuschtiWindow;
        private PlatformForm _window;
        private FocusableSKGLControl _skglControl;
        private string _title = "Chnuschti - Windows Forms";

        public PlatformWindow(Window chnuschtiWindow)
        {
            _chnuschtiWindow = chnuschtiWindow;

            _window = new PlatformForm();
            _window.SuspendLayout();

            _skglControl = new FocusableSKGLControl();
            _skglControl.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            _skglControl.APIVersion = new Version(3, 3, 0, 0);
            _skglControl.Dock = DockStyle.Fill;
            _skglControl.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            _skglControl.IsEventDriven = true;
            _skglControl.Location = new Point(0, 0);
            _skglControl.Name = "skglControl";
            _skglControl.Profile = OpenTK.Windowing.Common.ContextProfile.Core;
            _skglControl.SharedContext = null;
            _skglControl.TabIndex = 0;

            _skglControl.Resize += Control_Resize;
            _skglControl.PaintSurface += Control_PaintSurface;
            _skglControl.KeyDown += Control_KeyDown;
            _skglControl.KeyUp += Control_KeyUp;
            _skglControl.KeyPress += Control_KeyPress;
            _skglControl.MouseMove += Control_MouseMove;
            _skglControl.MouseDown += Control_MouseDown;
            _skglControl.MouseUp += Control_MouseUp;
            _skglControl.MouseWheel += Control_MouseWheel;

            _window.AutoScaleMode = AutoScaleMode.None;
            _window.ClientSize = new Size((int)chnuschtiWindow.Width, (int)chnuschtiWindow.Height);
            _window.Controls.Add(_skglControl);
            _window.Name = "ChnuschtiWindow";
            _window.Text = _title;
            //_window.Padding = new Padding(20);

            //Center the window on screen
            _window.StartPosition = FormStartPosition.CenterScreen;
            _window.FormBorderStyle = FormBorderStyle.None;

            _window.Shown += (s, e) => _skglControl.Focus();

            _window.ResumeLayout(false);
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                _window.Text = value;
            }
        }

        private void Control_PaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            _chnuschtiWindow.Render(e.Surface.Canvas);
            _skglControl.Invalidate();
        }

        private void Control_Resize(object? sender, EventArgs e)
        {
            _chnuschtiWindow.SetSize(_skglControl.Width, _skglControl.Height);
        }

        public void Dispose()
        {
            _window.Close();

            _skglControl.Resize -= Control_Resize;
            _skglControl.PaintSurface -= Control_PaintSurface;
            _skglControl.KeyDown -= Control_KeyDown;
            _skglControl.KeyUp -= Control_KeyUp;
            _skglControl.KeyPress -= Control_KeyPress;
            _skglControl.MouseMove -= Control_MouseMove;
            _skglControl.MouseDown -= Control_MouseDown;
            _skglControl.MouseUp -= Control_MouseUp;
            _skglControl.MouseWheel -= Control_MouseWheel;
            _skglControl.Dispose(); 
            _window.Dispose();
        }

        public void Run()
        {
            Application.Run(_window);
        }

        #region Input handling

        bool _lastShift = false;
        bool _lastControl = false;
        bool _lastAlt = false;

        private void Control_KeyDown(object? sender, KeyEventArgs e)
        {
            _lastShift = e.Shift;
            _lastControl = e.Control;
            _lastAlt = e.Alt;
            _chnuschtiWindow.ProcessInputEvent(InputEvent.KeyDown((Key)e.KeyValue, _lastShift, _lastControl, _lastAlt));
        }

        private void Control_KeyUp(object? sender, KeyEventArgs e)
        {
            _lastShift = e.Shift;
            _lastControl = e.Control;
            _lastAlt = e.Alt;
            _chnuschtiWindow.ProcessInputEvent(InputEvent.KeyUp((Key)e.KeyValue, _lastShift, _lastControl, _lastAlt));
        }

        private void Control_KeyPress(object? sender, KeyPressEventArgs e)
        {
            _chnuschtiWindow.ProcessInputEvent(InputEvent.KeyPress(e.KeyChar.ToString()));
        }

        public void Control_MouseMove(object? sender, MouseEventArgs e)
        {
            _chnuschtiWindow.ProcessInputEvent(InputEvent.MouseMove(e.X, e.Y, (Chnuschti.MouseButtons)((int)e.Button >> 20), _lastShift, _lastControl, _lastAlt));
        }

        private void Control_MouseDown(object? sender, MouseEventArgs e)
        {
            _chnuschtiWindow.ProcessInputEvent(InputEvent.MouseDown(e.X, e.Y, (Chnuschti.MouseButtons)((int)e.Button >> 20), _lastShift, _lastControl, _lastAlt));
        }

        private void Control_MouseUp(object? sender, MouseEventArgs e)
        {
            _chnuschtiWindow.ProcessInputEvent(InputEvent.MouseUp(e.X, e.Y, (Chnuschti.MouseButtons)((int)e.Button >> 20), _lastShift, _lastControl, _lastAlt));
        }

        private void Control_MouseWheel(object? sender, MouseEventArgs e)
        {
            _chnuschtiWindow.ProcessInputEvent(InputEvent.MouseWheel(e.Delta, _lastShift, _lastControl, _lastAlt));
        }

        #endregion
    }
}
