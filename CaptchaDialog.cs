namespace train_automation;

public sealed class CaptchaDialog : Form
{
    private readonly PictureBox _imageBox = new() { SizeMode = PictureBoxSizeMode.StretchImage, Size = new Size(280, 40) };
    private readonly TextBox _answerBox = new() { Width = 120 };
    private readonly Button _okButton = new() { Text = "OK", DialogResult = DialogResult.OK };
    private readonly Button _cancelButton = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

    public string Answer => _answerBox.Text.Trim();

    public CaptchaDialog(byte[] imageBytes)
    {
        Text = "Enter Captcha";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(320, 150);
        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        using var stream = new MemoryStream(imageBytes);
        _imageBox.Image = Image.FromStream(stream);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        layout.Controls.Add(new Label { Text = "Please enter the captcha shown below:", AutoSize = true }, 0, 0);
        layout.Controls.Add(_imageBox, 0, 1);
        layout.Controls.Add(_answerBox, 0, 2);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        buttons.Controls.Add(_cancelButton);
        buttons.Controls.Add(_okButton);
        layout.Controls.Add(buttons, 0, 3);
        Controls.Add(layout);
    }
}
