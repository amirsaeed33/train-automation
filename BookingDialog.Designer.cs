namespace train_automation;

partial class BookingDialog
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
        headerLabel = new Label();
        passengersPanel = new FlowLayoutPanel();
        addMoreButton = new Button();
        preferencesGroup = new GroupBox();
        autoUpgradationCheck = new CheckBox();
        confirmBerthsCheck = new CheckBox();
        reservationChoiceLabel = new Label();
        reservationChoiceCombo = new ComboBox();
        preferredCoachLabel = new Label();
        preferredCoachText = new TextBox();
        insuranceGroup = new GroupBox();
        insuranceYesRadio = new RadioButton();
        insuranceNoRadio = new RadioButton();
        paymentGroup = new GroupBox();
        paymentCardsRadio = new RadioButton();
        paymentBhupiRadio = new RadioButton();
        backButton = new Button();
        continueButton = new Button();
        mainPanel = new Panel();
        bottomPanel = new Panel();
        preferencesGroup.SuspendLayout();
        insuranceGroup.SuspendLayout();
        paymentGroup.SuspendLayout();
        mainPanel.SuspendLayout();
        bottomPanel.SuspendLayout();
        SuspendLayout();
        // 
        // headerLabel
        // 
        headerLabel.Dock = DockStyle.Top;
        headerLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        headerLabel.Location = new Point(0, 0);
        headerLabel.Name = "headerLabel";
        headerLabel.Padding = new Padding(12, 12, 12, 8);
        headerLabel.Size = new Size(684, 52);
        headerLabel.TabIndex = 0;
        headerLabel.Text = "Train Booking";
        // 
        // passengersPanel
        // 
        passengersPanel.AutoScroll = true;
        passengersPanel.Dock = DockStyle.Top;
        passengersPanel.FlowDirection = FlowDirection.TopDown;
        passengersPanel.Location = new Point(0, 52);
        passengersPanel.Name = "passengersPanel";
        passengersPanel.Padding = new Padding(12, 0, 12, 0);
        passengersPanel.Size = new Size(684, 180);
        passengersPanel.TabIndex = 1;
        passengersPanel.WrapContents = false;
        // 
        // addMoreButton
        // 
        addMoreButton.Location = new Point(12, 240);
        addMoreButton.Name = "addMoreButton";
        addMoreButton.Size = new Size(120, 32);
        addMoreButton.TabIndex = 2;
        addMoreButton.Text = "Add More";
        addMoreButton.UseVisualStyleBackColor = true;
        addMoreButton.Click += AddMoreButton_Click;
        // 
        // preferencesGroup
        // 
        preferencesGroup.Controls.Add(autoUpgradationCheck);
        preferencesGroup.Controls.Add(confirmBerthsCheck);
        preferencesGroup.Controls.Add(reservationChoiceLabel);
        preferencesGroup.Controls.Add(reservationChoiceCombo);
        preferencesGroup.Controls.Add(preferredCoachLabel);
        preferencesGroup.Controls.Add(preferredCoachText);
        preferencesGroup.Location = new Point(12, 284);
        preferencesGroup.Name = "preferencesGroup";
        preferencesGroup.Size = new Size(660, 150);
        preferencesGroup.TabIndex = 3;
        preferencesGroup.TabStop = false;
        preferencesGroup.Text = "Other Preferences";
        // 
        // autoUpgradationCheck
        // 
        autoUpgradationCheck.AutoSize = true;
        autoUpgradationCheck.Location = new Point(16, 28);
        autoUpgradationCheck.Name = "autoUpgradationCheck";
        autoUpgradationCheck.Size = new Size(470, 24);
        autoUpgradationCheck.TabIndex = 0;
        autoUpgradationCheck.Text = "Consider for Auto Upgradation";
        autoUpgradationCheck.UseVisualStyleBackColor = true;
        // 
        // confirmBerthsCheck
        // 
        confirmBerthsCheck.AutoSize = true;
        confirmBerthsCheck.Location = new Point(16, 56);
        confirmBerthsCheck.Name = "confirmBerthsCheck";
        confirmBerthsCheck.Size = new Size(260, 24);
        confirmBerthsCheck.TabIndex = 1;
        confirmBerthsCheck.Text = "Book only if confirm berths are allotted.";
        confirmBerthsCheck.UseVisualStyleBackColor = true;
        // 
        // reservationChoiceLabel
        // 
        reservationChoiceLabel.AutoSize = true;
        reservationChoiceLabel.Location = new Point(16, 90);
        reservationChoiceLabel.Name = "reservationChoiceLabel";
        reservationChoiceLabel.Size = new Size(133, 20);
        reservationChoiceLabel.TabIndex = 2;
        reservationChoiceLabel.Text = "Reservation Choice";
        // 
        // reservationChoiceCombo
        // 
        reservationChoiceCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        reservationChoiceCombo.FormattingEnabled = true;
        reservationChoiceCombo.Location = new Point(170, 86);
        reservationChoiceCombo.Name = "reservationChoiceCombo";
        reservationChoiceCombo.Size = new Size(470, 28);
        reservationChoiceCombo.TabIndex = 3;
        // 
        // preferredCoachLabel
        // 
        preferredCoachLabel.AutoSize = true;
        preferredCoachLabel.Location = new Point(16, 122);
        preferredCoachLabel.Name = "preferredCoachLabel";
        preferredCoachLabel.Size = new Size(136, 20);
        preferredCoachLabel.TabIndex = 4;
        preferredCoachLabel.Text = "Preferred Coach No.";
        // 
        // preferredCoachText
        // 
        preferredCoachText.Location = new Point(170, 118);
        preferredCoachText.Name = "preferredCoachText";
        preferredCoachText.Size = new Size(180, 27);
        preferredCoachText.TabIndex = 5;
        // 
        // insuranceGroup
        // 
        insuranceGroup.Controls.Add(insuranceYesRadio);
        insuranceGroup.Controls.Add(insuranceNoRadio);
        insuranceGroup.Location = new Point(12, 444);
        insuranceGroup.Name = "insuranceGroup";
        insuranceGroup.Size = new Size(660, 88);
        insuranceGroup.TabIndex = 4;
        insuranceGroup.TabStop = false;
        insuranceGroup.Text = "Travel Insurance (Incl. of GST)";
        // 
        // insuranceYesRadio
        // 
        insuranceYesRadio.AutoSize = true;
        insuranceYesRadio.Checked = true;
        insuranceYesRadio.Location = new Point(16, 28);
        insuranceYesRadio.Name = "insuranceYesRadio";
        insuranceYesRadio.Size = new Size(360, 24);
        insuranceYesRadio.TabIndex = 0;
        insuranceYesRadio.TabStop = true;
        insuranceYesRadio.Text = "Yes, and I accept the terms && conditions";
        insuranceYesRadio.UseVisualStyleBackColor = true;
        // 
        // insuranceNoRadio
        // 
        insuranceNoRadio.AutoSize = true;
        insuranceNoRadio.Location = new Point(16, 56);
        insuranceNoRadio.Name = "insuranceNoRadio";
        insuranceNoRadio.Size = new Size(250, 24);
        insuranceNoRadio.TabIndex = 1;
        insuranceNoRadio.Text = "No, I don't want travel insurance";
        insuranceNoRadio.UseVisualStyleBackColor = true;
        // 
        // paymentGroup
        // 
        paymentGroup.Controls.Add(paymentCardsRadio);
        paymentGroup.Controls.Add(paymentBhupiRadio);
        paymentGroup.Location = new Point(12, 542);
        paymentGroup.Name = "paymentGroup";
        paymentGroup.Size = new Size(660, 100);
        paymentGroup.TabIndex = 5;
        paymentGroup.TabStop = false;
        paymentGroup.Text = "Payment Mode";
        // 
        // paymentCardsRadio
        // 
        paymentCardsRadio.AutoSize = true;
        paymentCardsRadio.Checked = true;
        paymentCardsRadio.Location = new Point(16, 28);
        paymentCardsRadio.Name = "paymentCardsRadio";
        paymentCardsRadio.Size = new Size(560, 24);
        paymentCardsRadio.TabIndex = 0;
        paymentCardsRadio.TabStop = true;
        paymentCardsRadio.Text = "Pay through Credit & Debit Cards / Net Banking / Wallets / UPI / Others";
        paymentCardsRadio.UseVisualStyleBackColor = true;
        // 
        // paymentBhupiRadio
        // 
        paymentBhupiRadio.AutoSize = true;
        paymentBhupiRadio.Location = new Point(16, 58);
        paymentBhupiRadio.Name = "paymentBhupiRadio";
        paymentBhupiRadio.Size = new Size(180, 24);
        paymentBhupiRadio.TabIndex = 1;
        paymentBhupiRadio.Text = "Pay through BHIM/UPI";
        paymentBhupiRadio.UseVisualStyleBackColor = true;
        // 
        // backButton
        // 
        backButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        backButton.DialogResult = DialogResult.Cancel;
        backButton.Location = new Point(452, 10);
        backButton.Name = "backButton";
        backButton.Size = new Size(100, 34);
        backButton.TabIndex = 0;
        backButton.Text = "Back";
        backButton.UseVisualStyleBackColor = true;
        // 
        // continueButton
        // 
        continueButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        continueButton.BackColor = Color.FromArgb(255, 140, 0);
        continueButton.FlatStyle = FlatStyle.Flat;
        continueButton.ForeColor = Color.White;
        continueButton.Location = new Point(560, 10);
        continueButton.Name = "continueButton";
        continueButton.Size = new Size(112, 34);
        continueButton.TabIndex = 1;
        continueButton.Text = "Continue";
        continueButton.UseVisualStyleBackColor = false;
        continueButton.Click += ContinueButton_Click;
        // 
        // mainPanel
        // 
        mainPanel.AutoScroll = true;
        mainPanel.Controls.Add(paymentGroup);
        mainPanel.Controls.Add(insuranceGroup);
        mainPanel.Controls.Add(preferencesGroup);
        mainPanel.Controls.Add(addMoreButton);
        mainPanel.Controls.Add(passengersPanel);
        mainPanel.Controls.Add(headerLabel);
        mainPanel.Dock = DockStyle.Fill;
        mainPanel.Location = new Point(0, 0);
        mainPanel.Name = "mainPanel";
        mainPanel.Size = new Size(684, 711);
        mainPanel.TabIndex = 0;
        // 
        // bottomPanel
        // 
        bottomPanel.Controls.Add(backButton);
        bottomPanel.Controls.Add(continueButton);
        bottomPanel.Dock = DockStyle.Bottom;
        bottomPanel.Location = new Point(0, 711);
        bottomPanel.Name = "bottomPanel";
        bottomPanel.Size = new Size(684, 54);
        bottomPanel.TabIndex = 1;
        // 
        // BookingDialog
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = backButton;
        ClientSize = new Size(684, 765);
        Controls.Add(mainPanel);
        Controls.Add(bottomPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "BookingDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Passenger Details";
        preferencesGroup.ResumeLayout(false);
        preferencesGroup.PerformLayout();
        insuranceGroup.ResumeLayout(false);
        insuranceGroup.PerformLayout();
        paymentGroup.ResumeLayout(false);
        paymentGroup.PerformLayout();
        mainPanel.ResumeLayout(false);
        bottomPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Label headerLabel;
    private FlowLayoutPanel passengersPanel;
    private Button addMoreButton;
    private GroupBox preferencesGroup;
    private CheckBox autoUpgradationCheck;
    private CheckBox confirmBerthsCheck;
    private Label reservationChoiceLabel;
    private ComboBox reservationChoiceCombo;
    private Label preferredCoachLabel;
    private TextBox preferredCoachText;
    private GroupBox insuranceGroup;
    private RadioButton insuranceYesRadio;
    private RadioButton insuranceNoRadio;
    private GroupBox paymentGroup;
    private RadioButton paymentCardsRadio;
    private RadioButton paymentBhupiRadio;
    private Button backButton;
    private Button continueButton;
    private Panel mainPanel;
    private Panel bottomPanel;
}
