namespace train_automation;

public partial class Form1 : Form
{
    private EtrainScraperService? _scraper;
    private readonly IReadOnlyList<StationInfo> _stations = HardcodedStations.All;
    private TrainSearchSettings? _lastSearchSettings;
    private TrainSelection? _selectedTrain;

    public Form1()
    {
        InitializeComponent();
        travelDatePicker.Value = DateTime.Today.AddDays(1);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        LoadStations();
        ConfigurePassengerGrid();
        ConfigureDropdowns();
    }

    private void LoadStations()
    {
        PopulateStationCombo(fromStationCombo, _stations);
        PopulateStationCombo(toStationCombo, _stations);
        SelectDefaultStation(fromStationCombo, "NDLS", "NEW DELHI");
        SelectDefaultStation(toStationCombo, "DLI", "DELHI");
        UpdateTicketName();
    }

    private void ConfigureDropdowns()
    {
        trainTypeCombo.Items.AddRange(["Mail/Express-E", "Passenger", "EMU", "Superfast", "Rajdhani"]);
        trainTypeCombo.SelectedIndex = 0;

        ticketSlotCombo.Items.AddRange(["Select_Auto Slot", "Slot-1", "Slot-2"]);
        ticketSlotCombo.SelectedIndex = 0;

        gatewayCombo.Items.AddRange(["Netbanking / Wallet", "BHIM/UPI", "Credit/Debit Card"]);
        gatewayCombo.SelectedIndex = 0;

        priorBankCombo.Items.AddRange(["PayTM-QR_paytm@qr", "PhonePe", "HDFC Netbanking"]);
        priorBankCombo.SelectedIndex = 0;

        backupBankCombo.Items.AddRange(["No Alternate Bank", "PayTM", "PhonePe"]);
        backupBankCombo.SelectedIndex = 0;
    }

    private void ConfigurePassengerGrid()
    {
        passengerGrid.AutoGenerateColumns = false;
        passengerGrid.AllowUserToAddRows = false;
        passengerGrid.AllowUserToDeleteRows = false;
        passengerGrid.RowHeadersVisible = false;
        passengerGrid.Columns.Clear();

        passengerGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sno", Name = "Sno", Width = 40, ReadOnly = true });
        passengerGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", Name = "Name", Width = 120 });
        passengerGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Age", Name = "Age", Width = 45 });

        var sexColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = "Sex",
            Name = "Sex",
            Width = 50,
            FlatStyle = FlatStyle.Flat
        };
        sexColumn.Items.AddRange("M", "F", "T");
        passengerGrid.Columns.Add(sexColumn);

