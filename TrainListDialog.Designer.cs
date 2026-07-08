namespace train_automation;

partial class TrainListDialog
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
        headerPanel = new Panel();
        routeLabel = new Label();
        quotaPanel = new FlowLayoutPanel();
        quotaGeneralRadio = new RadioButton();
        quotaLadiesRadio = new RadioButton();
        quotaTatkalRadio = new RadioButton();
        quotaPremiumRadio = new RadioButton();
        trainGrid = new DataGridView();
        headerPanel.SuspendLayout();
        quotaPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)trainGrid).BeginInit();
        SuspendLayout();
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.FromArgb(180, 160, 220);
        headerPanel.Controls.Add(routeLabel);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Size = new Size(784, 34);
        headerPanel.TabIndex = 0;
        // 
        // routeLabel
        // 
        routeLabel.AutoSize = true;
        routeLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        routeLabel.Location = new Point(10, 6);
        routeLabel.Name = "routeLabel";
        routeLabel.Size = new Size(78, 23);
        routeLabel.TabIndex = 0;
        routeLabel.Text = "Train List";
        // 
        // quotaPanel
        // 
        quotaPanel.Controls.Add(quotaGeneralRadio);
        quotaPanel.Controls.Add(quotaLadiesRadio);
        quotaPanel.Controls.Add(quotaTatkalRadio);
        quotaPanel.Controls.Add(quotaPremiumRadio);
        quotaPanel.Dock = DockStyle.Top;
        quotaPanel.Location = new Point(0, 34);
        quotaPanel.Name = "quotaPanel";
        quotaPanel.Padding = new Padding(8, 4, 8, 4);
        quotaPanel.Size = new Size(784, 36);
        quotaPanel.TabIndex = 1;
        // 
        // quotaGeneralRadio
        // 
        quotaGeneralRadio.AutoSize = true;
        quotaGeneralRadio.Checked = true;
        quotaGeneralRadio.Location = new Point(11, 7);
        quotaGeneralRadio.Name = "quotaGeneralRadio";
        quotaGeneralRadio.Size = new Size(45, 24);
        quotaGeneralRadio.TabIndex = 0;
        quotaGeneralRadio.TabStop = true;
        quotaGeneralRadio.Text = "GN";
        quotaGeneralRadio.UseVisualStyleBackColor = true;
        // 
        // quotaLadiesRadio
        // 
        quotaLadiesRadio.AutoSize = true;
        quotaLadiesRadio.Location = new Point(62, 7);
        quotaLadiesRadio.Name = "quotaLadiesRadio";
        quotaLadiesRadio.Size = new Size(43, 24);
        quotaLadiesRadio.TabIndex = 1;
        quotaLadiesRadio.Text = "LD";
        quotaLadiesRadio.UseVisualStyleBackColor = true;
        // 
        // quotaTatkalRadio
        // 
        quotaTatkalRadio.AutoSize = true;
        quotaTatkalRadio.Location = new Point(111, 7);
        quotaTatkalRadio.Name = "quotaTatkalRadio";
        quotaTatkalRadio.Size = new Size(43, 24);
        quotaTatkalRadio.TabIndex = 2;
        quotaTatkalRadio.Text = "TQ";
        quotaTatkalRadio.UseVisualStyleBackColor = true;
        // 
        // quotaPremiumRadio
        // 
        quotaPremiumRadio.AutoSize = true;
        quotaPremiumRadio.Location = new Point(160, 7);
        quotaPremiumRadio.Name = "quotaPremiumRadio";
        quotaPremiumRadio.Size = new Size(43, 24);
        quotaPremiumRadio.TabIndex = 3;
        quotaPremiumRadio.Text = "PT";
        quotaPremiumRadio.UseVisualStyleBackColor = true;
        // 
        // trainGrid
        // 
        trainGrid.AllowUserToAddRows = false;
        trainGrid.AllowUserToDeleteRows = false;
        trainGrid.BackgroundColor = SystemColors.Window;
        trainGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        trainGrid.Dock = DockStyle.Fill;
        trainGrid.Location = new Point(0, 70);
        trainGrid.MultiSelect = false;
        trainGrid.Name = "trainGrid";
        trainGrid.ReadOnly = true;
        trainGrid.RowHeadersVisible = false;
        trainGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        trainGrid.Size = new Size(784, 391);
        trainGrid.TabIndex = 2;
        trainGrid.CellClick += TrainGrid_CellClick;
        // 
        // TrainListDialog
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 461);
        Controls.Add(trainGrid);
        Controls.Add(quotaPanel);
        Controls.Add(headerPanel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "TrainListDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Train List";
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        quotaPanel.ResumeLayout(false);
        quotaPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)trainGrid).EndInit();
        ResumeLayout(false);
    }

    private Panel headerPanel;
    private Label routeLabel;
    private FlowLayoutPanel quotaPanel;
    private RadioButton quotaGeneralRadio;
    private RadioButton quotaLadiesRadio;
    private RadioButton quotaTatkalRadio;
    private RadioButton quotaPremiumRadio;
    private DataGridView trainGrid;
}
