namespace train_automation;

public sealed class IrctcAccountsPanel : UserControl
{
    private const int ROW_PX  = 26;
    private const int BASE_H  = 112;
    private const int MAX_H   = 390;

    private readonly DataGridView _grid = new();
    private readonly Label _countLabel = new();

    public IrctcAccountsPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        Padding = new Padding(12);

        // ── Slim header ──────────────────────────────────────────────────────
        var header = new Label
        {
            Text      = "Account Manager",
            Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UiTheme.Text,
            AutoSize  = true,
            Location  = new Point(12, 10)
        };

        var addBtn = UiTheme.CreatePrimaryButton("Add Account", 110, 26);
        addBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        addBtn.Location = new Point(Width - addBtn.Width - 12, 8);
        addBtn.Click += (_, _) => ShowAddAccount();

        // ── Card wrapping the grid ───────────────────────────────────────────
        var card = UiTheme.CreateCard();
        card.Location = new Point(12, 40);
        card.Anchor   = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        ConfigureGrid();
        _grid.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);

        // ── Slim footer ──────────────────────────────────────────────────────
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
        Controls.Add(addBtn);
        Controls.Add(card);

        Load += (_, _) => ReloadFromConfig();
    }

    private void ReloadFromConfig()
    {
        _grid.Rows.Clear();
        var config = BookingConfiguration.Load();
        
        // Migrate legacy single account if it exists and isn't in SavedAccounts yet
        if (!string.IsNullOrWhiteSpace(config.Credentials.Username) && 
            !config.SavedAccounts.Any(a => a.Username.Equals(config.Credentials.Username, StringComparison.OrdinalIgnoreCase)))
        {
            config.SavedAccounts.Add(new IrctcCredentials
            {
                Username = config.Credentials.Username,
                Password = config.Credentials.Password
            });
            config.Save();
        }

        var mobile = string.IsNullOrWhiteSpace(config.MobileNumber) ? "—" : MaskMobile(config.MobileNumber);

        foreach (var acc in config.SavedAccounts)
        {
            _grid.Rows.Add(acc.Username, "••••••••", $"{config.Passengers.Count} pax saved", mobile, "Active");
        }

        int count = _grid.Rows.Count;
        _countLabel.Text = $"{count} account{(count == 1 ? "" : "s")}";
        
        AutoSizeParent();
    }

    /// <summary>Shrinks or grows the host Form so there is no wasted space below the grid.</summary>
    private void AutoSizeParent()
    {
        int idealClient = BASE_H + ROW_PX * _grid.Rows.Count;
        int capped      = Math.Min(idealClient, MAX_H);
        if (FindForm() is { } frm)
            frm.ClientSize = new Size(frm.ClientSize.Width, capped);
    }

    private static string MaskMobile(string mobile)
    {
        if (mobile.Length < 4) return mobile;
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
            
            // Add to saved accounts list
            if (!config.SavedAccounts.Any(a => a.Username.Equals(userBox.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                config.SavedAccounts.Add(new IrctcCredentials
                {
                    Username = userBox.Text.Trim(),
                    Password = passBox.Text
                });
            }
            
            // Also keep it as the primary selected account for legacy fields
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
        _grid.Font = new Font("Segoe UI", 8F);

        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.SurfaceLowest,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            SelectionBackColor = UiTheme.SurfaceLowest
        };
        _grid.ColumnHeadersHeight = 22;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.SurfaceLowest,
            ForeColor = UiTheme.Text,
            Font = new Font("Segoe UI", 8F),
            SelectionBackColor = UiTheme.SurfaceHigh,
            SelectionForeColor = UiTheme.Text
        };
        _grid.EnableHeadersVisualStyles = false;
        _grid.RowTemplate.Height = 26;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Username", Name = "Username", FillWeight = 25, MinimumWidth = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Password", Name = "Password", FillWeight = 15, MinimumWidth = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Saved Data", Name = "Pnrs", FillWeight = 22, MinimumWidth = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Linked Mobile", Name = "Mobile", FillWeight = 20, MinimumWidth = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", Name = "Status", FillWeight = 18, MinimumWidth = 60 });
    }
}
