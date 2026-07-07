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
            searchPanel = new Panel();
            searchButton = new Button();
            travelDatePicker = new DateTimePicker();
            dateLabel = new Label();
            toStationCombo = new ComboBox();
            toLabel = new Label();
            fromStationCombo = new ComboBox();
            fromLabel = new Label();
            trainGrid = new DataGridView();
            searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trainGrid).BeginInit();
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
            // searchPanel
            // 
            searchPanel.Controls.Add(searchButton);
            searchPanel.Controls.Add(travelDatePicker);
            searchPanel.Controls.Add(dateLabel);
            searchPanel.Controls.Add(toStationCombo);
            searchPanel.Controls.Add(toLabel);
            searchPanel.Controls.Add(fromStationCombo);
            searchPanel.Controls.Add(fromLabel);
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Location = new Point(0, 42);
            searchPanel.Name = "searchPanel";
            searchPanel.Padding = new Padding(12, 8, 12, 8);
            searchPanel.Size = new Size(1184, 56);
            searchPanel.TabIndex = 1;
            // 
            // searchButton
            // 
            searchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            searchButton.Location = new Point(1048, 10);
            searchButton.Name = "searchButton";
            searchButton.Size = new Size(120, 32);
            searchButton.TabIndex = 6;
            searchButton.Text = "Search";
            searchButton.UseVisualStyleBackColor = true;
            searchButton.Click += SearchButton_Click;
            // 
            // travelDatePicker
            // 
            travelDatePicker.Format = DateTimePickerFormat.Short;
            travelDatePicker.Location = new Point(892, 12);
            travelDatePicker.MinDate = DateTime.Today;
            travelDatePicker.Name = "travelDatePicker";
            travelDatePicker.Size = new Size(140, 27);
            travelDatePicker.TabIndex = 5;
            // 
            // dateLabel
            // 
            dateLabel.AutoSize = true;
            dateLabel.Location = new Point(844, 15);
            dateLabel.Name = "dateLabel";
            dateLabel.Size = new Size(42, 20);
            dateLabel.TabIndex = 4;
            dateLabel.Text = "Date";
            // 
            // toStationCombo
            // 
            toStationCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            toStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            toStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            toStationCombo.FormattingEnabled = true;
            toStationCombo.Location = new Point(494, 12);
            toStationCombo.Name = "toStationCombo";
            toStationCombo.Size = new Size(330, 28);
            toStationCombo.TabIndex = 3;
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(462, 15);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(25, 20);
            toLabel.TabIndex = 2;
            toLabel.Text = "To";
            // 
            // fromStationCombo
            // 
            fromStationCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            fromStationCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            fromStationCombo.FormattingEnabled = true;
            fromStationCombo.Location = new Point(62, 12);
            fromStationCombo.Name = "fromStationCombo";
            fromStationCombo.Size = new Size(380, 28);
            fromStationCombo.TabIndex = 1;
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(12, 15);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(44, 20);
            fromLabel.TabIndex = 0;
            fromLabel.Text = "From";
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
            trainGrid.Location = new Point(0, 98);
            trainGrid.MultiSelect = false;
            trainGrid.Name = "trainGrid";
            trainGrid.ReadOnly = true;
            trainGrid.RowHeadersVisible = false;
            trainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            trainGrid.Size = new Size(1184, 563);
            trainGrid.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 661);
            Controls.Add(trainGrid);
            Controls.Add(searchPanel);
            Controls.Add(statusLabel);
            MinimumSize = new Size(900, 500);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tripozo Train Availability";
            Load += Form1_Load;
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trainGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label statusLabel;
        private Panel searchPanel;
        private Label fromLabel;
        private ComboBox fromStationCombo;
        private Label toLabel;
        private ComboBox toStationCombo;
        private Label dateLabel;
        private DateTimePicker travelDatePicker;
        private Button searchButton;
        private DataGridView trainGrid;
    }
}
