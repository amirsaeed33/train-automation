namespace train_automation;

public partial class Form1 : Form
{
    private EtrainScraperService? _scraper;
    private readonly IReadOnlyList<StationInfo> _stations = HardcodedStations.All;

    public Form1()
    {
        InitializeComponent();
        ConfigureGrid();
        travelDatePicker.Value = DateTime.Today.AddDays(1);
    }

    private void ConfigureGrid()
    {
        trainGrid.AutoGenerateColumns = false;
        trainGrid.Columns.Clear();

        AddColumn(nameof(TrainResult.TrainNumber), "Train Number", 75);
        AddColumn(nameof(TrainResult.TrainName), "Train Name", 150);
        AddColumn(nameof(TrainResult.FromStation), "From", 55);
        AddColumn(nameof(TrainResult.Departure), "Depart. Time", 75);
        AddColumn(nameof(TrainResult.ToStation), "To", 55);
        AddColumn(nameof(TrainResult.Arrival), "Arrival Time", 75);
        AddColumn(nameof(TrainResult.TravelTime), "Travel Time", 75);
        AddColumn(nameof(TrainResult.Sunday), "Su", 35);
        AddColumn(nameof(TrainResult.Monday), "Mo", 35);
        AddColumn(nameof(TrainResult.Tuesday), "Tu", 35);
        AddColumn(nameof(TrainResult.Wednesday), "We", 35);
        AddColumn(nameof(TrainResult.Thursday), "Th", 35);
        AddColumn(nameof(TrainResult.Friday), "Fr", 35);
        AddColumn(nameof(TrainResult.Saturday), "Sa", 35);
        AddColumn(nameof(TrainResult.AvailableClasses), "Available Classes", 120);
    }

    private void AddColumn(string propertyName, string headerText, int fillWeight)
    {
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = headerText,
            FillWeight = fillWeight
        });
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        LoadStations();
    }

    private void LoadStations()
    {
        PopulateStationCombo(fromStationCombo, _stations);
        PopulateStationCombo(toStationCombo, _stations);

        SelectDefaultStation(fromStationCombo, "NDLS", "NEW DELHI");
        SelectDefaultStation(toStationCombo, "DLI", "DELHI");

        statusLabel.Text = "Select From, To, and Date, then click Search.";
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
