using System.Windows;
using DecktopHoles;

namespace DecktopHoles.Services.Mouse
{
    public interface IMouseInputService
    {
        Rect GetVirtualScreenBounds();

        Rect TranslateToScreen(Rect overlayRect, Point overlayOrigin);

        SelectionResult BuildSelectionResult(Rect screenRect);
    }
}
