using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopHoles;

internal sealed class SettingsForm : Form
{
    private static readonly Color Bg = ColorTranslator.FromHtml("#18181b");
    private static readonly Color Fg = ColorTranslator.FromHtml("#e4e4e7");
    private static readonly Color Muted = ColorTranslator.FromHtml("#a1a1aa");
    private static readonly Color Card = ColorTranslator.FromHtml("#1f1f23");
    private static readonly Color CardBorder = ColorTranslator.FromHtml("#2a2a31");
    private static readonly Color Accent = ColorTranslator.FromHtml("#3f3f46");
    private static readonly Color ButtonBg = ColorTranslator.FromHtml("#27272a");
    private static readonly Color ButtonBorder = ColorTranslator.FromHtml("#3f3f46");

    private readonly CheckBox _startupCheckBox;

    public SettingsForm()
    {
        Text = "Desktop Holes Settings";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 250);

        BackColor = Bg;
        ForeColor = Fg;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        DoubleBuffered = true;

        _startupCheckBox = new CheckBox
        {
            Text = "Start up with Windows?",
            AutoSize = true,
            Location = new Point(18, 40),
            ForeColor = Fg,
            BackColor = Card
        };

        BuildLayout();
        LoadCurrentSettings();
    }

    private void BuildLayout()
    {
        var titleLabel = new Label
        {
            Text = "Settings",
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Fg,
            Location = new Point(24, 18),
            AutoSize = true
        };

        var subtitleLabel = new Label
        {
            Text = "Configure Desktop Holes behavior.",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Muted,
            Location = new Point(26, 56),
            AutoSize = true
        };

        var card = new Panel
        {
            Location = new Point(24, 92),
            Size = new Size(472, 84),
            BackColor = Card
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(CardBorder, 1);
            var r = card.ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            e.Graphics.DrawRectangle(pen, r);
        };

        var startupDescription = new Label
        {
            Text = "Launch Desktop Holes automatically when you sign in.",
            Location = new Point(18, 18),
            AutoSize = true,
            ForeColor = Muted,
            BackColor = Card
        };

        card.Controls.Add(startupDescription);
        card.Controls.Add(_startupCheckBox);

        var closeButton = MakeButton(
            text: "Close",
            size: new Size(100, 34),
            location: new Point(396, 202),
            isPrimary: false
        );
        closeButton.DialogResult = DialogResult.Cancel;

        var saveButton = MakeButton(
            text: "Save",
            size: new Size(120, 34),
            location: new Point(268, 202),
            isPrimary: true
        );
        saveButton.Click += OnSaveClick;

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(card);
        Controls.Add(saveButton);
        Controls.Add(closeButton);

        AcceptButton = saveButton;
        CancelButton = closeButton;
    }

    private void LoadCurrentSettings()
    {
        try
        {
            _startupCheckBox.Checked = StartupManager.IsEnabled();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not read startup setting.{Environment.NewLine}{ex.Message}",
                "Desktop Holes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        try
        {
            StartupManager.SetEnabled(_startupCheckBox.Checked);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not update startup setting.{Environment.NewLine}{ex.Message}",
                "Desktop Holes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private Button MakeButton(string text, Size size, Point location, bool isPrimary)
    {
        var btn = new Button
        {
            Text = text,
            Size = size,
            Location = location,
            FlatStyle = FlatStyle.Flat,
            BackColor = isPrimary ? Accent : ButtonBg,
            ForeColor = isPrimary ? Color.White : Fg,
            TabStop = true
        };

        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = isPrimary ? Accent : ButtonBorder;

        btn.MouseEnter += (_, _) =>
        {
            if (isPrimary)
            {
                btn.BackColor = ColorTranslator.FromHtml("#3f3f46");
            }
            else
            {
                btn.BackColor = ColorTranslator.FromHtml("#2f2f35");
            }
        };

        btn.MouseLeave += (_, _) =>
        {
            btn.BackColor = isPrimary ? Accent : ButtonBg;
        };

        btn.MouseDown += (_, _) =>
        {
            if (isPrimary)
            {
                btn.BackColor = ColorTranslator.FromHtml("#1d4ed8");
            }
            else
            {
                btn.BackColor = ColorTranslator.FromHtml("#3a3a40");
            }
        };

        btn.MouseUp += (_, _) =>
        {
            bool over = btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position));
            if (over)
            {
                if (isPrimary)
                {
                    btn.BackColor = ColorTranslator.FromHtml("#2563eb");
                }
                else
                {
                    btn.BackColor = ColorTranslator.FromHtml("#2f2f35");
                }
            }
            else
            {
                btn.BackColor = isPrimary ? Accent : ButtonBg;
            }
        };

        return btn;
    }
}
