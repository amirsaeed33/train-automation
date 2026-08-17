namespace train_automation;

/// <summary>
/// Launcher page matching the Stitch Main Launcher:
/// 4x2 action cards, then Target Environment + Global Settings side by side.
/// </summary>
public sealed class LauncherPanel : UserControl
{
    private readonly Panel _canvas = new();

    public event EventHandler<string>? NavigateRequested;

    public LauncherPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        AutoScroll = true;

        _canvas.Location = new Point(12, 12);
        _canvas.Size = new Size(820, 520);
        Controls.Add(_canvas);

        Rebuild(820);
        Resize += (_, _) =>
        {
            var w = Math.Clamp(ClientSize.Width - 24, 600, 980);
            if (Math.Abs(_canvas.Width - w) > 8)
            {
                Rebuild(w);
            }
        };
    }

    private void Rebuild(int width)
    {
        _canvas.SuspendLayout();
        _canvas.Controls.Clear();
        _canvas.Width = width;

        var actions = BuildActionGrid(width);
        actions.Location = new Point(0, 0);
        _canvas.Controls.Add(actions);

        var envCard = BuildEnvironmentCard((width / 2) - 8);
        envCard.Location = new Point(0, actions.Bottom + 16);
        _canvas.Controls.Add(envCard);

        var globalCard = BuildGlobalSettingsCard((width / 2) - 8);
        globalCard.Location = new Point(envCard.Right + 16, actions.Bottom + 16);
        
        var cardH = Math.Max(envCard.Height, globalCard.Height);
        envCard.Height = cardH;
        globalCard.Height = cardH;
        _canvas.Controls.Add(globalCard);

        _canvas.Height = Math.Max(envCard.Bottom, globalCard.Bottom) + 8;
        _canvas.ResumeLayout();
    }

    private Panel BuildActionGrid(int width)
    {
        const int cols = 4;
        const int rows = 2;
        const int gap = 8;
        const int cardH = 44;
        var cardW = (width - (gap * (cols - 1))) / cols;

        var host = new Panel
        {
            Size = new Size(width, (cardH * rows) + (gap * (rows - 1)))
        };

        var items = new (string Line1, string Page, bool Primary)[]
        {
            ("Add IRCTC Account", "accounts", false),
            ("New Ticket", "new-ticket", true),
            ("Open Tickets", "tickets", false),
            ("History and Logs", "logs", false),
            ("Add Bank / UPI", "", false),
            ("OTP Bypass Settings", "", false),
            ("Backup and Restore", "", false),
            ("IP / Block Check", "", false)
        };

        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var col = i % cols;
            var row = i / cols;
            var btn = new Button
            {
                Location = new Point(col * (cardW + gap), row * (cardH + gap)),
                Size = new Size(cardW, cardH),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseMnemonic = false,
                Text = item.Line1
            };
            btn.FlatAppearance.BorderSize = 1;

            if (item.Primary)
            {
                btn.BackColor = UiTheme.Primary;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = UiTheme.Primary;
            }
            else
            {
                btn.BackColor = UiTheme.SurfaceLowest;
                btn.ForeColor = UiTheme.Text;
                btn.FlatAppearance.BorderColor = UiTheme.OutlineVariant;
            }

            var page = item.Page;
            var label = item.Line1;
            btn.Click += (_, _) => OnActionClick(page, label);
            host.Controls.Add(btn);
        }

        return host;
    }

    private void OnActionClick(string page, string label)
    {
        if (string.IsNullOrEmpty(page))
        {
            MessageBox.Show(FindForm(), $"{label} will be added next.", "RailBot Pro",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (page == "logs")
        {
            MessageBox.Show(FindForm(), "Logs screen will be wired next.", "RailBot Pro",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        NavigateRequested?.Invoke(this, page);
    }

    private static Panel BuildEnvironmentCard(int width)
    {
        var items = new (string Label, bool On)[]
        {
            ("Chrome", true), ("Edge", false),
            ("Opera", true), ("Brave", false),
            ("Mobile App 1", false), ("Mobile App 2", false)
        };

        return BuildToggleCard("TARGET ENVIRONMENT", width, items, columns: 2);
    }

    private static Panel BuildGlobalSettingsCard(int width)
    {
        var items = new (string Label, bool On)[]
        {
            ("Beta Booking Flow", false),
            ("Check Alternate Availability", true),
            ("Screen Recording Mode", false),
            ("Direct Login (skip OTP)", true)
        };

        return BuildToggleCard("GLOBAL SETTINGS", width, items, columns: 1);
    }

    private static Panel BuildToggleCard(string title, int width, (string Label, bool On)[] items, int columns)
    {
        const int pad = 14;
        const int gap = 8;
        const int rowH = 42;
        const int headerH = 28;

        var rows = (int)Math.Ceiling(items.Length / (double)columns);
        var height = pad + headerH + (rows * rowH) + ((rows - 1) * gap) + pad;

        var card = new Panel
        {
            Size = new Size(width, height),
            BackColor = UiTheme.SurfaceLowest,
            BorderStyle = BorderStyle.FixedSingle
        };

        var header = new Label
        {
            Text = title,
            Font = UiTheme.LabelSm,
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(pad, 12)
        };
        card.Controls.Add(header);

        var innerW = width - (pad * 2);
        var cellW = columns == 1
            ? innerW
            : (innerW - gap) / 2;

        for (var i = 0; i < items.Length; i++)
        {
            var col = i % columns;
            var row = i / columns;
            var x = pad + (col * (cellW + gap));
            var y = pad + headerH + (row * (rowH + gap));
            card.Controls.Add(CreateToggleCell(items[i].Label, items[i].On, x, y, cellW, rowH));
        }

        return card;
    }

    private static Panel CreateToggleCell(string label, bool on, int x, int y, int width, int height)
    {
        var cell = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = UiTheme.SurfaceContainer,
            BorderStyle = BorderStyle.FixedSingle
        };

        var toggle = new ToggleSwitch
        {
            Checked = on,
            Size = new Size(38, 20),
            Location = new Point(width - 38 - 10, (height - 20) / 2)
        };

        var text = new Label
        {
            Text = label,
            Font = UiTheme.LabelMd,
            ForeColor = UiTheme.Text,
            AutoSize = false,
            AutoEllipsis = true,
            UseMnemonic = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(10, 0),
            Size = new Size(Math.Max(40, toggle.Left - 18), height)
        };

        cell.Controls.Add(text);
        cell.Controls.Add(toggle);
        return cell;
    }
}
