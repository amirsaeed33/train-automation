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

    private void FlattenLayout()
    {
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
                lbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                lbl.ForeColor = UiTheme.Text;
                lbl.AutoSize = true;
            }
        }

        // Revert form background
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
        fareText.Visible      = false;  // hide the big-font fare amount; "Fare: Base" label is sufficient
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
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = UiTheme.Text,
        };
        _confirmBerthsNative = new CheckBox 
        { 
            Text = "Book only if confirm berths allotted.", 
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = UiTheme.Text,
        };
        contentPanel.Controls.Add(_autoUpgradeNative);
        contentPanel.Controls.Add(_confirmBerthsNative);
        _autoUpgradeNative.CheckedChanged += (_, _) => useBetaViewCheck.Checked = _autoUpgradeNative.Checked;
        _confirmBerthsNative.CheckedChanged += (_, _) => confirmBerthsCheck.Checked = _confirmBerthsNative.Checked;

        int y = 15;
        int rowH = 32;

        // ── ROW 1 ──────────────────────────
        fromCaption.Location = new Point(15, y + 4);
        fromStationCombo.Location = new Point(100, y); // pushed right
        fromStationCombo.Width = 160;

        toCaption.Location = new Point(280, y + 4);
        toStationCombo.Location = new Point(340, y); // pushed right
        toStationCombo.Width = 160;

        dateCaption.Location = new Point(520, y + 4);
        travelDatePicker.Location = new Point(590, y); // pushed right
        travelDatePicker.Width = 140;
        
        findButton.Location = new Point(750, y - 2);
        findButton.Size = new Size(80, 28);
        findButton.FlatStyle = FlatStyle.Flat;
        findButton.BackColor = UiTheme.Primary;
        findButton.ForeColor = Color.White;
        findButton.FlatAppearance.BorderSize = 0;

        y += rowH;

        // ── ROW 2 ──────────────────────────
        bdgCaption.Location = new Point(15, y + 4);
        boardingPointText.Location = new Point(100, y);
        boardingPointText.Width = 160;

        trainNoCaption.Location = new Point(280, y + 4);
        trainNoText.Location = new Point(370, y);
        trainNoText.Width = 130;

        trainTypeCaption.Location = new Point(520, y + 4);
        trainTypeCombo.Location = new Point(620, y);
        trainTypeCombo.Width = 110;

        availabilityLink.Location = new Point(750, y + 4);

        y += rowH;

        // ── ROW 3 ──────────────────────────
        classCaption.Location = new Point(15, y + 4);
        classCombo.Location = new Point(100, y);
        classCombo.Width = 160;
        
        quotaCaption.Location = new Point(280, y + 4);
        
        var radios = new[] { quotaGeneralRadio, quotaLadiesRadio, quotaTatkalRadio, quotaPremiumRadio };
        int qx = 350;
        foreach (var r in radios)
        {
            r.Appearance = Appearance.Normal;
            r.AutoSize = true;
            r.BackColor = Color.Transparent;
            r.ForeColor = UiTheme.Text;
            r.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            r.Location = new Point(qx, y + 2);
            qx = r.Right + 12;
        }

        y += rowH;

        // ── PASSENGER TABLE ──────────────────────────
        passengerGrid.Dock = DockStyle.None;
        passengerGrid.Location = new Point(15, y);
        // Widened grid so columns fit better
        passengerGrid.Width = 825;

        PositionBottomElements();
    }

    private void PositionBottomElements()
    {
        if (_autoUpgradeNative == null) return; 

        // Fix radio button widths manually
        quotaGeneralRadio.Left = 350;
        quotaLadiesRadio.Left = 430;
        quotaTatkalRadio.Left = 500;
        quotaPremiumRadio.Left = 570;

        int y = passengerGrid.Bottom + 15;
        int rowH = 32;

        // ── ROW 4: Mobile | Fare label | Get Fare ─────────────────────────────────
        mobileCaption.Location = new Point(15, y + 4);
        mobileText.Location = new Point(120, y);
        mobileText.Width = 140;

        rupeeLabel.Location = new Point(280, y + 4);
        rupeeLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        getFareButton.Location = new Point(380, y);
        getFareButton.Size = new Size(90, 26);
        getFareButton.BackColor = UiTheme.Primary;
        getFareButton.ForeColor = Color.White;
        getFareButton.FlatStyle = FlatStyle.Flat;
        getFareButton.FlatAppearance.BorderSize = 0;

        y += rowH;

        // ── ROW 5: Ticket Slot | Getways | Prior Bank ────────────────
        ticketSlotCaption.Location = new Point(15, y + 4);
        ticketSlotCombo.Location = new Point(120, y);
        ticketSlotCombo.Width = 140;

        gatewayCaption.Location = new Point(280, y + 4);
        gatewayCombo.Location = new Point(360, y);
        gatewayCombo.Width = 140;

        priorBankCaption.Location = new Point(520, y + 4);
        priorBankCombo.Location = new Point(620, y);
        priorBankCombo.Width = 155;

        y += rowH;

        // ── ROW 6: Auto Upgrade | Backup Bank ───────
        _autoUpgradeNative.Location = new Point(15, y);
        
        backupBankCaption.Location = new Point(520, y + 2);
        backupBankCombo.Location = new Point(620, y - 2);
        backupBankCombo.Width = 155;

        y += rowH;
        
        // ── ROW 7: Confirm Berths ───────
        _confirmBerthsNative.Location = new Point(15, y);
        y += rowH;

        // ── ROW 8: IRCTC User | Name | Save | Book IRCTC | Stop ──────────────────────────
        userCaption.Location = new Point(15, y + 4);
        irctcUserCombo.Location = new Point(110, y);
        irctcUserCombo.Width = 150;

        ticketNameCaption.Location = new Point(280, y + 4);
        ticketNameText.Location = new Point(340, y);
        ticketNameText.Width = 160;

        // Save button
        contentPanel.Controls.Add(saveButton);
        saveButton.Location = new Point(520, y - 2);
        saveButton.Size = new Size(80, 32);
        saveButton.FlatStyle = FlatStyle.Flat;
        saveButton.BackColor = UiTheme.SurfaceLow;
        saveButton.ForeColor = UiTheme.TextMuted;
        saveButton.FlatAppearance.BorderColor = UiTheme.OutlineVariant;
        saveButton.FlatAppearance.BorderSize = 1;
        saveButton.BringToFront();

        // Book IRCTC button
        contentPanel.Controls.Add(bookIrctcButton);
        bookIrctcButton.Location = new Point(610, y - 4);
        bookIrctcButton.Size = new Size(130, 36);
        bookIrctcButton.FlatStyle = FlatStyle.Flat;
        bookIrctcButton.BackColor = UiTheme.Primary;
        bookIrctcButton.ForeColor = Color.White;
        bookIrctcButton.FlatAppearance.BorderSize = 0;
        bookIrctcButton.BringToFront();

        // Stop button
        contentPanel.Controls.Add(stopButton);
        stopButton.Location = new Point(750, y - 2);
        stopButton.Size = new Size(80, 32); // Widened from 60 to 80 to prevent "Sto" clipping
        stopButton.FlatStyle = FlatStyle.Flat;
        stopButton.BackColor = UiTheme.PageBg;
        stopButton.ForeColor = UiTheme.Danger;
        stopButton.FlatAppearance.BorderColor = UiTheme.Danger;
        stopButton.FlatAppearance.BorderSize = 1;
        stopButton.BringToFront();

        y += 44;

        // ── STATUS BAR ──────────────────────────────────────────────────────────
        // Restore status bar — shows booking progress messages (statusLabel.Text = ...)
        if (actionPanel != null)
        {
            actionPanel.Visible = true;
            actionPanel.Dock = DockStyle.None;
            actionPanel.Location = new Point(0, y);
            actionPanel.Size = new Size(860, 30);
            actionPanel.BackColor = UiTheme.SurfaceLow;
            // Move statusDotLabel and statusLabel into contentPanel so they're visible
            if (!contentPanel.Controls.Contains(statusDotLabel))
            {
                contentPanel.Controls.Add(actionSeparator);
                contentPanel.Controls.Add(statusDotLabel);
                contentPanel.Controls.Add(statusLabel);
            }
            actionPanel.Visible = false; // panel itself hidden; controls moved to contentPanel
        }

        actionSeparator.Location = new Point(0, y);
        actionSeparator.Size = new Size(860, 1);
        actionSeparator.BackColor = UiTheme.OutlineVariant;

        statusDotLabel.Location = new Point(10, y + 7);
        statusDotLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        statusLabel.Location = new Point(28, y + 5);
        statusLabel.Font = new Font("Segoe UI", 9F);
        statusLabel.ForeColor = UiTheme.TextMuted;
        statusLabel.AutoSize = true;

        y += 30;

        this.ClientSize = new Size(860, y + 6);
    }
}
