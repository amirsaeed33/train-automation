using System.ComponentModel;

namespace train_automation;

public partial class Form1 : Form
{
    private EtrainScraperService? _scraper;
    private readonly IReadOnlyList<StationInfo> _stations = HardcodedStations.All;
    private BookingConfiguration _config = new();
    private BindingList<Passenger> _passengersList = new();
    private IrctcBookingService? _irctcService;
    private CancellationTokenSource? _cts;

    public Form1()
    {
        InitializeComponent();
        ConfigureGrid();
        ConfigurePassengerGrid();
        PopulateBookingOptionCombos();
        travelDatePicker.Value = DateTime.Today.AddDays(1);
    }

    private void PopulateBookingOptionCombos()
    {
        quotaCombo.DisplayMember = "Label";
        quotaCombo.ValueMember = "Code";
        quotaCombo.DataSource = IrctcQuotaLabels.Options
            .Select(o => new QuotaItem(o.Code, o.Label))
            .ToList();

        classCombo.DisplayMember = "Label";
        classCombo.ValueMember = "Code";
        classCombo.DataSource = IrctcClassOptions.Options
            .Select(o => new ClassItem(o.Code, o.Label))
            .ToList();

        paymentCombo.Items.Clear();
        foreach (var option in IrctcPaymentOptions.Options)
        {
            paymentCombo.Items.Add(option);
        }
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

        passengerGrid.Columns.Add(new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(Passenger.Gender),
            HeaderText = "Gender",
            FillWeight = 80,
            DataSource = new[] { "Male", "Female", "Transgender" }
        });

