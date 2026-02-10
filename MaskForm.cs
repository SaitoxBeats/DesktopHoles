using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopHoles;

internal sealed class MaskForm : Form
{
    private const int EdgeSnapThreshold = 24;
    private const int ResizeGripSize = 8;
    private const int MinimumThickness = 2;
    private const uint AbmNew = 0x00000000;
    private const uint AbmRemove = 0x00000001;
    private const uint AbmQueryPos = 0x00000002;
    private const uint AbmSetPos = 0x00000003;
    private const int AbnPosChanged = 0x00000001;
    private const int WmNcHitTest = 0x0084;
    private const int WmEnterSizeMove = 0x0231;
    private const int WmExitSizeMove = 0x0232;
    private const int HtClient = 1;
    private const int HtLeft = 10;
    private const int HtRight = 11;
    private const int HtTop = 12;
    private const int HtBottom = 15;

    private uint _callbackMessageId;
    private bool _registered;
    private bool _isUserResizing;
    private bool _isApplyingAppBarPosition;
    private AppBarEdge _edge;
    private Rectangle _monitorBounds;
    private int _thickness;

    public MaskForm()
    {
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;

        BackColor = Color.Black;
        Opacity = 1;
    }

    public bool TrySetHole(Rectangle screenRect)
    {
        if (!TryGetEdgeSpec(screenRect, out _edge, out _monitorBounds, out _thickness))
        {
            return false;
        }

        EnsureAppBarRegistered();
        ApplyAppBarPosition();
        return true;
    }

    protected override void WndProc(ref Message m)
    {
        if (_callbackMessageId != 0 && m.Msg == _callbackMessageId && m.WParam.ToInt32() == AbnPosChanged)
        {
            if (!_isUserResizing)
            {
                ApplyAppBarPosition();
            }

            return;
        }

        // Not working :(
        //if (m.Msg == WmNcHitTest)
        //{
        //    base.WndProc(ref m);
        //    if ((int)m.Result == HtClient && TryGetResizeHitTest(m.LParam, out int resizeHit))
        //    {
        //        m.Result = (IntPtr)resizeHit;
        //    }

        //    return;
        //}

        if (m.Msg == WmEnterSizeMove)
        {
            _isUserResizing = true;
        }
        else if (m.Msg == WmExitSizeMove && _isUserResizing)
        {
            _isUserResizing = false;
            SyncThicknessFromBounds();
            ApplyAppBarPosition();
        }

        base.WndProc(ref m);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (_registered)
        {
            var abd = new APPBARDATA
            {
                cbSize = Marshal.SizeOf<APPBARDATA>(),
                hWnd = Handle
            };

            SHAppBarMessage(AbmRemove, ref abd);
            _registered = false;
        }

        base.OnFormClosed(e);
    }

    private void EnsureAppBarRegistered()
    {
        if (_registered)
        {
            return;
        }

        // Registering a custom window message for callbacks. This took forever to get right because Windows is picky as hell
        // about message IDs. Remember: always use a unique string, or you'll collide with other apps and shit hits the fan.
        _callbackMessageId = RegisterWindowMessage("DesktopHolesAppBarMessage");
        var abd = new APPBARDATA
        {
            cbSize = Marshal.SizeOf<APPBARDATA>(),
            hWnd = Handle,
            uCallbackMessage = _callbackMessageId
        };

        // Calling SHAppBarMessage to register as a new AppBar. If you forget to set cbSize properly, marshal will screw you over
        // with access violations. Damn P/Invoke gotchas, but it's stable now, thank fuck.
        SHAppBarMessage(AbmNew, ref abd);
        _registered = true;
    }

