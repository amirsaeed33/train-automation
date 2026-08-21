using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace train_automation;

public partial class Form1
{
    // contentPanel is 700px wide, no scrollbar. Controls: X=8 to X=688
    private const int LPAD = 12;   // left padding
    private const int RPAD = 12;   // right padding
    private const int FULL = 880; // contentPanel width

    // Each row: caption at rowTop, input at rowTop+19, next row at rowTop+56
    private const int INP_OFF = 19;
    private const int ROW_H   = 56;

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

        foreach (var c in controlsToMove)
        {
            if (c == journeyHeader || c == passengerHeader || c == paymentHeader || c == preferencesHeader) continue;
            contentPanel.Controls.Add(c);
        }

        int usable = FULL - LPAD - RPAD;  // 684px usable

        // ── ROW 1: FROM | TO | DATE | FIND | AVAILABILITY ──────────────
        int r1 = 8;
        int x_from  = LPAD;                     
        int x_to    = x_from + 175 + 12;        
        int x_date  = x_to   + 175 + 12;        
        int x_find  = x_date + 125 + 12;        
        int x_avail = x_find + 80 + 12;         

        fromCaption.Location      = new Point(x_from, r1);
        fromStationCombo.Location = new Point(x_from, r1 + INP_OFF);
        fromStationCombo.Width    = 175;

        toCaption.Location        = new Point(x_to, r1);
        toStationCombo.Location   = new Point(x_to, r1 + INP_OFF);
        toStationCombo.Width      = 175;

        dateCaption.Location      = new Point(x_date, r1);
        travelDatePicker.Location = new Point(x_date, r1 + INP_OFF);
        travelDatePicker.Width    = 125;

        findButton.Location       = new Point(x_find, r1 + INP_OFF);
        findButton.Size           = new Size(80, 27);

        availabilityLink.Location = new Point(x_avail, r1 + INP_OFF + 5);

        // ── ROW 2: BOARDING PT | TRAIN NO | TRAIN TYPE ──────────────────
        int r2 = r1 + ROW_H;
        int col = usable / 3;  // ~278
        int c2 = LPAD + col;
        int c3 = LPAD + col * 2;

        bdgCaption.Location         = new Point(LPAD,  r2);
        boardingPointText.Location  = new Point(LPAD,  r2 + INP_OFF);
        boardingPointText.Width     = col - 16;

        trainNoCaption.Location     = new Point(c2, r2);
        trainNoText.Location        = new Point(c2, r2 + INP_OFF);
        trainNoText.Width           = col - 16;

        trainTypeCaption.Location   = new Point(c3, r2);
        trainTypeCombo.Location     = new Point(c3, r2 + INP_OFF);
        trainTypeCombo.Width        = FULL - c3 - RPAD;
        trainTypeCombo.Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── ROW 3: CLASS | QUOTA (same row) ──────────────────────────────
        int r3    = r2 + ROW_H;
        int cls_w = 130;

        classCaption.Location  = new Point(LPAD, r3);
        classCombo.Location    = new Point(LPAD, r3 + INP_OFF);
        classCombo.Width       = cls_w;
        classCombo.MaximumSize = new Size(cls_w, 100);

        // QUOTA label starts right after CLASS combo
        int x_qcap = LPAD + cls_w + 20;   // = 162
        quotaCaption.AutoSize = false;
        quotaCaption.Width    = 80;        // fixed width so it never bleeds into pills
        quotaCaption.Location = new Point(x_qcap, r3 + INP_OFF + 3);

        // Pills start after the fixed-width QUOTA label with extra padding
        int x_pills = x_qcap + 90;        // = 232 — well clear of "QUOTA" text
        int pillGap = 8;
        int pillW1  = 82;    // "General"
        int pillW2  = 72;    // "Ladies"
        int pillW3  = 72;    // "Tatkal"
        int pillW4  = 138;   // "Premium Tatkal"

        quotaGeneralRadio.Location = new Point(x_pills,                                                          r3 + INP_OFF - 2);
        quotaGeneralRadio.Size     = new Size(pillW1, 24);

        quotaLadiesRadio.Location  = new Point(x_pills + pillW1 + pillGap,                                       r3 + INP_OFF - 2);
        quotaLadiesRadio.Size      = new Size(pillW2, 24);

        quotaTatkalRadio.Location  = new Point(x_pills + pillW1 + pillGap + pillW2 + pillGap,                    r3 + INP_OFF - 2);
        quotaTatkalRadio.Size      = new Size(pillW3, 24);

