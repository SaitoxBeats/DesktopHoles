using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DesktopHoles;

internal sealed class SelectionForm : Form
{
    private Point _start;
    private Point _current;
    private bool _selecting;

    public Rectangle SelectedBounds { get; private set; }

    public SelectionForm()
    {
        StartPosition = FormStartPosition.Manual;
        Bounds = SystemInformation.VirtualScreen;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        KeyPreview = true;
        Cursor = Cursors.Cross;

        BackColor = Color.Black;
        Opacity = 0.25;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _selecting = true;
        _start = e.Location;
        _current = e.Location;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_selecting)
        {
            return;
        }

        _current = e.Location;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!_selecting || e.Button != MouseButtons.Left)
        {
            return;
        }

        _selecting = false;
        _current = e.Location;
        var rect = GetSelectionRect();
        if (rect.Width < 2 || rect.Height < 2)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }
        
        // Convert to screen coordinates — took me way too long to remember we need to add form offset
        SelectedBounds = new Rectangle(rect.X + Left, rect.Y + Top, rect.Width, rect.Height);
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_selecting)
        {
            return;
        }

        var rect = GetSelectionRect();
        
        // damn it works.
        using var pen = new Pen(Color.DeepSkyBlue, 2);
        using var brush = new SolidBrush(Color.FromArgb(60, 0, 120, 215));

        e.Graphics.SmoothingMode = SmoothingMode.None;
        e.Graphics.FillRectangle(brush, rect);
        e.Graphics.DrawRectangle(pen, rect);
    }

    private Rectangle GetSelectionRect()
    {
        // uwu
        // One wrong Math.Min and the rect flips and disappears — never forget.
        int x = Math.Min(_start.X, _current.X);
        int y = Math.Min(_start.Y, _current.Y);
        int w = Math.Abs(_start.X - _current.X);
        int h = Math.Abs(_start.Y - _current.Y);
        return new Rectangle(x, y, w, h);
    }
}