    private void ApplyAppBarPosition()
    {
        if (_isApplyingAppBarPosition)
        {
            return;
        }

        _isApplyingAppBarPosition = true;
        try
        {
            var desiredRect = GetDesiredRect();
            var abd = new APPBARDATA
            {
                cbSize = Marshal.SizeOf<APPBARDATA>(),
                hWnd = Handle,
                uEdge = (uint)_edge,
                rc = ToRect(desiredRect)
            };

            // Query the proposed position first. This is crucial because Windows might adjust for other bars.
            // Skip this and your bar overlaps everything, pissing off the taskbar. Learned that the hard way after hours of trial and error.
            SHAppBarMessage(AbmQueryPos, ref abd);

            // Adjust the rect based on edge. This part is tricky: you have to shrink it to the thickness AFTER querying,
            // or Windows ignores your request. Fucking obscure docs on this, but it finally sticks to the edge like glue.
            switch (_edge)
            {
                case AppBarEdge.Left:
                    abd.rc.right = abd.rc.left + _thickness;
                    break;
                case AppBarEdge.Right:
                    abd.rc.left = abd.rc.right - _thickness;
                    break;
                case AppBarEdge.Top:
                    abd.rc.bottom = abd.rc.top + _thickness;
                    break;
                case AppBarEdge.Bottom:
                    abd.rc.top = abd.rc.bottom - _thickness;
                    break;
            }

            SHAppBarMessage(AbmSetPos, ref abd);
            Bounds = FromRect(abd.rc);
        }
        finally
        {
            _isApplyingAppBarPosition = false;
        }
    }

    private Rectangle GetDesiredRect()
    {
        return _edge switch
        {
            AppBarEdge.Left => new Rectangle(_monitorBounds.Left, _monitorBounds.Top, _thickness, _monitorBounds.Height),
            AppBarEdge.Right => new Rectangle(_monitorBounds.Right - _thickness, _monitorBounds.Top, _thickness, _monitorBounds.Height),
            AppBarEdge.Top => new Rectangle(_monitorBounds.Left, _monitorBounds.Top, _monitorBounds.Width, _thickness),
            AppBarEdge.Bottom => new Rectangle(_monitorBounds.Left, _monitorBounds.Bottom - _thickness, _monitorBounds.Width, _thickness),
            _ => _monitorBounds
        };
    }

    private static bool TryGetEdgeSpec(Rectangle screenRect, out AppBarEdge edge, out Rectangle monitorBounds, out int thickness)
    {
        var screen = Screen.FromRectangle(screenRect);
        monitorBounds = screen.Bounds;

        // Calculating distances to edges. This logic evolved over years; started with simple mins but had to handle snapping thresholds.
        var distances = new[]
        {
            new EdgeDistance(AppBarEdge.Top, Math.Abs(screenRect.Top - monitorBounds.Top)),
            new EdgeDistance(AppBarEdge.Bottom, Math.Abs(monitorBounds.Bottom - screenRect.Bottom)),
            new EdgeDistance(AppBarEdge.Left, Math.Abs(screenRect.Left - monitorBounds.Left)),
            new EdgeDistance(AppBarEdge.Right, Math.Abs(monitorBounds.Right - screenRect.Right))
        };

        int min = int.MaxValue;
        foreach (var distance in distances)
        {
            if (distance.Distance < min)
            {
                min = distance.Distance;
            }
        }

        // Snap threshold check. If too far, bail out. Tweaked this value after testing on multiple resolutions;
        // too low and it snaps accidentally, too high and it never does. 24px seems golden.
        if (min > EdgeSnapThreshold)
        {
            edge = default;
            thickness = 0;
            return false;
        }

        edge = PickEdge(distances, screenRect);
        // Thickness calc: min of rect and monitor dims to avoid overflows. Don't forget to check for <2, or it creates invisible bars that fuck up the desktop.
        thickness = edge == AppBarEdge.Top || edge == AppBarEdge.Bottom
            ? Math.Min(screenRect.Height, monitorBounds.Height)
            : Math.Min(screenRect.Width, monitorBounds.Width);

        if (thickness < MinimumThickness)
        {
            edge = default;
            thickness = 0;
            return false;
        }

        return true;
    }

