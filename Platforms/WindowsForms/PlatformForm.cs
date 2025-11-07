using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsForms
{


    public class PlatformForm : Form
    {
        private const int cGrip = 4;      // Grip size
        private const int cCaption = 8;   // Caption bar height;

        public PlatformForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                // Remove anything that would force caption rendering
                cp.Style &= ~(Win32.WS_CAPTION | Win32.WS_SYSMENU | Win32.WS_MINIMIZEBOX | Win32.WS_MAXIMIZEBOX);
                // Keep only size box
                cp.Style |= Win32.WS_THICKFRAME;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Rounded corners (Windows 11). Ignored on older versions.
            if (Environment.OSVersion.Version.Major >= 10)
            {
                // Disable non-client rendering entirely
                int ncrpDisabled = Win32.DWMNCRP_DISABLED;
                Win32.DwmSetWindowAttribute(Handle, Win32.DWMWA_NCRENDERING_POLICY, ref ncrpDisabled, sizeof(int));

                var cornerPref = (int)Win32.DwmWindowCornerPreference.Round;
                Win32.DwmSetWindowAttribute(Handle, Win32.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));

                // Optional dark mode hint (if you have a dark UI)
                int dark = 1;
                Win32.DwmSetWindowAttribute(Handle, Win32.DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));

                // Make border color transparent (removes faint border lines)
                // ARGB: 0xAARRGGBB — use 0 for fully transparent
                uint transparent = 0x00000000;
                Win32.DwmSetWindowAttribute(Handle, Win32.DWMWA_BORDER_COLOR, ref transparent, sizeof(uint));
            }

            // Tell Windows that the non-client changed (important on Win10/11)
            Win32.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOZORDER | Win32.SWP_FRAMECHANGED);
        }



        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_NCCALCSIZE:
                    if (m.WParam != IntPtr.Zero)
                    {
                        // Tell Windows the whole rect is client; no default frame/title
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case Win32.WM_NCHITTEST:               
                    // Convert to client coordinates
                    Point screenPos = new Point(m.LParam.ToInt32());
                    Point clientPos = PointToClient(screenPos);
                    int x = clientPos.X;
                    int y = clientPos.Y;
                    int w = ClientSize.Width;
                    int h = ClientSize.Height;

                    bool caption = y <= cCaption;
                    bool left = x <= cGrip;
                    bool right = x >= w - cGrip;
                    bool top = y <= cGrip;
                    bool bottom = y >= h - cGrip;

                    if (caption) { m.Result = (IntPtr)Win32.HitTestValues.Caption; return; }
                    if (left && top) { m.Result = (IntPtr)Win32.HitTestValues.TopLeft; return; }
                    if (right && top) { m.Result = (IntPtr)Win32.HitTestValues.TopRight; return; }
                    if (left && bottom) { m.Result = (IntPtr)Win32.HitTestValues.BottomLeft; return; }
                    if (right && bottom) { m.Result = (IntPtr)Win32.HitTestValues.BottomRight; return; }
                    if (left) { m.Result = (IntPtr)Win32.HitTestValues.Left; return; }
                    if (right) { m.Result = (IntPtr)Win32.HitTestValues.Right; return; }
                    if (top) { m.Result = (IntPtr)Win32.HitTestValues.Top; return; }
                    if (bottom) { m.Result = (IntPtr)Win32.HitTestValues.Bottom; return; }
                    break;
            }

            base.WndProc(ref m);
        }
    }
}
