namespace train_automation
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            statusLabel = new Label();
            tabControl = new TabControl();
            tabRun = new TabPage();
            trainGrid = new DataGridView();
            searchPanel = new Panel();
            stopButton = new Button();
            bookIrctcButton = new Button();
            searchButton = new Button();
            classCombo = new ComboBox();
            classLabel = new Label();
            quotaCombo = new ComboBox();
            quotaLabel = new Label();
            travelDatePicker = new DateTimePicker();
            dateLabel = new Label();
            toStationCombo = new ComboBox();
            toLabel = new Label();
            fromStationCombo = new ComboBox();
            fromLabel = new Label();
            tabPassengers = new TabPage();
            passengerGrid = new DataGridView();
            tabSettings = new TabPage();
            settingsScroll = new Panel();
            saveSettingsButton = new Button();
            scheduleHintLabel = new Label();
            scheduleTimeText = new TextBox();
            scheduleTimeLabel = new Label();
            refreshIntervalNumeric = new NumericUpDown();
            refreshIntervalLabel = new Label();
            availabilityTimeoutNumeric = new NumericUpDown();
            availabilityTimeoutLabel = new Label();
            autoUpgradeCheck = new CheckBox();
            confirmBerthsCheck = new CheckBox();
            paymentCombo = new ComboBox();
            paymentLabel = new Label();
            mobileText = new TextBox();
            mobileLabel = new Label();
            passwordText = new TextBox();
            passwordLabel = new Label();
            usernameText = new TextBox();
            usernameLabel = new Label();
            securityHintLabel = new Label();
            tabControl.SuspendLayout();
            tabRun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trainGrid).BeginInit();
            searchPanel.SuspendLayout();
            tabPassengers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)passengerGrid).BeginInit();
            tabSettings.SuspendLayout();
            settingsScroll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)refreshIntervalNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)availabilityTimeoutNumeric).BeginInit();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.Dock = DockStyle.Top;
            statusLabel.Font = new Font("Segoe UI", 10F);
            statusLabel.Location = new Point(0, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Padding = new Padding(12, 10, 12, 10);
            statusLabel.Size = new Size(1280, 42);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Loading stations...";
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabRun);
            tabControl.Controls.Add(tabPassengers);
            tabControl.Controls.Add(tabSettings);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 42);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1280, 678);
            tabControl.TabIndex = 1;
            // 
            // tabRun
            // 
            tabRun.Controls.Add(trainGrid);
            tabRun.Controls.Add(searchPanel);
            tabRun.Location = new Point(4, 29);
            tabRun.Name = "tabRun";
            tabRun.Padding = new Padding(3);
            tabRun.Size = new Size(1272, 645);
            tabRun.TabIndex = 0;
            tabRun.Text = "Search & Run";
            tabRun.UseVisualStyleBackColor = true;
            // 
            // trainGrid
            // 
            trainGrid.AllowUserToAddRows = false;
            trainGrid.AllowUserToDeleteRows = false;
            trainGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            trainGrid.BackgroundColor = SystemColors.Window;
            trainGrid.BorderStyle = BorderStyle.None;
            trainGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            trainGrid.Dock = DockStyle.Fill;
            trainGrid.Location = new Point(3, 96);
            trainGrid.MultiSelect = false;
            trainGrid.Name = "trainGrid";
            trainGrid.ReadOnly = true;
            trainGrid.RowHeadersVisible = false;
            trainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            trainGrid.Size = new Size(1266, 546);
            trainGrid.TabIndex = 3;
            // 
            // searchPanel
            // 
            searchPanel.BackColor = Color.WhiteSmoke;
            searchPanel.Controls.Add(stopButton);
            searchPanel.Controls.Add(bookIrctcButton);
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(classCombo);
            searchPanel.Controls.Add(classLabel);
            searchPanel.Controls.Add(quotaCombo);
            searchPanel.Controls.Add(quotaLabel);
            searchPanel.Controls.Add(travelDatePicker);
            searchPanel.Controls.Add(dateLabel);
            searchPanel.Controls.Add(toStationCombo);
            searchPanel.Controls.Add(toLabel);
            searchPanel.Controls.Add(fromStationCombo);
            searchPanel.Controls.Add(fromLabel);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Location = new Point(3, 3);
            searchPanel.Name = "searchPanel";
            searchPanel.Padding = new Padding(12, 8, 12, 8);
            searchPanel.Size = new Size(1266, 93);
            searchPanel.TabIndex = 2;
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(10, 18);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(46, 20);
            fromLabel.TabIndex = 0;
            fromLabel.Text = "From:";
            // 
            // fromStationCombo
            // 
            fromStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            fromStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            fromStationCombo.FormattingEnabled = true;
            fromStationCombo.Location = new Point(60, 15);
            fromStationCombo.Name = "fromStationCombo";
            fromStationCombo.Size = new Size(250, 28);
            fromStationCombo.TabIndex = 1;
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(325, 18);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(28, 20);
            toLabel.TabIndex = 2;
            toLabel.Text = "To:";
            // 
            // toStationCombo
            // 
            toStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            toStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            toStationCombo.FormattingEnabled = true;
            toStationCombo.Location = new Point(360, 15);
            toStationCombo.Name = "toStationCombo";
            toStationCombo.Size = new Size(230, 28);
            toStationCombo.TabIndex = 3;
            // 
            // dateLabel
            // 
            dateLabel.AutoSize = true;
            dateLabel.Location = new Point(605, 18);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(42, 20);
            dateLabel.TabIndex = 4;
            dateLabel.Text = "Date:";
            // 
            // travelDatePicker
            // 
            travelDatePicker.Format = DateTimePickerFormat.Short;
            travelDatePicker.Location = new Point(655, 15);
            travelDatePicker.MinDate = new DateTime(2023, 1, 1, 0, 0, 0, 0);
            travelDatePicker.Name = "travelDatePicker";
            travelDatePicker.Size = new Size(120, 27);
            travelDatePicker.TabIndex = 5;
            // 
            // quotaLabel
            // 
            quotaLabel.AutoSize = true;
            quotaLabel.Location = new Point(10, 56);
            quotaLabel.Name = "quotaLabel";
            quotaLabel.Size = new Size(52, 20);
            quotaLabel.TabIndex = 6;
            quotaLabel.Text = "Quota:";
            // 
            // quotaCombo
            // 
            quotaCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            quotaCombo.FormattingEnabled = true;
            quotaCombo.Location = new Point(70, 53);
            quotaCombo.Name = "quotaCombo";
            quotaCombo.Size = new Size(180, 28);
            quotaCombo.TabIndex = 7;
            // 
            // classLabel
            // 
            classLabel.AutoSize = true;
            classLabel.Location = new Point(270, 56);
            classLabel.Name = "classLabel";
            classLabel.Size = new Size(46, 20);
            classLabel.TabIndex = 8;
            classLabel.Text = "Class:";
            // 
            // classCombo
            // 
            classCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            classCombo.FormattingEnabled = true;
            classCombo.Location = new Point(325, 53);
            classCombo.Name = "classCombo";
            classCombo.Size = new Size(180, 28);
            classCombo.TabIndex = 9;
            // 
            // searchButton
            // 
            searchButton.BackColor = Color.DodgerBlue;
            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.FlatStyle = FlatStyle.Flat;
            searchButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            searchButton.ForeColor = Color.White;
            searchButton.Location = new Point(790, 30);
            searchButton.Name = "searchButton";
            searchButton.Size = new Size(140, 36);
            searchButton.TabIndex = 10;
            searchButton.Text = "Search Trains";
            searchButton.UseVisualStyleBackColor = false;
            searchButton.Click += SearchButton_Click;
            // 
            // bookIrctcButton
            // 
            bookIrctcButton.BackColor = Color.FromArgb(231, 76, 60);
            bookIrctcButton.FlatAppearance.BorderSize = 0;
            bookIrctcButton.FlatStyle = FlatStyle.Flat;
            bookIrctcButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            bookIrctcButton.ForeColor = Color.White;
            bookIrctcButton.Location = new Point(940, 30);
            bookIrctcButton.Name = "bookIrctcButton";
            bookIrctcButton.Size = new Size(150, 36);
            bookIrctcButton.TabIndex = 11;
            bookIrctcButton.Text = "Book on IRCTC";
            bookIrctcButton.UseVisualStyleBackColor = false;
            bookIrctcButton.Click += BookIrctcButton_Click;
            // 
            // stopButton
            // 
            stopButton.BackColor = Color.FromArgb(80, 80, 80);
            stopButton.Enabled = false;
            stopButton.FlatAppearance.BorderSize = 0;
            stopButton.FlatStyle = FlatStyle.Flat;
            stopButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            stopButton.ForeColor = Color.White;
            stopButton.Location = new Point(1100, 30);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(40, 36);
            stopButton.TabIndex = 12;
            stopButton.Text = "⏹";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.Click += StopButton_Click;
            // 
            // tabPassengers
            // 
            tabPassengers.Controls.Add(passengerGrid);
            tabPassengers.Location = new Point(4, 29);
            tabPassengers.Name = "tabPassengers";
            tabPassengers.Padding = new Padding(3);
            tabPassengers.Size = new Size(1272, 645);
            tabPassengers.TabIndex = 1;
            tabPassengers.Text = "Passengers";
            tabPassengers.UseVisualStyleBackColor = true;
            // 
            // passengerGrid
            // 
            passengerGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            passengerGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            passengerGrid.Dock = DockStyle.Fill;
            passengerGrid.Location = new Point(3, 3);
            passengerGrid.Name = "passengerGrid";
            passengerGrid.RowHeadersWidth = 51;
            passengerGrid.Size = new Size(1266, 639);
            passengerGrid.TabIndex = 0;
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(settingsScroll);
            tabSettings.Location = new Point(4, 29);
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Padding(3);
            tabSettings.Size = new Size(1272, 645);
            tabSettings.TabIndex = 2;
            tabSettings.Text = "IRCTC Settings";
            tabSettings.UseVisualStyleBackColor = true;
            // 
            // settingsScroll
            // 
            settingsScroll.AutoScroll = true;
            settingsScroll.Controls.Add(securityHintLabel);
            settingsScroll.Controls.Add(saveSettingsButton);
            settingsScroll.Controls.Add(scheduleHintLabel);
            settingsScroll.Controls.Add(scheduleTimeText);
            settingsScroll.Controls.Add(scheduleTimeLabel);
            settingsScroll.Controls.Add(refreshIntervalNumeric);
            settingsScroll.Controls.Add(refreshIntervalLabel);
            settingsScroll.Controls.Add(availabilityTimeoutNumeric);
            settingsScroll.Controls.Add(availabilityTimeoutLabel);
            settingsScroll.Controls.Add(autoUpgradeCheck);
            settingsScroll.Controls.Add(confirmBerthsCheck);
            settingsScroll.Controls.Add(paymentCombo);
            settingsScroll.Controls.Add(paymentLabel);
            settingsScroll.Controls.Add(mobileText);
            settingsScroll.Controls.Add(mobileLabel);
            settingsScroll.Controls.Add(passwordText);
            settingsScroll.Controls.Add(passwordLabel);
            settingsScroll.Controls.Add(usernameText);
            settingsScroll.Controls.Add(usernameLabel);
            settingsScroll.Dock = DockStyle.Fill;
            settingsScroll.Location = new Point(3, 3);
            settingsScroll.Name = "settingsScroll";
            settingsScroll.Padding = new Padding(30);
            settingsScroll.Size = new Size(1266, 639);
            settingsScroll.TabIndex = 0;
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            usernameLabel.Location = new Point(33, 30);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(140, 23);
            usernameLabel.TabIndex = 0;
            usernameLabel.Text = "IRCTC Username";
            // 
            // usernameText
            // 
            usernameText.Location = new Point(33, 56);
            usernameText.Name = "usernameText";
            usernameText.Size = new Size(320, 27);
            usernameText.TabIndex = 1;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            passwordLabel.Location = new Point(33, 95);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(135, 23);
            passwordLabel.TabIndex = 2;
            passwordLabel.Text = "IRCTC Password";
            // 
            // passwordText
            // 
            passwordText.Location = new Point(33, 121);
            passwordText.Name = "passwordText";
            passwordText.PasswordChar = '*';
            passwordText.Size = new Size(320, 27);
            passwordText.TabIndex = 3;
            // 
            // securityHintLabel
            // 
            securityHintLabel.AutoSize = true;
            securityHintLabel.ForeColor = Color.DimGray;
            securityHintLabel.Location = new Point(33, 152);
            securityHintLabel.Name = "securityHintLabel";
            securityHintLabel.Size = new Size(480, 20);
            securityHintLabel.TabIndex = 4;
            securityHintLabel.Text = "Password is encrypted with Windows DPAPI when saved to config.json.";
            // 
            // mobileLabel
            // 
            mobileLabel.AutoSize = true;
            mobileLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            mobileLabel.Location = new Point(33, 190);
            mobileLabel.Name = "mobileLabel";
            mobileLabel.Size = new Size(130, 23);
            mobileLabel.TabIndex = 5;
            mobileLabel.Text = "Mobile Number";
            // 
            // mobileText
            // 
            mobileText.Location = new Point(33, 216);
            mobileText.MaxLength = 10;
            mobileText.Name = "mobileText";
            mobileText.Size = new Size(200, 27);
            mobileText.TabIndex = 6;
            // 
            // paymentLabel
            // 
            paymentLabel.AutoSize = true;
            paymentLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            paymentLabel.Location = new Point(33, 260);
            paymentLabel.Name = "paymentLabel";
            paymentLabel.Size = new Size(135, 23);
            paymentLabel.TabIndex = 7;
            paymentLabel.Text = "Payment Method";
            // 
            // paymentCombo
            // 
            paymentCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            paymentCombo.FormattingEnabled = true;
            paymentCombo.Location = new Point(33, 286);
            paymentCombo.Name = "paymentCombo";
            paymentCombo.Size = new Size(420, 28);
            paymentCombo.TabIndex = 8;
            // 
            // confirmBerthsCheck
            // 
            confirmBerthsCheck.AutoSize = true;
            confirmBerthsCheck.Location = new Point(33, 335);
            confirmBerthsCheck.Name = "confirmBerthsCheck";
            confirmBerthsCheck.Size = new Size(280, 24);
            confirmBerthsCheck.TabIndex = 9;
            confirmBerthsCheck.Text = "Book only if confirm berths (no RAC/WL)";
            confirmBerthsCheck.UseVisualStyleBackColor = true;
            // 
            // autoUpgradeCheck
            // 
            autoUpgradeCheck.AutoSize = true;
            autoUpgradeCheck.Location = new Point(33, 365);
            autoUpgradeCheck.Name = "autoUpgradeCheck";
            autoUpgradeCheck.Size = new Size(220, 24);
            autoUpgradeCheck.TabIndex = 10;
            autoUpgradeCheck.Text = "Consider for auto upgradation";
            autoUpgradeCheck.UseVisualStyleBackColor = true;
            // 
            // refreshIntervalLabel
            // 
            refreshIntervalLabel.AutoSize = true;
            refreshIntervalLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            refreshIntervalLabel.Location = new Point(33, 410);
            refreshIntervalLabel.Name = "refreshIntervalLabel";
            refreshIntervalLabel.Size = new Size(250, 23);
            refreshIntervalLabel.TabIndex = 11;
            refreshIntervalLabel.Text = "Availability refresh interval (ms)";
            // 
            // refreshIntervalNumeric
            // 
            refreshIntervalNumeric.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            refreshIntervalNumeric.Location = new Point(33, 436);
            refreshIntervalNumeric.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            refreshIntervalNumeric.Minimum = new decimal(new int[] { 500, 0, 0, 0 });
            refreshIntervalNumeric.Name = "refreshIntervalNumeric";
            refreshIntervalNumeric.Size = new Size(120, 27);
            refreshIntervalNumeric.TabIndex = 12;
            refreshIntervalNumeric.Value = new decimal(new int[] { 1500, 0, 0, 0 });
            // 
            // availabilityTimeoutLabel
            // 
            availabilityTimeoutLabel.AutoSize = true;
            availabilityTimeoutLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            availabilityTimeoutLabel.Location = new Point(200, 410);
            availabilityTimeoutLabel.Name = "availabilityTimeoutLabel";
            availabilityTimeoutLabel.Size = new Size(230, 23);
            availabilityTimeoutLabel.TabIndex = 13;
            availabilityTimeoutLabel.Text = "Availability timeout (seconds)";
            // 
            // availabilityTimeoutNumeric
            // 
            availabilityTimeoutNumeric.Location = new Point(200, 436);
            availabilityTimeoutNumeric.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            availabilityTimeoutNumeric.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
            availabilityTimeoutNumeric.Name = "availabilityTimeoutNumeric";
            availabilityTimeoutNumeric.Size = new Size(120, 27);
            availabilityTimeoutNumeric.TabIndex = 14;
            availabilityTimeoutNumeric.Value = new decimal(new int[] { 120, 0, 0, 0 });
            // 
            // scheduleTimeLabel
            // 
            scheduleTimeLabel.AutoSize = true;
            scheduleTimeLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            scheduleTimeLabel.Location = new Point(33, 485);
            scheduleTimeLabel.Name = "scheduleTimeLabel";
            scheduleTimeLabel.Size = new Size(280, 23);
            scheduleTimeLabel.TabIndex = 15;
            scheduleTimeLabel.Text = "Scheduled Search Time (HH:mm:ss)";
            // 
            // scheduleTimeText
            // 
            scheduleTimeText.Location = new Point(33, 511);
            scheduleTimeText.Name = "scheduleTimeText";
            scheduleTimeText.PlaceholderText = "e.g. 09:59:55 (empty = immediate)";
            scheduleTimeText.Size = new Size(220, 27);
            scheduleTimeText.TabIndex = 16;
            // 
            // scheduleHintLabel
            // 
            scheduleHintLabel.AutoSize = true;
            scheduleHintLabel.ForeColor = Color.DimGray;
            scheduleHintLabel.Location = new Point(33, 545);
            scheduleHintLabel.Name = "scheduleHintLabel";
            scheduleHintLabel.Size = new Size(520, 20);
            scheduleHintLabel.TabIndex = 17;
            scheduleHintLabel.Text = "For Tatkal: set quota TQ, class, time ~09:59:55 / 10:59:55, then Book before that time.";
            // 
            // saveSettingsButton
            // 
            saveSettingsButton.BackColor = Color.FromArgb(46, 204, 113);
            saveSettingsButton.FlatAppearance.BorderSize = 0;
            saveSettingsButton.FlatStyle = FlatStyle.Flat;
            saveSettingsButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            saveSettingsButton.ForeColor = Color.White;
            saveSettingsButton.Location = new Point(33, 585);
            saveSettingsButton.Name = "saveSettingsButton";
            saveSettingsButton.Size = new Size(180, 42);
            saveSettingsButton.TabIndex = 18;
            saveSettingsButton.Text = "Save Settings";
            saveSettingsButton.UseVisualStyleBackColor = false;
            saveSettingsButton.Click += SaveSettingsButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 720);
            Controls.Add(tabControl);
            Controls.Add(statusLabel);
            MinimumSize = new Size(1100, 600);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Train Booking Automation";
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            tabControl.ResumeLayout(false);
            tabRun.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trainGrid).EndInit();
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            tabPassengers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)passengerGrid).EndInit();
            tabSettings.ResumeLayout(false);
            settingsScroll.ResumeLayout(false);
            settingsScroll.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)refreshIntervalNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)availabilityTimeoutNumeric).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label statusLabel;
        private TabControl tabControl;
        private TabPage tabRun;
        private TabPage tabPassengers;
        private TabPage tabSettings;
        private Panel searchPanel;
        private Button searchButton;
        private DateTimePicker travelDatePicker;
        private Label dateLabel;
        private ComboBox toStationCombo;
        private Label toLabel;
        private ComboBox fromStationCombo;
        private Label fromLabel;
        private ComboBox quotaCombo;
        private Label quotaLabel;
        private ComboBox classCombo;
        private Label classLabel;
        private DataGridView trainGrid;
        private Panel settingsScroll;
        private TextBox passwordText;
        private Label passwordLabel;
        private TextBox usernameText;
        private Label usernameLabel;
        private Label securityHintLabel;
        private TextBox mobileText;
        private Label mobileLabel;
        private ComboBox paymentCombo;
        private Label paymentLabel;
        private CheckBox confirmBerthsCheck;
        private CheckBox autoUpgradeCheck;
        private NumericUpDown refreshIntervalNumeric;
        private Label refreshIntervalLabel;
        private NumericUpDown availabilityTimeoutNumeric;
        private Label availabilityTimeoutLabel;
        private TextBox scheduleTimeText;
        private Label scheduleTimeLabel;
        private Label scheduleHintLabel;
        private Button saveSettingsButton;
        private DataGridView passengerGrid;
        private Button bookIrctcButton;
        private Button stopButton;
    }
}
