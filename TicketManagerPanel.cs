namespace train_automation;

public sealed class TicketManagerPanel : UserControl
{
    private const int ROW_PX  = 26;   // must match _grid.RowTemplate.Height
    private const int BASE_H  = 112;  // header-area + col-header + footer + padding
    private const int MAX_H   = 390;  // cap so window doesn't run off screen

    private readonly DataGridView _grid = new();
    private readonly Label _countLabel = new();
    private List<TrainBookingRecord> _bookings = [];

    public event EventHandler<TrainBookingRecord>? OpenRunnerRequested;
    public event EventHandler? NavigateToNewTicket;

    public TicketManagerPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        Padding = new Padding(12);

        // ── Slim header ──────────────────────────────────────────────────────
        var header = new Label
        {
            Text      = "Ticket Manager",
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UiTheme.Text,
            AutoSize  = true,
            Location  = new Point(12, 10)
        };

        // ── Card wrapping the grid ───────────────────────────────────────────
        var card = UiTheme.CreateCard();
        card.Location = new Point(12, 36);
        card.Anchor   = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        ConfigureGrid();
        _grid.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);

        // ── Slim footer: count label only ────────────────────────────────────
        var footer = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 30,
            BackColor = UiTheme.SurfaceLow
        };
        var footerBorder = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = UiTheme.OutlineVariant };
        _countLabel.AutoSize  = true;
        _countLabel.Font      = new Font("Segoe UI", 8F);
        _countLabel.ForeColor = UiTheme.TextMuted;
        _countLabel.Location  = new Point(10, 8);
        footer.Controls.Add(_countLabel);
        footer.Controls.Add(footerBorder);
        card.Controls.Add(footer);

        Controls.Add(header);
        Controls.Add(card);

        Resize += (_, _) =>
        {
            card.Size = new Size(Math.Max(300, Width - 24), Math.Max(150, Height - 52));
        };

        Load += (_, _) => Reload();
    }

    public void Reload()
    {
        _bookings = BookingJsonStore.LoadAll();
        _grid.Rows.Clear();
        foreach (var booking in _bookings)
        {
            var name = string.IsNullOrWhiteSpace(booking.Preferences.TicketName)
                ? $"{booking.FromStation}_{booking.ToStation}"
                : booking.Preferences.TicketName;
            var idx = _grid.Rows.Add(
                name,
                $"{booking.FromStation} → {booking.ToStation}",
                booking.TravelDate.ToString("dd MMM yy"),
                $"{booking.Quota} / {booking.TravelClass}",
                1,
                0);
            _grid.Rows[idx].Tag = booking;
        }

        _countLabel.Text = $"{_bookings.Count} profile{(_bookings.Count == 1 ? "" : "s")}";
        AutoSizeParent();
    }

    /// <summary>Shrinks or grows the host Form so there is no wasted space below the grid.</summary>
    private void AutoSizeParent()
    {
        int idealClient = BASE_H + ROW_PX * _bookings.Count;
        int capped      = Math.Min(idealClient, MAX_H);
        if (FindForm() is { } frm)
            frm.ClientSize = new Size(frm.ClientSize.Width, capped);
    }

    private void ConfigureGrid()
    {
        _grid.AllowUserToAddRows    = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible     = false;
        _grid.BackgroundColor       = UiTheme.SurfaceLowest;
        _grid.BorderStyle           = BorderStyle.None;
        _grid.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect           = false;
        _grid.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Font                  = new Font("Segoe UI", 8F);

        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = UiTheme.SurfaceLowest,
            ForeColor          = UiTheme.TextMuted,
            Font               = new Font("Segoe UI", 8F, FontStyle.Bold),
            SelectionBackColor = UiTheme.SurfaceLowest
        };
        _grid.ColumnHeadersHeight        = 22;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = UiTheme.SurfaceLowest,
            ForeColor          = UiTheme.Text,
            Font               = new Font("Segoe UI", 8F),
            SelectionBackColor = UiTheme.SurfaceHigh,
            SelectionForeColor = UiTheme.Text
        };
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowTemplate.Height        = 26;   // compact rows

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Profile",       Name = "Profile",    FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Journey",       Name = "Journey",    FillWeight = 18 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date",          Name = "Date",       FillWeight = 13 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Quota/Class",   Name = "QuotaClass", FillWeight = 16 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Web",           Name = "Web",        FillWeight = 6  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "App",           Name = "App",        FillWeight = 6  });

        var editAction = new DataGridViewButtonColumn
        {
            HeaderText             = "",
            Name                   = "Edit",
            Text                   = "✎",
            UseColumnTextForButtonValue = true,
            FillWeight             = 5,
            FlatStyle              = FlatStyle.Flat
        };
        var delAction = new DataGridViewButtonColumn
        {
            HeaderText             = "",
            Name                   = "Delete",
            Text                   = "🗑",
            UseColumnTextForButtonValue = true,
            FillWeight             = 5,
            FlatStyle              = FlatStyle.Flat
        };
        var actions = new DataGridViewButtonColumn
        {
            HeaderText             = "",
            Name                   = "Actions",
            Text                   = "Open",
            UseColumnTextForButtonValue = true,
            FillWeight             = 8,
            FlatStyle              = FlatStyle.Flat
        };
        _grid.Columns.Add(editAction);
        _grid.Columns.Add(delAction);
        _grid.Columns.Add(actions);
        _grid.CellContentClick += Grid_CellContentClick;
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var colName = _grid.Columns[e.ColumnIndex].Name;
        if (_grid.Rows[e.RowIndex].Tag is TrainBookingRecord booking)
        {
            if (colName == "Actions")
            {
                OpenRunnerRequested?.Invoke(this, booking);
            }
            else if (colName == "Edit")
            {
                MessageBox.Show(FindForm(), "Editing existing bookings will be available in the next update.",
                    "Edit Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show(FindForm(), $"Delete booking profile '{booking.TrainNumber}'?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _bookings.Remove(booking);
                    BookingJsonStore.SaveAll(_bookings);
                    Reload();
                }
            }
        }
    }
}
