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
        private PlatformWindow _platformWindow;

        public PlatformForm(PlatformWindow platformWindow)
        {
            _platformWindow = platformWindow;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                // Remove anything that would force caption rendering
                //cp.Style &= ~(Win32.WS_THICKFRAME);
                // Keep only size box
                // cp.Style |= Win32.WS_THICKFRAME | Win32.WS_MINIMIZEBOX | Win32.WS_MAXIMIZEBOX;
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
                    // If you’re extending client to cover the title area, you MUST handle this.
                    // When maximized, shrink the client rect by the resize border thickness so content isn’t cut off.
                    if (m.WParam != IntPtr.Zero)
                    {
                        var p = (Win32.NCCALCSIZE_PARAMS)System.Runtime.InteropServices.Marshal.PtrToStructure(m.LParam, typeof(Win32.NCCALCSIZE_PARAMS));

                        if (Win32.IsZoomed(this.Handle)) // window is maximized
                        {
                            var inset = Win32.GetResizeBorderThicknessForWindowDpi(this.Handle);
                            p.rgrc0.Left += inset.X;
                            p.rgrc0.Top += inset.Y;
                            p.rgrc0.Right -= inset.X;
                            p.rgrc0.Bottom -= inset.Y;
                        }

                        System.Runtime.InteropServices.Marshal.StructureToPtr(p, m.LParam, false);
                        m.Result = IntPtr.Zero; // we handled it
                        return;
                    }
                    break;

                case Win32.WM_NCHITTEST:
                    // Convert to client coordinates
                    Point screenPos = new Point(m.LParam.ToInt32());
                    Point clientPos = PointToClient(screenPos);

                    var hitTestValue = _platformWindow.GetHitZone(clientPos, ClientSize.Width, ClientSize.Height);
                    if (hitTestValue != Win32.HitTestValues.Nowhere)
                    {
                        m.Result = (IntPtr)hitTestValue;
                        return;
                    }
                    break;
            }

            base.WndProc(ref m);
        }
    }
}
