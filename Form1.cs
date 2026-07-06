namespace train_automation;

public partial class Form1 : Form
{
    private readonly TrainSearchSettings _settings = new();
    private EtrainScraperService? _scraper;

    public Form1()
    {
        InitializeComponent();
        ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        trainGrid.AutoGenerateColumns = false;
        trainGrid.Columns.Clear();
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.TrainNumber),
            HeaderText = "Train No.",
            FillWeight = 70
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.TrainName),
            HeaderText = "Train Name",
            FillWeight = 140
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.FromStation),
            HeaderText = "From",
            FillWeight = 90
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.Departure),
            HeaderText = "Departure",
            FillWeight = 70
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.ToStation),
            HeaderText = "To",
            FillWeight = 90
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.Arrival),
            HeaderText = "Arrival",
            FillWeight = 70
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.Duration),
            HeaderText = "Duration",
            FillWeight = 70
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.RunsOn),
            HeaderText = "Runs On",
            FillWeight = 110
        });
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TrainResult.Availability),
            HeaderText = "Availability",
            FillWeight = 180
        });
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        await RunSearchAsync();
    }

    private async Task RunSearchAsync()
    {
        UseWaitCursor = true;
        trainGrid.DataSource = null;

        var progress = new Progress<string>(message =>
        {
            if (IsHandleCreated)
            {
                statusLabel.Text = message;
            }
        });

        try
        {
            _scraper ??= new EtrainScraperService();
            var results = await _scraper.SearchTrainsAsync(_settings, progress);

            trainGrid.DataSource = results;
            statusLabel.Text =
                $"Showing {results.Count} train(s): {_settings.FromStation} → {_settings.ToStation} on {_settings.TravelDate:dd-MMM-yyyy}";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Search failed: {ex.Message}";
            MessageBox.Show(
                this,
                ex.Message,
                "Train Search Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    protected override async void OnFormClosed(FormClosedEventArgs e)
    {
        if (_scraper is not null)
        {
            await _scraper.DisposeAsync();
            _scraper = null;
        }

        base.OnFormClosed(e);
    }
}
