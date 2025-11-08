using Chnuschti;
using Chnuschti.Controls;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace WindowsForms
{
    public class PlatformWindow : IPlatformWindow, IDisposable
    {
        private Window _chnuschtiWindow;
        private PlatformForm _window;
        private Chnuschti.WindowsForms.FocusableSKGLControl _skglControl;
        private string _title = "Chnuschti - Windows Forms";

        public PlatformWindow(Window chnuschtiWindow)
        {
            _chnuschtiWindow = chnuschtiWindow;

            _window = new PlatformForm(this);
            _window.SuspendLayout();

            _skglControl = new Chnuschti.WindowsForms.FocusableSKGLControl(this);
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
            _window.FormBorderStyle = FormBorderStyle.Sizable;
            _window.MaximizeBox = true;
            _window.MinimizeBox = true;

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

        public WindowState WindowState
        {
            get => _window.WindowState == FormWindowState.Normal ? WindowState.Normal :
                        _window.WindowState == FormWindowState.Minimized ? WindowState.Minimized :
                        WindowState.Maximized;
            set
            {
                _window.WindowState = value == WindowState.Normal ? FormWindowState.Normal :
                                        value == WindowState.Minimized ? FormWindowState.Minimized :
                                        FormWindowState.Maximized;
            }
        }

        public SKPoint Location
        {
            get => new SKPoint(_window.Location.X, _window.Location.Y);
            set => _window.Location = new Point((int)value.X, (int)value.Y);
        }

        public SKPoint Size
        {
            get => new SKPoint(_window.Size.Width, _window.Size.Height);
            set => _window.Size = new Size((int)value.X, (int)value.Y);
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

        #region Client Area Detection

        private const int cGrip = 4;      // Grip size

        public Win32.HitTestValues GetHitZone(Point pos, int windowWidth, int windowHeight)
        {
            int x = pos.X;
            int y = pos.Y;
            int w = windowWidth;
            int h = windowHeight;

            bool left = x <= cGrip;
            bool right = x >= w - cGrip;
            bool top = y <= cGrip;
            bool bottom = y >= h - cGrip;

            if (left && top) return Win32.HitTestValues.TopLeft;
            if (right && top) return Win32.HitTestValues.TopRight;
            if (left && bottom) return Win32.HitTestValues.BottomLeft;
            if (right && bottom) return Win32.HitTestValues.BottomRight;
            if (top) return Win32.HitTestValues.Top;
            if (left) return Win32.HitTestValues.Left;
            if (right) return Win32.HitTestValues.Right;
            if (bottom) return Win32.HitTestValues.Bottom;

            var element = _chnuschtiWindow.HitTest(new SkiaSharp.SKPoint(x, y), false);
            if (element?.IsWindowDragArea == true) return Win32.HitTestValues.Caption;

            return Win32.HitTestValues.Nowhere;
        }

        #endregion

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
