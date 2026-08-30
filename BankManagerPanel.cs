namespace train_automation;

public sealed class BankManagerPanel : UserControl
{
    private readonly FlowLayoutPanel _tabStrip = new();
    private readonly Panel _formPane = new();
    private readonly Panel _listPane = new();
    private readonly ListBox _savedList = new();
    
    // Tabs
    private readonly Button _btnTabBank = new();
    private readonly Button _btnTabDebit = new();
    private readonly Button _btnTabCredit = new();
    private readonly Button _btnTabSaved = new();
    private string _currentTab = "Bank"; // "Bank", "Debit", "Credit", "Saved"

    // Fields - Shared
    private readonly ComboBox _gatewayCombo = new();
    private readonly ComboBox _bankNameCombo = new();
    private readonly TextBox _nameToSaveBox = new();
    private readonly CheckBox _showPasswordCheck = new();
    private readonly Button _saveButton = new();

    // Fields - Bank specific
    private readonly TextBox _bankUserBox = new();
    private readonly TextBox _bankPassBox = new();

    // Fields - Card specific
    private readonly ComboBox _cardTypeCombo = new();
    private readonly TextBox _cardNoBox = new();
    private readonly ComboBox _expMonthCombo = new();
    private readonly ComboBox _expYearCombo = new();
    private readonly TextBox _nameOnCardBox = new();
    private readonly TextBox _pinBox = new();
    private readonly TextBox _cvvBox = new();
    private readonly TextBox _threeDPassBox = new();
    
    // Labels for dynamic toggling
    private readonly Label _lblGateway = new();
    private readonly Label _lblBankName = new();
    private readonly Label _lblUserOrType = new(); // User Name OR Select Your Type
    private readonly Label _lblPassOrCardNo = new();
    private readonly Label _lblExpDate = new();
    private readonly Label _lblNameOnCard = new();
    private readonly Label _lblPin = new();
    private readonly Label _lblCvv = new();
    private readonly Label _lblThreeD = new();
    private readonly Label _lblNameToSave = new();

    public BankManagerPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.PageBg;
        Font = UiTheme.BodySm;

