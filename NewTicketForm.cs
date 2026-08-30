namespace train_automation;

public partial class NewTicketForm : Form
{
    private IrctcBookingService? _irctcService;
    private readonly BookingConfiguration _config = BookingConfiguration.Load();
    private CancellationTokenSource? _cts;
    private readonly IReadOnlyList<StationInfo> _stations = HardcodedStations.All;
    private TrainSearchSettings? _lastSearchSettings;
    private TrainSelection? _selectedTrain;
    private TrainListDialog? _trainListDialog;
    private Panel passengerCustomGridPanel = null!;
    private readonly List<PassengerRowControls> _passengerRows = new();
    
    public class PassengerRowControls
    {
        public Label SNo { get; set; } = null!;
        public TextBox Name { get; set; } = null!;
        public TextBox Age { get; set; } = null!;
        public ComboBox Sex { get; set; } = null!;
        public ComboBox Berth { get; set; } = null!;
        public ComboBox Food { get; set; } = null!;
        public ComboBox Nationality { get; set; } = null!;
        public TextBox Passport { get; set; } = null!;
        public CheckBox Child { get; set; } = null!;
        public CheckBox Senior { get; set; } = null!;
        public CheckBox Bed { get; set; } = null!;
    }

    public NewTicketForm()
    {
        InitializeComponent();
        travelDatePicker.Value = DateTime.Today.AddDays(1);
    }

    /// <summary>Hide the standalone title bar when this form is hosted inside MainShellForm.</summary>
    public void PrepareForShellEmbed()
    {
        if (titlePanel is not null)
        {
            titlePanel.Visible = false;
            titlePanel.Height = 0;
        }

        if (contentPanel is not null)
        {
            contentPanel.Dock = DockStyle.Fill;
        }
    }

    private void NewTicketForm_Load(object sender, EventArgs e)
    {
        this.Font = new Font("Segoe UI", 7.5F); // compact base font
        FlattenLayout();
        LoadStations();
        ConfigureDropdowns();
        ApplyDarkThemeToInputs();
        LoadBookingConfigIntoUi();
    }

    private void ApplyDarkThemeToInputs()
    {
        var inputs = new Control[] { 
            fromStationCombo, toStationCombo, boardingPointText,
            trainNoText, trainTypeCombo, classCombo,
            mobileText, ticketSlotCombo, gatewayCombo, priorBankCombo, backupBankCombo,
            ticketNameText, irctcUserCombo 
        };

        foreach (var c in inputs)
        {
            c.Font = new Font("Segoe UI", 7.5F);
            c.BackColor = UiTheme.Surface;
            c.ForeColor = UiTheme.Text;
        }
        
        travelDatePicker.Font = new Font("Segoe UI", 7.5F);
        travelDatePicker.CalendarMonthBackground = UiTheme.Surface;

        // Increase all label fonts in content panel back to readable sizes
        foreach (Control c in contentPanel.Controls)
        {
            if (c is Label lbl && c != findButton && c != getFareButton && c != availabilityLink)
            {
                lbl.Font = new Font("Segoe UI", 7.5F);
            }
        }
    }

    private void LoadBookingConfigIntoUi()
    {
        irctcUserCombo.Items.Clear();
        if (!string.IsNullOrWhiteSpace(_config.Credentials.Username))
        {
            irctcUserCombo.Items.Add(_config.Credentials.Username);
            irctcUserCombo.SelectedIndex = 0;
        }
        else
        {
            irctcUserCombo.Text = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(_config.MobileNumber))
        {
            mobileText.Text = _config.MobileNumber;
        }

        confirmBerthsCheck.Checked = _config.ConfirmBerthsOnly;
        useBetaViewCheck.Checked = _config.UseBetaView;
        useRealChromeCheck.Checked = _config.UseRealChrome;

        if (!string.IsNullOrWhiteSpace(_config.PaymentMethod))
        {
            for (var i = 0; i < gatewayCombo.Items.Count; i++)
            {
                var text = gatewayCombo.Items[i]?.ToString() ?? string.Empty;
                if (text.Contains("BHIM", StringComparison.OrdinalIgnoreCase)
                    || text.Equals(_config.PaymentMethod, StringComparison.OrdinalIgnoreCase))
                {
                    gatewayCombo.SelectedIndex = i;
                    break;
                }
            }
        }

        ApplySavedPassengersToGrid();
        passengerGrid.CellEndEdit += (_, _) => SaveIrctcConfigFromUi();
        passengerGrid.CurrentCellDirtyStateChanged += PassengerGrid_CurrentCellDirtyStateChanged;
        mobileText.Leave += (_, _) => SaveIrctcConfigFromUi();
        irctcUserCombo.Leave += (_, _) => SaveIrctcConfigFromUi();
    }