        var berthColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = "Berth",
            Name = "Berth",
            Width = 90,
            FlatStyle = FlatStyle.Flat
        };
        berthColumn.Items.AddRange("No Choice", "Lower", "Middle", "Upper", "Side Lower", "Side Upper");
        passengerGrid.Columns.Add(berthColumn);

        var foodColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = "Food",
            Name = "Food",
            Width = 80,
            FlatStyle = FlatStyle.Flat
        };
        foodColumn.Items.AddRange("No Choice", "Veg", "Non-Veg");
        passengerGrid.Columns.Add(foodColumn);

        var nationalityColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = "Nationality",
            Name = "Nationality",
            Width = 90,
            FlatStyle = FlatStyle.Flat
        };
        nationalityColumn.Items.Add("India-IN");
        passengerGrid.Columns.Add(nationalityColumn);

        passengerGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Passport", Name = "Passport", Width = 80 });
        passengerGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Child", Name = "Child", Width = 50 });
        passengerGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Senior", Name = "Senior", Width = 55 });
        passengerGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Bed", Name = "Bed", Width = 45 });

        for (var index = 1; index <= 6; index++)
        {
            passengerGrid.Rows.Add(index, string.Empty, string.Empty, "M", "No Choice", "No Choice", "India-IN", string.Empty, false, false, false);
        }
    }

    private static void PopulateStationCombo(ComboBox comboBox, IReadOnlyList<StationInfo> stations)
    {
        comboBox.BeginUpdate();
        comboBox.DataSource = null;
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

    private async void FindButton_Click(object sender, EventArgs e)
    {
        var fromStation = GetSelectedStation(fromStationCombo);
        var toStation = GetSelectedStation(toStationCombo);

        if (fromStation is null || toStation is null)
        {
            MessageBox.Show(this, "Please select valid From and To stations.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (fromStation.Code.Equals(toStation.Code, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, "From and To stations must be different.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var settings = new TrainSearchSettings
        {
            FromStationCode = fromStation.Code,
            FromStationName = fromStation.Name,
            ToStationCode = toStation.Code,
            ToStationName = toStation.Name,
            TravelDate = travelDatePicker.Value.Date
        };

        UseWaitCursor = true;
        findButton.Enabled = false;
        statusLabel.Text = "Searching trains...";

        try
        {
            _scraper ??= new EtrainScraperService();
            var results = await _scraper.SearchTrainsAsync(settings);
            _lastSearchSettings = settings;

            if (results.Count == 0)
            {
                MessageBox.Show(this, "No trains found for selected route.", "Train List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                statusLabel.Text = "No trains found.";
                return;
            }

            using var trainListDialog = new TrainListDialog(results, $"{fromStation.Code} -> {toStation.Code}");
            if (trainListDialog.ShowDialog(this) == DialogResult.OK && trainListDialog.SelectedTrain is not null)
            {
                ApplyTrainSelection(trainListDialog.SelectedTrain, fromStation.Code, toStation.Code);
                statusLabel.Text = $"Selected train {trainListDialog.SelectedTrain.Train.TrainNumber} for {trainListDialog.SelectedTrain.SelectedDay}.";
            }
            else
            {
                statusLabel.Text = "Train selection cancelled.";
            }
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Search failed.";
            MessageBox.Show(this, ex.Message, "Search Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            findButton.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private void ApplyTrainSelection(TrainSelection selection, string fromCode, string toCode)
    {
        _selectedTrain = selection;
        var train = selection.Train;

        boardingPointText.Text = fromCode;
        trainNoText.Text = train.TrainNumber;
        trainTypeCombo.SelectedIndex = 0;

        classCombo.Items.Clear();
        var classes = train.AvailableClasses
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();

        if (classes.Count == 0)
        {
            classes.Add("UNRESERVED");
        }

        classCombo.Items.AddRange(classes.ToArray());
        classCombo.SelectedIndex = 0;
        UpdateTicketName();
    }

    private void UpdateTicketName()
    {
        var fromCode = GetSelectedStation(fromStationCombo)?.Code ?? fromStationCombo.Text.Trim();
        var toCode = GetSelectedStation(toStationCombo)?.Code ?? toStationCombo.Text.Trim();
        if (!string.IsNullOrWhiteSpace(fromCode) && !string.IsNullOrWhiteSpace(toCode))
        {
            ticketNameText.Text = $"{fromCode}_{toCode}";
        }
    }

    private void AvailabilityLink_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (_selectedTrain is null)
        {
            MessageBox.Show(this, "Please find and select a train first.", "Availability", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        MessageBox.Show(
            this,
            $"Train: {_selectedTrain.Train.TrainNumber}\nClasses: {_selectedTrain.Train.AvailableClasses}\nDay: {_selectedTrain.SelectedDay}",
            "Availability",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void GetFareButton_Click(object? sender, EventArgs e)
    {
        var passengerCount = GetPassengers().Count;
        if (passengerCount == 0 || _selectedTrain is null)
        {
            MessageBox.Show(this, "Select a train and enter at least one passenger.", "Get Fare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        fareText.Text = (passengerCount * 170).ToString();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_selectedTrain is null || _lastSearchSettings is null)
        {
            MessageBox.Show(this, "Please find and select a train first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var passengers = GetPassengers();
        if (passengers.Count == 0)
        {
            MessageBox.Show(this, "Enter at least one passenger in the grid.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var mobile = mobileText.Text.Trim();
        if (mobile.Length != 10 || !mobile.All(char.IsDigit))
        {
            MessageBox.Show(this, "Enter a valid 10-digit mobile number.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var booking = new TrainBookingRecord
        {
            TrainNumber = trainNoText.Text.Trim(),
            TrainName = _selectedTrain.Train.TrainName,
            TrainType = trainTypeCombo.SelectedItem?.ToString() ?? string.Empty,
            FromStation = _lastSearchSettings.FromStationCode,
            ToStation = _lastSearchSettings.ToStationCode,
            BoardingPoint = boardingPointText.Text.Trim(),
            TravelClass = classCombo.SelectedItem?.ToString() ?? string.Empty,
            Quota = GetSelectedQuota(),
            SelectedDay = _selectedTrain.SelectedDay,
            TravelDate = _lastSearchSettings.TravelDate,
            Mobile = mobile,
            Fare = fareText.Text.Trim(),
            Passengers = passengers,
            Preferences = new BookingPreferences
            {
                AutoUpgradation = autoUpgradeCheck.Checked,
                BookOnlyIfConfirmBerths = confirmBerthsCheck.Checked,
                TicketSlot = ticketSlotCombo.SelectedItem?.ToString() ?? string.Empty,
                Gateway = gatewayCombo.SelectedItem?.ToString() ?? string.Empty,
                PriorBank = priorBankCombo.SelectedItem?.ToString() ?? string.Empty,
                BackupBank = backupBankCombo.SelectedItem?.ToString() ?? string.Empty,
                TicketName = ticketNameText.Text.Trim()
            }
        };

        try
        {
            BookingJsonStore.Append(booking);
            MessageBox.Show(this, $"Ticket saved.\n{BookingJsonStore.FilePath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            statusLabel.Text = "Ticket saved to JSON.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string GetSelectedQuota()
    {
        if (quotaLadiesRadio.Checked) return "Ladies";
        if (quotaTatkalRadio.Checked) return "Tatkal";
        if (quotaPremiumRadio.Checked) return "Premium Tatkal";
        return "General";
    }

    private List<PassengerInfo> GetPassengers()
    {
        var passengers = new List<PassengerInfo>();
        foreach (DataGridViewRow row in passengerGrid.Rows)
        {
            var name = row.Cells["Name"].Value?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            _ = int.TryParse(row.Cells["Age"].Value?.ToString(), out var age);
            passengers.Add(new PassengerInfo
            {
                Name = name,
                Age = age,
                Gender = row.Cells["Sex"].Value?.ToString() ?? "M",
                Berth = row.Cells["Berth"].Value?.ToString() ?? "No Choice",
                Food = row.Cells["Food"].Value?.ToString() ?? "No Choice",
                Nationality = row.Cells["Nationality"].Value?.ToString() ?? "India-IN",
                Passport = row.Cells["Passport"].Value?.ToString() ?? string.Empty,
                IsChild = row.Cells["Child"].Value is true,
                IsSenior = row.Cells["Senior"].Value is true,
                BedRoll = row.Cells["Bed"].Value is true
            });
        }

        return passengers;
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
