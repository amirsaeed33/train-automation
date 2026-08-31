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
            ("Add Bank / UPI",    "bank",       false),
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
        var chromeCheck = new CheckBox { Text = "WEB-Chrome" };
        var operaCheck  = new CheckBox { Text = "WEB-Opera" };
        var braveCheck  = new CheckBox { Text = "WEB-Brave" };
        var cometCheck  = new CheckBox { Text = "WEB-Comet" };
        var app1Check   = new CheckBox { Text = "APP-1" };
        var app2Check   = new CheckBox { Text = "APP-2" };
        var betaCheck   = new CheckBox { Text = "Beta UI" };

        var config = BookingConfiguration.Load();
        betaCheck.Checked = config.UseBetaView;
        betaCheck.CheckedChanged += (_, _) => {
            var c = BookingConfiguration.Load();
            c.UseBetaView = betaCheck.Checked;
            c.Save();
        };

        var browsers = new[] { chromeCheck, operaCheck, braveCheck, cometCheck, app1Check, app2Check };
        foreach (var b in browsers)
        {
            if (b.Text == config.SelectedBrowser) b.Checked = true;
            b.CheckedChanged += (s, e) => {
                var chk = (CheckBox)s;
                if (chk.Checked) {
                    foreach (var other in browsers) if (other != chk) other.Checked = false;
                    var c = BookingConfiguration.Load();
                    c.SelectedBrowser = chk.Text;
                    c.Save();
                } else if (browsers.All(x => !x.Checked)) {
                    var c = BookingConfiguration.Load();
                    c.SelectedBrowser = "WEB-Chrome";
                    c.Save();
                }
            };
        }
        if (browsers.All(x => !x.Checked)) chromeCheck.Checked = true;

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
            chromeCheck.Checked = true;
        };

        var altAvailCheck    = new CheckBox { Text = "Alternate Avail", Checked = false };
        var directLoginCheck = new CheckBox { Text = "Direct Login",    Checked = false };

        Controls.Add(BuildCheckRow(chromeCheck, operaCheck, braveCheck, cometCheck, app1Check, app2Check));
        Controls.Add(BuildCheckRow(betaCheck, altAvailCheck, directLoginCheck, resetBtn));

        // Bring to front so DockStyle.Top stacks correctly
        foreach (Control c in Controls) c.BringToFront();

        ResumeLayout();
    }

    private static FlowLayoutPanel BuildCheckRow(params Control[] controls)
    {
        var row = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 32,
            BackColor     = UiTheme.PageBg,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            Padding       = new Padding(10, 6, 0, 0)
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
        if (string.IsNullOrEmpty(page))
        {
            MessageBox.Show(FindForm(), $"{label} will be added next.", "RailBot Pro",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        NavigateRequested?.Invoke(this, page);
    }
}
