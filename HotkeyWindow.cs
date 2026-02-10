using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;


internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    
    [Flags]
    private enum Modifiers : uint
    {
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008,
        NoRepeat = 0x4000, // no repeat
    }
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    private readonly int _id;
    private bool _registered;
    
    public event EventHandler? Pressed;
    
    public HotkeyWindow(int id)
    {
        _id = id;
        CreateHandle(new CreateParams());
    }
    
    public bool TryRegisterCtrlWinAltL()
    {
        // Ctrl + Win + Alt + L
        uint mods = (uint)(Modifiers.Control | Modifiers.Win | Modifiers.Alt | Modifiers.NoRepeat);
        uint vk = (uint)Keys.L;

        _registered = RegisterHotKey(Handle, _id, mods, vk);
        return _registered;
    }
    
    public bool TryRegisterCtrlWinAltP()
    {
        uint mods = (uint)(Modifiers.Control | Modifiers.Win | Modifiers.Alt | Modifiers.NoRepeat);
        uint vk = (uint)Keys.P;

        _registered = RegisterHotKey(Handle, _id, mods, vk);
        return _registered;
    }
    
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _id)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        base.WndProc(ref m);
    }
    
    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(Handle, _id);
            _registered = false;
        }

        DestroyHandle();
    }
}