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
            var primary = UiTheme.Primary;
            var primaryLight = UiTheme.SurfaceLow;
            var headerText = UiTheme.Text;
            var pageBg = UiTheme.PageBg;
            var borderColor = UiTheme.OutlineVariant;
            var textMuted = UiTheme.TextMuted;
            var danger = UiTheme.Danger;
            var dangerBorder = UiTheme.Danger;

            titlePanel = new Panel();
            titleLabel = new Label();
            titleSubLabel = new Label();
            contentPanel = new Panel();

            journeyCard = new Panel();
            journeyHeader = new Label();
            fromCaption = new Label();
            fromStationCombo = new ComboBox();
            toCaption = new Label();
            toStationCombo = new ComboBox();
            dateCaption = new Label();
            travelDatePicker = new DateTimePicker();
            findButton = new Button();
            bdgCaption = new Label();
            boardingPointText = new TextBox();
            trainNoCaption = new Label();
            trainNoText = new TextBox();
            trainTypeCaption = new Label();
            trainTypeCombo = new ComboBox();
            availabilityLink = new LinkLabel();
            classCaption = new Label();
            classCombo = new ComboBox();
            quotaCaption = new Label();
            quotaGeneralRadio = new RadioButton();
            quotaLadiesRadio = new RadioButton();
            quotaTatkalRadio = new RadioButton();
            quotaPremiumRadio = new RadioButton();



            passengerCard = new Panel();
            passengerHeader = new Label();
            passengerGrid = new DataGridView();

            paymentCard = new Panel();
            paymentHeader = new Label();
            fareCaption = new Label();
            rupeeLabel = new Label();
            fareText = new Label();
            getFareButton = new Button();
            mobileCaption = new Label();
            mobileText = new TextBox();
            ticketSlotCaption = new Label();
            ticketSlotCombo = new ComboBox();
            gatewayCaption = new Label();
            gatewayCombo = new ComboBox();
            priorBankCaption = new Label();
            priorBankCombo = new ComboBox();
            backupBankCaption = new Label();
            backupBankCombo = new ComboBox();
            ticketNameCaption = new Label();
            ticketNameText = new TextBox();

            preferencesCard = new Panel();
            preferencesHeader = new Label();
            prefSeparator1 = new Panel();
            confirmBerthsTitleLabel = new Label();
            confirmBerthsCheck = new ToggleSwitch();
            useBetaViewTitleLabel = new Label();
            useBetaViewCheck = new ToggleSwitch();
            useRealChromeTitleLabel = new Label();
            useRealChromeCheck = new ToggleSwitch();

            userCaption = new Label();
            irctcUserCombo = new ComboBox();

            actionPanel = new Panel();
            actionSeparator = new Panel();
            statusDotLabel = new Label();
            statusLabel = new Label();
            saveButton = new Button();
            bookIrctcButton = new Button();
            stopButton = new Button();

            titlePanel.SuspendLayout();
            contentPanel.SuspendLayout();
            journeyCard.SuspendLayout();
            passengerCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)passengerGrid).BeginInit();
            paymentCard.SuspendLayout();
            preferencesCard.SuspendLayout();
            actionPanel.SuspendLayout();
            SuspendLayout();
            //
            // titlePanel
            //
            titlePanel.BackColor = UiTheme.PageBg;
            titlePanel.Controls.Add(titleLabel);
            titlePanel.Controls.Add(titleSubLabel);
            titlePanel.Dock = DockStyle.Top;
            titlePanel.Location = new Point(0, 0);
            titlePanel.Name = "titlePanel";
            titlePanel.Size = new Size(700, 56);
            titlePanel.TabIndex = 0;
            //
            // titleLabel
            //
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            titleLabel.ForeColor = UiTheme.Text;
            titleLabel.Location = new Point(18, 8);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(120, 27);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "New Ticket";
            //
            // titleSubLabel
            //
            titleSubLabel.AutoSize = true;
            titleSubLabel.Font = new Font("Segoe UI", 9F);
            titleSubLabel.ForeColor = UiTheme.TextMuted;
            titleSubLabel.Location = new Point(19, 34);
            titleSubLabel.Name = "titleSubLabel";
            titleSubLabel.Size = new Size(170, 20);
            titleSubLabel.TabIndex = 1;
            titleSubLabel.Text = "IRCTC Booking Automation";
            //
            // contentPanel
            //
            contentPanel.AutoScroll = false;
            contentPanel.BackColor = pageBg;
            contentPanel.Controls.Add(journeyCard);
            contentPanel.Controls.Add(passengerCard);
            contentPanel.Controls.Add(paymentCard);
            contentPanel.Controls.Add(preferencesCard);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 56);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(700, 700);
            contentPanel.TabIndex = 1;

            // =========================================================
            // LEFT COLUMN — Journey Details
            // =========================================================
            //
            // journeyCard
            //
            journeyCard.BorderStyle = BorderStyle.FixedSingle;
            journeyCard.Controls.Add(journeyHeader);
            journeyCard.Controls.Add(fromCaption);
            journeyCard.Controls.Add(fromStationCombo);
            journeyCard.Controls.Add(toCaption);
            journeyCard.Controls.Add(toStationCombo);
            journeyCard.Controls.Add(dateCaption);
            journeyCard.Controls.Add(travelDatePicker);
            journeyCard.Controls.Add(findButton);
            journeyCard.Controls.Add(bdgCaption);
            journeyCard.Controls.Add(boardingPointText);
            journeyCard.Controls.Add(trainNoCaption);
            journeyCard.Controls.Add(trainNoText);
            journeyCard.Controls.Add(trainTypeCaption);
            journeyCard.Controls.Add(trainTypeCombo);
            journeyCard.Controls.Add(availabilityLink);
            journeyCard.Controls.Add(classCaption);
            journeyCard.Controls.Add(classCombo);
            journeyCard.Controls.Add(quotaCaption);
            journeyCard.Controls.Add(quotaGeneralRadio);
            journeyCard.Controls.Add(quotaLadiesRadio);
            journeyCard.Controls.Add(quotaTatkalRadio);
            journeyCard.Controls.Add(quotaPremiumRadio);
            journeyCard.Location = new Point(20, 12);
            journeyCard.Name = "journeyCard";
            journeyCard.Size = new Size(660, 236);
            journeyCard.TabIndex = 0;
            //
            // journeyHeader
            //
            journeyHeader.BackColor = primaryLight;
            journeyHeader.Dock = DockStyle.Top;
            journeyHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            journeyHeader.ForeColor = headerText;
            journeyHeader.Location = new Point(0, 0);
            journeyHeader.Name = "journeyHeader";
            journeyHeader.Padding = new Padding(14, 0, 0, 0);
            journeyHeader.Size = new Size(658, 34);
            journeyHeader.TabIndex = 0;
            journeyHeader.Text = "Journey Details";
            journeyHeader.TextAlign = ContentAlignment.MiddleLeft;
            //
            // fromCaption
            //
            fromCaption.AutoSize = true;
            fromCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            fromCaption.ForeColor = textMuted;
            fromCaption.Location = new Point(16, 44);
            fromCaption.Name = "fromCaption";
            fromCaption.Size = new Size(38, 15);
            fromCaption.TabIndex = 1;
            fromCaption.Text = "FROM";
            //
            // fromStationCombo
            //
            fromStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            fromStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            fromStationCombo.FormattingEnabled = true;
            fromStationCombo.Location = new Point(16, 60);
            fromStationCombo.Name = "fromStationCombo";
            fromStationCombo.Size = new Size(160, 28);
            fromStationCombo.TabIndex = 2;
            //
            // toCaption
            //
            toCaption.AutoSize = true;
            toCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            toCaption.ForeColor = textMuted;
            toCaption.Location = new Point(200, 44);
            toCaption.Name = "toCaption";
            toCaption.Size = new Size(22, 15);
            toCaption.TabIndex = 3;
            toCaption.Text = "TO";
            //
            // toStationCombo
            //
            toStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            toStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            toStationCombo.FormattingEnabled = true;
            toStationCombo.Location = new Point(200, 60);
            toStationCombo.Name = "toStationCombo";
            toStationCombo.Size = new Size(160, 28);
            toStationCombo.TabIndex = 4;
            //
            // dateCaption
            //
            dateCaption.AutoSize = true;
            dateCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            dateCaption.ForeColor = textMuted;
            dateCaption.Location = new Point(388, 44);
            dateCaption.Name = "dateCaption";
            dateCaption.Size = new Size(36, 15);
            dateCaption.TabIndex = 5;
            dateCaption.Text = "DATE";
            //
            // travelDatePicker
            //
            travelDatePicker.Format = DateTimePickerFormat.Short;
            travelDatePicker.Location = new Point(388, 60);
            travelDatePicker.Name = "travelDatePicker";
            travelDatePicker.Size = new Size(110, 27);
            travelDatePicker.TabIndex = 6;
            //
            // findButton
            //
            findButton.BackColor = primary;
            findButton.FlatStyle = FlatStyle.Flat;
            findButton.FlatAppearance.BorderSize = 0;
            findButton.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            findButton.ForeColor = Color.White;
            findButton.Location = new Point(520, 56);
            findButton.Name = "findButton";
            findButton.Size = new Size(120, 34);
            findButton.TabIndex = 7;
            findButton.Text = "Find Trains";
            findButton.UseVisualStyleBackColor = false;
            findButton.Click += FindButton_Click;
            //
            // bdgCaption
            //
            bdgCaption.AutoSize = true;
            bdgCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            bdgCaption.ForeColor = textMuted;
            bdgCaption.Location = new Point(16, 102);
            bdgCaption.Name = "bdgCaption";
            bdgCaption.Size = new Size(88, 15);
            bdgCaption.TabIndex = 8;
            bdgCaption.Text = "BOARDING PT";
            //
            // boardingPointText
            //
            boardingPointText.Location = new Point(16, 118);
            boardingPointText.Name = "boardingPointText";
            boardingPointText.ReadOnly = true;
            boardingPointText.Size = new Size(88, 27);
            boardingPointText.TabIndex = 9;
            //
            // trainNoCaption
            //
            trainNoCaption.AutoSize = true;
            trainNoCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            trainNoCaption.ForeColor = textMuted;
            trainNoCaption.Location = new Point(116, 102);
            trainNoCaption.Name = "trainNoCaption";
            trainNoCaption.Size = new Size(62, 15);
            trainNoCaption.TabIndex = 10;
            trainNoCaption.Text = "TRAIN NO";
            //
            // trainNoText
            //
            trainNoText.Location = new Point(116, 118);
            trainNoText.Name = "trainNoText";
            trainNoText.ReadOnly = true;
            trainNoText.Size = new Size(86, 27);
            trainNoText.TabIndex = 11;
            //
            // trainTypeCaption
            //
            trainTypeCaption.AutoSize = true;
            trainTypeCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            trainTypeCaption.ForeColor = textMuted;
            trainTypeCaption.Location = new Point(214, 102);
            trainTypeCaption.Name = "trainTypeCaption";
            trainTypeCaption.Size = new Size(72, 15);
            trainTypeCaption.TabIndex = 12;
            trainTypeCaption.Text = "TRAIN TYPE";
            //
            // trainTypeCombo
            //
            trainTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            trainTypeCombo.FormattingEnabled = true;
            trainTypeCombo.Location = new Point(214, 118);
            trainTypeCombo.Name = "trainTypeCombo";
            trainTypeCombo.Size = new Size(160, 28);
            trainTypeCombo.TabIndex = 13;
            //
            // availabilityLink
            //
            availabilityLink.AutoSize = true;
            availabilityLink.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            availabilityLink.LinkColor = primary;
            availabilityLink.Location = new Point(388, 124);
            availabilityLink.Name = "availabilityLink";
            availabilityLink.Size = new Size(112, 20);
            availabilityLink.TabIndex = 14;
            availabilityLink.TabStop = true;
            availabilityLink.Text = "View Availability →";
            availabilityLink.LinkClicked += AvailabilityLink_LinkClicked;
            //
            // classCaption
            //
            classCaption.AutoSize = true;
            classCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            classCaption.ForeColor = textMuted;
            classCaption.Location = new Point(16, 158);
            classCaption.Name = "classCaption";
            classCaption.Size = new Size(38, 15);
            classCaption.TabIndex = 15;
            classCaption.Text = "CLASS";
            //
            // classCombo
            //
            classCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            classCombo.FormattingEnabled = true;
            classCombo.Location = new Point(16, 174);
            classCombo.Name = "classCombo";
            classCombo.Size = new Size(150, 28);
            classCombo.TabIndex = 16;
            //
            // quotaCaption
            //
            quotaCaption.AutoSize = true;
            quotaCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            quotaCaption.ForeColor = textMuted;
            quotaCaption.Location = new Point(180, 158);
            quotaCaption.Name = "quotaCaption";
            quotaCaption.Size = new Size(42, 15);
            quotaCaption.TabIndex = 17;
            quotaCaption.Text = "QUOTA";
            //
            // quotaGeneralRadio (pill — active by default)
            //
            quotaGeneralRadio.Appearance = Appearance.Button;
            quotaGeneralRadio.BackColor = primary;
            quotaGeneralRadio.Checked = true;
            quotaGeneralRadio.FlatStyle = FlatStyle.Flat;
            quotaGeneralRadio.FlatAppearance.BorderSize = 1;
            quotaGeneralRadio.FlatAppearance.BorderColor = primary;
            quotaGeneralRadio.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            quotaGeneralRadio.ForeColor = Color.White;
            quotaGeneralRadio.Location = new Point(180, 174);
            quotaGeneralRadio.Name = "quotaGeneralRadio";
            quotaGeneralRadio.Size = new Size(75, 24);
            quotaGeneralRadio.TabIndex = 18;
            quotaGeneralRadio.TabStop = true;
            quotaGeneralRadio.Text = "General";
            quotaGeneralRadio.TextAlign = ContentAlignment.MiddleCenter;
            quotaGeneralRadio.UseVisualStyleBackColor = false;
            quotaGeneralRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            //
            // quotaLadiesRadio (pill — inactive)
            //
            quotaLadiesRadio.Appearance = Appearance.Button;
            quotaLadiesRadio.BackColor = pageBg;
            quotaLadiesRadio.FlatStyle = FlatStyle.Flat;
            quotaLadiesRadio.FlatAppearance.BorderSize = 1;
            quotaLadiesRadio.FlatAppearance.BorderColor = borderColor;
            quotaLadiesRadio.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            quotaLadiesRadio.ForeColor = textMuted;
            quotaLadiesRadio.Location = new Point(260, 174);
            quotaLadiesRadio.Name = "quotaLadiesRadio";
            quotaLadiesRadio.Size = new Size(75, 24);
            quotaLadiesRadio.TabIndex = 19;
            quotaLadiesRadio.Text = "Ladies";
            quotaLadiesRadio.TextAlign = ContentAlignment.MiddleCenter;
            quotaLadiesRadio.UseVisualStyleBackColor = false;
            quotaLadiesRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            //
            // quotaTatkalRadio (pill — inactive)
            //
            quotaTatkalRadio.Appearance = Appearance.Button;
            quotaTatkalRadio.BackColor = pageBg;
            quotaTatkalRadio.FlatStyle = FlatStyle.Flat;
            quotaTatkalRadio.FlatAppearance.BorderSize = 1;
            quotaTatkalRadio.FlatAppearance.BorderColor = borderColor;
            quotaTatkalRadio.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            quotaTatkalRadio.ForeColor = textMuted;
            quotaTatkalRadio.Location = new Point(180, 202);
            quotaTatkalRadio.Name = "quotaTatkalRadio";
            quotaTatkalRadio.Size = new Size(75, 24);
            quotaTatkalRadio.TabIndex = 20;
            quotaTatkalRadio.Text = "Tatkal";
            quotaTatkalRadio.TextAlign = ContentAlignment.MiddleCenter;
            quotaTatkalRadio.UseVisualStyleBackColor = false;
            quotaTatkalRadio.CheckedChanged += QuotaRadio_CheckedChanged;
            //
            // quotaPremiumRadio (pill — inactive)
            //
            quotaPremiumRadio.Appearance = Appearance.Button;
            quotaPremiumRadio.BackColor = pageBg;
            quotaPremiumRadio.FlatStyle = FlatStyle.Flat;
            quotaPremiumRadio.FlatAppearance.BorderSize = 1;
            quotaPremiumRadio.FlatAppearance.BorderColor = borderColor;
            quotaPremiumRadio.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            quotaPremiumRadio.ForeColor = textMuted;
            quotaPremiumRadio.Location = new Point(260, 202);
            quotaPremiumRadio.Name = "quotaPremiumRadio";
            quotaPremiumRadio.Size = new Size(100, 24);
            quotaPremiumRadio.TabIndex = 21;
            quotaPremiumRadio.Text = "Premium Tatkal";
            quotaPremiumRadio.TextAlign = ContentAlignment.MiddleCenter;
            quotaPremiumRadio.UseVisualStyleBackColor = false;
            quotaPremiumRadio.CheckedChanged += QuotaRadio_CheckedChanged;

            // =========================================================
            // LEFT COLUMN — Passenger Details
            // =========================================================
            //
            // passengerCard
            //
            passengerCard.BorderStyle = BorderStyle.FixedSingle;
            passengerCard.Controls.Add(passengerGrid);
            passengerCard.Controls.Add(passengerHeader);
            passengerCard.Location = new Point(20, 260);
            passengerCard.Name = "passengerCard";
            passengerCard.Size = new Size(660, 218);
            passengerCard.TabIndex = 2;
            //
            // passengerHeader
            //
            passengerHeader.BackColor = primaryLight;
            passengerHeader.Dock = DockStyle.Top;
            passengerHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            passengerHeader.ForeColor = headerText;
            passengerHeader.Location = new Point(0, 0);
            passengerHeader.Name = "passengerHeader";
            passengerHeader.Padding = new Padding(14, 0, 0, 0);
            passengerHeader.Size = new Size(658, 34);
            passengerHeader.TabIndex = 0;
            passengerHeader.Text = "Passenger Details";
            passengerHeader.TextAlign = ContentAlignment.MiddleLeft;
            //
            // passengerGrid
            //
            passengerGrid.BorderStyle = BorderStyle.None;
            passengerGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            passengerGrid.Dock = DockStyle.Fill;
            passengerGrid.Location = new Point(0, 34);
            passengerGrid.Name = "passengerGrid";
            passengerGrid.RowHeadersVisible = false;
            passengerGrid.RowHeadersWidth = 51;
            passengerGrid.Size = new Size(798, 176);
            passengerGrid.TabIndex = 1;

            // =========================================================
            // RIGHT COLUMN — Fare & Payment
            // =========================================================
            //
            // paymentCard
            //
            paymentCard.BorderStyle = BorderStyle.FixedSingle;
            paymentCard.Controls.Add(paymentHeader);
            paymentCard.Controls.Add(fareCaption);
            paymentCard.Controls.Add(rupeeLabel);
            paymentCard.Controls.Add(fareText);
            paymentCard.Controls.Add(getFareButton);
            paymentCard.Controls.Add(mobileCaption);
            paymentCard.Controls.Add(mobileText);
            paymentCard.Controls.Add(ticketSlotCaption);
            paymentCard.Controls.Add(ticketSlotCombo);
            paymentCard.Controls.Add(gatewayCaption);
            paymentCard.Controls.Add(gatewayCombo);
            paymentCard.Controls.Add(priorBankCaption);
            paymentCard.Controls.Add(priorBankCombo);
            paymentCard.Controls.Add(backupBankCaption);
            paymentCard.Controls.Add(backupBankCombo);
            paymentCard.Controls.Add(ticketNameCaption);
            paymentCard.Controls.Add(ticketNameText);
            paymentCard.Location = new Point(20, 470);
            paymentCard.Name = "paymentCard";
            paymentCard.Size = new Size(320, 275);
            paymentCard.TabIndex = 3;
            //
            // paymentHeader
            //
            paymentHeader.BackColor = primaryLight;
            paymentHeader.Dock = DockStyle.Top;
            paymentHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            paymentHeader.ForeColor = headerText;
            paymentHeader.Location = new Point(0, 0);
            paymentHeader.Name = "paymentHeader";
            paymentHeader.Padding = new Padding(14, 0, 0, 0);
            paymentHeader.Size = new Size(318, 34);
            paymentHeader.TabIndex = 0;
            paymentHeader.UseMnemonic = false;
            paymentHeader.Text = "Fare & Payment Setup";
            paymentHeader.TextAlign = ContentAlignment.MiddleLeft;
            //
            // fareCaption
            //
            fareCaption.AutoSize = true;
            fareCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            fareCaption.ForeColor = textMuted;
            fareCaption.Location = new Point(16, 44);
            fareCaption.Name = "fareCaption";
            fareCaption.Size = new Size(62, 15);
            fareCaption.TabIndex = 1;
            fareCaption.Text = "TOTAL FARE";
            //
            // rupeeLabel
            //
            rupeeLabel.AutoSize = true;
            rupeeLabel.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            rupeeLabel.ForeColor = primary;
            rupeeLabel.Location = new Point(16, 60);
            rupeeLabel.Name = "rupeeLabel";
            rupeeLabel.Size = new Size(18, 28);
            rupeeLabel.TabIndex = 2;
            rupeeLabel.Text = "₹";
            //
            // fareText
            //
            fareText.AutoSize = false;
            fareText.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            fareText.ForeColor = UiTheme.Text;
            fareText.Location = new Point(36, 54);
            fareText.Name = "fareText";
            fareText.Size = new Size(110, 40);
            fareText.TabIndex = 3;
            fareText.Text = "0";
            fareText.TextAlign = ContentAlignment.MiddleLeft;
            //
            // getFareButton
            //
            getFareButton.BackColor = pageBg;
            getFareButton.FlatStyle = FlatStyle.Flat;
            getFareButton.FlatAppearance.BorderSize = 1;
            getFareButton.FlatAppearance.BorderColor = primary;
            getFareButton.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            getFareButton.ForeColor = primary;
            getFareButton.Location = new Point(200, 56);
            getFareButton.Name = "getFareButton";
            getFareButton.Size = new Size(100, 34);
            getFareButton.TabIndex = 4;
            getFareButton.Text = "Get Fare";
            getFareButton.UseVisualStyleBackColor = false;
            getFareButton.Click += GetFareButton_Click;
            //
            // mobileCaption
            //
            mobileCaption.AutoSize = true;
            mobileCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            mobileCaption.ForeColor = textMuted;
            mobileCaption.Location = new Point(16, 102);
            mobileCaption.Name = "mobileCaption";
            mobileCaption.Size = new Size(53, 15);
            mobileCaption.TabIndex = 5;
            mobileCaption.Text = "MOBILE";
            //
            // mobileText
            //
            mobileText.Location = new Point(16, 118);
            mobileText.Name = "mobileText";
            mobileText.Size = new Size(130, 27);
            mobileText.TabIndex = 6;
            //
            // ticketSlotCaption
            //
            ticketSlotCaption.AutoSize = true;
            ticketSlotCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            ticketSlotCaption.ForeColor = textMuted;
            ticketSlotCaption.Location = new Point(156, 102);
            ticketSlotCaption.Name = "ticketSlotCaption";
            ticketSlotCaption.Size = new Size(77, 15);
            ticketSlotCaption.TabIndex = 7;
            ticketSlotCaption.Text = "TICKET SLOT";
            //
            // ticketSlotCombo
            //
            ticketSlotCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            ticketSlotCombo.FormattingEnabled = true;
            ticketSlotCombo.Location = new Point(156, 118);
            ticketSlotCombo.Name = "ticketSlotCombo";
            ticketSlotCombo.Size = new Size(148, 28);
            ticketSlotCombo.TabIndex = 8;
            //
            // gatewayCaption
            //
            gatewayCaption.AutoSize = true;
            gatewayCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            gatewayCaption.ForeColor = textMuted;
            gatewayCaption.Location = new Point(16, 160);
            gatewayCaption.Name = "gatewayCaption";
            gatewayCaption.Size = new Size(62, 15);
            gatewayCaption.TabIndex = 9;
            gatewayCaption.Text = "GATEWAY";
            //
            // gatewayCombo
            //
            gatewayCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            gatewayCombo.FormattingEnabled = true;
            gatewayCombo.Location = new Point(16, 176);
            gatewayCombo.Name = "gatewayCombo";
            gatewayCombo.Size = new Size(138, 28);
            gatewayCombo.TabIndex = 10;
            //
            // priorBankCaption
            //
            priorBankCaption.AutoSize = true;
            priorBankCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            priorBankCaption.ForeColor = textMuted;
            priorBankCaption.Location = new Point(166, 160);
            priorBankCaption.Name = "priorBankCaption";
            priorBankCaption.Size = new Size(106, 15);
            priorBankCaption.TabIndex = 11;
            priorBankCaption.Text = "PRIOR BANK / UPI";
            //
            // priorBankCombo
            //
            priorBankCombo.FormattingEnabled = true;
            priorBankCombo.Location = new Point(166, 176);
            priorBankCombo.Name = "priorBankCombo";
            priorBankCombo.Size = new Size(138, 28);
            priorBankCombo.TabIndex = 12;
            //
            // backupBankCaption
            //
            backupBankCaption.AutoSize = true;
            backupBankCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            backupBankCaption.ForeColor = textMuted;
            backupBankCaption.Location = new Point(16, 218);
            backupBankCaption.Name = "backupBankCaption";
            backupBankCaption.Size = new Size(88, 15);
            backupBankCaption.TabIndex = 13;
            backupBankCaption.Text = "BACKUP BANK";
            //
            // backupBankCombo
            //
            backupBankCombo.FormattingEnabled = true;
            backupBankCombo.Location = new Point(16, 234);
            backupBankCombo.Name = "backupBankCombo";
            backupBankCombo.Size = new Size(138, 28);
            backupBankCombo.TabIndex = 14;
            //
            // ticketNameCaption
            //
            ticketNameCaption.AutoSize = true;
            ticketNameCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            ticketNameCaption.ForeColor = textMuted;
            ticketNameCaption.Location = new Point(166, 218);
            ticketNameCaption.Name = "ticketNameCaption";
            ticketNameCaption.Size = new Size(127, 15);
            ticketNameCaption.TabIndex = 15;
            ticketNameCaption.Text = "TICKET PROFILE NAME";
            //
            // ticketNameText
            //
            ticketNameText.Location = new Point(166, 234);
            ticketNameText.Name = "ticketNameText";
            ticketNameText.Size = new Size(138, 27);
            ticketNameText.TabIndex = 16;

            // =========================================================
            // RIGHT COLUMN — Booking Preferences
            // =========================================================
            //
            // preferencesCard
            //
            preferencesCard.BorderStyle = BorderStyle.FixedSingle;
            preferencesCard.Controls.Add(preferencesHeader);
            preferencesCard.Controls.Add(userCaption);
            preferencesCard.Controls.Add(irctcUserCombo);
            preferencesCard.Controls.Add(prefSeparator1);
            preferencesCard.Controls.Add(confirmBerthsTitleLabel);
            preferencesCard.Controls.Add(confirmBerthsCheck);
            preferencesCard.Controls.Add(useBetaViewTitleLabel);
            preferencesCard.Controls.Add(useBetaViewCheck);
            preferencesCard.Controls.Add(useRealChromeTitleLabel);
            preferencesCard.Controls.Add(useRealChromeCheck);
            preferencesCard.Location = new Point(360, 470);
            preferencesCard.Name = "preferencesCard";
            preferencesCard.Size = new Size(320, 275);
            preferencesCard.TabIndex = 4;
            //
            // preferencesHeader
            //
            preferencesHeader.BackColor = primaryLight;
            preferencesHeader.Dock = DockStyle.Top;
            preferencesHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            preferencesHeader.ForeColor = headerText;
            preferencesHeader.Location = new Point(0, 0);
            preferencesHeader.Name = "preferencesHeader";
            preferencesHeader.Padding = new Padding(14, 0, 0, 0);
            preferencesHeader.Size = new Size(318, 34);
            preferencesHeader.TabIndex = 0;
            preferencesHeader.Text = "Booking Preferences";
            preferencesHeader.TextAlign = ContentAlignment.MiddleLeft;
            //
            // userCaption
            //
            userCaption.AutoSize = true;
            userCaption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            userCaption.ForeColor = textMuted;
            userCaption.Location = new Point(16, 56);
            userCaption.Name = "userCaption";
            userCaption.Size = new Size(105, 15);
            userCaption.TabIndex = 1;
            userCaption.Text = "IRCTC USERNAME";
            //
            // irctcUserCombo
            //
            irctcUserCombo.Location = new Point(16, 72);
            irctcUserCombo.Name = "irctcUserCombo";
            irctcUserCombo.Size = new Size(288, 27);
            irctcUserCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            irctcUserCombo.TabIndex = 2;
            //
            // prefSeparator1
            //
            prefSeparator1.BackColor = borderColor;
            prefSeparator1.Location = new Point(16, 115);
            prefSeparator1.Name = "prefSeparator1";
            prefSeparator1.Size = new Size(288, 1);
            prefSeparator1.TabIndex = 3;
            //
            // confirmBerthsTitleLabel
            //
            confirmBerthsTitleLabel.AutoSize = true;
            confirmBerthsTitleLabel.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            confirmBerthsTitleLabel.ForeColor = textMuted;
            confirmBerthsTitleLabel.Location = new Point(16, 128);
            confirmBerthsTitleLabel.Name = "confirmBerthsTitleLabel";
            confirmBerthsTitleLabel.Size = new Size(95, 15);
            confirmBerthsTitleLabel.TabIndex = 4;
            confirmBerthsTitleLabel.Text = "CONFIRM BERTHS";
            //
            // confirmBerthsCheck
            //
            confirmBerthsCheck.Checked = false;
            confirmBerthsCheck.Location = new Point(115, 126);
            confirmBerthsCheck.Name = "confirmBerthsCheck";
            confirmBerthsCheck.Size = new Size(38, 20);
            confirmBerthsCheck.TabIndex = 5;
            //
            // useBetaViewTitleLabel
            //
            useBetaViewTitleLabel.AutoSize = true;
            useBetaViewTitleLabel.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            useBetaViewTitleLabel.ForeColor = textMuted;
            useBetaViewTitleLabel.Location = new Point(170, 128);
            useBetaViewTitleLabel.Name = "useBetaViewTitleLabel";
            useBetaViewTitleLabel.Size = new Size(47, 15);
            useBetaViewTitleLabel.TabIndex = 7;
            useBetaViewTitleLabel.Text = "BETA UI";
            //
            // useBetaViewCheck
            //
            useBetaViewCheck.Location = new Point(250, 126);
            useBetaViewCheck.Name = "useBetaViewCheck";
            useBetaViewCheck.Size = new Size(38, 20);
            useBetaViewCheck.TabIndex = 8;
            //
            // useRealChromeTitleLabel
            //
            useRealChromeTitleLabel.AutoSize = true;
            useRealChromeTitleLabel.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            useRealChromeTitleLabel.ForeColor = textMuted;
            useRealChromeTitleLabel.Location = new Point(16, 173);
            useRealChromeTitleLabel.Name = "useRealChromeTitleLabel";
            useRealChromeTitleLabel.Size = new Size(111, 15);
            useRealChromeTitleLabel.TabIndex = 10;
            useRealChromeTitleLabel.Text = "REAL CHROME (CDP)";
            //
            // useRealChromeCheck
            //
            useRealChromeCheck.Checked = true;
            useRealChromeCheck.Location = new Point(135, 171);
            useRealChromeCheck.Name = "useRealChromeCheck";
            useRealChromeCheck.Size = new Size(38, 20);
            useRealChromeCheck.TabIndex = 11;


            // =========================================================
            // Bottom action bar (pinned — never requires scrolling)
            // =========================================================
            //
            // actionPanel
            //
            actionPanel.BackColor = pageBg;
            actionPanel.Controls.Add(actionSeparator);
            actionPanel.Controls.Add(statusDotLabel);
            actionPanel.Controls.Add(statusLabel);
            actionPanel.Controls.Add(saveButton);
            actionPanel.Controls.Add(bookIrctcButton);
            actionPanel.Controls.Add(stopButton);
            actionPanel.Dock = DockStyle.Bottom;
            actionPanel.Location = new Point(0, 756);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(700, 64);
            actionPanel.TabIndex = 2;
            //
            // actionSeparator
            //
            actionSeparator.BackColor = borderColor;
            actionSeparator.Dock = DockStyle.Top;
            actionSeparator.Location = new Point(0, 0);
            actionSeparator.Name = "actionSeparator";
            actionSeparator.Size = new Size(700, 1);
            actionSeparator.TabIndex = 0;
            //
            // statusDotLabel
            //
            statusDotLabel.AutoSize = true;
            statusDotLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            statusDotLabel.ForeColor = UiTheme.Success;
            statusDotLabel.Location = new Point(14, 11);
            statusDotLabel.Name = "statusDotLabel";
            statusDotLabel.Size = new Size(13, 20);
            statusDotLabel.TabIndex = 1;
            statusDotLabel.Text = "●";
            //
            // statusLabel
            //
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font("Segoe UI", 9.5F);
            statusLabel.ForeColor = textMuted;
            statusLabel.Location = new Point(32, 24);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(179, 20);
            statusLabel.TabIndex = 2;
            statusLabel.Text = "Click Find to search trains.";
            //
            // saveButton
            //
            saveButton.BackColor = pageBg;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.FlatAppearance.BorderColor = borderColor;
            saveButton.FlatAppearance.BorderSize = 1;
            saveButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            saveButton.ForeColor = textMuted;
            saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saveButton.Location = new Point(380, 14);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(80, 36);
            saveButton.TabIndex = 3;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = false;
            saveButton.Click += SaveButton_Click;
            //
            // bookIrctcButton
            //
            bookIrctcButton.BackColor = primary;
            bookIrctcButton.FlatStyle = FlatStyle.Flat;
            bookIrctcButton.FlatAppearance.BorderSize = 0;
            bookIrctcButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            bookIrctcButton.ForeColor = Color.White;
            bookIrctcButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            bookIrctcButton.Location = new Point(470, 14);
            bookIrctcButton.Name = "bookIrctcButton";
            bookIrctcButton.Size = new Size(130, 36);
            bookIrctcButton.TabIndex = 4;
            bookIrctcButton.Text = "Book IRCTC";
            bookIrctcButton.UseVisualStyleBackColor = false;
            bookIrctcButton.Click += BookIrctcButton_Click;
            //
            // stopButton
            //
            stopButton.BackColor = pageBg;
            stopButton.FlatStyle = FlatStyle.Flat;
            stopButton.FlatAppearance.BorderColor = danger;
            stopButton.FlatAppearance.BorderSize = 1;
            stopButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            stopButton.ForeColor = danger;
            stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            stopButton.Location = new Point(616, 14);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(70, 36);
            stopButton.TabIndex = 5;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.Click += StopButton_Click;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = pageBg;
            ClientSize = new Size(700, 800);
            MinimumSize = new Size(700, 600);
            Controls.Add(contentPanel);
            Controls.Add(actionPanel);
            Controls.Add(titlePanel);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "New Ticket";
            Load += Form1_Load;
            titlePanel.ResumeLayout(false);
            titlePanel.PerformLayout();
            contentPanel.ResumeLayout(false);
            journeyCard.ResumeLayout(false);
            journeyCard.PerformLayout();
            passengerCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)passengerGrid).EndInit();
            paymentCard.ResumeLayout(false);
            paymentCard.PerformLayout();
            preferencesCard.ResumeLayout(false);
            preferencesCard.PerformLayout();

            actionPanel.ResumeLayout(false);
            actionPanel.PerformLayout();
            ResumeLayout(false);
        }

        private Panel titlePanel;
        private Label titleLabel;
        private Label titleSubLabel;
        private Panel contentPanel;

        private Panel journeyCard;
        private Label journeyHeader;
        private Label fromCaption;
        private ComboBox fromStationCombo;
        private Label toCaption;
        private ComboBox toStationCombo;
        private Label dateCaption;
        private DateTimePicker travelDatePicker;
        private Button findButton;
        private Label bdgCaption;
        private TextBox boardingPointText;
        private Label trainNoCaption;
        private TextBox trainNoText;
        private Label trainTypeCaption;
        private ComboBox trainTypeCombo;
        private LinkLabel availabilityLink;
        private Label classCaption;
        private ComboBox classCombo;
        private Label quotaCaption;
        private RadioButton quotaGeneralRadio;
        private RadioButton quotaLadiesRadio;
        private RadioButton quotaTatkalRadio;
        private RadioButton quotaPremiumRadio;



        private Panel passengerCard;
        private Label passengerHeader;
        private DataGridView passengerGrid;

        private Panel paymentCard;
        private Label paymentHeader;
        private Label fareCaption;
        private Label rupeeLabel;
        private Label fareText;
        private Button getFareButton;
        private Label mobileCaption;
        private TextBox mobileText;
        private Label ticketSlotCaption;
        private ComboBox ticketSlotCombo;
        private Label gatewayCaption;
        private ComboBox gatewayCombo;
        private Label priorBankCaption;
        private ComboBox priorBankCombo;
        private Label backupBankCaption;
        private ComboBox backupBankCombo;
        private Label ticketNameCaption;
        private TextBox ticketNameText;

        private Panel preferencesCard;
        private Label preferencesHeader;
        private Panel prefSeparator1;
        private Label confirmBerthsTitleLabel;
        private ToggleSwitch confirmBerthsCheck;
        private Label useBetaViewTitleLabel;
        private ToggleSwitch useBetaViewCheck;
        private Label useRealChromeTitleLabel;
        private ToggleSwitch useRealChromeCheck;

        private Label userCaption;
        private ComboBox irctcUserCombo;

        private Panel actionPanel;
        private Panel actionSeparator;
        private Label statusDotLabel;
        private Label statusLabel;
        private Button saveButton;
        private Button bookIrctcButton;
        private Button stopButton;
    }
}
