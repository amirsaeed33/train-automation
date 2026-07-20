using System.ComponentModel;

namespace train_automation;

public partial class Form1 : Form
{
    private EtrainScraperService? _scraper;
    private readonly IReadOnlyList<StationInfo> _stations = HardcodedStations.All;
    private BookingConfiguration _config = new();
    private BindingList<Passenger> _passengersList = new();

    public Form1()
    {
        InitializeComponent();
        ConfigureGrid();
        ConfigurePassengerGrid();
        travelDatePicker.Value = DateTime.Today.AddDays(1);
    }

    private void ConfigureGrid()
    {
        trainGrid.AutoGenerateColumns = false;
        trainGrid.Columns.Clear();

        AddColumn(trainGrid, nameof(TrainResult.TrainNumber), "Train Number", 75);
        AddColumn(trainGrid, nameof(TrainResult.TrainName), "Train Name", 150);
        AddColumn(trainGrid, nameof(TrainResult.FromStation), "From", 55);
        AddColumn(trainGrid, nameof(TrainResult.Departure), "Depart. Time", 75);
        AddColumn(trainGrid, nameof(TrainResult.ToStation), "To", 55);
        AddColumn(trainGrid, nameof(TrainResult.Arrival), "Arrival Time", 75);
        AddColumn(trainGrid, nameof(TrainResult.TravelTime), "Travel Time", 75);
        AddColumn(trainGrid, nameof(TrainResult.Sunday), "Su", 35);
        AddColumn(trainGrid, nameof(TrainResult.Monday), "Mo", 35);
        AddColumn(trainGrid, nameof(TrainResult.Tuesday), "Tu", 35);
        AddColumn(trainGrid, nameof(TrainResult.Wednesday), "We", 35);
        AddColumn(trainGrid, nameof(TrainResult.Thursday), "Th", 35);
        AddColumn(trainGrid, nameof(TrainResult.Friday), "Fr", 35);
        AddColumn(trainGrid, nameof(TrainResult.Saturday), "Sa", 35);
        AddColumn(trainGrid, nameof(TrainResult.AvailableClasses), "Available Classes", 120);
    }

    private void ConfigurePassengerGrid()
    {
        passengerGrid.AutoGenerateColumns = false;
        passengerGrid.Columns.Clear();
        
        AddColumn(passengerGrid, nameof(Passenger.Name), "Full Name", 150);
        AddColumn(passengerGrid, nameof(Passenger.Age), "Age", 50);
        
        var genderCol = new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(Passenger.Gender),
            HeaderText = "Gender",
            FillWeight = 80,
            DataSource = new[] { "Male", "Female", "Transgender" }
        };
        passengerGrid.Columns.Add(genderCol);
        
        var berthCol = new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(Passenger.BerthPreference),
            HeaderText = "Berth Preference",
            FillWeight = 100,
            DataSource = new[] { "No Preference", "Lower", "Middle", "Upper", "Side Lower", "Side Upper", "Window" }
        };
        passengerGrid.Columns.Add(berthCol);
    }

    private void AddColumn(DataGridView grid, string propertyName, string headerText, int fillWeight)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = headerText,
            FillWeight = fillWeight
        });
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        LoadStations();
        
        // Load Configuration
        _config = BookingConfiguration.Load();
        
        // Bind UI
        usernameText.Text = _config.Credentials.Username;
        passwordText.Text = _config.Credentials.Password;
        
        _passengersList = new BindingList<Passenger>(_config.Passengers);
        passengerGrid.DataSource = _passengersList;
    }

    private void LoadStations()
    {
        PopulateStationCombo(fromStationCombo, _stations);
        PopulateStationCombo(toStationCombo, _stations);

        SelectDefaultStation(fromStationCombo, "NDLS", "NEW DELHI");
        SelectDefaultStation(toStationCombo, "PNBE", "PATNA JN");

        statusLabel.Text = "Ready. Configure passengers and settings, then search.";
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

    private void SaveConfig()
    {
        _config.Credentials.Username = usernameText.Text;
        _config.Credentials.Password = passwordText.Text;
        _config.Passengers = _passengersList.ToList();
        _config.Save();
        statusLabel.Text = "Settings saved.";
    }

    private void SaveSettingsButton_Click(object sender, EventArgs e)
    {
        SaveConfig();
    }
    
    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveConfig();
    }

    private async void SearchButton_Click(object sender, EventArgs e)
    {
        SaveConfig(); // Save configuration implicitly on search

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

    private IrctcBookingService? _irctcService;
    private CancellationTokenSource? _cts;

    private async void BookIrctcButton_Click(object sender, EventArgs e)
    {
        SaveConfig();

        if (trainGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "Please search for and select a train first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedTrain = trainGrid.SelectedRows[0].DataBoundItem as TrainResult;
        if (selectedTrain == null) return;

        if (string.IsNullOrWhiteSpace(_config.Credentials.Username) || string.IsNullOrWhiteSpace(_config.Credentials.Password))
        {
            MessageBox.Show(this, "Please configure IRCTC username and password in the Settings tab.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_config.Passengers.Count == 0)
        {
            MessageBox.Show(this, "Please add at least one passenger in the Passengers tab.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var settings = new TrainSearchSettings
        {
            FromStationCode = GetSelectedStation(fromStationCombo)?.Code ?? "",
            FromStationName = GetSelectedStation(fromStationCombo)?.Name ?? "",
            ToStationCode = GetSelectedStation(toStationCombo)?.Code ?? "",
            ToStationName = GetSelectedStation(toStationCombo)?.Name ?? "",
            TravelDate = travelDatePicker.Value.Date
        };

        // Lock UI and activate Stop button
        bookIrctcButton.Enabled = false;
        searchButton.Enabled = false;
        stopButton.Enabled = true;
        UseWaitCursor = true;

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var progress = new Progress<string>(message =>
        {
            if (IsHandleCreated) statusLabel.Text = message;
        });

        try
        {
            // Dispose old service so browser is fresh each run
            if (_irctcService is not null)
            {
                await _irctcService.DisposeAsync();
                _irctcService = null;
            }
            _irctcService = new IrctcBookingService();
            await _irctcService.BookTrainAsync(settings, selectedTrain, _config, progress, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Automation stopped by user.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Automation stopped: {ex.Message}";
        }
        finally
        {
            // ALWAYS re-enable UI no matter what happened
            bookIrctcButton.Enabled = true;
            searchButton.Enabled = true;
            stopButton.Enabled = false;
            UseWaitCursor = false;
            Cursor = Cursors.Default;
        }
    }

    private void StopButton_Click(object sender, EventArgs e)
    {
        _cts?.Cancel();
        statusLabel.Text = "Stopping automation...";
        stopButton.Enabled = false;
    }

    protected override async void OnFormClosed(FormClosedEventArgs e)
    {
        if (_scraper is not null)
        {
            await _scraper.DisposeAsync();
            _scraper = null;
        }
        
        if (_irctcService is not null)
        {
            await _irctcService.DisposeAsync();
            _irctcService = null;
        }

        base.OnFormClosed(e);
    }
}