        BuildLayout();
        SwitchTab("Bank");
        ReloadSavedList();
    }

    private void BuildLayout()
    {
        // ── TOP TABS ──────────────────────────────
        _tabStrip.Dock = DockStyle.Top;
        _tabStrip.Height = 40;
        _tabStrip.Padding = new Padding(12, 6, 12, 0);
        _tabStrip.BackColor = UiTheme.PageBg;
        
        StyleTabButton(_btnTabBank, "Bank");
        StyleTabButton(_btnTabDebit, "Debit Card");
        StyleTabButton(_btnTabCredit, "Credit Card");
        StyleTabButton(_btnTabSaved, "Saved Items");
        
        _btnTabBank.Click += (_, _) => SwitchTab("Bank");
        _btnTabDebit.Click += (_, _) => SwitchTab("Debit");
        _btnTabCredit.Click += (_, _) => SwitchTab("Credit");
        _btnTabSaved.Click += (_, _) => SwitchTab("Saved");

        _tabStrip.Controls.Add(_btnTabBank);
        _tabStrip.Controls.Add(_btnTabDebit);
        _tabStrip.Controls.Add(_btnTabCredit);
        _tabStrip.Controls.Add(_btnTabSaved);
        
        Controls.Add(_tabStrip);

        // ── MAIN CONTENT ──────────────────────────
        _formPane.Dock = DockStyle.Fill;
        _formPane.BackColor = UiTheme.PageBg;
        
        _listPane.Dock = DockStyle.Fill;
        _listPane.BackColor = UiTheme.PageBg;
        
        var padPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        padPanel.Controls.Add(_formPane);
        padPanel.Controls.Add(_listPane);
        
        Controls.Add(padPanel);
        padPanel.BringToFront();

        BuildFormPane();
        BuildListPane();
    }

    private void BuildFormPane()
    {
        int y = 10;
        int lh = 22; 
        int bh = 24; 
        int lw = 185; 
        int bw = 350; 
        
        SetupField(_lblGateway, "Gateway :", 10, y, lw, _gatewayCombo, lw + 10, y, bw, bh);
        _gatewayCombo.Items.AddRange(new object[] { "IRCTC", "PAYTM", "PHONEPE" });
        _gatewayCombo.SelectedIndex = 0;
        y += 35;

        SetupField(_lblBankName, "Bank Name :", 10, y, lw, _bankNameCombo, lw + 10, y, bw, bh);
        _bankNameCombo.Items.AddRange(new object[] { "Select Bank", "HDFC", "ICICI", "SBI" });
        _bankNameCombo.SelectedIndex = 0;
        y += 35;

        SetupField(_lblUserOrType, "User Name :", 10, y, lw, _bankUserBox, lw + 10, y, bw, bh);
        _cardTypeCombo.Location = _bankUserBox.Location;
        _cardTypeCombo.Size = new Size(130, bh);
        _cardTypeCombo.Items.AddRange(new object[] { "Visa", "MasterCard", "RuPay" });
        _formPane.Controls.Add(_cardTypeCombo);
        y += 35;

        SetupField(_lblPassOrCardNo, "Login Password :", 10, y, lw, _bankPassBox, lw + 10, y, bw, bh);
        _bankPassBox.PasswordChar = '*';
        _cardNoBox.Location = _bankPassBox.Location;
        _cardNoBox.Size = _bankPassBox.Size;
        _formPane.Controls.Add(_cardNoBox);
        y += 35;

        _lblExpDate.Text = "Valid Thru/Expiry Date :";
        _lblExpDate.Location = new Point(10, y);
        _lblExpDate.Size = new Size(lw, lh);
        _formPane.Controls.Add(_lblExpDate);
        
        _expMonthCombo.Location = new Point(lw + 10, y);
        _expMonthCombo.Size = new Size(110, bh);
        _expMonthCombo.Items.AddRange(new object[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" });
        _formPane.Controls.Add(_expMonthCombo);
        
        _expYearCombo.Location = new Point(lw + 130, y);
        _expYearCombo.Size = new Size(110, bh);
        int currentYear = DateTime.Now.Year;
        for (int i = 0; i < 15; i++) _expYearCombo.Items.Add((currentYear + i).ToString());
        _formPane.Controls.Add(_expYearCombo);
        y += 35;

        SetupField(_lblNameOnCard, "Name on Card:", 10, y, lw, _nameOnCardBox, lw + 10, y, bw, bh);
        y += 35;

        _lblPin.Text = "Visa / Master Pin :";
        _lblPin.Location = new Point(10, y);
        _lblPin.Size = new Size(lw - 10, lh);
        _formPane.Controls.Add(_lblPin);
        
        _pinBox.Location = new Point(lw + 10, y);
        _pinBox.Size = new Size(100, bh);
        _pinBox.PasswordChar = '*';
        _formPane.Controls.Add(_pinBox);
        
        _lblCvv.Text = "CVV :";
        _lblCvv.Location = new Point(lw + 120, y);
        _lblCvv.Size = new Size(50, lh);
        _formPane.Controls.Add(_lblCvv);
        
        _cvvBox.Location = new Point(lw + 180, y);
        _cvvBox.Size = new Size(100, bh);
        _cvvBox.PasswordChar = '*';
        _formPane.Controls.Add(_cvvBox);
        y += 35;

        SetupField(_lblThreeD, "Card 3D Password :", 10, y, lw, _threeDPassBox, lw + 10, y, bw, bh);
        _threeDPassBox.PasswordChar = '*';
        y += 35;

        SetupField(_lblNameToSave, "Name to Save :", 10, y, lw, _nameToSaveBox, lw + 10, y, bw, bh);
        _lblNameToSave.ForeColor = UiTheme.Danger; 
        y += 40;

        _showPasswordCheck.Text = "Show Password";
        _showPasswordCheck.Location = new Point(10, y);
        _showPasswordCheck.AutoSize = true;
        _showPasswordCheck.ForeColor = UiTheme.Primary; 
        _showPasswordCheck.CheckedChanged += (_, _) => TogglePasswordMasks();
        _formPane.Controls.Add(_showPasswordCheck);

        _saveButton.Text = "Save";
        _saveButton.Location = new Point(lw + 10, y - 5);
        _saveButton.Size = new Size(bw, 32);
        _saveButton.FlatStyle = FlatStyle.Flat;
        _saveButton.BackColor = UiTheme.Primary;
        _saveButton.ForeColor = Color.White;
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Cursor = Cursors.Hand;
        _saveButton.Click += (_, _) => SaveCurrentData();
        _formPane.Controls.Add(_saveButton);

        foreach (Control c in _formPane.Controls)
        {
            if (c is Label l && l != _lblNameToSave) l.ForeColor = UiTheme.Text;
            if (c is TextBox t) { t.BackColor = UiTheme.Surface; t.ForeColor = UiTheme.Text; t.BorderStyle = BorderStyle.FixedSingle; }
            if (c is ComboBox cb) { cb.BackColor = UiTheme.Surface; cb.ForeColor = UiTheme.Text; cb.FlatStyle = FlatStyle.Flat; }
        }
    }

    private void SetupField(Label lbl, string labelText, int lx, int ly, int lw, Control box, int bx, int by, int bw, int bh)
    {
        lbl.Text = labelText;
        lbl.Location = new Point(lx, ly);
        lbl.Size = new Size(lw, bh);
        lbl.TextAlign = ContentAlignment.MiddleLeft;
        _formPane.Controls.Add(lbl);
        
        box.Location = new Point(bx, by);
        box.Size = new Size(bw, bh);
        _formPane.Controls.Add(box);
    }

    private void BuildListPane()
    {
        _savedList.Dock = DockStyle.Fill;
        _savedList.BorderStyle = BorderStyle.None;
        _savedList.Font = new Font("Segoe UI", 9F);
        _savedList.BackColor = UiTheme.Surface;
        _savedList.ForeColor = UiTheme.Text;
        
        var listWrapper = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = UiTheme.OutlineVariant
        };
        listWrapper.Controls.Add(_savedList);
        _listPane.Controls.Add(listWrapper);

        var bottomActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(10, 5, 10, 5),
            BackColor = UiTheme.PageBg
        };

        var btnEdit = UiTheme.CreatePrimaryButton("Edit", 80, 26);
        btnEdit.BackColor = UiTheme.Surface;
        btnEdit.ForeColor = UiTheme.Text;
        var btnDelete = UiTheme.CreatePrimaryButton("Delete", 80, 26);
        btnDelete.BackColor = UiTheme.Surface;
        btnDelete.ForeColor = UiTheme.Text;
        var btnDeleteAll = UiTheme.CreatePrimaryButton("Delete All", 90, 26);
        btnDeleteAll.BackColor = UiTheme.Surface;
        btnDeleteAll.ForeColor = UiTheme.Text;

        btnDelete.Click += (_, _) =>
        {
            if (_savedList.SelectedIndex < 0) return;
            var config = BookingConfiguration.Load();
            
            string sel = _savedList.SelectedItem?.ToString() ?? "";
            
            var bankMatch = config.SavedBanks.FirstOrDefault(b => sel == $"{b.NameToSave} ({b.BankName})");
            if (bankMatch != null) config.SavedBanks.Remove(bankMatch);
            else
            {
                var cardMatch = config.SavedCards.FirstOrDefault(c => sel == $"{c.NameToSave} ({c.CardNumber})");
                if (cardMatch != null) config.SavedCards.Remove(cardMatch);
            }
            
            config.Save();
            ReloadSavedList();
        };

        btnDeleteAll.Click += (_, _) =>
        {
            var config = BookingConfiguration.Load();
            config.SavedBanks.Clear();
            config.SavedCards.Clear();
            config.Save();
            ReloadSavedList();
        };

        bottomActions.Controls.Add(btnEdit);
        bottomActions.Controls.Add(btnDelete);
        bottomActions.Controls.Add(btnDeleteAll);
        
        _listPane.Controls.Add(bottomActions);
    }

    private void StyleTabButton(Button btn, string text)
    {
        btn.Text = text;
        btn.Size = new Size(130, 28);
        btn.FlatStyle = FlatStyle.Flat;
        btn.Margin = new Padding(0, 0, 10, 0);
        btn.Cursor = Cursors.Hand;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = UiTheme.Primary;
        SetTabActive(btn, false);
    }

    private void SetTabActive(Button btn, bool active)
    {
        if (active)
        {
            btn.BackColor = UiTheme.Primary;
            btn.ForeColor = Color.White;
        }
        else
        {
            btn.BackColor = UiTheme.PageBg;
            btn.ForeColor = UiTheme.Primary;
        }
    }

    private void SwitchTab(string tab)
    {
        _currentTab = tab;
        SetTabActive(_btnTabBank, tab == "Bank");
        SetTabActive(_btnTabDebit, tab == "Debit");
        SetTabActive(_btnTabCredit, tab == "Credit");
        SetTabActive(_btnTabSaved, tab == "Saved");

        if (tab == "Saved")
        {
            _listPane.BringToFront();
            ReloadSavedList();
            return;
        }
        else
        {
            _formPane.BringToFront();
        }

        bool isBank = tab == "Bank";
        
        _bankUserBox.Visible = isBank;
        _bankPassBox.Visible = isBank;
        
        _cardTypeCombo.Visible = !isBank;
        _cardNoBox.Visible = !isBank;
        _expMonthCombo.Visible = !isBank;
        _expYearCombo.Visible = !isBank;
        _nameOnCardBox.Visible = !isBank;
        _pinBox.Visible = !isBank;
        _cvvBox.Visible = !isBank;
        _threeDPassBox.Visible = !isBank;
        
        _lblExpDate.Visible = !isBank;
        _lblNameOnCard.Visible = !isBank;
        _lblPin.Visible = !isBank;
        _lblCvv.Visible = !isBank;
        _lblThreeD.Visible = !isBank;

        if (isBank)
        {
            _lblBankName.Text = "Bank Name :";
            _lblUserOrType.Text = "User Name :";
            _lblPassOrCardNo.Text = "Login Password :";
        }
        else
        {
            _lblBankName.Text = "Bank Name :";
            _lblUserOrType.Text = "Card Type :";
            _lblPassOrCardNo.Text = "Card No. :";
        }
    }

    private void TogglePasswordMasks()
    {
        char mask = _showPasswordCheck.Checked ? '\0' : '*';
        _bankPassBox.PasswordChar = mask;
        _pinBox.PasswordChar = mask;
        _cvvBox.PasswordChar = mask;
        _threeDPassBox.PasswordChar = mask;
    }

    private void SaveCurrentData()
    {
        var config = BookingConfiguration.Load();
        
        if (_currentTab == "Bank")
        {
            config.SavedBanks.Add(new BankDetails
            {
                Gateway = _gatewayCombo.Text,
                BankName = _bankNameCombo.Text,
                UserName = _bankUserBox.Text,
                Password = _bankPassBox.Text,
                NameToSave = _nameToSaveBox.Text
            });
        }
        else
        {
            config.SavedCards.Add(new CardDetails
            {
                CardCategory = _currentTab == "Debit" ? "Debit" : "Credit",
                Gateway = _gatewayCombo.Text,
                BankName = _bankNameCombo.Text,
                CardType = _cardTypeCombo.Text,
                CardNumber = _cardNoBox.Text,
                ExpiryMonth = _expMonthCombo.Text,
                ExpiryYear = _expYearCombo.Text,
                NameOnCard = _nameOnCardBox.Text,
                Pin = _pinBox.Text,
                Cvv = _cvvBox.Text,
                ThreeDPassword = _threeDPassBox.Text,
                NameToSave = _nameToSaveBox.Text
            });
        }
        
        config.Save();
        MessageBox.Show("Saved successfully!", "Bank Manager");
        
        _nameToSaveBox.Text = "";
        _bankUserBox.Text = "";
        _bankPassBox.Text = "";
        _cardNoBox.Text = "";
        _pinBox.Text = "";
        _cvvBox.Text = "";
        _threeDPassBox.Text = "";
    }

    private void ReloadSavedList()
    {
        _savedList.Items.Clear();
        var config = BookingConfiguration.Load();
        
        foreach (var b in config.SavedBanks)
            _savedList.Items.Add($"{b.NameToSave} ({b.BankName})");
            
        foreach (var c in config.SavedCards)
            _savedList.Items.Add($"{c.NameToSave} ({c.CardNumber})");
    }
}
