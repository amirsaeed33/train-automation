namespace train_automation;

public sealed class IrctcAccountsPanel : UserControl
{
    private readonly DataGridView _grid = new();

    public IrctcAccountsPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        AutoScroll = true;
        Padding = new Padding(24);

        var title = new Label
        {
            Text = "Account Manager",
            Font = UiTheme.HeadlineLg,
            ForeColor = UiTheme.Text,
            AutoSize = true,
            Location = new Point(24, 16)
        };
        var sub = new Label
        {
            Text = "Manage and monitor linked IRCTC credentials for automated booking.",
            Font = UiTheme.BodySm,
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(24, 48)
        };

        var addBtn = UiTheme.CreateSecondaryButton("Add Account", 120, 34);
        addBtn.Location = new Point(700, 24);
        addBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        addBtn.Click += (_, _) => ShowAddAccount();

        var manageBtn = UiTheme.CreatePrimaryButton("Manage Bookings", 150, 34);
        manageBtn.Location = new Point(830, 24);
        manageBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        manageBtn.Click += (_, _) =>
            MessageBox.Show(FindForm(), "Manage Bookings will open the Ticket Manager.", "IRCTC Accounts",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        var statsHost = new Panel { Location = new Point(24, 90), Size = new Size(1000, 80) };
        AddStat(statsHost, "1", "Total IDs", UiTheme.Text, 0);
        AddStat(statsHost, "1", "Active", UiTheme.Success, 1);
        AddStat(statsHost, "0", "Deactivated", UiTheme.TextMuted, 2);
        AddStat(statsHost, "0", "Disabled", UiTheme.Warning, 3);
        AddStat(statsHost, "0", "Invalid", UiTheme.Danger, 4);
        AddStat(statsHost, "0", "Unverified", UiTheme.TextMuted, 5);

        var card = UiTheme.CreateCard();
        card.Location = new Point(24, 186);
        card.Size = new Size(1000, 360);
        card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        ConfigureGrid();
        _grid.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);

        Controls.Add(title);
        Controls.Add(sub);
        Controls.Add(addBtn);
        Controls.Add(manageBtn);
        Controls.Add(statsHost);
        Controls.Add(card);

        Resize += (_, _) =>
        {
            manageBtn.Left = Math.Max(400, Width - manageBtn.Width - 24);
            addBtn.Left = manageBtn.Left - addBtn.Width - 10;
            card.Width = Math.Max(600, Width - 48);
            card.Height = Math.Max(220, Height - 210);
            statsHost.Width = card.Width;
        };

        Load += (_, _) => ReloadFromConfig();
    }

    private void ReloadFromConfig()
    {
        _grid.Rows.Clear();
        var config = BookingConfiguration.Load();
        var user = config.Credentials.Username;
        if (string.IsNullOrWhiteSpace(user))
        {
            return;
        }

        var mobile = string.IsNullOrWhiteSpace(config.MobileNumber) ? "—" : MaskMobile(config.MobileNumber);
        _grid.Rows.Add(user, "••••••••", $"{config.Passengers.Count} pax saved", mobile, "Active");
    }

    private static string MaskMobile(string mobile)
    {
        if (mobile.Length < 4)
        {
            return mobile;
        }

        return mobile[..2] + "****" + mobile[^4..];
    }

    private void ShowAddAccount()
    {
        using var dlg = new Form
        {
            Text = "Add IRCTC Account",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(360, 180),
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = UiTheme.PageBg,
            ForeColor = UiTheme.Text
        };
        var userCap = UiTheme.CreateCaption("User ID");
        userCap.Location = new Point(20, 16);
        var userBox = new TextBox { Location = new Point(20, 34), Size = new Size(310, 28) };
        var passCap = UiTheme.CreateCaption("Password");
        passCap.Location = new Point(20, 72);
        var passBox = new TextBox { Location = new Point(20, 90), Size = new Size(310, 28), PasswordChar = '*' };
        var save = UiTheme.CreatePrimaryButton("Save", 100, 32);
        save.Location = new Point(230, 130);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(userBox.Text) || string.IsNullOrWhiteSpace(passBox.Text))
            {
                MessageBox.Show(dlg, "Enter username and password.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var config = BookingConfiguration.Load();
            config.Credentials.Username = userBox.Text.Trim();
            config.Credentials.Password = passBox.Text;
            config.Save();
            dlg.DialogResult = DialogResult.OK;
        };
        dlg.Controls.Add(userCap);
        dlg.Controls.Add(userBox);
        dlg.Controls.Add(passCap);
        dlg.Controls.Add(passBox);
        dlg.Controls.Add(save);
        if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
        {
            ReloadFromConfig();
        }
    }

    private static void AddStat(Control host, string value, string label, Color valueColor, int index)
    {
        var card = UiTheme.CreateCard();
        card.Size = new Size(150, 70);
        card.Location = new Point(index * 160, 0);
        var v = new Label
        {
            Text = value,
            Font = UiTheme.HeadlineMd,
            ForeColor = valueColor,
            AutoSize = true,
            Location = new Point(16, 12)
        };
        var l = new Label
        {
            Text = label.ToUpperInvariant(),
            Font = UiTheme.LabelSm,
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(16, 42)
        };
        card.Controls.Add(v);
        card.Controls.Add(l);
        host.Controls.Add(card);
    }

    private void ConfigureGrid()
    {
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = UiTheme.SurfaceLowest;
        _grid.BorderStyle = BorderStyle.None;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            ForeColor = UiTheme.TextMuted,
            Font = UiTheme.LabelMd,
            BackColor = UiTheme.SurfaceLowest,
            SelectionBackColor = UiTheme.SurfaceLowest
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.SurfaceLowest,
            ForeColor = UiTheme.Text,
            SelectionBackColor = UiTheme.SurfaceHigh,
            SelectionForeColor = UiTheme.Text
        };
        _grid.Columns.Add("Username", "Username");
        _grid.Columns.Add("Password", "Password");
        _grid.Columns.Add("Pnrs", "Saved Data");
        _grid.Columns.Add("Mobile", "Linked Mobile");
        _grid.Columns.Add("Status", "Status");
    }
}