    private static AppBarEdge PickEdge(EdgeDistance[] distances, Rectangle screenRect)
    {
        // Finding the actual min distance. Duplicated loop from above, but hey, it's old code, barely worked.
        int min = int.MaxValue;
        foreach (var distance in distances)
        {
            if (distance.Distance < min)
            {
                min = distance.Distance;
            }
        }

        // Prefer horizontal/vertical based on rect aspect. This was a pain to figure out for non-square holes;
        // without it, it'd pick wrong edges on wide/tall monitors. Damn edge cases.
        bool horizontal = screenRect.Width >= screenRect.Height;
        if (horizontal)
        {
            foreach (var distance in distances)
            {
                if (distance.Distance == min && (distance.Edge == AppBarEdge.Top || distance.Edge == AppBarEdge.Bottom))
                {
                    return distance.Edge;
                }
            }
        }
        else
        {
            foreach (var distance in distances)
            {
                if (distance.Distance == min && (distance.Edge == AppBarEdge.Left || distance.Edge == AppBarEdge.Right))
                {
                    return distance.Edge;
                }
            }
        }

        // Fallback to any min edge. Rarely hits, but covers ties.
        foreach (var distance in distances)
        {
            if (distance.Distance == min)
            {
                return distance.Edge;
            }
        }

        return distances[0].Edge;
    }

    private static RECT ToRect(Rectangle rect)
    {
        // Simple conversion, but remember: RECT is LTRB, and if you swap left/right, Windows freaks out with negative widths. Stupid but true.
        return new RECT
        {
            left = rect.Left,
            top = rect.Top,
            right = rect.Right,
            bottom = rect.Bottom
        };
    }

    private static Rectangle FromRect(RECT rect)
    {
        return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
    }

    // Not working :(
    //private bool TryGetResizeHitTest(IntPtr lParam, out int hitTest)
    //{
    //    hitTest = 0;
    //    if (!_registered)
    //    {
    //        return false;
    //    }

    //    Point screenPoint = DecodeLParamPoint(lParam);
    //    Point clientPoint = PointToClient(screenPoint);
    //    int grip = Math.Max(1, Math.Min(ResizeGripSize, Math.Max(Width, Height)));

    //    switch (_edge)
    //    {
    //        case AppBarEdge.Left when clientPoint.X >= Width - grip:
    //            hitTest = HtRight;
    //            return true;
    //        case AppBarEdge.Right when clientPoint.X <= grip:
    //            hitTest = HtLeft;
    //            return true;
    //        case AppBarEdge.Top when clientPoint.Y >= Height - grip:
    //            hitTest = HtBottom;
    //            return true;
    //        case AppBarEdge.Bottom when clientPoint.Y <= grip:
    //            hitTest = HtTop;
    //            return true;
    //        default:
    //            return false;
    //    }
    //}

    private void SyncThicknessFromBounds()
    {
        int monitorLimit = _edge == AppBarEdge.Top || _edge == AppBarEdge.Bottom
            ? _monitorBounds.Height
            : _monitorBounds.Width;

        int candidate = _edge == AppBarEdge.Top || _edge == AppBarEdge.Bottom
            ? Height
            : Width;

        _thickness = Math.Max(MinimumThickness, Math.Min(candidate, monitorLimit));
    }

    private static Point DecodeLParamPoint(IntPtr lParam)
    {
        int raw = lParam.ToInt32();
        int x = unchecked((short)(raw & 0xFFFF));
        int y = unchecked((short)((raw >> 16) & 0xFFFF));
        return new Point(x, y);
    }

    private readonly struct EdgeDistance
    {
        public EdgeDistance(AppBarEdge edge, int distance)
        {
            Edge = edge;
            Distance = distance;
        }

        public AppBarEdge Edge { get; }
        public int Distance { get; }
    }

    private enum AppBarEdge
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string lpString);
}
