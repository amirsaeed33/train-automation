namespace train_automation;

public sealed class PassengerRowControl : Panel
{
    private readonly Label _titleLabel;
    private readonly TextBox _nameText;
    private readonly NumericUpDown _ageInput;
    private readonly ComboBox _genderCombo;
    private readonly TextBox _phoneText;
    private readonly Button _removeButton;

    public event EventHandler? RemoveRequested;

    public PassengerRowControl(int passengerNumber)
    {
        Size = new Size(640, 110);
        Margin = new Padding(0, 0, 0, 8);
        BorderStyle = BorderStyle.FixedSingle;

        _titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(10, 8),
            Text = $"Passenger {passengerNumber}"
        };

        var nameLabel = CreateLabel("Name", 10, 36);
        _nameText = new TextBox
        {
            Location = new Point(90, 32),
            Size = new Size(220, 27)
        };

        var ageLabel = CreateLabel("Age", 330, 36);
        _ageInput = new NumericUpDown
        {
            Location = new Point(370, 32),
            Minimum = 1,
            Maximum = 120,
            Value = 25,
            Size = new Size(70, 27)
        };

        var genderLabel = CreateLabel("Gender", 460, 36);
        _genderCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(530, 32),
            Size = new Size(90, 28)
        };
        _genderCombo.Items.AddRange(["Male", "Female", "Transgender"]);
        _genderCombo.SelectedIndex = 0;

        var phoneLabel = CreateLabel("Phone", 10, 72);
        _phoneText = new TextBox
        {
            Location = new Point(90, 68),
            MaxLength = 10,
            Size = new Size(180, 27)
        };

        _removeButton = new Button
        {
            Location = new Point(530, 66),
            Size = new Size(90, 30),
            Text = "Remove",
            Visible = false
        };
        _removeButton.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);

        Controls.AddRange(
        [
            _titleLabel, nameLabel, _nameText, ageLabel, _ageInput,
            genderLabel, _genderCombo, phoneLabel, _phoneText, _removeButton
        ]);
    }

    public void SetPassengerNumber(int passengerNumber)
    {
        _titleLabel.Text = $"Passenger {passengerNumber}";
    }

    public void ShowRemoveButton(bool visible)
    {
        _removeButton.Visible = visible;
    }

    public bool TryGetPassenger(out PassengerInfo passenger, out string error)
    {
        passenger = new PassengerInfo();
        error = string.Empty;

        var name = _nameText.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            error = $"{_titleLabel.Text}: Please enter passenger name.";
            return false;
        }

        var phone = _phoneText.Text.Trim();
        if (phone.Length != 10 || !phone.All(char.IsDigit))
        {
            error = $"{_titleLabel.Text}: Please enter a valid 10-digit phone number.";
            return false;
        }

        passenger = new PassengerInfo
        {
            Name = name,
            Age = (int)_ageInput.Value,
            Gender = _genderCombo.SelectedItem?.ToString() ?? string.Empty,
            Phone = phone
        };
        return true;
    }

    private static Label CreateLabel(string text, int x, int y) =>
        new()
        {
            AutoSize = true,
            Location = new Point(x, y),
            Text = text
        };
}
