namespace train_automation;

public sealed class MainShellForm : Form
{
    public MainShellForm()
    {
        Text            = "RailBot Pro";
        StartPosition   = FormStartPosition.CenterScreen;
        ClientSize      = new Size(760, 175);
        BackColor       = UiTheme.PageBg;
        Font            = UiTheme.BodySm;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        MinimizeBox     = false;

        var launcher = new LauncherPanel();
        launcher.Dock = DockStyle.Fill;
        launcher.NavigateRequested += OnNavigate;
        Controls.Add(launcher);
    }

    private void OnNavigate(object? sender, string page)
    {
        switch (page)
        {
            case "new-ticket":
                OpenFixedWindow(() => new NewTicketForm(), "New Ticket");
                break;

            case "accounts":
                OpenFixedWindow(() =>
                {
                    var panel = new IrctcAccountsPanel();
                    var w = WrapInWindow(panel, "IRCTC Accounts", 645, 500);
                    return w;
                }, "IRCTC Accounts");
                break;

            case "bank":
                OpenFixedWindow(() =>
                {
                    var panel = new BankManagerPanel();
                    var w = WrapInWindow(panel, "Bank Manager", 620, 440);
                    return w;
                }, "Bank Manager");
                break;

            case "tickets":
                OpenFixedWindow(() =>
                {
                    var panel = new TicketManagerPanel();
                    panel.OpenRunnerRequested += (_, booking) =>
                    {
                        var runner = new TicketRunnerForm(booking);
                        runner.Show(this);
                    };
                    panel.NavigateToNewTicket += (_, _) => OnNavigate(this, "new-ticket");
                    return WrapInWindow(panel, "Open Tickets", 580, 310);
                }, "Open Tickets");
                break;

            case "logs":
                OpenFixedWindow(() =>
                {
                    var panel = new HistoryLogsPanel();
                    return WrapInWindow(panel, "History & Logs", 450, 350);
                }, "History & Logs");
                break;

            default:
                MessageBox.Show(this, $"{page} coming soon.", "RailBot Pro",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
        }
    }

    /// <summary>Opens a single instance of a window type (brings to front if already open).</summary>
    private static readonly Dictionary<string, Form> _openWindows = new(StringComparer.Ordinal);

    private void OpenFixedWindow(Func<Form> factory, string key)
    {
        if (_openWindows.TryGetValue(key, out var existing) && !existing.IsDisposed)
        {
            existing.BringToFront();
            existing.Activate();
            return;
        }

        var frm = factory();
        ApplyWindowPolicy(frm);
        frm.FormClosed += (_, _) => _openWindows.Remove(key);
        _openWindows[key] = frm;
        frm.Show(this);
    }

    /// <summary>Enforces fixed-size, no-min/max policy on all popup windows.</summary>
    public static void ApplyWindowPolicy(Form frm)
    {
        frm.FormBorderStyle = FormBorderStyle.FixedSingle;
        frm.MaximizeBox     = false;
        frm.MinimizeBox     = false;
        frm.StartPosition   = FormStartPosition.CenterScreen;
    }

    /// <summary>Wraps a UserControl in a properly-styled popup Form.</summary>
    private static Form WrapInWindow(UserControl panel, string title, int width, int height)
    {
        var frm = new Form
        {
            Text        = title,
            ClientSize  = new Size(width, height),
            BackColor   = UiTheme.PageBg,
            Font        = UiTheme.BodySm,
        };
        panel.Dock = DockStyle.Fill;
        frm.Controls.Add(panel);
        return frm;
    }
}


