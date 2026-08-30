namespace train_automation;

public sealed class IrctcAccountsPanel : UserControl
{
    private const int ROW_PX  = 26;
    private const int BASE_H  = 112;
    private const int MAX_H   = 390;

    private readonly Panel _accountsContainer = new();
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

        _accountsContainer.Dock = DockStyle.Fill;
        _accountsContainer.AutoScroll = true;
        _accountsContainer.BackColor = UiTheme.PageBg;
        card.Controls.Add(_accountsContainer);

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
        _accountsContainer.Controls.Clear();
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

        int[] w = { 180, 110, 120, 110, 70 };
        string[] headers = { "Username", "Password", "Saved Data", "Linked Mobile", "Status" };
        int currentX = 10;
        int currentY = 10;

        // Header
        for (int i = 0; i < headers.Length; i++)
        {
            var lbl = new Label
            {
                Text = headers[i],
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Location = new Point(currentX, currentY),
                Size = new Size(w[i], 18),
                ForeColor = UiTheme.Text,
                TextAlign = ContentAlignment.BottomLeft
            };
            _accountsContainer.Controls.Add(lbl);
            currentX += w[i] + 4;
        }

        currentY += 18;

        // Separator
        var sep = new Panel
        {
            Location = new Point(10, currentY),
            Size = new Size(620, 1),
            BackColor = UiTheme.OutlineVariant
        };
        _accountsContainer.Controls.Add(sep);
        currentY += 4;

        foreach (var acc in config.SavedAccounts)
        {
            currentX = 10;

            var tUser = new TextBox { Text = acc.Username, Location = new Point(currentX, currentY), Size = new Size(w[0], 18), Font = new Font("Segoe UI", 7.5F), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            _accountsContainer.Controls.Add(tUser);
            currentX += w[0] + 4;

            var tPass = new TextBox { Text = "••••••••", Location = new Point(currentX, currentY), Size = new Size(w[1], 18), Font = new Font("Segoe UI", 7.5F), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            _accountsContainer.Controls.Add(tPass);
            currentX += w[1] + 4;

            var tData = new TextBox { Text = $"{config.Passengers.Count} pax saved", Location = new Point(currentX, currentY), Size = new Size(w[2], 18), Font = new Font("Segoe UI", 7.5F), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            _accountsContainer.Controls.Add(tData);
            currentX += w[2] + 4;

            var tMob = new TextBox { Text = mobile, Location = new Point(currentX, currentY), Size = new Size(w[3], 18), Font = new Font("Segoe UI", 7.5F), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            _accountsContainer.Controls.Add(tMob);
            currentX += w[3] + 4;

            var tStat = new TextBox { Text = "Active", Location = new Point(currentX, currentY), Size = new Size(w[4], 18), Font = new Font("Segoe UI", 7.5F), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            _accountsContainer.Controls.Add(tStat);

            currentY += 26;
        }

        int count = config.SavedAccounts.Count;
        _countLabel.Text = $"{count} account{(count == 1 ? "" : "s")}";
        
        AutoSizeParent(count);
    }

    /// <summary>Shrinks or grows the host Form so there is no wasted space below the grid.</summary>
    private void AutoSizeParent(int rowCount)
    {
        int idealClient = BASE_H + ROW_PX * rowCount + 40;
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


}
