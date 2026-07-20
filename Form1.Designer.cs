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
            searchButton = new Button();
            travelDatePicker = new DateTimePicker();
            dateLabel = new Label();
            toStationCombo = new ComboBox();
            toLabel = new Label();
            fromStationCombo = new ComboBox();
            fromLabel = new Label();
            bookIrctcButton = new Button();
            stopButton = new Button();
            tabPassengers = new TabPage();
            passengerGrid = new DataGridView();
            tabSettings = new TabPage();
            saveSettingsButton = new Button();
            passwordText = new TextBox();
            passwordLabel = new Label();
            usernameText = new TextBox();
            usernameLabel = new Label();
            tabControl.SuspendLayout();
            tabRun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trainGrid).BeginInit();
            searchPanel.SuspendLayout();
            tabPassengers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)passengerGrid).BeginInit();
            tabSettings.SuspendLayout();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.Dock = DockStyle.Top;
            statusLabel.Font = new Font("Segoe UI", 10F);
            statusLabel.Location = new Point(0, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Padding = new Padding(12, 10, 12, 10);
            statusLabel.Size = new Size(1184, 42);
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
            tabControl.Size = new Size(1184, 619);
            tabControl.TabIndex = 1;
            // 
            // tabRun
            // 
            tabRun.Controls.Add(trainGrid);
            tabRun.Controls.Add(searchPanel);
            tabRun.Location = new Point(4, 29);
            tabRun.Name = "tabRun";
            tabRun.Padding = new Padding(3);
            tabRun.Size = new Size(1176, 586);
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
            trainGrid.Location = new Point(3, 59);
            trainGrid.MultiSelect = false;
            trainGrid.Name = "trainGrid";
            trainGrid.ReadOnly = true;
            trainGrid.RowHeadersVisible = false;
            trainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            trainGrid.Size = new Size(1170, 524);
            trainGrid.TabIndex = 3;
            // 
            // searchPanel
            // 
            searchPanel.Controls.Add(stopButton);
            searchPanel.Controls.Add(bookIrctcButton);
            searchPanel.Controls.Add(searchButton);
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
            searchPanel.Size = new Size(1170, 60);
            searchPanel.TabIndex = 2;
            searchPanel.BackColor = Color.WhiteSmoke;
            // 
            // searchButton
            // 
            searchButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            searchButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            searchButton.Location = new Point(870, 12);
            searchButton.Name = "searchButton";
            searchButton.Size = new Size(130, 34);
            searchButton.TabIndex = 6;
            searchButton.Text = "Search Trains";
            searchButton.UseVisualStyleBackColor = false;
            searchButton.BackColor = Color.DodgerBlue;
            searchButton.ForeColor = Color.White;
            searchButton.FlatStyle = FlatStyle.Flat;
            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.Click += SearchButton_Click;
            // 
            // travelDatePicker
            // 
            travelDatePicker.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            travelDatePicker.Format = DateTimePickerFormat.Short;
            travelDatePicker.Location = new Point(730, 15);
            travelDatePicker.MinDate = new DateTime(2023, 1, 1, 0, 0, 0, 0);
            travelDatePicker.Name = "travelDatePicker";
            travelDatePicker.Size = new Size(120, 27);
            travelDatePicker.TabIndex = 5;
            // 
            // dateLabel
            // 
            dateLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            dateLabel.AutoSize = true;
            dateLabel.Location = new Point(680, 18);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(42, 20);
            dateLabel.TabIndex = 4;
            dateLabel.Text = "Date:";
            // 
            // toStationCombo
            // 
            toStationCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            toStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            toStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            toStationCombo.FormattingEnabled = true;
            toStationCombo.Location = new Point(410, 15);
            toStationCombo.Name = "toStationCombo";
            toStationCombo.Size = new Size(250, 28);
            toStationCombo.TabIndex = 3;
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(370, 18);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(28, 20);
            toLabel.TabIndex = 2;
            toLabel.Text = "To:";
            // 
            // fromStationCombo
            // 
            fromStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            fromStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            fromStationCombo.FormattingEnabled = true;
            fromStationCombo.Location = new Point(60, 15);
            fromStationCombo.Name = "fromStationCombo";
            fromStationCombo.Size = new Size(290, 28);
            fromStationCombo.TabIndex = 1;
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
            // tabPassengers
            // 
            tabPassengers.Controls.Add(passengerGrid);
            tabPassengers.Location = new Point(4, 29);
            tabPassengers.Name = "tabPassengers";
            tabPassengers.Padding = new Padding(3);
            tabPassengers.Size = new Size(1176, 586);
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
            passengerGrid.Size = new Size(1170, 580);
            passengerGrid.TabIndex = 0;
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(saveSettingsButton);
            tabSettings.Controls.Add(passwordText);
            tabSettings.Controls.Add(passwordLabel);
            tabSettings.Controls.Add(usernameText);
            tabSettings.Controls.Add(usernameLabel);
            tabSettings.Location = new Point(4, 29);
            tabSettings.Name = "tabSettings";
            tabSettings.Padding = new Padding(30);
            tabSettings.Size = new Size(1176, 586);
            tabSettings.TabIndex = 2;
            tabSettings.Text = "IRCTC Settings";
            tabSettings.UseVisualStyleBackColor = true;
            // 
            // saveSettingsButton
            // 
            saveSettingsButton.Location = new Point(33, 190);
            saveSettingsButton.Name = "saveSettingsButton";
            saveSettingsButton.Size = new Size(180, 42);
            saveSettingsButton.TabIndex = 4;
            saveSettingsButton.Text = "Save Settings";
            saveSettingsButton.UseVisualStyleBackColor = false;
            saveSettingsButton.BackColor = Color.FromArgb(46, 204, 113);
            saveSettingsButton.ForeColor = Color.White;
            saveSettingsButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            saveSettingsButton.FlatStyle = FlatStyle.Flat;
            saveSettingsButton.FlatAppearance.BorderSize = 0;
            saveSettingsButton.Click += SaveSettingsButton_Click;
            // 
            // passwordText
            // 
            passwordText.Location = new Point(33, 126);
            passwordText.Name = "passwordText";
            passwordText.PasswordChar = '*';
            passwordText.Size = new Size(300, 27);
            passwordText.TabIndex = 3;
            // 
            // usernameText
            // 
            usernameText.Location = new Point(33, 63);
            usernameText.Name = "usernameText";
            usernameText.Size = new Size(300, 27);
            usernameText.TabIndex = 1;
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            usernameLabel.Location = new Point(33, 40);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(75, 20);
            usernameLabel.TabIndex = 0;
            usernameLabel.Text = "IRCTC Username";
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            passwordLabel.Location = new Point(33, 103);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(70, 20);
            passwordLabel.TabIndex = 2;
            passwordLabel.Text = "IRCTC Password";
            // 
            // bookIrctcButton
            // 
            bookIrctcButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            bookIrctcButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            bookIrctcButton.Location = new Point(1010, 12);
            bookIrctcButton.Name = "bookIrctcButton";
            bookIrctcButton.Size = new Size(150, 34);
            bookIrctcButton.TabIndex = 7;
            bookIrctcButton.Text = "Book on IRCTC";
            bookIrctcButton.UseVisualStyleBackColor = false;
            bookIrctcButton.BackColor = Color.FromArgb(231, 76, 60);
            bookIrctcButton.ForeColor = Color.White;
            bookIrctcButton.FlatStyle = FlatStyle.Flat;
            bookIrctcButton.FlatAppearance.BorderSize = 0;
            bookIrctcButton.Click += BookIrctcButton_Click;
            // 
            // stopButton
            // 
            stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            stopButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            stopButton.Location = new Point(1168, 12);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(34, 34);
            stopButton.TabIndex = 8;
            stopButton.Text = "⏹";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.BackColor = Color.FromArgb(80, 80, 80);
            stopButton.ForeColor = Color.White;
            stopButton.FlatStyle = FlatStyle.Flat;
            stopButton.FlatAppearance.BorderSize = 0;
            stopButton.Enabled = false;
            stopButton.Click += StopButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 661);
            Controls.Add(tabControl);
            Controls.Add(statusLabel);
            MinimumSize = new Size(900, 500);
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
            tabSettings.PerformLayout();
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
        private DataGridView trainGrid;
        private TextBox passwordText;
        private Label passwordLabel;
        private TextBox usernameText;
        private Label usernameLabel;
        private Button saveSettingsButton;
        private DataGridView passengerGrid;
        private Button bookIrctcButton;
        private Button stopButton;
    }
}
