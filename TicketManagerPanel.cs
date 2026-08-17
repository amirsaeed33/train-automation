namespace train_automation;

public sealed class TicketManagerPanel : UserControl
{
    private readonly DataGridView _grid = new();
    private readonly Label _countLabel = new();
    private List<TrainBookingRecord> _bookings = [];

    public event EventHandler<TrainBookingRecord>? OpenRunnerRequested;
    public event EventHandler? NavigateToNewTicket;

    public TicketManagerPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        Padding = new Padding(24);

        var header = new Label
        {
            Text = "Ticket Manager",
            Font = UiTheme.HeadlineLg,
            ForeColor = UiTheme.Text,
            AutoSize = true,
            Location = new Point(24, 20)
        };
        var sub = new Label
        {
            Text = "Manage saved booking profiles. Web/App counts control how many parallel sessions attempt each booking.",
            Font = UiTheme.BodySm,
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(24, 52),
            MaximumSize = new Size(900, 0)
        };

        var card = UiTheme.CreateCard();
        card.Location = new Point(24, 90);
        card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        ConfigureGrid();
        _grid.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = UiTheme.SurfaceLow
        };
        var footerBorder = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = UiTheme.OutlineVariant };
        _countLabel.AutoSize = true;
        _countLabel.Font = UiTheme.BodySm;
        _countLabel.ForeColor = UiTheme.TextMuted;
        _countLabel.Location = new Point(16, 18);
        var deleteAll = UiTheme.CreateSecondaryButton("Delete All", 120, 34);
        deleteAll.ForeColor = UiTheme.Danger;
        deleteAll.FlatAppearance.BorderColor = UiTheme.Danger;
        deleteAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        deleteAll.Click += (_, _) => DeleteAll();
        var openAll = UiTheme.CreatePrimaryButton("Open All Tickets", 150, 34);
        openAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        openAll.Click += (_, _) => OpenAll();
        footer.Controls.Add(_countLabel);
        footer.Controls.Add(deleteAll);
        footer.Controls.Add(openAll);
        footer.Controls.Add(footerBorder);
        footer.Resize += (_, _) =>
        {
            openAll.Left = footer.Width - openAll.Width - 16;
            openAll.Top = 10;
            deleteAll.Left = openAll.Left - deleteAll.Width - 10;
            deleteAll.Top = 10;
        };
        card.Controls.Add(footer);

        Controls.Add(header);
        Controls.Add(sub);
        Controls.Add(card);

        Resize += (_, _) =>
        {
            card.Size = new Size(Math.Max(400, Width - 48), Math.Max(200, Height - 120));
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
                booking.TravelDate.ToString("dd MMM yyyy"),
                $"{booking.Quota} / {booking.TravelClass}",
                1,
                0);
            _grid.Rows[idx].Tag = booking;
        }

        _countLabel.Text = $"{_bookings.Count} profile{(_bookings.Count == 1 ? "" : "s")} total";
    }

    private void ConfigureGrid()
    {
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = UiTheme.SurfaceLowest;
        _grid.BorderStyle = BorderStyle.None;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.SurfaceLowest,
            ForeColor = UiTheme.TextMuted,
            Font = UiTheme.LabelMd,
            SelectionBackColor = UiTheme.SurfaceLowest
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.SurfaceLowest,
            ForeColor = UiTheme.Text,
            SelectionBackColor = UiTheme.SurfaceHigh,
            SelectionForeColor = UiTheme.Text
        };
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowTemplate.Height = 56;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Profile", Name = "Profile", FillWeight = 18 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Journey", Name = "Journey", FillWeight = 16 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", Name = "Date", FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Quota / Class", Name = "QuotaClass", FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Web", Name = "Web", FillWeight = 8 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "App", Name = "App", FillWeight = 8 });
        var editAction = new DataGridViewButtonColumn
        {
            HeaderText = "",
            Name = "Edit",
            Text = "✎",
            UseColumnTextForButtonValue = true,
            FillWeight = 5,
            FlatStyle = FlatStyle.Flat
        };
        var delAction = new DataGridViewButtonColumn
        {
            HeaderText = "",
            Name = "Delete",
            Text = "🗑",
            UseColumnTextForButtonValue = true,
            FillWeight = 5,
            FlatStyle = FlatStyle.Flat
        };
        var actions = new DataGridViewButtonColumn
        {
            HeaderText = "Actions",
            Name = "Actions",
            Text = "Open",
            UseColumnTextForButtonValue = true,
            FillWeight = 10,
            FlatStyle = FlatStyle.Flat
        };
        _grid.Columns.Add(editAction);
        _grid.Columns.Add(delAction);
        _grid.Columns.Add(actions);
        _grid.CellContentClick += Grid_CellContentClick;
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var colName = _grid.Columns[e.ColumnIndex].Name;
        if (_grid.Rows[e.RowIndex].Tag is TrainBookingRecord booking)
        {
            if (colName == "Actions")
            {
                OpenRunnerRequested?.Invoke(this, booking);
            }
            else if (colName == "Edit")
            {
                MessageBox.Show(FindForm(), "Editing existing bookings will be available in the next update.", "Edit Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show(FindForm(), $"Delete booking profile '{booking.TrainNumber}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _bookings.Remove(booking);
                    BookingJsonStore.SaveAll(_bookings);
                    Reload();
                }
            }
        }
    }

    private void OpenAll()
    {
        if (_bookings.Count == 0)
        {
            MessageBox.Show(FindForm(), "No saved tickets yet. Create one from New Ticket.", "Ticket Manager",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            NavigateToNewTicket?.Invoke(this, EventArgs.Empty);
            return;
        }

        foreach (var booking in _bookings)
        {
            OpenRunnerRequested?.Invoke(this, booking);
        }
    }

    private void DeleteAll()
    {
        if (_bookings.Count == 0)
        {
            return;
        }

        if (MessageBox.Show(FindForm(), "Delete all saved ticket profiles?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        BookingJsonStore.ClearAll();
        Reload();
    }
}
