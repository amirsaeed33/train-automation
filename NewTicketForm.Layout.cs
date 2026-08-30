using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace train_automation;

public partial class NewTicketForm
{
    // contentPanel is 700px wide, no scrollbar. Controls: X=8 to X=688
    private const int LPAD = 12;   // left padding
    private const int RPAD = 12;   // right padding
    private const int FULL = 880; // contentPanel width

    // Each row: caption at rowTop, input at rowTop+19, next row at rowTop+56
    private const int INP_OFF = 19;
    private const int ROW_H   = 56;

    private CheckBox? _autoUpgradeNative;
    private CheckBox? _confirmBerthsNative;
    private Panel? _quotaStrip;
    private Panel? _bottomStrip;

    private void FlattenLayout()
    {
        float scale = this.DeviceDpi / 96f;
        var controlsToMove = new List<Control>();
        foreach (Control c in journeyCard.Controls)     controlsToMove.Add(c);
        foreach (Control c in passengerCard.Controls)   controlsToMove.Add(c);
        foreach (Control c in paymentCard.Controls)     controlsToMove.Add(c);
        foreach (Control c in preferencesCard.Controls) controlsToMove.Add(c);

        journeyCard.Visible     = false;
        passengerCard.Visible   = false;
        paymentCard.Visible     = false;
        preferencesCard.Visible = false;
        
        if (titlePanel != null) titlePanel.Visible = false;

        foreach (var c in controlsToMove)
        {
            if (c == journeyHeader || c == passengerHeader || c == paymentHeader || c == preferencesHeader) continue;
            // Hide old separators and legacy checkboxes
            if (c is Panel && c.Height == 1) { c.Visible = false; continue; }
            if (c == useBetaViewCheck || c == useBetaViewTitleLabel) { c.Visible = false; continue; }
            if (c == useRealChromeCheck || c == useRealChromeTitleLabel) { c.Visible = false; continue; }
            if (c == confirmBerthsTitleLabel || c == confirmBerthsCheck) { c.Visible = false; continue; }

            contentPanel.Controls.Add(c);
            
            if (c is Label lbl && lbl != titleLabel && lbl != titleSubLabel && lbl != availabilityLink && lbl != rupeeLabel)
            {
                lbl.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                lbl.ForeColor = UiTheme.Text;
                lbl.AutoSize = true;
            }
        }

        this.BackColor = UiTheme.PageBg;
        contentPanel.BackColor = UiTheme.PageBg;

        // Apply Hitman-style short texts
        fromCaption.Text      = "From:";
        toCaption.Text        = "To:";
        dateCaption.Text      = "Date:";
        bdgCaption.Text       = "Bdg Pt:";
        trainNoCaption.Text   = "Train No:";
        trainTypeCaption.Text = "Train Type:";
        classCaption.Text     = "Class:";
        quotaCaption.Text     = "Quota:";
        mobileCaption.Text    = "Mobile +91";
        fareCaption.Visible   = false;
        rupeeLabel.Text       = "Fare: Base";
        fareText.Visible      = false;
        ticketSlotCaption.Text= "Ticket Slot:";
        gatewayCaption.Text   = "Getways:";
        priorBankCaption.Text = "Prior Bank:";
        backupBankCaption.Text= "BackUp Bank:";
        ticketNameCaption.Text= "Name:";
        userCaption.Text      = "IRCTC User:";
        
        // Native Checkboxes
        _autoUpgradeNative = new CheckBox 
        { 
            Text = "Consider for Auto Upgradation.", 
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Regular),
            ForeColor = UiTheme.Text,
        };
        _confirmBerthsNative = new CheckBox 
        { 
            Text = "Book only if confirm berths allotted.", 
            AutoSize = true,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Regular),
            ForeColor = UiTheme.Text,
        };
        contentPanel.Controls.Add(_autoUpgradeNative);
        contentPanel.Controls.Add(_confirmBerthsNative);
        _autoUpgradeNative.CheckedChanged += (_, _) => useBetaViewCheck.Checked = _autoUpgradeNative.Checked;
        _confirmBerthsNative.CheckedChanged += (_, _) => confirmBerthsCheck.Checked = _confirmBerthsNative.Checked;

        int y = (int)(6 * scale);
        int rowH = (int)(24 * scale); // strict vertical spacing

        // ── ROW 1 ──────────────────────────
        fromCaption.Location = new Point((int)(12 * scale), y + (int)(4 * scale));
        fromStationCombo.Location = new Point((int)(60 * scale), y);
        fromStationCombo.Size = new Size((int)(140 * scale), (int)(24 * scale));

        toCaption.Location = new Point((int)(210 * scale), y + (int)(4 * scale));
        toStationCombo.Location = new Point((int)(235 * scale), y);
        toStationCombo.Size = new Size((int)(140 * scale), (int)(24 * scale));

        dateCaption.Location = new Point((int)(385 * scale), y + (int)(4 * scale));
        travelDatePicker.Location = new Point((int)(420 * scale), y);
        travelDatePicker.Size = new Size((int)(100 * scale), (int)(24 * scale));

        findButton.Location = new Point((int)(530 * scale), y);
        findButton.Size = new Size((int)(80 * scale), (int)(24 * scale));
        findButton.FlatStyle = FlatStyle.Flat;
        findButton.BackColor = UiTheme.Primary;
        findButton.ForeColor = Color.White;
        findButton.FlatAppearance.BorderSize = 0;

        y += rowH;

        // ── ROW 2 ──────────────────────────
        bdgCaption.Location = new Point((int)(12 * scale), y + (int)(4 * scale));
        boardingPointText.Location = new Point((int)(60 * scale), y);
        boardingPointText.Size = new Size((int)(140 * scale), (int)(24 * scale));

        trainNoCaption.Location = new Point((int)(210 * scale), y + (int)(4 * scale));
        trainNoText.Location = new Point((int)(265 * scale), y);
        trainNoText.Size = new Size((int)(90 * scale), (int)(24 * scale));

        trainTypeCaption.Location = new Point((int)(365 * scale), y + (int)(4 * scale));
        trainTypeCombo.Location = new Point((int)(430 * scale), y);
        trainTypeCombo.Size = new Size((int)(95 * scale), (int)(24 * scale));

        availabilityLink.Location = new Point((int)(535 * scale), y + (int)(4 * scale));

        y += rowH;

        // ── ROW 3 (Quota Strip) ──────────────────────────
        if (_quotaStrip == null)
        {
            _quotaStrip = new Panel { BackColor = UiTheme.SurfaceHigh };
            contentPanel.Controls.Add(_quotaStrip);
        }
        _quotaStrip.Location = new Point(0, y);
        _quotaStrip.Size = new Size((int)(665 * scale), (int)(30 * scale));
        _quotaStrip.BringToFront();

        classCaption.Location = new Point((int)(12 * scale), (int)(8 * scale));
        classCombo.Location = new Point((int)(60 * scale), (int)(4 * scale));
        classCombo.Size = new Size((int)(140 * scale), (int)(24 * scale));
        _quotaStrip.Controls.Add(classCaption);
        _quotaStrip.Controls.Add(classCombo);
        
        quotaCaption.Location = new Point((int)(210 * scale), (int)(8 * scale));
        _quotaStrip.Controls.Add(quotaCaption);
        
        var radios = new[] { quotaGeneralRadio, quotaLadiesRadio, quotaTatkalRadio, quotaPremiumRadio };
        int[] rx = { (int)(265 * scale), (int)(340 * scale), (int)(410 * scale), (int)(480 * scale) };
        for (int i = 0; i < radios.Length; i++)
        {
            var r = radios[i];
            r.Appearance = Appearance.Normal;
            r.AutoSize = true;
            r.BackColor = Color.Transparent;
            r.ForeColor = r.Checked ? UiTheme.Primary : UiTheme.Text;
            r.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            r.Location = new Point(rx[i], (int)(6 * scale));
            r.CheckedChanged += (s, e) => {
                var rad = (RadioButton)s!;
                rad.ForeColor = rad.Checked ? UiTheme.Primary : UiTheme.Text;
            };
            _quotaStrip.Controls.Add(r);
        }

        y += (int)(32 * scale);

        // ── PASSENGER TABLE ──────────────────────────
        int w = (int)(665 * scale);
        
        passengerGrid.Visible = false;

        y = BuildCustomPassengerGrid(y, scale, w);

        PositionBottomElements(scale, y, w);
    }

    private int BuildCustomPassengerGrid(int startY, float scale, int formWidth)
    {
        if (passengerCustomGridPanel == null)
        {
            passengerCustomGridPanel = new Panel();
            passengerCustomGridPanel.BackColor = UiTheme.PageBg;
            contentPanel.Controls.Add(passengerCustomGridPanel);
        }

        passengerCustomGridPanel.Location = new Point((int)(12 * scale), startY);
        passengerCustomGridPanel.Width = formWidth - (int)(24 * scale);

        int[] w = { 
            (int)(35 * scale),  // SNo
            (int)(110 * scale), // Name
            (int)(35 * scale),  // Age
            (int)(50 * scale),  // Sex
            (int)(75 * scale),  // Berth
            (int)(70 * scale),  // Food
            (int)(65 * scale),  // Nationality
            (int)(65 * scale),  // Passport
            (int)(30 * scale),  // Child
            (int)(30 * scale),  // Senior
            (int)(30 * scale)   // Bed
        };

        string[] headers = { "SNo", "Name", "Age", "Sex", "Berth", "Food", "Nationality", "Passport", "Child", "Senior", "Bed" };
        
        passengerCustomGridPanel.Controls.Clear();
        _passengerRows.Clear();

        int currentX = 0;
        int currentY = 0;

        // Header Row
        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label
            {
                Text = headers[i],
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Location = new Point(currentX, currentY),
                Size = new Size(w[i], (int)(20 * scale)),
                ForeColor = UiTheme.Text,
                TextAlign = ContentAlignment.BottomLeft
            };
            if (i >= 8) lbl.TextAlign = ContentAlignment.BottomCenter; // checkboxes aligned center
            
            passengerCustomGridPanel.Controls.Add(lbl);
            currentX += w[i] + (int)(4 * scale);
        }

        currentY += (int)(20 * scale);

        // Separator
        Panel sep = new Panel
        {
            Location = new Point(0, currentY),
            Size = new Size(passengerCustomGridPanel.Width, 1),
            BackColor = UiTheme.OutlineVariant
        };
        passengerCustomGridPanel.Controls.Add(sep);
        currentY += (int)(6 * scale);

        // 6 Data Rows
        for (int r = 1; r <= 6; r++)
        {
            currentX = 0;
            var rowControls = new PassengerRowControls();

            // 0: SNo
            Label sno = new Label { Text = r.ToString(), Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), Location = new Point(currentX, currentY + (int)(4 * scale)), Size = new Size(w[0], (int)(20 * scale)), TextAlign = ContentAlignment.TopCenter, ForeColor = UiTheme.Text };
            passengerCustomGridPanel.Controls.Add(sno);
            rowControls.SNo = sno;
            currentX += w[0] + (int)(4 * scale);

            // 1: Name
            TextBox name = new TextBox { Location = new Point(currentX, currentY), Size = new Size(w[1], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            name.TextChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(name);
            rowControls.Name = name;
            currentX += w[1] + (int)(4 * scale);

            // 2: Age
            TextBox age = new TextBox { Location = new Point(currentX, currentY), Size = new Size(w[2], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            age.TextChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(age);
            rowControls.Age = age;
            currentX += w[2] + (int)(4 * scale);

            // 3: Sex
            ComboBox sex = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(currentX, currentY), Size = new Size(w[3], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            sex.Items.AddRange(new object[] { "Select", "M", "F", "T" });
            sex.SelectedIndex = 0;
            sex.SelectedIndexChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(sex);
            rowControls.Sex = sex;
            currentX += w[3] + (int)(4 * scale);

            // 4: Berth
            ComboBox berth = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(currentX, currentY), Size = new Size(w[4], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            berth.Items.AddRange(new object[] { "No Choice", "Lower", "Middle", "Upper", "Side Lower", "Side Upper" });
            berth.SelectedIndex = 0;
            berth.SelectedIndexChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(berth);
            rowControls.Berth = berth;
            currentX += w[4] + (int)(4 * scale);

            // 5: Food
            ComboBox food = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(currentX, currentY), Size = new Size(w[5], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            food.Items.AddRange(new object[] { "No Choice", "Veg", "Non Veg" });
            food.SelectedIndex = 0;
            food.SelectedIndexChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(food);
            rowControls.Food = food;
            currentX += w[5] + (int)(4 * scale);

            // 6: Nationality
            ComboBox nat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(currentX, currentY), Size = new Size(w[6], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            nat.Items.AddRange(new object[] { "India-IN", "Other" });
            nat.SelectedIndex = 0;
            nat.SelectedIndexChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(nat);
            rowControls.Nationality = nat;
            currentX += w[6] + (int)(4 * scale);

            // 7: Passport
            TextBox pass = new TextBox { Location = new Point(currentX, currentY), Size = new Size(w[7], (int)(20 * scale)), Font = new Font("Segoe UI", 7.5F) };
            pass.TextChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(pass);
            rowControls.Passport = pass;
            currentX += w[7] + (int)(4 * scale);

            // Checkboxes
            CheckBox child = new CheckBox { Location = new Point(currentX + (w[8]/2) - (int)(6 * scale), currentY + (int)(3 * scale)), Size = new Size((int)(15 * scale), (int)(15 * scale)) };
            child.CheckedChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(child);
            rowControls.Child = child;
            currentX += w[8] + (int)(4 * scale);

            CheckBox senior = new CheckBox { Location = new Point(currentX + (w[9]/2) - (int)(6 * scale), currentY + (int)(3 * scale)), Size = new Size((int)(15 * scale), (int)(15 * scale)) };
            senior.CheckedChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(senior);
            rowControls.Senior = senior;
            currentX += w[9] + (int)(4 * scale);

            CheckBox bed = new CheckBox { Location = new Point(currentX + (w[10]/2) - (int)(6 * scale), currentY + (int)(3 * scale)), Size = new Size((int)(15 * scale), (int)(15 * scale)) };
            bed.CheckedChanged += (_, _) => SaveIrctcConfigFromUi();
            passengerCustomGridPanel.Controls.Add(bed);
            rowControls.Bed = bed;

            _passengerRows.Add(rowControls);
            currentY += (int)(24 * scale);
        }

        passengerCustomGridPanel.Height = currentY;
        return passengerCustomGridPanel.Bottom;
    }

    private void PositionBottomElements(float scale, int y, int w)
    {
        if (_autoUpgradeNative == null) return; 

        int rowH = (int)(26 * scale);

        int c1 = (int)(12 * scale);
        int c2 = (int)(w * 0.38);
        int c3 = (int)(w * 0.68);

        // ── ROW 4: Mobile | Fare label | Get Fare ─────────────────────────────────
        mobileCaption.Location = new Point(c1, y + 4);
        mobileText.Location = new Point(mobileCaption.Right + 5, y);
        mobileText.Size = new Size(c2 - mobileText.Left - 10, 24);

        rupeeLabel.Location = new Point(c2, y + 4);
        rupeeLabel.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);

        // Ensure Get Fare button is placed and fully visible
        getFareButton.Location = new Point(rupeeLabel.Right + 15, y);
        getFareButton.Size = new Size((int)(90 * scale), 24);
        getFareButton.BackColor = UiTheme.Primary;
        getFareButton.ForeColor = Color.White;
        getFareButton.FlatStyle = FlatStyle.Flat;
        getFareButton.FlatAppearance.BorderSize = 0;
        getFareButton.Visible = true;
        if (contentPanel.Controls.Contains(getFareButton)) {
            getFareButton.BringToFront();
        }

        y += (int)(30 * scale);

        // ── BOTTOM STRIP (Settings & Payment) ──────────────────────────────────
        if (_bottomStrip == null)
        {
            _bottomStrip = new Panel { BackColor = UiTheme.SurfaceHigh };
            contentPanel.Controls.Add(_bottomStrip);
        }
        _bottomStrip.Location = new Point(0, y);
        _bottomStrip.Width = w;
        _bottomStrip.BringToFront();

        int sy = (int)(6 * scale); 

        // ── ROW 5: Ticket Slot | Getways | Prior Bank ────────────────
        ticketSlotCaption.Location = new Point(c1, sy + 4);
        ticketSlotCombo.Location = new Point(ticketSlotCaption.Right + 5, sy);
        ticketSlotCombo.Size = new Size(c2 - ticketSlotCombo.Left - 15, 24);
        _bottomStrip.Controls.Add(ticketSlotCaption);
        _bottomStrip.Controls.Add(ticketSlotCombo);

        gatewayCaption.Location = new Point(c2, sy + 4);
        gatewayCombo.Location = new Point(gatewayCaption.Right + 5, sy);
        gatewayCombo.Size = new Size(c3 - gatewayCombo.Left - 15, 24);
        _bottomStrip.Controls.Add(gatewayCaption);
        _bottomStrip.Controls.Add(gatewayCombo);

        priorBankCaption.Location = new Point(c3, sy + 4);
        priorBankCombo.Location = new Point(priorBankCaption.Right + 5, sy);
        priorBankCombo.Size = new Size(w - priorBankCombo.Left - 15, 24);
        _bottomStrip.Controls.Add(priorBankCaption);
        _bottomStrip.Controls.Add(priorBankCombo);

        sy += rowH;

        // ── ROW 6: Auto Upgrade | Name | Backup Bank ───────
        _autoUpgradeNative.Location = new Point(c1, sy + 2);
        _bottomStrip.Controls.Add(_autoUpgradeNative);
        
        ticketNameCaption.Location = new Point(c2, sy + 4);
        ticketNameText.Location = new Point(ticketNameCaption.Right + 5, sy);
        ticketNameText.Size = new Size(c3 - ticketNameText.Left - 15, 24);
        _bottomStrip.Controls.Add(ticketNameCaption);
        _bottomStrip.Controls.Add(ticketNameText);

        backupBankCaption.Location = new Point(c3, sy + 4);
        backupBankCombo.Location = new Point(backupBankCaption.Right + 5, sy);
        backupBankCombo.Size = new Size(w - backupBankCombo.Left - 15, 24);
        _bottomStrip.Controls.Add(backupBankCaption);
        _bottomStrip.Controls.Add(backupBankCombo);

        sy += rowH;
        
        // ── ROW 7: Confirm Berths | IRCTC User ───────
        _confirmBerthsNative.Location = new Point(c1, sy + 2);
        _bottomStrip.Controls.Add(_confirmBerthsNative);

        userCaption.Location = new Point(c2, sy + 4);
        irctcUserCombo.Location = new Point(userCaption.Right + 5, sy);
        irctcUserCombo.Size = new Size(c3 - irctcUserCombo.Left - 15, 24);
        _bottomStrip.Controls.Add(userCaption);
        _bottomStrip.Controls.Add(irctcUserCombo);

        sy += rowH + (int)(6 * scale);
        _bottomStrip.Height = sy;

        y = _bottomStrip.Bottom + (int)(4 * scale);

        // ── STATUS BAR & BUTTONS ─────────────────────────────────────────
        if (actionPanel != null)
        {
            actionPanel.Visible = false; // keep it hidden, we extracted buttons!
            if (!contentPanel.Controls.Contains(statusDotLabel))
            {
                contentPanel.Controls.Add(actionSeparator);
                contentPanel.Controls.Add(statusDotLabel);
                contentPanel.Controls.Add(statusLabel);
            }
        }

        actionSeparator.Location = new Point(0, y);
        actionSeparator.Size = new Size(w, 1);
        actionSeparator.BackColor = UiTheme.OutlineVariant;

        statusDotLabel.Location = new Point(c1, y + 7);
        statusDotLabel.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

        statusLabel.Location = new Point(statusDotLabel.Right + 5, y + 5);
        statusLabel.Font = new Font("Segoe UI", 7.5F);
        statusLabel.ForeColor = UiTheme.TextMuted;
        statusLabel.AutoSize = true;

        // Ensure buttons are on the form, placed on the far right of the status bar row
        int bx = w - 15;
        
        stopButton.Text = "Stop";
        stopButton.Visible = true;
        stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        stopButton.Size = new Size((int)(80 * scale), (int)(26 * scale));
        bx -= stopButton.Width;
        stopButton.Location = new Point(bx, y + 4);
        stopButton.FlatStyle = FlatStyle.Flat;
        stopButton.BackColor = UiTheme.PageBg;
        stopButton.ForeColor = UiTheme.Danger;
        stopButton.FlatAppearance.BorderColor = UiTheme.Danger;
        stopButton.FlatAppearance.BorderSize = 1;
        contentPanel.Controls.Add(stopButton);
        stopButton.BringToFront();

        bx -= 10;
        bookIrctcButton.Text = "Book IRCTC";
        bookIrctcButton.Visible = true;
        bookIrctcButton.AutoSize = false;
        bookIrctcButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        bookIrctcButton.Size = new Size((int)(110 * scale), (int)(26 * scale));
        bx -= bookIrctcButton.Width;
        bookIrctcButton.Location = new Point(bx, y + 4);
        bookIrctcButton.FlatStyle = FlatStyle.Flat;
        bookIrctcButton.BackColor = UiTheme.Primary;
        bookIrctcButton.ForeColor = Color.White;
        bookIrctcButton.FlatAppearance.BorderSize = 0;
        contentPanel.Controls.Add(bookIrctcButton);
        bookIrctcButton.BringToFront();

        bx -= 10;
        saveButton.Text = "Save";
        saveButton.Visible = true;
        saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        saveButton.Size = new Size((int)(80 * scale), (int)(26 * scale));
        bx -= saveButton.Width;
        saveButton.Location = new Point(bx, y + 4);
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.BackColor = UiTheme.Surface;
        saveButton.ForeColor = UiTheme.TextMuted;
        saveButton.FlatAppearance.BorderColor = UiTheme.OutlineVariant;
        saveButton.FlatAppearance.BorderSize = 1;
        contentPanel.Controls.Add(saveButton);
        saveButton.BringToFront();

        y += (int)(40 * scale);

        this.ClientSize = new Size(w, y + 6);
    }
}
