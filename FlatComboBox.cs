namespace train_automation;

/// <summary>
/// A ComboBox that visually hides the native dropdown arrow button by painting over it
/// after every WM_PAINT, giving the field a plain text-box look while keeping full
/// keyboard autocomplete (AutoCompleteMode = Suggest).
/// </summary>
internal sealed class FlatComboBox : ComboBox
{
    private const int WM_PAINT = 0x000F;

    public FlatComboBox()
    {
        FlatStyle = FlatStyle.Flat;
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg == WM_PAINT)
        {
            using var g = Graphics.FromHwnd(Handle);

            // The arrow button occupies the rightmost ~17 px of the control.
            int btnW = SystemInformation.HorizontalScrollBarArrowWidth + 2;
            var btnRect = new Rectangle(Width - btnW, 1, btnW - 1, Height - 2);

            // Paint over the button with the combo's own BackColor.
            using var brush = new SolidBrush(BackColor);
            g.FillRectangle(brush, btnRect);
            
            // Draw a border around the entire control to restore edges
            using var pen = new Pen(UiTheme.OutlineVariant);
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
