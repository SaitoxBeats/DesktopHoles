using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using DecktopHoles;

namespace DecktopHoles.Services.Mouse
{
    public class MouseInputService : IMouseInputService
    {
        public Rect GetVirtualScreenBounds()
        {
            var bounds = SystemInformation.VirtualScreen;
            return new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        }

        public Rect TranslateToScreen(Rect overlayRect, Point overlayOrigin)
        {
            return new Rect(
                overlayRect.Left + overlayOrigin.X,
                overlayRect.Top + overlayOrigin.Y,
                overlayRect.Width,
                overlayRect.Height);
        }

        public SelectionResult BuildSelectionResult(Rect screenRect)
        {
            var centerPoint = new System.Drawing.Point(
                (int)Math.Round(screenRect.X + screenRect.Width / 2),
                (int)Math.Round(screenRect.Y + screenRect.Height / 2));
            var screen = Screen.FromPoint(centerPoint);
            var monitorBounds = screen.Bounds;
            var monitorRect = new Rect(
                screenRect.Left - monitorBounds.Left,
                screenRect.Top - monitorBounds.Top,
                screenRect.Width,
                screenRect.Height);

            return new SelectionResult(screenRect, monitorRect, screen.DeviceName);
        }
    }
}
