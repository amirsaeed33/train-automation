namespace train_automation;

public sealed class TrainListDialog : Form
{
    private readonly DataGridView _grid = new();
    private readonly Label _headerLabel = new();
    private readonly Font _classLinkFont;

    public event EventHandler<TrainClassSelectedEventArgs>? ClassSelected;

    public TrainListDialog()
    {
        Text = "Train List";
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1040, 380);
        MinimumSize = new Size(760, 280);
        _classLinkFont = new Font(Font, FontStyle.Underline);

        BackColor = UiTheme.PageBg;

        _headerLabel.Dock = DockStyle.Top;
        _headerLabel.Height = 32;
        _headerLabel.BackColor = UiTheme.SurfaceHigh;
        _headerLabel.ForeColor = UiTheme.Text;
        _headerLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _headerLabel.Padding = new Padding(8, 6, 8, 6);
        _headerLabel.Text = "Train List — click a class to select for IRCTC booking";

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.AutoGenerateColumns = false;
        _grid.BackgroundColor = UiTheme.PageBg;
        _grid.BorderStyle = BorderStyle.None;
        _grid.GridColor = UiTheme.OutlineVariant;
        _grid.EnableHeadersVisualStyles = false;

        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            SelectionBackColor = UiTheme.Surface
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.Surface,
            ForeColor = UiTheme.Text,
            Font = new Font("Segoe UI", 9F),
            SelectionBackColor = UiTheme.SurfaceHigh,
            SelectionForeColor = UiTheme.Text
        };

        _grid.CellMouseClick += Grid_CellMouseClick;
        _grid.CellFormatting += Grid_CellFormatting;
        _grid.CellMouseMove += Grid_CellMouseMove;

        Controls.Add(_grid);
        Controls.Add(_headerLabel);
        ConfigureGrid();
    }

    public void ShowTrains(IReadOnlyList<TrainResult> trains, string routeTitle)
    {
        _headerLabel.Text = $"Train List ({routeTitle}) — click a class";
        _grid.DataSource = trains.ToList();
        StyleCells();
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        AddCol(nameof(TrainResult.TrainNumber), "No", 60);
        AddCol(nameof(TrainResult.TrainName), "Train", 190);
        AddCol(nameof(TrainResult.FromStation), "From", 60);
        AddCol(nameof(TrainResult.Departure), "Depart", 70);
        AddCol(nameof(TrainResult.ToStation), "To", 60);
        AddCol(nameof(TrainResult.Arrival), "Arrival", 70);
        AddCol(nameof(TrainResult.TravelTime), "Travel", 65);
        AddCol(nameof(TrainResult.AvailableClasses), "Classes", 190);
        AddCol(nameof(TrainResult.Monday), "M", 32);
        AddCol(nameof(TrainResult.Tuesday), "T", 32);
        AddCol(nameof(TrainResult.Wednesday), "W", 32);
        AddCol(nameof(TrainResult.Thursday), "T", 32);
        AddCol(nameof(TrainResult.Friday), "F", 32);
        AddCol(nameof(TrainResult.Saturday), "S", 32);
        AddCol(nameof(TrainResult.Sunday), "S", 32);
    }

    private void AddCol(string property, string header, int width)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Resizable = DataGridViewTriState.True
        });
    }

    private void StyleCells()
    {
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is not TrainResult)
            {
                continue;
            }

            foreach (DataGridViewCell cell in row.Cells)
            {
                var column = _grid.Columns[cell.ColumnIndex];
                if (column.DataPropertyName.Equals(nameof(TrainResult.AvailableClasses), StringComparison.Ordinal))
                {
                    cell.Style.ForeColor = Color.LightSkyBlue;
                    cell.Style.Font = _classLinkFont;
                    cell.Style.SelectionForeColor = Color.LightSkyBlue;
                    continue;
                }

                if (column.DataPropertyName is nameof(TrainResult.Monday) or nameof(TrainResult.Tuesday)
                    or nameof(TrainResult.Wednesday) or nameof(TrainResult.Thursday) or nameof(TrainResult.Friday)
                    or nameof(TrainResult.Saturday) or nameof(TrainResult.Sunday))
                {
                    var runs = cell.Value?.ToString() is "Y";
                    cell.Style.ForeColor = runs ? Color.MediumSeaGreen : UiTheme.TextMuted;
                    cell.Style.Font = runs ? new Font(_grid.Font, FontStyle.Bold) : _grid.Font;
                }
            }
        }
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        var column = _grid.Columns[e.ColumnIndex];
        if (!column.DataPropertyName.Equals(nameof(TrainResult.AvailableClasses), StringComparison.Ordinal))
        {
            return;
        }

        e.CellStyle.ForeColor = Color.LightSkyBlue;
        e.CellStyle.Font = _classLinkFont;
        e.CellStyle.SelectionForeColor = Color.LightSkyBlue;
    }

    private void Grid_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            _grid.Cursor = Cursors.Default;
            return;
        }

        var column = _grid.Columns[e.ColumnIndex];
        _grid.Cursor = column.DataPropertyName.Equals(nameof(TrainResult.AvailableClasses), StringComparison.Ordinal)
            ? Cursors.Hand
            : Cursors.Default;
    }

    private void Grid_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        var column = _grid.Columns[e.ColumnIndex];
        if (!column.DataPropertyName.Equals(nameof(TrainResult.AvailableClasses), StringComparison.Ordinal))
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].DataBoundItem is not TrainResult train)
        {
            return;
        }

        var classes = GetTrainClasses(train);
        if (classes.Count == 0)
        {
            MessageBox.Show(this, "No travel classes found for this train.", "Class",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var clicked = GetClickedClass(classes, e.Location);
        if (classes.Count > 1 && clicked is null)
        {
            using var menu = new ContextMenuStrip();
            foreach (var classCode in classes)
            {
                menu.Items.Add(classCode, null, (_, _) => RaiseClassSelected(train, classCode));
            }

            var cellRect = _grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
            menu.Show(_grid, cellRect.Left + e.Location.X, cellRect.Top + e.Location.Y);
            return;
        }

        RaiseClassSelected(train, clicked ?? classes[0]);
    }

    private static List<string> GetTrainClasses(TrainResult train)
    {
        if (train.ClassLinkKeys.Count > 0)
        {
            return train.ClassLinkKeys.Keys.ToList();
        }

        return train.AvailableClasses
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string? GetClickedClass(IReadOnlyList<string> classes, Point clickLocation)
    {
        if (classes.Count == 0)
        {
            return null;
        }

        using var graphics = _grid.CreateGraphics();
        float offset = 6;
        foreach (var classCode in classes)
        {
            var segment = classCode + "  ";
            var width = graphics.MeasureString(segment, _classLinkFont).Width;
            if (clickLocation.X >= offset && clickLocation.X < offset + width)
            {
                return classCode;
            }

            offset += width;
        }

        return null;
    }

    private void RaiseClassSelected(TrainResult train, string travelClass)
    {
        string classLinkKey;
        if (train.ClassLinkKeys.TryGetValue(travelClass, out var existingKey)
            && !string.IsNullOrWhiteSpace(existingKey))
        {
            classLinkKey = existingKey;
        }
        else
        {
            // Key can be resolved later from the live Indian Railways page
            classLinkKey = $"{train.TrainNumber}^{travelClass}";
        }

        ClassSelected?.Invoke(this, new TrainClassSelectedEventArgs(train, travelClass, classLinkKey));
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // TrainListDialog
        // 
        ClientSize = new Size(282, 253);
        Name = "TrainListDialog";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Train list";
        ResumeLayout(false);

    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _classLinkFont.Dispose();
        }

        base.Dispose(disposing);
    }
}

public sealed class TrainClassSelectedEventArgs : EventArgs
{
    public TrainClassSelectedEventArgs(TrainResult train, string travelClass, string classLinkKey)
    {
        Train = train;
        TravelClass = travelClass;
        ClassLinkKey = classLinkKey;
    }

    public TrainResult Train { get; }
    public string TravelClass { get; }
    public string ClassLinkKey { get; }
}
