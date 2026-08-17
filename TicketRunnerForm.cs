namespace train_automation;

/// <summary>
/// Per-ticket runner popup (Stitch "Ticket Runner") — sized so the two-column
/// layout and parallel-session grid are fully visible.
/// </summary>
public sealed class TicketRunnerForm : Form
{
    private const int FormWidth  = 510;
    private const int FormHeight = 420;

    private readonly TrainBookingRecord _booking;
    private readonly Label _statusLabel = new();
    private readonly Label _runLabel = new();
    private readonly ToggleSwitch _runningToggle = new();

    public TicketRunnerForm(TrainBookingRecord booking)
    {
        _booking = booking;
        var route = $"{booking.FromStation} → {booking.ToStation}";
        Text = route;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = true;
        BackColor = UiTheme.PageBg;
        Font = UiTheme.BodySm;
        ClientSize = new Size(FormWidth, FormHeight);
        MinimumSize = new Size(FormWidth + 16, FormHeight + 40);
        AutoSize = false;

        Controls.Add(BuildBody(booking));
        Controls.Add(BuildFooter());
        Controls.Add(BuildHeader(route));
    }

    private static Panel BuildHeader(string route)
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = UiTheme.PageBg
        };
        var border = new Panel
        {
            Height = 1,
            Dock = DockStyle.Bottom,
            BackColor = UiTheme.OutlineVariant
        };
        var title = new Label
        {
            Text = route,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UiTheme.Text,
            AutoSize = true,
            Location = new Point(16, 13)
        };
        header.Controls.Add(title);
        header.Controls.Add(border);
        return header;
    }

    private Panel BuildBody(TrainBookingRecord booking)
    {
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.PageBg,
            Padding = new Padding(18)
        };

        // ── Top row: LIVE STATUS card | Train detail card ────────────────
        var statusCard = UiTheme.CreateCard();
        statusCard.Location = new Point(18, 10);
        statusCard.Size = new Size(230, 80);
        var statusCap = UiTheme.CreateCaption("Live Status");
        statusCap.Location = new Point(14, 10);
        _statusLabel.Text = "●  Ready";
        _statusLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _statusLabel.ForeColor = UiTheme.Success;
        _statusLabel.AutoSize = true;
        _statusLabel.Location = new Point(14, 36);
        statusCard.Controls.Add(statusCap);
        statusCard.Controls.Add(_statusLabel);

        var detailCard = UiTheme.CreateCard();
        detailCard.Location = new Point(260, 10);
        detailCard.Size = new Size(230, 80);
        var trainText = string.IsNullOrWhiteSpace(booking.TrainName)
            ? booking.TrainNumber
            : $"{booking.TrainNumber}  {booking.TrainName}";
        var trainLbl = new Label
        {
            Text = trainText,
            Font = UiTheme.LabelMd,
            ForeColor = UiTheme.TextMuted,
            AutoSize = false,
            AutoEllipsis = true,
            Location = new Point(14, 12),
            Size = new Size(200, 20)
        };
        var meta = new Label
        {
            Text = $"{booking.TravelClass}   ·   {booking.TravelDate:dd MMM yyyy}",
            Font = UiTheme.BodySm,
            ForeColor = UiTheme.Text,
            AutoSize = false,
            AutoEllipsis = true,
            Location = new Point(14, 40),
            Size = new Size(200, 20)
        };
        detailCard.Controls.Add(trainLbl);
        detailCard.Controls.Add(meta);

        // ── Second row: FARE LIMIT | QUOTA ───────────────────────────────
        var fareCap = UiTheme.CreateCaption("Fare Limit (Rs)");
        fareCap.Location = new Point(18, 104);
        var fareBox = new TextBox
        {
            Text = string.IsNullOrWhiteSpace(booking.Fare) || booking.Fare == "0" ? "0" : booking.Fare,
            Location = new Point(18, 122),
            Size = new Size(230, 28),
            Font = UiTheme.BodySm,
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Text,
            BorderStyle = BorderStyle.FixedSingle
        };

        var quotaCap = UiTheme.CreateCaption("Quota");
        quotaCap.Location = new Point(260, 104);
        var quotaBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(260, 122),
            Size = new Size(230, 28),
            Font = UiTheme.BodySm,
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Text,
            FlatStyle = FlatStyle.Flat
        };
        quotaBox.Items.AddRange(["General (GN)", "Tatkal (TQ)", "Premium Tatkal (PT)", "Ladies (LD)"]);
        quotaBox.SelectedIndex = booking.Quota.Contains("Tatkal", StringComparison.OrdinalIgnoreCase)
            ? booking.Quota.Contains("Premium", StringComparison.OrdinalIgnoreCase) ? 2 : 1
            : booking.Quota.Contains("Ladies", StringComparison.OrdinalIgnoreCase) ? 3 : 0;

        // ── Parallel Sessions ─────────────────────────────────────────────
        var sessionsCap = UiTheme.CreateCaption("Parallel Sessions");
        sessionsCap.Location = new Point(18, 168);

        var sessionsHost = new TableLayoutPanel
        {
            Location = new Point(18, 188),
            Size = new Size(472, 130),
            ColumnCount = 2,
            RowCount = 2,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = UiTheme.PageBg
        };
        sessionsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        sessionsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        sessionsHost.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        sessionsHost.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        sessionsHost.Controls.Add(CreateSessionButton("Browser 1", "Idle"), 0, 0);
        sessionsHost.Controls.Add(CreateSessionButton("Browser 2", "Idle"), 1, 0);
        sessionsHost.Controls.Add(CreateSessionButton("Browser 3", "Idle"), 0, 1);
        sessionsHost.Controls.Add(CreateSessionButton("Mobile App", "Idle"), 1, 1);

        body.Controls.Add(statusCard);
        body.Controls.Add(detailCard);
        body.Controls.Add(fareCap);
        body.Controls.Add(fareBox);
        body.Controls.Add(quotaCap);
        body.Controls.Add(quotaBox);
        body.Controls.Add(sessionsCap);
        body.Controls.Add(sessionsHost);

        body.Resize += (_, _) =>
        {
            var innerW = Math.Max(400, body.ClientSize.Width - 36);
            var half = (innerW - 12) / 2;
            statusCard.Width = half;
            detailCard.Left = 18 + half + 12;
            detailCard.Width = half;
            fareBox.Width = half;
            quotaCap.Left = detailCard.Left;
            quotaBox.Left = detailCard.Left;
            quotaBox.Width = half;
            sessionsHost.Width = innerW;
            trainLbl.Width = Math.Max(120, half - 28);
            meta.Width = trainLbl.Width;
        };

        return body;
    }

    private Panel BuildFooter()
    {
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = UiTheme.SurfaceContainer
        };
        var border = new Panel
        {
            Height = 1,
            Dock = DockStyle.Top,
            BackColor = UiTheme.OutlineVariant
        };

        _runningToggle.Location = new Point(18, 20);
        _runningToggle.CheckedChanged += (_, _) => UpdateRunningState();

        _runLabel.Text = "Ready";
        _runLabel.AutoSize = true;
        _runLabel.Font = UiTheme.LabelMd;
        _runLabel.Location = new Point(66, 20);

        var pairBtn = UiTheme.CreatePrimaryButton("Pair Device", 130, 34);
        pairBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pairBtn.Location = new Point(FormWidth - 150, 13);
        pairBtn.Click += (_, _) =>
            MessageBox.Show(this, "Pair Device will attach this ticket to a logged-in IRCTC session.", "Ticket Runner",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        footer.Controls.Add(_runningToggle);
        footer.Controls.Add(_runLabel);
        footer.Controls.Add(pairBtn);
        footer.Controls.Add(border);

        footer.Resize += (_, _) =>
        {
            pairBtn.Left = Math.Max(200, footer.ClientSize.Width - pairBtn.Width - 18);
        };

        return footer;
    }

    private void UpdateRunningState()
    {
        if (_runningToggle.Checked)
        {
            _statusLabel.Text = "●  Running";
            _statusLabel.ForeColor = UiTheme.Success;
            _runLabel.Text = "Running";
        }
        else
        {
            _statusLabel.Text = "●  Ready";
            _statusLabel.ForeColor = UiTheme.Success;
            _runLabel.Text = "Ready";
        }
    }

    private Button CreateSessionButton(string name, string state)
    {
        var btn = new Button
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5),
            FlatStyle = FlatStyle.Flat,
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Text,
            TextAlign = ContentAlignment.TopLeft,
            Padding = new Padding(10, 8, 0, 0),
            Cursor = Cursors.Hand,
            UseMnemonic = false,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Text = $"{name}{Environment.NewLine}{state}"
        };
        btn.FlatAppearance.BorderColor = UiTheme.OutlineVariant;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.MouseOverBackColor = UiTheme.SurfaceHigh;
        btn.MouseEnter += (_, _) => btn.ForeColor = Color.White;
        btn.MouseLeave += (_, _) => btn.ForeColor = UiTheme.Text;
        btn.Click += (_, _) =>
        {
            _runningToggle.Checked = true;
            UpdateRunningState();
            btn.Text = $"{name}{Environment.NewLine}● Active";
            btn.ForeColor = UiTheme.Success;
            MessageBox.Show(this,
                $"{name} would launch an independent booking session for {_booking.FromStation} → {_booking.ToStation}.\n\n" +
                "Parallel multi-browser automation will be wired to the booking engine next.",
                "Parallel Session",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };
        return btn;
    }
}
