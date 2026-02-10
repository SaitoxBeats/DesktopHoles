using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopHoles;

internal sealed class WelcomeForm : Form
{
    private const string GithubUrl = "https://github.com/SaitoxBeats";
    
    //This was definitely the WORST idea I've ever had in my entire life.
    //This was definitely the WORST idea I've ever had in my entire life.
    //This was definitely the WORST idea I've ever had in my entire life.


    // Theme
    private static readonly Color Bg = ColorTranslator.FromHtml("#18181b");
    private static readonly Color Fg = ColorTranslator.FromHtml("#e4e4e7");
    private static readonly Color Muted = ColorTranslator.FromHtml("#a1a1aa");
    private static readonly Color Card = ColorTranslator.FromHtml("#1f1f23");
    private static readonly Color CardBorder = ColorTranslator.FromHtml("#2a2a31");
    private static readonly Color Accent = ColorTranslator.FromHtml("#3f3f46"); // link + primary
    private static readonly Color ButtonBg = ColorTranslator.FromHtml("#27272a");
    private static readonly Color ButtonBorder = ColorTranslator.FromHtml("#3f3f46");

    public WelcomeForm()
    {
        Text = "Desktop Holes";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 300);

        BackColor = Bg;
        ForeColor = Fg;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        // nice rendering UWU
        DoubleBuffered = true;

        BuildLayout();
    }

    private void BuildLayout()
    {
        var titleLabel = new Label
        {
            Text = "Desktop Holes",
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Fg,
            Location = new Point(24, 18),
            AutoSize = true
        };

        var subtitleLabel = new Label
        {
            Text = "Create edge holes on your desktop quickly.",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Muted,
            Location = new Point(26, 56),
            AutoSize = true
        };

        var card = new Panel
        {
            Location = new Point(24, 92),
            Size = new Size(472, 126),
            BackColor = Card
        };
        card.Paint += (_, e) =>
        {
            // Subtle border
            using var pen = new Pen(CardBorder, 1);
            var r = card.ClientRectangle;
            r.Width -= 1;
            r.Height -= 1;
            e.Graphics.DrawRectangle(pen, r);
        };

        var tipsLabel = new Label
        {
            Text =
                "How to use:" + Environment.NewLine +
                "• Ctrl + Win + Alt + L — create/reset hole" + Environment.NewLine +
                "• Ctrl + Win + Alt + P — remove current hole" + Environment.NewLine +
                "• Left-click tray icon — toggle create/remove",
            Location = new Point(16, 14),
            Size = new Size(440, 94),
            ForeColor = Fg
        };
        card.Controls.Add(tipsLabel);

        var githubLink = new LinkLabel
        {
            Text = "github.com/SaitoxBeats",
            Location = new Point(24, 236),
            AutoSize = true,
            LinkColor = Accent,
            ActiveLinkColor = Accent,
            VisitedLinkColor = Accent,
            BackColor = Bg
        };
        githubLink.LinkBehavior = LinkBehavior.NeverUnderline;
        githubLink.LinkClicked += (_, _) => OpenGithubProfile();

        var closeButton = MakeButton(
            text: "Close",
            size: new Size(100, 34),
            location: new Point(396, 252),
            isPrimary: false
        );
        closeButton.DialogResult = DialogResult.Cancel;

        var createButton = MakeButton(
            text: "Create Hole Now",
            size: new Size(140, 34),
            location: new Point(248, 252),
            isPrimary: true
        );
        createButton.DialogResult = DialogResult.OK;

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(card);
        Controls.Add(githubLink);
        Controls.Add(createButton);
        Controls.Add(closeButton);

        AcceptButton = createButton;
        CancelButton = closeButton;
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

        // hover
        btn.MouseEnter += (_, _) =>
        {
            if (isPrimary) btn.BackColor = ColorTranslator.FromHtml("#3f3f46");
            else btn.BackColor = ColorTranslator.FromHtml("#2f2f35");
        };

        btn.MouseLeave += (_, _) =>
        {
            btn.BackColor = isPrimary ? Accent : ButtonBg;
        };

        btn.MouseDown += (_, _) =>
        {
            if (isPrimary) btn.BackColor = ColorTranslator.FromHtml("#1d4ed8");
            else btn.BackColor = ColorTranslator.FromHtml("#3a3a40");
        };

        btn.MouseUp += (_, _) =>
        {
            // return to hover color if still over it
            var over = btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position));
            if (over)
            {
                if (isPrimary) btn.BackColor = ColorTranslator.FromHtml("#2563eb");
                else btn.BackColor = ColorTranslator.FromHtml("#2f2f35");
            }
            else
            {
                btn.BackColor = isPrimary ? Accent : ButtonBg;
            }
        };

        return btn;
    }

    private static void OpenGithubProfile()
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
}