        quotaPremiumRadio.Location = new Point(x_pills + pillW1 + pillGap + pillW2 + pillGap + pillW3 + pillGap, r3 + INP_OFF - 2);
        quotaPremiumRadio.Size     = new Size(pillW4, 24);

        // ── PASSENGER TABLE ───────────────────────────────────────────────
        int tableTop = r3 + ROW_H;
        passengerGrid.Dock     = DockStyle.None;
        passengerGrid.Anchor   = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        passengerGrid.Location = new Point(LPAD, tableTop);
        passengerGrid.Width    = FULL - LPAD - RPAD;
        passengerGrid.Height   = 196;

        rupeeLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        fareText.Font   = new Font("Segoe UI", 9F, FontStyle.Bold);

        PositionBottomElements();
    }

    private void PositionBottomElements()
    {
        int usable = FULL - LPAD - RPAD;  // 684px
        // Three equal columns across 684px: col width ≈ 228px each
        int col = usable / 3;  // ~228
        int c1 = LPAD;               // 8
        int c2 = LPAD + col;         // 236
        int c3 = LPAD + col * 2;     // 464

        // ── ROW 4: MOBILE | TOTAL FARE + Get | IRCTC USERNAME ──────────
        int r4 = passengerGrid.Bottom + 10;

        mobileCaption.Location    = new Point(c1, r4);
        mobileText.Location       = new Point(c1, r4 + INP_OFF);
        mobileText.Width          = col - 16;  // ~212

        fareCaption.Location      = new Point(c2, r4);
        rupeeLabel.Location       = new Point(c2, r4 + INP_OFF + 4);
        fareText.Location         = new Point(c2 + 18, r4 + INP_OFF + 2);
        fareText.Size             = new Size(60, 23);
        getFareButton.Location    = new Point(c2 + 86, r4 + INP_OFF);
        getFareButton.Size        = new Size(85, 27);
        getFareButton.BackColor   = UiTheme.Primary;
        getFareButton.ForeColor   = Color.White;
        getFareButton.FlatStyle   = FlatStyle.Flat;
        getFareButton.FlatAppearance.BorderSize = 0;

        userCaption.Location      = new Point(c3, r4);
        irctcUserCombo.Location   = new Point(c3, r4 + INP_OFF);
        irctcUserCombo.Width      = FULL - c3 - RPAD;  // fills to right edge
        irctcUserCombo.Anchor     = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── ROW 5: TICKET SLOT | GATEWAY | PRIOR BANK / UPI ────────────
        int r5 = r4 + ROW_H;

        ticketSlotCaption.Location = new Point(c1, r5);
        ticketSlotCombo.Location   = new Point(c1, r5 + INP_OFF);
        ticketSlotCombo.Width      = col - 16;

        gatewayCaption.Location    = new Point(c2, r5);
        gatewayCombo.Location      = new Point(c2, r5 + INP_OFF);
        gatewayCombo.Width         = col - 16;

        priorBankCaption.Location  = new Point(c3, r5);
        priorBankCombo.Location    = new Point(c3, r5 + INP_OFF);
        priorBankCombo.Width       = FULL - c3 - RPAD;
        priorBankCombo.Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── ROW 6: CONFIRM BERTHS | TICKET PROFILE NAME | BACKUP BANK ──
        int r6 = r5 + ROW_H;

        confirmBerthsTitleLabel.Location = new Point(c1, r6);
        confirmBerthsCheck.Location      = new Point(c1, r6 + INP_OFF + 1);

        ticketNameCaption.Location  = new Point(c2, r6);
        ticketNameText.Location     = new Point(c2, r6 + INP_OFF);
        ticketNameText.Width        = col - 16;

        backupBankCaption.Location  = new Point(c3, r6);
        backupBankCombo.Location    = new Point(c3, r6 + INP_OFF);
        backupBankCombo.Width       = FULL - c3 - RPAD;
        backupBankCombo.Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── ROW 7: BETA UI | REAL CHROME ─────────────────────────────────
        int r7 = r6 + ROW_H;

        useBetaViewTitleLabel.Location  = new Point(c1, r7);
        useBetaViewCheck.Location       = new Point(c1, r7 + INP_OFF - 2);

        useRealChromeTitleLabel.Location = new Point(c2, r7);
        useRealChromeCheck.Location      = new Point(c2, r7 + INP_OFF - 2);

        // Tightly size the form so no empty space below row 7
        this.ClientSize = new Size(FULL, 56 /* titlePanel */ + r7 + 35 + 64 /* actionPanel */);
    }
}
