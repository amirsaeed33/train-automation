namespace train_automation;

/// <summary>
/// Per-ticket runner popup — compact Hitman-style layout.
/// Layout constants are documented inline so pixel math is easy to verify.
/// </summary>
public sealed class TicketRunnerForm : Form
{
    private readonly TrainBookingRecord _booking;
    private readonly Label _statusLabel = new();

    public TicketRunnerForm(TrainBookingRecord booking)
    {
        _booking = booking;
        Text            = $"{booking.FromStation} → {booking.ToStation}";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        StartPosition   = FormStartPosition.CenterParent;
        MaximizeBox     = false;
        MinimizeBox     = true;
        BackColor       = UiTheme.PageBg;
        Font            = new Font("Segoe UI", 9F);
        BuildUi(booking);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void BuildUi(TrainBookingRecord booking)
    {
        // ── Layout constants ─────────────────────────────────────────────────
        const int W     = 520;  // form client width (compact again)
        const int PAD   = 10;   // left/right edge padding
        const int ROW_H = 36;   // vertical stride (plenty of breathing room for smaller font)
        const int C_H   = 24;   // combo height
        const int C_TOP = 6;    // offset from row-start to combo top
        const int L_TOP = 9;    // offset from row-start to label baseline
        const int GAP   = 6;    // gap between session buttons
        int y = 0;

        // ── STATUS STRIP (green, 22 px) ───────────────────────────────────────
        var strip = new Panel { Location = new Point(0, y), Size = new Size(W, 22), BackColor = UiTheme.Success };
        _statusLabel.Text      = "Ready";
        _statusLabel.Font      = new Font("Segoe UI", 8F, FontStyle.Bold); // smaller font
        _statusLabel.ForeColor = Color.Black;
        _statusLabel.AutoSize  = true;
        _statusLabel.Location  = new Point(PAD, 4);
        strip.Controls.Add(_statusLabel);
        Controls.Add(strip);
        y += 22;

        // ── INFO BAR (dark, 24 px): From   To   TrainNo   Class:Quota   Date ─
        var infoBar = new Panel { Location = new Point(0, y), Size = new Size(W, 24), BackColor = UiTheme.SurfaceLow };
        var quota   = booking.Quota.Contains("Premium", StringComparison.OrdinalIgnoreCase) ? "PT"
                    : booking.Quota.Contains("Tatkal",  StringComparison.OrdinalIgnoreCase) ? "TQ"
                    : booking.Quota.Contains("Ladies",  StringComparison.OrdinalIgnoreCase) ? "LD" : "GN";
        
        string tClass = string.IsNullOrWhiteSpace(booking.TravelClass) ? "" : booking.TravelClass + ":";
        
        infoBar.Controls.Add(new Label
        {
            Text      = $"{booking.FromStation}   {booking.ToStation}   {booking.TrainNumber}   {tClass}{quota}   {booking.TravelDate:dd-MM}",
            Font      = new Font("Segoe UI", 8F, FontStyle.Bold), // smaller font
            ForeColor = UiTheme.Text,
            AutoSize  = true, 
            Location  = new Point(PAD, 5)
        });
        Controls.Add(infoBar);
        y += 24 + 8;  // 8 px breathing gap before interactive rows

        // ── ROW A: [Account: label] [acctCombo] [Slot: label] [slotCombo] [Pair] ─
        Controls.Add(MakeLbl("Account:", PAD, y + L_TOP));

        var cfg       = BookingConfiguration.Load();
        var acctCombo = MakeCmb(y + C_TOP, 95, 140); // Pushed x to 95 to give label maximum room
        acctCombo.Items.Add(string.IsNullOrWhiteSpace(cfg.Credentials.Username) ? "—" : cfg.Credentials.Username);
        acctCombo.SelectedIndex = 0;
        Controls.Add(acctCombo);

        Controls.Add(MakeLbl("Slot:", 245, y + L_TOP)); // Pushed right

        var slotCombo = MakeCmb(y + C_TOP, 305, 110); // Pushed x to 305
        slotCombo.Items.AddRange(["Auto Slot", "Slot-1", "Slot-2"]);
        slotCombo.SelectedIndex = 0;
        Controls.Add(slotCombo);

        // Shrink Pair button width to 80px (starts at 430, ends at 510 = W-PAD)
        Controls.Add(MakeBtn("Pair", 430, y + C_TOP - 1, 80, C_H + 2, UiTheme.Primary, Color.White));
        y += ROW_H;

        // ── ROW B: [□ Stop] [payCombo — fills rest] ───────────────────────────
        var stopCheck = new CheckBox
        {
            Text      = "Stop",
            Font      = new Font("Segoe UI", 8F), // smaller font
            ForeColor = UiTheme.Text,
            AutoSize  = true,
            Location  = new Point(PAD, y + L_TOP - 2)
        };
        Controls.Add(stopCheck);

        // Match payment combo X with account combo X
        var payCmb = MakeCmb(y + C_TOP, 95, W - 95 - PAD);
        payCmb.Items.AddRange(["PayTM-QR_paytm@qr", "PhonePe", "Amazon Pay", "BHIM/UPI"]);
        payCmb.SelectedIndex = 0;
        Controls.Add(payCmb);
        y += ROW_H;

        // ── ROW C: [Web-1] [Web-2] [Web-3] [APP] — equal widths ──────────────
        int btnW  = (W - 2 * PAD - 3 * GAP) / 4;
        var names = new[] { "Web-1", "Web-2", "Web-3", "APP" };
        for (int i = 0; i < names.Length; i++)
        {
            var b = MakeBtn(names[i], PAD + i * (btnW + GAP), y, btnW, 28, UiTheme.Surface, UiTheme.Text); // 28px tall buttons
            b.FlatAppearance.BorderColor        = UiTheme.OutlineVariant;
            b.FlatAppearance.BorderSize         = 1;
            b.FlatAppearance.MouseOverBackColor = UiTheme.Primary;
            int idx = i;
            b.Click += (_, _) => Launch(names[idx], b, stopCheck);
            Controls.Add(b);
        }
        y += 28; // button height

        // generous bottom padding
        ClientSize = new Size(W, y + 16);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void Launch(string name, Button btn, CheckBox stop)
    {
        _statusLabel.Text = $"Running — {name}";
        ((Panel)_statusLabel.Parent!).BackColor = UiTheme.Success;
        btn.BackColor = UiTheme.Primary;
        btn.ForeColor = Color.White;
        MessageBox.Show(this,
            $"{name} will launch a booking session for {_booking.FromStation} → {_booking.ToStation}.\n\n" +
            "Parallel multi-browser automation will be wired to the booking engine next.",
            "Session", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static Label MakeLbl(string text, int x, int y) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 8F), // smaller font
        ForeColor = UiTheme.Text,
        AutoSize  = true,
        Location  = new Point(x, y)
    };

    private static ComboBox MakeCmb(int y, int x, int w) => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Location      = new Point(x, y),
        Size          = new Size(w, 24),
        Font          = new Font("Segoe UI", 8F), // smaller font
        BackColor     = UiTheme.Surface,
        ForeColor     = UiTheme.Text,
        FlatStyle     = FlatStyle.Flat
    };

    private static Button MakeBtn(string text, int x, int y, int w, int h,
                                   Color bg, Color fg) => new()
    {
        Text      = text,
        Location  = new Point(x, y),
        Size      = new Size(w, h),
        FlatStyle = FlatStyle.Flat,
        BackColor = bg,
        ForeColor = fg,
        Font      = new Font("Segoe UI", 8F, FontStyle.Bold), // smaller font
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderSize = 0 }
    };
}
