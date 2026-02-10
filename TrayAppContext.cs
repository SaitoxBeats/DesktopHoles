using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopHoles;

internal sealed class TrayAppContext : ApplicationContext
{
    private const string GithubUrl = "https://github.com/SaitoxBeats";
    
    private readonly NotifyIcon _trayIcon;
    private MaskForm? _maskForm;
    private HotkeyWindow? _hotkeySelect;
    private HotkeyWindow? _hotkeyRemove;

    public TrayAppContext()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            Text = "Desktop Holes",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _trayIcon.MouseClick += OnTrayIconMouseClick;

        RegisterHotkeys();
        ShowWelcomeScreen();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Create New Hole...", null, (_, _) => StartSelection());
        menu.Items.Add("Remove Hole", null, (_, _) => RemoveHole());
        menu.Items.Add("Settings...", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("About", null, (_, _) => OpenGithub());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());

        return menu;
    }

    private void RegisterHotkeys()
    {
        _hotkeySelect = new HotkeyWindow(id: 1);
        _hotkeySelect.Pressed += (_, _) => StartSelection();
        _hotkeySelect.TryRegisterCtrlWinAltL();

        _hotkeyRemove = new HotkeyWindow(id: 2);
        _hotkeyRemove.Pressed += (_, _) => RemoveHole();
        _hotkeyRemove.TryRegisterCtrlWinAltP();
    }

    private void ShowWelcomeScreen()
    {
        using var welcome = new WelcomeForm();
        if (welcome.ShowDialog() == DialogResult.OK)
        {
            StartSelection();
        }
    }

    private void OnTrayIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        if (_maskForm != null)
        {
            RemoveHole();
        }
        else
        {
            StartSelection();
        }
    }

    private static void OpenGithub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GithubUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not open GitHub profile.{Environment.NewLine}{ex.Message}",
                "Desktop Holes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void StartSelection()
    {
        bool restoreMask = _maskForm?.Visible == true;
        _maskForm?.Hide();

        using var selector = new SelectionForm();
        var result = selector.ShowDialog();
        if (result == DialogResult.OK)
        {
            bool created = EnsureMaskForm();
            if (_maskForm!.TrySetHole(selector.SelectedBounds))
            {
                _maskForm!.Show();
            }
            else
            {
                MessageBox.Show(
                    "To crop the desktop, select a strip that is right next to one edge of the monitor.",
                    "Desktop Holes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                if (created)
                {
                    _maskForm.Close();
                    _maskForm = null;
                }
                else if (restoreMask)
                {
                    _maskForm?.Show();
                }
            }
        }
        else if (restoreMask)
        {
            _maskForm?.Show();
        }
    }

    private void RemoveHole()
    {
        if (_maskForm == null)
        {
            return;
        }

        _maskForm.Close();
        _maskForm = null;
    }

    private static void ShowSettings()
    {
        using var settings = new SettingsForm();
        settings.ShowDialog();
    }

    private bool EnsureMaskForm()
    {
        if (_maskForm != null)
        {
            return false;
        }

        _maskForm = new MaskForm();
        _maskForm.FormClosed += (_, _) => _maskForm = null;
        return true;
    }

    protected override void ExitThreadCore()
    {
        _hotkeySelect?.Dispose();
        _hotkeySelect = null;
        
        _hotkeyRemove?.Dispose();
        _hotkeyRemove = null;
        
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _maskForm?.Close();
        base.ExitThreadCore();
    }
}
