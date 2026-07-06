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
            trainGrid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)trainGrid).BeginInit();
            SuspendLayout();
            // 
            // statusLabel
            // 
            statusLabel.Dock = DockStyle.Top;
            statusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            statusLabel.Location = new Point(0, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Padding = new Padding(12, 10, 12, 10);
            statusLabel.Size = new Size(1184, 42);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "Starting train search...";
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
            trainGrid.Location = new Point(0, 42);
            trainGrid.MultiSelect = false;
            trainGrid.Name = "trainGrid";
            trainGrid.ReadOnly = true;
            trainGrid.RowHeadersVisible = false;
            trainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            trainGrid.Size = new Size(1184, 619);
            trainGrid.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 661);
            Controls.Add(trainGrid);
            Controls.Add(statusLabel);
            MinimumSize = new Size(900, 500);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tripozo Train Availability";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)trainGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label statusLabel;
        private DataGridView trainGrid;
    }
}
