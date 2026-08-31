namespace train_automation;

public sealed class HistoryLogsPanel : UserControl
{
    private readonly FlowLayoutPanel _tabStrip = new();
    private readonly Panel _historyPane = new();
    private readonly Panel _logsPane = new();
    
    private readonly Button _btnHistory = new();
    private readonly Button _btnLogs = new();
    
    private readonly ListBox _historyList = new();
    private readonly RichTextBox _logsText = new();

    public HistoryLogsPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        Font = UiTheme.BodySm;

        BuildLayout();
        SwitchTab("Logs");
    }

    private void BuildLayout()
    {
        _tabStrip.Dock = DockStyle.Top;
        _tabStrip.Height = 40;
        _tabStrip.Padding = new Padding(12, 6, 12, 0);
        _tabStrip.BackColor = UiTheme.PageBg;
        
        StyleTabButton(_btnHistory, "History");
        StyleTabButton(_btnLogs, "System Logs");
        
        _btnHistory.Click += (_, _) => SwitchTab("History");
        _btnLogs.Click += (_, _) => SwitchTab("Logs");

        _tabStrip.Controls.Add(_btnHistory);
        _tabStrip.Controls.Add(_btnLogs);
        
        Controls.Add(_tabStrip);

        var padPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        Controls.Add(padPanel);
        padPanel.BringToFront();

        // History Pane
        _historyPane.Dock = DockStyle.Fill;
        _historyPane.BackColor = UiTheme.Surface;
        
        _historyList.Dock = DockStyle.Fill;
        _historyList.BorderStyle = BorderStyle.None;
        _historyList.BackColor = UiTheme.Surface;
        _historyList.ForeColor = UiTheme.Text;
        _historyList.Font = new Font("Segoe UI", 9F);
        _historyList.Items.Add("No history records found.");
        
        var historyWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = UiTheme.OutlineVariant };
        historyWrapper.Controls.Add(_historyList);
        _historyPane.Controls.Add(historyWrapper);

        // Logs Pane
        _logsPane.Dock = DockStyle.Fill;
        _logsPane.BackColor = UiTheme.Surface;
        
        _logsText.Dock = DockStyle.Fill;
        _logsText.BorderStyle = BorderStyle.None;
        _logsText.BackColor = UiTheme.Surface;
        _logsText.ForeColor = UiTheme.Success;
        _logsText.Font = new Font("Consolas", 9F);
        _logsText.ReadOnly = true;
        _logsText.Text = "System initialized.\nLogs will appear here...";
        
        var logsWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = UiTheme.OutlineVariant };
        logsWrapper.Controls.Add(_logsText);
        _logsPane.Controls.Add(logsWrapper);

        padPanel.Controls.Add(_historyPane);
        padPanel.Controls.Add(_logsPane);
    }

    private void StyleTabButton(Button btn, string text)
    {
        btn.Text = text;
        btn.Size = new Size(110, 28);
        btn.FlatStyle = FlatStyle.Flat;
        btn.Margin = new Padding(0, 0, 10, 0);
        btn.Cursor = Cursors.Hand;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = UiTheme.Primary;
        SetTabActive(btn, false);
    }

    private void SetTabActive(Button btn, bool active)
    {
        if (active)
        {
            btn.BackColor = UiTheme.Primary;
            btn.ForeColor = Color.White;
        }
        else
        {
            btn.BackColor = UiTheme.PageBg;
            btn.ForeColor = UiTheme.Primary;
        }
    }

    private void SwitchTab(string tab)
    {
        SetTabActive(_btnHistory, tab == "History");
        SetTabActive(_btnLogs, tab == "Logs");

        if (tab == "History") _historyPane.BringToFront();
        else _logsPane.BringToFront();
    }
}
