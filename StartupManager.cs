using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DesktopHoles;

internal static class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "DesktopHoles";

    public static bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        string? value = key?.GetValue(ValueName) as string;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string quotedPath = QuotePath(Application.ExecutablePath);
        return string.Equals(value, quotedPath, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, Application.ExecutablePath, StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using RegistryKey? key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        if (key == null)
        {
            throw new InvalidOperationException("Could not open the Windows startup registry key.");
        }

        if (enabled)
        {
            key.SetValue(ValueName, QuotePath(Application.ExecutablePath), RegistryValueKind.String);
            return;
        }

        key.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    private static string QuotePath(string path)
    {
        return $"\"{path}\"";
    }
}
