namespace train_automation;

/// <summary>
/// Launcher page — Hitman-style compact layout:
/// 2 rows of action buttons, then two rows of inline checkboxes.
/// </summary>
public sealed class LauncherPanel : UserControl
{
    public event EventHandler<string>? NavigateRequested;

    public LauncherPanel()
    {
        Dock      = DockStyle.Fill;
        BackColor = UiTheme.PageBg;

        BuildLayout();
    }

    private void BuildLayout()
    {
        SuspendLayout();

        // ── 4×2 Action button grid ────────────────────────────────────────
        var actions = new TableLayoutPanel
        {
            ColumnCount = 4,
            RowCount    = 2,
            Dock        = DockStyle.Top,
            Height      = 96,
            BackColor   = UiTheme.PageBg,
            Padding     = new Padding(8, 8, 8, 4),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        for (var i = 0; i < 4; i++) actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        for (var i = 0; i < 2; i++) actions.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        var items = new (string Label, string Page, bool Primary)[]
        {
            ("Add IRCTC Account", "accounts",   false),
            ("New Ticket",        "new-ticket", true ),
            ("Open Tickets",      "tickets",    false),
            ("History & Logs",    "logs",       false),
            ("Add Bank / UPI",    "",           false),
            ("OTP Bypass",        "",           false),
            ("IP Block Check",    "",           false),
        };

        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var btn  = MakeActionButton(item.Label, item.Primary);
            var page = item.Page;
            var lbl  = item.Label;
            btn.Margin = new Padding(3, 3, 3, 3);
            btn.Click += (_, _) => OnActionClick(page, lbl);
            actions.Controls.Add(btn, i % 4, i / 4);
        }
        Controls.Add(actions);

        // ── Divider ──────────────────────────────────────────────────────
        Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = UiTheme.OutlineVariant });

        // ── Checkbox row 1 ────────────────────────────────────────────────
        var betaCheck    = new CheckBox { Text = "Beta UI",         Checked = true  };
        var chromeCheck  = new CheckBox { Text = "Real Chrome (CDP)", Checked = true };
        var confirmCheck = new CheckBox { Text = "Confirm Berths",  Checked = false };

        var resetBtn = new Button
        {
            Text      = "Reset",
            Size      = new Size(56, 22),
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.Danger,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin    = new Padding(12, 0, 0, 0)
        };
        resetBtn.FlatAppearance.BorderSize = 0;
        resetBtn.Click += (_, _) =>
        {
            betaCheck.Checked = false;
            chromeCheck.Checked = false;
            confirmCheck.Checked = false;
        };

        Controls.Add(BuildCheckRow(betaCheck, chromeCheck, confirmCheck, resetBtn));

        // ── Checkbox row 2 ────────────────────────────────────────────────
        var altAvailCheck    = new CheckBox { Text = "Alternate Avail", Checked = false };
        var directLoginCheck = new CheckBox { Text = "Direct Login",    Checked = false };

        Controls.Add(BuildCheckRow(altAvailCheck, directLoginCheck));

        // Bring to front so DockStyle.Top stacks correctly
        foreach (Control c in Controls) c.BringToFront();

        ResumeLayout();
    }

    private static FlowLayoutPanel BuildCheckRow(params Control[] controls)
    {
        var row = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 26,
            BackColor     = UiTheme.PageBg,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            Padding       = new Padding(10, 4, 0, 0)
        };

        foreach (var ctrl in controls)
        {
            if (ctrl is CheckBox cb) StyleCheckbox(cb);
            row.Controls.Add(ctrl);
        }

        return row;
    }

    private static Button MakeActionButton(string label, bool primary)
    {
        var btn = new Button
        {
            Text        = label,
            Dock        = DockStyle.Fill,
            FlatStyle   = FlatStyle.Flat,
            Font        = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor      = Cursors.Hand,
            TextAlign   = ContentAlignment.MiddleCenter,
            UseMnemonic = false
        };
        btn.FlatAppearance.BorderSize = 1;

        if (primary)
        {
            btn.BackColor = UiTheme.Primary;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = UiTheme.Primary;
        }
        else
        {
            btn.BackColor = UiTheme.SurfaceLowest;
            btn.ForeColor = UiTheme.Text;
            btn.FlatAppearance.BorderColor = UiTheme.OutlineVariant;
        }

        return btn;
    }

    private static void StyleCheckbox(CheckBox cb)
    {
        cb.BackColor = UiTheme.PageBg;
        cb.ForeColor = Color.White;
        cb.Font      = new Font("Segoe UI", 8.5F);
        cb.AutoSize  = true;
        cb.Margin    = new Padding(0, 0, 16, 0);
        cb.UseVisualStyleBackColor = false;
    }

    private void OnActionClick(string page, string label)
    {
        if (string.IsNullOrEmpty(page) || page == "logs")
        {
            MessageBox.Show(FindForm(), $"{label} will be added next.", "RailBot Pro",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        NavigateRequested?.Invoke(this, page);
    }
}
