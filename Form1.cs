namespace train_automation;

public partial class Form1 : Form
{
    private readonly EtrainStationService _stationService = new();
    private EtrainScraperService? _scraper;
    private List<StationInfo> _stations = [];

    public Form1()
    {
        InitializeComponent();
        ConfigureGrid();
        travelDatePicker.Value = DateTime.Today.AddDays(1);
        searchButton.Enabled = false;
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
        await LoadStationsAsync();
    }

    private async Task LoadStationsAsync()
    {
        UseWaitCursor = true;
        searchButton.Enabled = false;

        var progress = new Progress<string>(message =>
        {
            if (IsHandleCreated)
            {
                statusLabel.Text = message;
            }
        });

        try
        {
            _stations = (await _stationService.GetStationsAsync(progress)).ToList();
            PopulateStationCombo(fromStationCombo, _stations);
            PopulateStationCombo(toStationCombo, _stations);

            SelectDefaultStation(fromStationCombo, "NDLS", "New Delhi");
            SelectDefaultStation(toStationCombo, "CSTM", "Mumbai");

            statusLabel.Text = "Select From, To, and Date, then click Search.";
            searchButton.Enabled = true;
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Failed to load stations: {ex.Message}";
            MessageBox.Show(
                this,
                ex.Message,
                "Station Load Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private static void PopulateStationCombo(ComboBox comboBox, IReadOnlyList<StationInfo> stations)
    {
        comboBox.BeginUpdate();
        comboBox.DataSource = null;
        comboBox.DisplayMember = string.Empty;
        comboBox.ValueMember = string.Empty;
        comboBox.Items.Clear();
        foreach (var station in stations)
        {
            comboBox.Items.Add(station);
        }
        comboBox.EndUpdate();
    }

    private static void SelectDefaultStation(ComboBox comboBox, string code, string nameContains)
    {
        var match = comboBox.Items.Cast<StationInfo>()
            .FirstOrDefault(station => station.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            ?? comboBox.Items.Cast<StationInfo>()
                .FirstOrDefault(station => station.Name.Contains(nameContains, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
        {
            comboBox.SelectedItem = match;
        }
    }

    private async void SearchButton_Click(object sender, EventArgs e)
    {
        var fromStation = GetSelectedStation(fromStationCombo);
        if (fromStation is null)
        {
            MessageBox.Show(this, "Please select a valid From station.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var toStation = GetSelectedStation(toStationCombo);
        if (toStation is null)
        {
            MessageBox.Show(this, "Please select a valid To station.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (fromStation.Code.Equals(toStation.Code, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "From and To stations must be different.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await RunSearchAsync(new TrainSearchSettings
        {
            FromStationCode = fromStation.Code,
            FromStationName = fromStation.Name,
            ToStationCode = toStation.Code,
            ToStationName = toStation.Name,
            TravelDate = travelDatePicker.Value.Date
        });
    }

    private async Task RunSearchAsync(TrainSearchSettings settings)
    {
        UseWaitCursor = true;
        searchButton.Enabled = false;
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
            var results = await _scraper.SearchTrainsAsync(settings, progress);

            trainGrid.DataSource = results;
            statusLabel.Text =
                $"Showing {results.Count} train(s): {settings.FromStationName} → {settings.ToStationName} on {settings.TravelDate:dd-MMM-yyyy}";
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
            searchButton.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private StationInfo? GetSelectedStation(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is StationInfo selected)
        {
            return selected;
        }

        var text = comboBox.Text.Trim();
        if (text.Length == 0)
        {
            return null;
        }

        return _stations.FirstOrDefault(station =>
                station.DisplayText.Equals(text, StringComparison.OrdinalIgnoreCase))
            ?? _stations.FirstOrDefault(station =>
                station.Code.Equals(text, StringComparison.OrdinalIgnoreCase))
            ?? _stations.FirstOrDefault(station =>
                station.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
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
