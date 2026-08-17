namespace train_automation;

public sealed class MainShellForm : Form
{
    private readonly Panel _topBar = new();
    private readonly Panel _contentHost = new();
    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Control> _pages = new(StringComparer.Ordinal);

    private Form1? _newTicketForm;
    private LauncherPanel? _launcher;
    private TicketManagerPanel? _ticketManager;
    private IrctcAccountsPanel? _accounts;
    private string _activePage = "launcher";

    public MainShellForm()
    {
        Text = "RailBot Pro";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(700, 300);
        ClientSize = new Size(740, 320);
        BackColor = UiTheme.PageBg;
        Font = UiTheme.BodySm;

        BuildTopBar();
        BuildContentHost();

        Controls.Add(_contentHost);
        Controls.Add(_topBar);

        Shown += (_, _) => ShowPage("launcher");
    }

    public void NavigateTo(string pageKey) => ShowPage(pageKey);

    private void BuildTopBar()
    {
        _topBar.Dock = DockStyle.Top;
        _topBar.Height = 40;
        _topBar.BackColor = UiTheme.SurfaceLowest;
        _topBar.Padding = new Padding(12, 0, 12, 0);

        var border = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = UiTheme.OutlineVariant
        };

        var title = new Label
        {
            Text = "RAILBOT PRO",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = true,
            Location = new Point(12, 10)
        };
        _topBar.Controls.Add(title);

        var navHost = new FlowLayoutPanel
        {
            Location = new Point(160, 0),
            Height = 39,
            Width = 500,
            BackColor = UiTheme.SurfaceLowest,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };
        
        AddNav(navHost, "launcher", "Launcher");
        AddNav(navHost, "tickets", "Ticket Manager");
        AddNav(navHost, "accounts", "Accounts");

        var keyBadge = new Label
        {
            AutoSize = true,
            Text = "Key: MS1 · 12 days left",
            Font = UiTheme.LabelMd,
            ForeColor = UiTheme.TextMuted,
            BackColor = UiTheme.SurfaceHigh,
            Padding = new Padding(6, 4, 6, 4),
            Location = new Point(ClientSize.Width - 280, 8)
        };
        keyBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        var connected = new Label
        {
            AutoSize = true,
            Text = "● Connected",
            Font = UiTheme.LabelMd,
            ForeColor = UiTheme.Success,
            Location = new Point(ClientSize.Width - 120, 12)
        };
        connected.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        _topBar.Controls.Add(navHost);
        _topBar.Controls.Add(keyBadge);
        _topBar.Controls.Add(connected);
        _topBar.Controls.Add(border);

        Resize += (_, _) =>
        {
            keyBadge.Left = Math.Max(500, _topBar.Width - 280);
            connected.Left = Math.Max(640, _topBar.Width - 120);
        };
    }

    private void AddNav(Control parent, string key, string label)
    {
        var btn = new Button
        {
            Text = label,
            Height = 32,
            AutoSize = true,
            MinimumSize = new Size(90, 32),
            FlatStyle = FlatStyle.Flat,
            Font = UiTheme.LabelMd,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 4, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        UiTheme.ApplyNavStyle(btn, false);
        btn.Click += (_, _) => ShowPage(key);
        _navButtons[key] = btn;
        parent.Controls.Add(btn);
    }

    private void BuildContentHost()
    {
        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = UiTheme.PageBg;
        _contentHost.Padding = new Padding(0);
    }

    private void ShowPage(string key)
    {
        if (key == "new-ticket")
        {
            var frm = new Form1();
            frm.Show(this);
            return;
        }

        if (key is "settings" or "logs")
        {
            MessageBox.Show(this,
                key == "settings"
                    ? "Settings screen will be wired next."
                    : "Logs screen will be wired next.",
                "RailBot Pro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _activePage = key;
        foreach (var (navKey, btn) in _navButtons)
        {
            if (navKey is "settings" or "logs")
            {
                UiTheme.ApplyNavStyle(btn, false);
                continue;
            }

            UiTheme.ApplyNavStyle(btn, navKey == key);
        }

        EnsurePage(key);
        _contentHost.SuspendLayout();
        _contentHost.Controls.Clear();
        var page = _pages[key];
        page.Dock = DockStyle.Fill;
        _contentHost.Controls.Add(page);
        page.BringToFront();
        if (page is Form form && !form.Visible)
        {
            form.Show();
        }

        if (key == "tickets" && _ticketManager is not null)
        {
            _ticketManager.Reload();
        }

        _contentHost.ResumeLayout();
    }

    private void EnsurePage(string key)
    {
        if (_pages.ContainsKey(key))
        {
            return;
        }

        Control page = key switch
        {
            "launcher" => _launcher ??= CreateLauncher(),
            "tickets" => _ticketManager ??= CreateTicketManager(),
            "accounts" => _accounts ??= new IrctcAccountsPanel(),
            _ => new Panel { BackColor = UiTheme.PageBg }
        };
        _pages[key] = page;
    }

    private LauncherPanel CreateLauncher()
    {
        var panel = new LauncherPanel();
        panel.NavigateRequested += (_, page) => ShowPage(page);
        return panel;
    }

    private TicketManagerPanel CreateTicketManager()
    {
        var panel = new TicketManagerPanel();
        panel.OpenRunnerRequested += (_, booking) =>
        {
            var runner = new TicketRunnerForm(booking);
            runner.Show(this);
        };
        panel.NavigateToNewTicket += (_, _) => ShowPage("new-ticket");
        return panel;
    }
}
