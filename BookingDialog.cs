namespace train_automation;

public partial class BookingDialog : Form
{
    private readonly TrainResult _train;
    private readonly TrainSearchSettings _searchSettings;
    private readonly List<PassengerRowControl> _passengerRows = [];

    public BookingDialog(TrainResult train, TrainSearchSettings searchSettings)
    {
        _train = train;
        _searchSettings = searchSettings;
        InitializeComponent();
        ConfigureForm();
    }

    private void ConfigureForm()
    {
        headerLabel.Text = $"Train {_train.TrainNumber} - {_train.TrainName}  |  {_searchSettings.FromStationName} → {_searchSettings.ToStationName}";

        reservationChoiceCombo.Items.AddRange(
        [
            "Book only if all berths are allotted in same coach",
            "Book only if at least 1 lower berth is allotted",
            "Book only if 2 lower berths are allotted",
            "None"
        ]);
        reservationChoiceCombo.SelectedIndex = 0;

        AddPassengerRow();
    }

    private void AddMoreButton_Click(object? sender, EventArgs e)
    {
        AddPassengerRow();
    }

    private void AddPassengerRow()
    {
        var row = new PassengerRowControl(_passengerRows.Count + 1);
        row.RemoveRequested += (_, _) => RemovePassengerRow(row);
        _passengerRows.Add(row);
        passengersPanel.Controls.Add(row);
        RefreshPassengerLabels();
    }

    private void RemovePassengerRow(PassengerRowControl row)
    {
        if (_passengerRows.Count <= 1)
        {
            MessageBox.Show(this, "At least one passenger is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _passengerRows.Remove(row);
        passengersPanel.Controls.Remove(row);
        row.Dispose();
        RefreshPassengerLabels();
    }

    private void RefreshPassengerLabels()
    {
        for (var index = 0; index < _passengerRows.Count; index++)
        {
            _passengerRows[index].SetPassengerNumber(index + 1);
            _passengerRows[index].ShowRemoveButton(_passengerRows.Count > 1);
        }
    }

    private void ContinueButton_Click(object? sender, EventArgs e)
    {
        var passengers = new List<PassengerInfo>();
        foreach (var row in _passengerRows)
        {
            if (!row.TryGetPassenger(out var passenger, out var error))
            {
                MessageBox.Show(this, error, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            passengers.Add(passenger);
        }

        var booking = new TrainBookingRecord
        {
            TrainNumber = _train.TrainNumber,
            TrainName = _train.TrainName,
            FromStation = _searchSettings.FromStationCode,
            ToStation = _searchSettings.ToStationCode,
            TravelDate = _searchSettings.TravelDate,
            Passengers = passengers,
            Preferences = new BookingPreferences
            {
                AutoUpgradation = autoUpgradationCheck.Checked,
                BookOnlyIfConfirmBerths = confirmBerthsCheck.Checked,
                ReservationChoice = reservationChoiceCombo.SelectedItem?.ToString() ?? string.Empty,
                PreferredCoachNo = preferredCoachText.Text.Trim()
            },
            TravelInsurance = insuranceYesRadio.Checked ? "Yes" : "No",
            PaymentMode = paymentCardsRadio.Checked
                ? "Credit/Debit Cards, Net Banking, Wallets, UPI and Others"
                : "BHIM/UPI"
        };

        try
        {
            BookingJsonStore.Append(booking);
            DialogResult = DialogResult.OK;
            MessageBox.Show(
                this,
                $"Booking saved successfully.\nFile: {BookingJsonStore.FilePath}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