        passengerGrid.Columns.Add(new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(Passenger.BerthPreference),
            HeaderText = "Berth Preference",
            FillWeight = 100,
            DataSource = new[] { "No Preference", "Lower", "Middle", "Upper", "Side Lower", "Side Upper", "Window" }
        });

        passengerGrid.Columns.Add(new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(Passenger.FoodPreference),
            HeaderText = "Food",
            FillWeight = 90,
            DataSource = new[] { "No Preference", "Veg", "Non Veg" }
        });
    }

    private static void AddColumn(DataGridView grid, string propertyName, string headerText, int fillWeight)
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

        _config = BookingConfiguration.Load();
        ApplyConfigToUi();

        statusLabel.Text = "Ready. Set class/quota, passengers & settings, then search and book.";
    }

    private void ApplyConfigToUi()
    {
        usernameText.Text = _config.Credentials.Username;
        passwordText.Text = _config.Credentials.Password;
        mobileText.Text = _config.MobileNumber;
        scheduleTimeText.Text = _config.ScheduledSearchTime;
        confirmBerthsCheck.Checked = _config.ConfirmBerthsOnly;
        autoUpgradeCheck.Checked = _config.AutoUpgrade;

        refreshIntervalNumeric.Value = Clamp(
            _config.RefreshIntervalMs,
            (int)refreshIntervalNumeric.Minimum,
            (int)refreshIntervalNumeric.Maximum);

        availabilityTimeoutNumeric.Value = Clamp(
            _config.AvailabilityTimeoutSeconds,
            (int)availabilityTimeoutNumeric.Minimum,
            (int)availabilityTimeoutNumeric.Maximum);

        SelectComboByCode(quotaCombo, _config.Quota);
        SelectComboByCode(classCombo, _config.PreferredClass);

        var paymentIndex = paymentCombo.Items.IndexOf(_config.PaymentMethod);
        paymentCombo.SelectedIndex = paymentIndex >= 0 ? paymentIndex : 0;

        _passengersList = new BindingList<Passenger>(_config.Passengers);
        passengerGrid.DataSource = _passengersList;
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;

    private static void SelectComboByCode(ComboBox combo, string code)
    {
        for (var i = 0; i < combo.Items.Count; i++)
        {
            var item = combo.Items[i];
            var itemCode = item switch
            {
                QuotaItem q => q.Code,
                ClassItem c => c.Code,
                _ => item?.ToString()
            };

            if (string.Equals(itemCode, code, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedIndex = i;
                return;
            }
        }

        if (combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
    }

    private void LoadStations()
    {
        PopulateStationCombo(fromStationCombo, _stations);
        PopulateStationCombo(toStationCombo, _stations);

        SelectDefaultStation(fromStationCombo, "NDLS", "NEW DELHI");
        SelectDefaultStation(toStationCombo, "PNBE", "PATNA");
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
        passengerGrid.EndEdit();

        _config.Credentials.Username = usernameText.Text.Trim();
        _config.Credentials.Password = passwordText.Text;
        _config.Passengers = _passengersList.ToList();
        _config.MobileNumber = mobileText.Text.Trim();
        _config.ConfirmBerthsOnly = confirmBerthsCheck.Checked;
        _config.AutoUpgrade = autoUpgradeCheck.Checked;
        _config.RefreshIntervalMs = (int)refreshIntervalNumeric.Value;
        _config.AvailabilityTimeoutSeconds = (int)availabilityTimeoutNumeric.Value;
        _config.ScheduledSearchTime = scheduleTimeText.Text.Trim();
        _config.Quota = GetSelectedQuotaCode();
        _config.PreferredClass = GetSelectedClassCode();
        _config.PaymentMethod = paymentCombo.SelectedItem?.ToString() ?? "BHIM/UPI";
        _config.Save();
        statusLabel.Text = "Settings saved.";
    }

    private string GetSelectedQuotaCode() =>
        quotaCombo.SelectedItem is QuotaItem item ? item.Code : "GN";

    private string GetSelectedClassCode() =>
        classCombo.SelectedItem is ClassItem item ? item.Code : "SL";

    private void SaveSettingsButton_Click(object sender, EventArgs e) => SaveConfig();

    private void Form1_FormClosing(object sender, FormClosingEventArgs e) => SaveConfig();

    private async void SearchButton_Click(object sender, EventArgs e)
    {
        SaveConfig();

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

        await RunSearchAsync(BuildSearchSettings(fromStation, toStation));
    }

    private TrainSearchSettings BuildSearchSettings(StationInfo fromStation, StationInfo toStation) => new()
    {
        FromStationCode = fromStation.Code,
        FromStationName = fromStation.Name,
        ToStationCode = toStation.Code,
        ToStationName = toStation.Name,
        TravelDate = travelDatePicker.Value.Date,
        Quota = GetSelectedQuotaCode(),
        PreferredClass = GetSelectedClassCode()
    };

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
                $"Showing {results.Count} train(s): {settings.FromStationName} → {settings.ToStationName} on {settings.TravelDate:dd-MMM-yyyy} [{settings.Quota}/{settings.PreferredClass}]";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Search failed: {ex.Message}";
            MessageBox.Show(this, ex.Message, "Train Search Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private async void BookIrctcButton_Click(object sender, EventArgs e)
    {
        SaveConfig();

        if (trainGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, "Please search for and select a train first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedTrain = trainGrid.SelectedRows[0].DataBoundItem as TrainResult;
        if (selectedTrain is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_config.Credentials.Username) ||
            string.IsNullOrWhiteSpace(_config.Credentials.Password))
        {
            MessageBox.Show(this, "Please configure IRCTC username and password in the Settings tab.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_config.Passengers.Count == 0 || _config.Passengers.All(p => string.IsNullOrWhiteSpace(p.Name)))
        {
            MessageBox.Show(this, "Please add at least one passenger in the Passengers tab.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var fromStation = GetSelectedStation(fromStationCombo);
        var toStation = GetSelectedStation(toStationCombo);
        if (fromStation is null || toStation is null)
        {
            MessageBox.Show(this, "Please select valid From and To stations.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var settings = BuildSearchSettings(fromStation, toStation);

        bookIrctcButton.Enabled = false;
        searchButton.Enabled = false;
        stopButton.Enabled = true;
        UseWaitCursor = true;

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var progress = new Progress<string>(message =>
        {
            if (IsHandleCreated)
            {
                statusLabel.Text = message;
            }
        });

        try
        {
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

    private sealed record QuotaItem(string Code, string Label);
    private sealed record ClassItem(string Code, string Label);
}