    private void PassengerGrid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (passengerGrid.IsCurrentCellDirty)
        {
            passengerGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            SaveIrctcConfigFromUi();
        }
    }

    private void ApplySavedPassengersToGrid()
    {
        if (_config.Passengers.Count == 0 || _passengerRows.Count == 0)
        {
            return;
        }

        var limit = GetPassengerRowLimit();
        UpdatePassengerRowCount(limit);

        for (var i = 0; i < _passengerRows.Count && i < _config.Passengers.Count; i++)
        {
            var p = _config.Passengers[i];
            var row = _passengerRows[i];
            
            row.Name.Text = p.Name;
            row.Age.Text = p.Age ?? string.Empty;
            
            string sexStr = p.Gender switch
            {
                "Female" => "F",
                "Transgender" => "T",
                _ => "M"
            };
            if (row.Sex.Items.Contains(sexStr)) row.Sex.SelectedItem = sexStr;
            
            string berthStr = MapBerthToGrid(p.BerthPreference);
            if (row.Berth.Items.Contains(berthStr)) row.Berth.SelectedItem = berthStr;
            
            string foodStr = MapFoodToGrid(p.FoodPreference);
            if (row.Food.Items.Contains(foodStr)) row.Food.SelectedItem = foodStr;
            
            row.Nationality.SelectedItem = "India-IN";
        }
    }

    private static string MapBerthToGrid(string berth) => berth switch
    {
        "Lower" => "Lower",
        "Middle" => "Middle",
        "Upper" => "Upper",
        "Side Lower" => "Side Lower",
        "Side Upper" => "Side Upper",
        _ => "No Choice"
    };

    private static string MapFoodToGrid(string food) => food switch
    {
        "Veg" => "Veg",
        "Non Veg" or "Non-Veg" => "Non-Veg",
        _ => "No Choice"
    };

    private void LoadStations()
    {
        PopulateStationCombo(fromStationCombo, _stations);
        PopulateStationCombo(toStationCombo, _stations);
        SelectDefaultStation(fromStationCombo, "NDLS", "NEW DELHI");
        SelectDefaultStation(toStationCombo, "PNBE", "PATNA");
        UpdateTicketName();
    }

    private void ConfigureDropdowns()
    {
        getFareButton.Visible = quotaTatkalRadio.Checked || quotaPremiumRadio.Checked;

        trainTypeCombo.Items.AddRange(["Mail/Express-E", "Passenger", "EMU", "Superfast", "Rajdhani"]);
        trainTypeCombo.SelectedIndexChanged += TrainTypeCombo_SelectedIndexChanged;
        trainTypeCombo.SelectedIndex = 0;

        ticketSlotCombo.Items.AddRange(["Select_Auto Slot", "Slot-1", "Slot-2"]);
        ticketSlotCombo.SelectedIndex = 0;

        gatewayCombo.Items.AddRange(["BHIM/UPI", "Netbanking / Wallet", "Credit/Debit Card", "IRCTC eWallet"]);
        gatewayCombo.SelectedIndex = 0;

        priorBankCombo.Items.AddRange(["PayTM-QR_paytm@qr", "PhonePe", "Amazon Pay", "HDFC Netbanking"]);
        priorBankCombo.SelectedIndex = 0;

        backupBankCombo.Items.AddRange(["No Alternate Bank", "PayTM", "PhonePe"]);
        backupBankCombo.SelectedIndex = 0;
    }

    // Custom grid config logic moved to FlattenLayout

    private int GetPassengerRowLimit() =>
        quotaGeneralRadio.Checked || quotaLadiesRadio.Checked ? 6 : 4;

    private void TrainTypeCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var type = trainTypeCombo.SelectedItem?.ToString() ?? string.Empty;
        var hasFood = type.Equals("Rajdhani", StringComparison.OrdinalIgnoreCase) || 
                      type.Equals("Shatabdi", StringComparison.OrdinalIgnoreCase) ||
                      type.Equals("Vande Bharat", StringComparison.OrdinalIgnoreCase);

        foreach (var row in _passengerRows)
        {
            row.Food.Enabled = hasFood;
        }
    }

    private void QuotaRadio_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton { Checked: false })
        {
            return;
        }

        UpdatePassengerRowCount(GetPassengerRowLimit());
        getFareButton.Visible = quotaTatkalRadio.Checked || quotaPremiumRadio.Checked;
    }

    private void UpdatePassengerRowCount(int limit)
    {
        for (int i = 0; i < _passengerRows.Count; i++)
        {
            bool isEnabled = i < limit;
            var row = _passengerRows[i];
            
            row.Name.Enabled = isEnabled;
            row.Age.Enabled = isEnabled;
            row.Sex.Enabled = isEnabled;
            row.Berth.Enabled = isEnabled;
            row.Food.Enabled = isEnabled;
            row.Nationality.Enabled = isEnabled;
            row.Passport.Enabled = isEnabled;
            row.Child.Enabled = isEnabled;
            row.Senior.Enabled = isEnabled;
            row.Bed.Enabled = isEnabled;
        }
    }

    private static void PopulateStationCombo(FlatComboBox comboBox, IReadOnlyList<StationInfo> stations)
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
            TravelDate = travelDatePicker.Value.Date,
            Quota = GetIndianRailQuotaCode()
        };

        UseWaitCursor = true;
        findButton.Enabled = false;

        try
        {
            _lastSearchSettings = settings;

            if (TrainRouteCache.TryGet(settings, out var cachedTrains))
            {
                ShowTrainListPopup(cachedTrains, $"{fromStation.Code} -> {toStation.Code}");
                statusLabel.Text = $"Loaded {cachedTrains.Count} train(s) from cache (valid 1 hour). Click a class in the popup.";
                return;
            }

            statusLabel.Text = "Searching trains...";
            var results = await SearchTrainsAsync(settings);
            TrainRouteCache.Save(settings, results);

            if (results.Count == 0)
            {
                MessageBox.Show(this, "No trains found for selected route.", "Train List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                statusLabel.Text = "No trains found.";
                return;
            }

            ShowTrainListPopup(results, $"{fromStation.Code} -> {toStation.Code}");
            statusLabel.Text = $"Found {results.Count} train(s). Click a class in the popup.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Search failed.";
            MessageBox.Show(this, GetFriendlySearchError(ex), "Search Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            findButton.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private async Task<IReadOnlyList<TrainResult>> SearchTrainsAsync(TrainSearchSettings settings)
    {
        // Using Ghumo.live as requested by user
        await using var scraper = new GhumoScraperService();
        return await scraper.SearchTrainsAsync(settings);
    }

    private Task<string?> SolveCaptchaDialogAsync(IWin32Window? owner, byte[] imageBytes)
    {
        var tcs = new TaskCompletionSource<string?>();
        this.Invoke(() =>
        {
            using var dialog = new CaptchaDialog(imageBytes);
            if (dialog.ShowDialog(owner ?? this) == DialogResult.OK)
            {
                tcs.SetResult(dialog.Answer);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return tcs.Task;
    }

    private static string GetFriendlySearchError(Exception ex)
    {
        var message = ex.Message;
        if (ex.InnerException is not null)
        {
            message = $"{message} {ex.InnerException.Message}";
        }

        if (message.Contains("ERR_NAME_NOT_RESOLVED", StringComparison.OrdinalIgnoreCase)
            || message.Contains("net::ERR_", StringComparison.OrdinalIgnoreCase))
        {
            return "Could not connect to the train enquiry website. Please check your internet connection and try again.";
        }

        if (message.Contains("Timeout", StringComparison.OrdinalIgnoreCase)
            || message.Contains("timed out", StringComparison.OrdinalIgnoreCase))
        {
            return "The train search timed out. Please try again.";
        }

        if (message.Contains("Unexpected response from Indian Railways", StringComparison.OrdinalIgnoreCase))
        {
            return "The train enquiry website returned an unexpected response. Please try again.";
        }

        return ex.Message;
    }

    private void ShowTrainListPopup(IReadOnlyList<TrainResult> results, string routeTitle)
    {
        _trainListDialog?.Dispose();
        _trainListDialog = new TrainListDialog();
        _trainListDialog.ClassSelected += TrainListDialog_ClassSelected;
        _trainListDialog.ShowTrains(results, routeTitle);
        _trainListDialog.Show(this);
    }

    private async void TrainListDialog_ClassSelected(object? sender, TrainClassSelectedEventArgs e)
    {
        // Keep the UI selection logic exactly like the etrain.info version
        ApplyTrainSelection(e.Train, e.TravelClass);
        ClearFareAvailabilityPanel();
        statusLabel.Text = $"Selected {e.Train.TrainNumber} ({e.TravelClass}). Ready to book!";
    }

    private void ClearFareAvailabilityPanel()
    {
        fareText.Text = "0";
    }

    private void ApplyTrainSelection(TrainResult train, string travelClass)
    {
        _selectedTrain = new TrainSelection
        {
            Train = train,
            SelectedDay = string.Empty
        };

        var fromCode = GetSelectedStation(fromStationCombo)?.Code ?? boardingPointText.Text;
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
            classes.Add(travelClass);
        }

        classCombo.Items.AddRange(classes.ToArray());
        classCombo.SelectedItem = travelClass;
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
            MessageBox.Show(this, "Please find a train and click a class first.", "Availability", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        MessageBox.Show(
            this,
            $"Train: {_selectedTrain.Train.TrainNumber}\nClass: {classCombo.SelectedItem}\n\n"
            + "Availability is checked on IRCTC when you click Book IRCTC (same as main).",
            "Availability",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void GetFareButton_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(fareText.Text) && fareText.Text != "0")
        {
            return;
        }

        var passengerCount = GetPassengers().Count;
        if (passengerCount == 0 || _selectedTrain is null)
        {
            MessageBox.Show(this, "Select a train class and enter at least one passenger.", "Get Fare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        fareText.Text = (passengerCount * 170).ToString();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_selectedTrain is null || _lastSearchSettings is null)
        {
            MessageBox.Show(this, "Please find and select a train class first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var passengers = GetPassengers();
        if (passengers.Count == 0)
        {
            MessageBox.Show(this, "Enter at least one passenger in the grid.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var mobile = mobileText.Text.Trim();

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
                AutoUpgradation = false,
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
            SaveIrctcConfigFromUi();
            MessageBox.Show(this, $"Ticket saved.\n{BookingJsonStore.FilePath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            statusLabel.Text = "Ticket saved to JSON.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static (string Code, string Name) ResolveIrctcStation(
        string? trainStationField,
        string fallbackCode,
        string fallbackName)
    {
        var fromTrain = HardcodedStations.Find(trainStationField);
        if (fromTrain is not null)
        {
            return (fromTrain.Code, fromTrain.Name);
        }

        // Etrain sometimes puts only a code like "NZM" or "NDLS"
        if (!string.IsNullOrWhiteSpace(trainStationField))
        {
            var raw = trainStationField.Trim();
            if (raw.Length is >= 2 and <= 5 && raw.All(char.IsLetter))
            {
                return (raw.ToUpperInvariant(), raw.ToUpperInvariant());
            }
        }

        return (fallbackCode, fallbackName);
    }

    private void SaveIrctcConfigFromUi()
    {
        _config.Credentials.Username = irctcUserCombo.Text.Trim();
        _config.MobileNumber = mobileText.Text.Trim();
        _config.ConfirmBerthsOnly = confirmBerthsCheck.Checked;
        _config.UseBetaView = useBetaViewCheck.Checked;
        _config.UseRealChrome = useRealChromeCheck.Checked;
        _config.PaymentMethod = gatewayCombo.SelectedItem?.ToString() ?? "BHIM/UPI";
        _config.PaymentProvider = priorBankCombo.SelectedItem?.ToString() ?? "PAYTM";
        _config.PreferredClass = classCombo.SelectedItem?.ToString() ?? _config.PreferredClass;
        _config.Quota = GetIndianRailQuotaCode();
        _config.Passengers = GetPassengers().Select(p => new Passenger
        {
            Name = p.Name,
            Age = p.Age > 0 ? p.Age.ToString() : string.Empty,
            Gender = p.Gender switch
            {
                "F" => "Female",
                "T" => "Transgender",
                _ => "Male"
            },
            BerthPreference = MapBerthPreference(p.Berth),
            FoodPreference = MapFoodPreference(p.Food)
        }).ToList();
        _config.Save();
    }

    private static string MapBerthPreference(string berth) => berth switch
    {
        "Lower" => "Lower",
        "Middle" => "Middle",
        "Upper" => "Upper",
        "Side Lower" => "Side Lower",
        "Side Upper" => "Side Upper",
        _ => "No Preference"
    };

    private static string MapFoodPreference(string food) => food switch
    {
        "Veg" => "Veg",
        "Non-Veg" => "Non Veg",
        _ => "No Preference"
    };

    private async void BookIrctcButton_Click(object? sender, EventArgs e)
    {
        if (_selectedTrain is null || _lastSearchSettings is null)
        {
            MessageBox.Show(this, "Find a train and click a class first.", "Book IRCTC",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var passengers = GetPassengers();
        if (passengers.Count == 0)
        {
            MessageBox.Show(this, "Enter at least one passenger.", "Book IRCTC",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SaveIrctcConfigFromUi();

        if (string.IsNullOrWhiteSpace(_config.Credentials.Username)
            || string.IsNullOrWhiteSpace(_config.Credentials.Password))
        {
            MessageBox.Show(this, "Enter IRCTC username and password.", "Book IRCTC",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var preferredClass = classCombo.SelectedItem?.ToString() ?? "SL";
        var trainNumber = !string.IsNullOrWhiteSpace(trainNoText.Text)
            ? trainNoText.Text.Trim()
            : _selectedTrain.Train.TrainNumber;

        // IRCTC must search the train's own from/to (Find list can show trains that
        // don't appear for the UI From/To, e.g. NZM–TVC 22654 vs NDLS–MMCT).
        var (fromCode, fromName) = ResolveIrctcStation(
            _selectedTrain.Train.FromStation,
            _lastSearchSettings.FromStationCode,
            _lastSearchSettings.FromStationName);
        var (toCode, toName) = ResolveIrctcStation(
            _selectedTrain.Train.ToStation,
            _lastSearchSettings.ToStationCode,
            _lastSearchSettings.ToStationName);

        if (!fromCode.Equals(_lastSearchSettings.FromStationCode, StringComparison.OrdinalIgnoreCase)
            || !toCode.Equals(_lastSearchSettings.ToStationCode, StringComparison.OrdinalIgnoreCase))
        {
            statusLabel.Text =
                $"IRCTC will search {fromCode} → {toCode} (train stations), not "
                + $"{_lastSearchSettings.FromStationCode} → {_lastSearchSettings.ToStationCode}.";
        }

        var settings = new TrainSearchSettings
        {
            FromStationCode = fromCode,
            FromStationName = fromName,
            ToStationCode = toCode,
            ToStationName = toName,
            TravelDate = _lastSearchSettings.TravelDate,
            Quota = GetIndianRailQuotaCode(),
            PreferredClass = preferredClass,
            SiteUrl = _lastSearchSettings.SiteUrl
        };

        var train = new TrainResult
        {
            TrainNumber = trainNumber,
            TrainName = _selectedTrain.Train.TrainName,
            FromStation = _selectedTrain.Train.FromStation,
            Departure = _selectedTrain.Train.Departure,
            ToStation = _selectedTrain.Train.ToStation,
            Arrival = _selectedTrain.Train.Arrival,
            TravelTime = _selectedTrain.Train.TravelTime,
            Sunday = _selectedTrain.Train.Sunday,
            Monday = _selectedTrain.Train.Monday,
            Tuesday = _selectedTrain.Train.Tuesday,
            Wednesday = _selectedTrain.Train.Wednesday,
            Thursday = _selectedTrain.Train.Thursday,
            Friday = _selectedTrain.Train.Friday,
            Saturday = _selectedTrain.Train.Saturday,
            AvailableClasses = _selectedTrain.Train.AvailableClasses,
            ClassLinkKeys = _selectedTrain.Train.ClassLinkKeys
        };

        var confirmMsg = $"Are you sure you want to book {trainNumber} ({_selectedTrain.Train.TrainName}) " +
                         $"from {fromCode} to {toCode} on {_lastSearchSettings.TravelDate:dd-MMM-yyyy} " +
                         $"for {passengers.Count} passenger(s) in {preferredClass}?";
                         
        var confirmResult = MessageBox.Show(this, confirmMsg, "Confirm Booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        bookIrctcButton.Enabled = false;
        findButton.Enabled = false;
        saveButton.Enabled = false;
        stopButton.Enabled = true;
        UseWaitCursor = true;

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var progress = new Progress<string>(message =>
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            statusLabel.Text = message;
        });

        try
        {
            statusLabel.Text = "Starting IRCTC booking...";
            _irctcService ??= new IrctcBookingService();
            await _irctcService.BookTrainAsync(settings, train, _config, progress, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Automation stopped by user.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = $"Automation stopped: {ex.Message}";
            MessageBox.Show(this, ex.Message, "Book IRCTC", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            bookIrctcButton.Enabled = true;
            findButton.Enabled = true;
            saveButton.Enabled = true;
            stopButton.Enabled = false;
            UseWaitCursor = false;
            Cursor = Cursors.Default;
        }
    }

    private void StopButton_Click(object? sender, EventArgs e)
    {
        if (_cts == null || _cts.IsCancellationRequested) return;
        _cts?.Cancel();
        statusLabel.Text = "Stopping automation...";
    }

    private string GetSelectedQuota()
    {
        if (quotaLadiesRadio.Checked) return "Ladies";
        if (quotaTatkalRadio.Checked) return "Tatkal";
        if (quotaPremiumRadio.Checked) return "Premium Tatkal";
        return "General";
    }

    private string GetIndianRailQuotaCode()
    {
        if (quotaLadiesRadio.Checked) return "LD";
        if (quotaTatkalRadio.Checked) return "TQ";
        if (quotaPremiumRadio.Checked) return "PT";
        return "GN";
    }

    private List<PassengerInfo> GetPassengers()
    {
        var passengers = new List<PassengerInfo>();
        foreach (var row in _passengerRows)
        {
            if (!row.Name.Enabled) continue; // Skip disabled rows

            var name = row.Name.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            _ = int.TryParse(row.Age.Text, out var age);
            passengers.Add(new PassengerInfo
            {
                Name = name,
                Age = age,
                Gender = row.Sex.SelectedItem?.ToString() ?? "M",
                Berth = row.Berth.SelectedItem?.ToString() ?? "No Choice",
                Food = row.Food.SelectedItem?.ToString() ?? "No Choice",
                Nationality = row.Nationality.SelectedItem?.ToString() ?? "India-IN",
                Passport = row.Passport.Text.Trim(),
                IsChild = row.Child.Checked,
                IsSenior = row.Senior.Checked,
                BedRoll = row.Bed.Checked
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
        try
        {
            SaveIrctcConfigFromUi();
        }
        catch
        {
            // ignore save errors on close
        }

        _cts?.Cancel();
        _trainListDialog?.Dispose();

        if (_irctcService is not null)
        {
            await _irctcService.DisposeAsync();
            _irctcService = null;
        }

        _cts?.Dispose();
        base.OnFormClosed(e);
    }
}
