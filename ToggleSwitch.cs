using System.Drawing.Drawing2D;

namespace train_automation;

/// <summary>
/// Minimal flat toggle switch used in place of CheckBox for booking preferences.
/// Exposes a <see cref="Checked"/> bool + <see cref="CheckedChanged"/> event so it
/// drops in wherever CheckBox.Checked was used previously.
/// </summary>
public class ToggleSwitch : Control
{
    private bool _checked;

    public event EventHandler? CheckedChanged;

    public ToggleSwitch()
    {
        SetStyle(
            ControlStyles.UserPaint
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.Selectable,
            true);
        Size = new Size(38, 20);
        Cursor = Cursors.Hand;
        TabStop = true;
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value)
            {
                return;
            }

            _checked = value;
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
    public Color OnColor { get; set; } = Color.FromArgb(108, 79, 224);

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
    public Color OffColor { get; set; } = Color.FromArgb(220, 218, 232);

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Checked = !Checked;
    }

    protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Space)
        {
            Checked = !Checked;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var trackRect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var trackPath = RoundedRect(trackRect, Height / 2);
        using var trackBrush = new SolidBrush(_checked ? OnColor : OffColor);
        g.FillPath(trackBrush, trackPath);

        var knobDiameter = Height - 6;
        var knobX = _checked ? Width - knobDiameter - 3 : 3;
        var knobRect = new Rectangle(knobX, 3, knobDiameter, knobDiameter);
        using var knobBrush = new SolidBrush(Color.White);
        g.FillEllipse(knobBrush, knobRect);

        if (Focused)
        {
            using var focusPen = new Pen(Color.FromArgb(120, OnColor), 1.5f);
            g.DrawPath(focusPen, trackPath);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
