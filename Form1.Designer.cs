namespace train_automation
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components is not null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            titlePanel = new Panel();
            titleLabel = new Label();
            fromLabel = new Label();
            fromStationCombo = new ComboBox();
            toLabel = new Label();
            toStationCombo = new ComboBox();
            dateLabel = new Label();
            travelDatePicker = new DateTimePicker();
            findButton = new Button();
            bdgPtLabel = new Label();
            boardingPointText = new TextBox();
            trainNoLabel = new Label();
            trainNoText = new TextBox();
            trainTypeLabel = new Label();
            trainTypeCombo = new ComboBox();
            availabilityLink = new LinkLabel();
            classLabel = new Label();
            classCombo = new ComboBox();
            quotaGeneralRadio = new RadioButton();
            quotaLadiesRadio = new RadioButton();
            quotaTatkalRadio = new RadioButton();
            quotaPremiumRadio = new RadioButton();
            trainListPanel = new Panel();
            fareGrid = new DataGridView();
            availabilityGrid = new DataGridView();
            trainListHeader = new Label();
            passengerGrid = new DataGridView();
            mobileLabel = new Label();
            mobileText = new TextBox();
            fareLabel = new Label();
            fareText = new TextBox();
            getFareButton = new Button();
            ticketSlotLabel = new Label();
            ticketSlotCombo = new ComboBox();
            gatewayLabel = new Label();
            gatewayCombo = new ComboBox();
            priorBankLabel = new Label();
            priorBankCombo = new ComboBox();
            backupBankLabel = new Label();
            backupBankCombo = new ComboBox();
            autoUpgradeCheck = new CheckBox();
            confirmBerthsCheck = new CheckBox();
            ticketNameLabel = new Label();
            ticketNameText = new TextBox();
            saveButton = new Button();
            bookIrctcButton = new Button();
            stopButton = new Button();
            irctcUserLabel = new Label();
            irctcUserText = new TextBox();
            irctcPassLabel = new Label();
            irctcPassText = new TextBox();
            statusLabel = new Label();
            titlePanel.SuspendLayout();
            trainListPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fareGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)availabilityGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)passengerGrid).BeginInit();
            SuspendLayout();
            // 
            // titlePanel
            // 
            titlePanel.BackColor = Color.FromArgb(180, 160, 220);
            titlePanel.Controls.Add(titleLabel);
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Location = new Point(0, 0);
            titlePanel.Name = "titlePanel";
            titlePanel.Size = new Size(780, 32);
            titlePanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            titleLabel.Location = new Point(8, 5);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(100, 23);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "New Ticket";
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(10, 42);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(43, 20);
            fromLabel.TabIndex = 1;
            fromLabel.Text = "From";
            // 
            // fromStationCombo
            // 
            fromStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            fromStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            fromStationCombo.FormattingEnabled = true;
            fromStationCombo.Location = new Point(58, 38);
            fromStationCombo.Name = "fromStationCombo";
            fromStationCombo.Size = new Size(142, 28);
            fromStationCombo.TabIndex = 2;
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(203, 42);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(25, 20);
            toLabel.TabIndex = 3;
            toLabel.Text = "To";
            // 
            // toStationCombo
            // 
            toStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            toStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            toStationCombo.FormattingEnabled = true;
            toStationCombo.Location = new Point(233, 38);
            toStationCombo.Name = "toStationCombo";
            toStationCombo.Size = new Size(169, 28);
            toStationCombo.TabIndex = 4;
            // 
            // dateLabel
            // 
            dateLabel.AutoSize = true;
            dateLabel.Location = new Point(402, 42);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(41, 20);
            dateLabel.TabIndex = 5;
            dateLabel.Text = "Date";
            // 
            // travelDatePicker
            // 
            travelDatePicker.Format = DateTimePickerFormat.Short;
            travelDatePicker.Location = new Point(448, 38);
            travelDatePicker.MinDate = new DateTime(2026, 7, 9, 0, 0, 0, 0);
            travelDatePicker.Name = "travelDatePicker";
            travelDatePicker.Size = new Size(120, 27);
            travelDatePicker.TabIndex = 6;
            // 
            // findButton
            // 
            findButton.Location = new Point(578, 36);
            findButton.Name = "findButton";
            findButton.Size = new Size(70, 30);
            findButton.TabIndex = 7;
            findButton.Text = "Find";
            findButton.UseVisualStyleBackColor = true;
            findButton.Click += FindButton_Click;
            // 
            // bdgPtLabel
            // 
            bdgPtLabel.AutoSize = true;
            bdgPtLabel.Location = new Point(10, 76);
            bdgPtLabel.Name = "bdgPtLabel";
            bdgPtLabel.Size = new Size(53, 20);
            bdgPtLabel.TabIndex = 8;
            bdgPtLabel.Text = "Bdg Pt";
            // 
            // boardingPointText
            // 
            boardingPointText.Location = new Point(68, 72);
            boardingPointText.Name = "boardingPointText";
            boardingPointText.ReadOnly = true;
            boardingPointText.Size = new Size(80, 27);
            boardingPointText.TabIndex = 9;
            // 
            // trainNoLabel
            // 
            trainNoLabel.AutoSize = true;
            trainNoLabel.Location = new Point(158, 76);
            trainNoLabel.Name = "trainNoLabel";
            trainNoLabel.Size = new Size(65, 20);
            trainNoLabel.TabIndex = 10;
            trainNoLabel.Text = "Train No";
            // 
            // trainNoText
            // 
            trainNoText.Location = new Point(230, 72);
            trainNoText.Name = "trainNoText";
            trainNoText.ReadOnly = true;
            trainNoText.Size = new Size(90, 27);
            trainNoText.TabIndex = 11;
            // 
            // trainTypeLabel
            // 
            trainTypeLabel.AutoSize = true;
            trainTypeLabel.Location = new Point(330, 76);
            trainTypeLabel.Name = "trainTypeLabel";
            trainTypeLabel.Size = new Size(76, 20);
            trainTypeLabel.TabIndex = 12;
            trainTypeLabel.Text = "Train Type";
            // 
            // trainTypeCombo
            // 
            trainTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            trainTypeCombo.FormattingEnabled = true;
            trainTypeCombo.Location = new Point(412, 72);
            trainTypeCombo.Name = "trainTypeCombo";
            trainTypeCombo.Size = new Size(150, 28);
            trainTypeCombo.TabIndex = 13;
            // 
            // availabilityLink
            // 
            availabilityLink.AutoSize = true;
            availabilityLink.Location = new Point(572, 76);
            availabilityLink.Name = "availabilityLink";
            availabilityLink.Size = new Size(83, 20);
            availabilityLink.TabIndex = 14;
            availabilityLink.TabStop = true;
            availabilityLink.Text = "Availability";
            availabilityLink.LinkClicked += AvailabilityLink_LinkClicked;
            // 
            // classLabel
            // 
            classLabel.AutoSize = true;
            classLabel.Location = new Point(10, 110);
            classLabel.Name = "classLabel";
            classLabel.Size = new Size(42, 20);
            classLabel.TabIndex = 15;
            classLabel.Text = "Class";
            // 
            // classCombo
            // 
            classCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            classCombo.FormattingEnabled = true;
            classCombo.Location = new Point(68, 106);
            classCombo.Name = "classCombo";
            classCombo.Size = new Size(180, 28);
            classCombo.TabIndex = 16;
            // 
            // quotaGeneralRadio
            // 
            quotaGeneralRadio.AutoSize = true;
            quotaGeneralRadio.Checked = true;
            quotaGeneralRadio.Location = new Point(270, 108);
            quotaGeneralRadio.Name = "quotaGeneralRadio";
            quotaGeneralRadio.Size = new Size(81, 24);
            quotaGeneralRadio.TabIndex = 17;
            quotaGeneralRadio.TabStop = true;
            quotaGeneralRadio.Text = "General";
            quotaGeneralRadio.UseVisualStyleBackColor = true;
            quotaGeneralRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            // 
            // quotaLadiesRadio
            // 
            quotaLadiesRadio.AutoSize = true;
            quotaLadiesRadio.Location = new Point(360, 108);
            quotaLadiesRadio.Name = "quotaLadiesRadio";
            quotaLadiesRadio.Size = new Size(72, 24);
            quotaLadiesRadio.TabIndex = 18;
            quotaLadiesRadio.Text = "Ladies";
            quotaLadiesRadio.UseVisualStyleBackColor = true;
            quotaLadiesRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            // 
            // quotaTatkalRadio
            // 
            quotaTatkalRadio.AutoSize = true;
            quotaTatkalRadio.Location = new Point(440, 108);
            quotaTatkalRadio.Name = "quotaTatkalRadio";
            quotaTatkalRadio.Size = new Size(68, 24);
            quotaTatkalRadio.TabIndex = 19;
            quotaTatkalRadio.Text = "Tatkal";
            quotaTatkalRadio.UseVisualStyleBackColor = true;
            quotaTatkalRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            // 
            // quotaPremiumRadio
            // 
            quotaPremiumRadio.AutoSize = true;
            quotaPremiumRadio.Location = new Point(520, 108);
            quotaPremiumRadio.Name = "quotaPremiumRadio";
            quotaPremiumRadio.Size = new Size(131, 24);
            quotaPremiumRadio.TabIndex = 20;
            quotaPremiumRadio.Text = "Premium Tatkal";
            quotaPremiumRadio.UseVisualStyleBackColor = true;
            quotaPremiumRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            // 
            // trainListPanel
            // 
            trainListPanel.Controls.Add(availabilityGrid);
            trainListPanel.Controls.Add(fareGrid);
            trainListPanel.Controls.Add(trainListHeader);
            trainListPanel.Location = new Point(10, 142);
            trainListPanel.Name = "trainListPanel";
            trainListPanel.Size = new Size(710, 150);
            trainListPanel.TabIndex = 21;
            // 
            // fareGrid
            // 
            fareGrid.AllowUserToAddRows = false;
            fareGrid.AllowUserToDeleteRows = false;
            fareGrid.BackgroundColor = SystemColors.Window;
            fareGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            fareGrid.Dock = DockStyle.Top;
            fareGrid.Location = new Point(0, 30);
            fareGrid.Name = "fareGrid";
            fareGrid.ReadOnly = true;
            fareGrid.RowHeadersVisible = false;
            fareGrid.RowTemplate.Height = 28;
            fareGrid.Size = new Size(710, 58);
            fareGrid.TabIndex = 2;
            // 
            // availabilityGrid
            // 
            availabilityGrid.AllowUserToAddRows = false;
            availabilityGrid.AllowUserToDeleteRows = false;
            availabilityGrid.BackgroundColor = SystemColors.Window;
            availabilityGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            availabilityGrid.Dock = DockStyle.Fill;
            availabilityGrid.Location = new Point(0, 88);
            availabilityGrid.Name = "availabilityGrid";
            availabilityGrid.ReadOnly = true;
            availabilityGrid.RowHeadersVisible = false;
            availabilityGrid.Size = new Size(710, 62);
            availabilityGrid.TabIndex = 1;
            // 
            // trainListHeader
            // 
            trainListHeader.BackColor = Color.FromArgb(180, 160, 220);
            trainListHeader.Dock = DockStyle.Top;
            trainListHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            trainListHeader.Location = new Point(0, 0);
            trainListHeader.Name = "trainListHeader";
            trainListHeader.Padding = new Padding(8, 6, 8, 6);
            trainListHeader.Size = new Size(710, 30);
            trainListHeader.TabIndex = 0;
            trainListHeader.Text = "Fare & Availability — select a class from Train List popup";
            // 
            // passengerGrid
            // 
            passengerGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            passengerGrid.Location = new Point(10, 302);
            passengerGrid.Name = "passengerGrid";
            passengerGrid.RowHeadersVisible = false;
            passengerGrid.RowHeadersWidth = 51;
            passengerGrid.Size = new Size(710, 170);
            passengerGrid.TabIndex = 22;
            // 
            // mobileLabel
            // 
            mobileLabel.AutoSize = true;
            mobileLabel.Location = new Point(10, 484);
            mobileLabel.Name = "mobileLabel";
            mobileLabel.Size = new Size(86, 20);
            mobileLabel.TabIndex = 23;
            mobileLabel.Text = "Mobile +91";
            // 
            // mobileText
            // 
            mobileText.Location = new Point(94, 480);
            mobileText.MaxLength = 10;
            mobileText.Name = "mobileText";
            mobileText.Size = new Size(120, 27);
            mobileText.TabIndex = 24;
            // 
            // fareLabel
            // 
            fareLabel.AutoSize = true;
            fareLabel.Location = new Point(230, 484);
            fareLabel.Name = "fareLabel";
            fareLabel.Size = new Size(36, 20);
            fareLabel.TabIndex = 25;
            fareLabel.Text = "Fare";
            // 
            // fareText
            // 
            fareText.Location = new Point(272, 480);
            fareText.Name = "fareText";
            fareText.ReadOnly = true;
            fareText.Size = new Size(80, 27);
            fareText.TabIndex = 26;
            fareText.Text = "0";
            // 
            // getFareButton
            // 
            getFareButton.Location = new Point(360, 478);
            getFareButton.Name = "getFareButton";
            getFareButton.Size = new Size(80, 30);
            getFareButton.TabIndex = 27;
            getFareButton.Text = "Get Fare";
            getFareButton.UseVisualStyleBackColor = true;
            getFareButton.Click += GetFareButton_Click;
            // 
            // ticketSlotLabel
            // 
            ticketSlotLabel.AutoSize = true;
            ticketSlotLabel.Location = new Point(10, 518);
            ticketSlotLabel.Name = "ticketSlotLabel";
            ticketSlotLabel.Size = new Size(78, 20);
            ticketSlotLabel.TabIndex = 28;
            ticketSlotLabel.Text = "Ticket Slot";
            // 
            // ticketSlotCombo
            // 
            ticketSlotCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            ticketSlotCombo.FormattingEnabled = true;
            ticketSlotCombo.Location = new Point(94, 514);
            ticketSlotCombo.Name = "ticketSlotCombo";
            ticketSlotCombo.Size = new Size(150, 28);
            ticketSlotCombo.TabIndex = 29;
            // 
            // gatewayLabel
            // 
            gatewayLabel.AutoSize = true;
            gatewayLabel.Location = new Point(260, 518);
            gatewayLabel.Name = "gatewayLabel";
            gatewayLabel.Size = new Size(72, 20);
            gatewayLabel.TabIndex = 30;
            gatewayLabel.Text = "Gateways";
            // 
            // gatewayCombo
            // 
            gatewayCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            gatewayCombo.FormattingEnabled = true;
            gatewayCombo.Location = new Point(338, 514);
            gatewayCombo.Name = "gatewayCombo";
            gatewayCombo.Size = new Size(170, 28);
            gatewayCombo.TabIndex = 31;
            // 
            // priorBankLabel
            // 
            priorBankLabel.AutoSize = true;
            priorBankLabel.Location = new Point(10, 552);
            priorBankLabel.Name = "priorBankLabel";
            priorBankLabel.Size = new Size(76, 20);
            priorBankLabel.TabIndex = 32;
            priorBankLabel.Text = "Prior Bank";
            // 
            // priorBankCombo
            // 
            priorBankCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            priorBankCombo.FormattingEnabled = true;
            priorBankCombo.Location = new Point(94, 548);
            priorBankCombo.Name = "priorBankCombo";
            priorBankCombo.Size = new Size(200, 28);
            priorBankCombo.TabIndex = 33;
            // 
            // backupBankLabel
            // 
            backupBankLabel.AutoSize = true;
            backupBankLabel.Location = new Point(310, 552);
            backupBankLabel.Name = "backupBankLabel";
            backupBankLabel.Size = new Size(95, 20);
            backupBankLabel.TabIndex = 34;
            backupBankLabel.Text = "BackUp Bank";
            // 
            // backupBankCombo
            // 
            backupBankCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            backupBankCombo.FormattingEnabled = true;
            backupBankCombo.Location = new Point(408, 548);
            backupBankCombo.Name = "backupBankCombo";
            backupBankCombo.Size = new Size(200, 28);
            backupBankCombo.TabIndex = 35;
            // 
            // autoUpgradeCheck
            // 
            autoUpgradeCheck.AutoSize = true;
            autoUpgradeCheck.Checked = true;
            autoUpgradeCheck.CheckState = CheckState.Checked;
            autoUpgradeCheck.Location = new Point(10, 586);
            autoUpgradeCheck.Name = "autoUpgradeCheck";
            autoUpgradeCheck.Size = new Size(236, 24);
            autoUpgradeCheck.TabIndex = 36;
            autoUpgradeCheck.Text = "Consider for Auto Upgradation";
            autoUpgradeCheck.UseVisualStyleBackColor = true;
            // 
            // confirmBerthsCheck
            // 
            confirmBerthsCheck.AutoSize = true;
            confirmBerthsCheck.Checked = false;
            confirmBerthsCheck.CheckState = CheckState.Unchecked;
            confirmBerthsCheck.Location = new Point(250, 586);
            confirmBerthsCheck.Name = "confirmBerthsCheck";
            confirmBerthsCheck.Size = new Size(269, 24);
            confirmBerthsCheck.TabIndex = 37;
            confirmBerthsCheck.Text = "Book only if confirm berths allotted.";
            confirmBerthsCheck.UseVisualStyleBackColor = true;
            // 
            // ticketNameLabel
            // 
            ticketNameLabel.AutoSize = true;
            ticketNameLabel.Location = new Point(10, 618);
            ticketNameLabel.Name = "ticketNameLabel";
            ticketNameLabel.Size = new Size(52, 20);
            ticketNameLabel.TabIndex = 38;
            ticketNameLabel.Text = "Name:";
            // 
            // ticketNameText
            // 
            ticketNameText.Location = new Point(68, 614);
            ticketNameText.Name = "ticketNameText";
            ticketNameText.Size = new Size(180, 27);
            ticketNameText.TabIndex = 39;
            // 
            // irctcUserLabel
            // 
            irctcUserLabel.AutoSize = true;
            irctcUserLabel.Location = new Point(480, 518);
            irctcUserLabel.Name = "irctcUserLabel";
            irctcUserLabel.Size = new Size(82, 20);
            irctcUserLabel.TabIndex = 42;
            irctcUserLabel.Text = "IRCTC User";
            // 
            // irctcUserText
            // 
            irctcUserText.Location = new Point(568, 514);
            irctcUserText.Name = "irctcUserText";
            irctcUserText.Size = new Size(150, 27);
            irctcUserText.TabIndex = 43;
            // 
            // irctcPassLabel
            // 
            irctcPassLabel.AutoSize = true;
            irctcPassLabel.Location = new Point(480, 550);
            irctcPassLabel.Name = "irctcPassLabel";
            irctcPassLabel.Size = new Size(70, 20);
            irctcPassLabel.TabIndex = 44;
            irctcPassLabel.Text = "Password";
            // 
            // irctcPassText
            // 
            irctcPassText.Location = new Point(568, 546);
            irctcPassText.Name = "irctcPassText";
            irctcPassText.PasswordChar = '*';
            irctcPassText.Size = new Size(150, 27);
            irctcPassText.TabIndex = 45;
            // 
            // saveButton
            // 
            saveButton.BackColor = Color.DimGray;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.ForeColor = Color.White;
            saveButton.Location = new Point(520, 610);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(80, 36);
            saveButton.TabIndex = 40;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = false;
            saveButton.Click += SaveButton_Click;
            // 
            // bookIrctcButton
            // 
            bookIrctcButton.BackColor = Color.FromArgb(0, 120, 215);
            bookIrctcButton.FlatStyle = FlatStyle.Flat;
            bookIrctcButton.ForeColor = Color.White;
            bookIrctcButton.Location = new Point(608, 610);
            bookIrctcButton.Name = "bookIrctcButton";
            bookIrctcButton.Size = new Size(100, 36);
            bookIrctcButton.TabIndex = 46;
            bookIrctcButton.Text = "Book IRCTC";
            bookIrctcButton.UseVisualStyleBackColor = false;
            bookIrctcButton.Click += BookIrctcButton_Click;
            // 
            // stopButton
            // 
            stopButton.BackColor = Color.IndianRed;
            stopButton.Enabled = false;
            stopButton.FlatStyle = FlatStyle.Flat;
            stopButton.ForeColor = Color.White;
            stopButton.Location = new Point(716, 610);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(50, 36);
            stopButton.TabIndex = 47;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.Click += StopButton_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.ForeColor = Color.DimGray;
            statusLabel.Location = new Point(10, 622);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(179, 20);
            statusLabel.TabIndex = 41;
            statusLabel.Text = "Click Find to search trains.";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(780, 660);
            Controls.Add(statusLabel);
            Controls.Add(stopButton);
            Controls.Add(bookIrctcButton);
            Controls.Add(saveButton);
            Controls.Add(irctcPassText);
            Controls.Add(irctcPassLabel);
            Controls.Add(irctcUserText);
            Controls.Add(irctcUserLabel);
            Controls.Add(ticketNameText);
            Controls.Add(ticketNameLabel);
            Controls.Add(confirmBerthsCheck);
            Controls.Add(autoUpgradeCheck);
            Controls.Add(backupBankCombo);
            Controls.Add(backupBankLabel);
            Controls.Add(priorBankCombo);
            Controls.Add(priorBankLabel);
            Controls.Add(gatewayCombo);
            Controls.Add(gatewayLabel);
            Controls.Add(ticketSlotCombo);
            Controls.Add(ticketSlotLabel);
            Controls.Add(getFareButton);
            Controls.Add(fareText);
            Controls.Add(fareLabel);
            Controls.Add(mobileText);
            Controls.Add(mobileLabel);
            Controls.Add(passengerGrid);
            Controls.Add(trainListPanel);
            Controls.Add(quotaPremiumRadio);
            Controls.Add(quotaTatkalRadio);
            Controls.Add(quotaLadiesRadio);
            Controls.Add(quotaGeneralRadio);
            Controls.Add(classCombo);
            Controls.Add(classLabel);
            Controls.Add(availabilityLink);
            Controls.Add(trainTypeCombo);
            Controls.Add(trainTypeLabel);
            Controls.Add(trainNoText);
            Controls.Add(trainNoLabel);
            Controls.Add(boardingPointText);
            Controls.Add(bdgPtLabel);
            Controls.Add(findButton);
            Controls.Add(travelDatePicker);
            Controls.Add(dateLabel);
            Controls.Add(toStationCombo);
            Controls.Add(toLabel);
            Controls.Add(fromStationCombo);
            Controls.Add(fromLabel);
            Controls.Add(titlePanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Ticket";
            Load += Form1_Load;
            titlePanel.ResumeLayout(false);
            titlePanel.PerformLayout();
            trainListPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)fareGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)availabilityGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)passengerGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Panel titlePanel;
        private Label titleLabel;
        private Label fromLabel;
        private ComboBox fromStationCombo;
        private Label toLabel;
        private ComboBox toStationCombo;
        private Label dateLabel;
        private DateTimePicker travelDatePicker;
        private Button findButton;
        private Label bdgPtLabel;
        private TextBox boardingPointText;
        private Label trainNoLabel;
        private TextBox trainNoText;
        private Label trainTypeLabel;
        private ComboBox trainTypeCombo;
        private LinkLabel availabilityLink;
        private Label classLabel;
        private ComboBox classCombo;
        private RadioButton quotaGeneralRadio;
        private RadioButton quotaLadiesRadio;
        private RadioButton quotaTatkalRadio;
        private RadioButton quotaPremiumRadio;
        private Panel trainListPanel;
        private Label trainListHeader;
        private DataGridView fareGrid;
        private DataGridView availabilityGrid;
        private DataGridView passengerGrid;
        private Label mobileLabel;
        private TextBox mobileText;
        private Label fareLabel;
        private TextBox fareText;
        private Button getFareButton;
        private Label ticketSlotLabel;
        private ComboBox ticketSlotCombo;
        private Label gatewayLabel;
        private ComboBox gatewayCombo;
        private Label priorBankLabel;
        private ComboBox priorBankCombo;
        private Label backupBankLabel;
        private ComboBox backupBankCombo;
        private CheckBox autoUpgradeCheck;
        private CheckBox confirmBerthsCheck;
        private Label ticketNameLabel;
        private TextBox ticketNameText;
        private Button saveButton;
        private Button bookIrctcButton;
        private Button stopButton;
        private Label irctcUserLabel;
        private TextBox irctcUserText;
        private Label irctcPassLabel;
        private TextBox irctcPassText;
        private Label statusLabel;
    }
}
