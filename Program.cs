using System;
using System.Windows.Forms;

namespace DesktopHoles;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles(); //bruh i forgot
        Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new TrayAppContext());
    }
}
