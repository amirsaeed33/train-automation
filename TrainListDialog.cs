namespace train_automation;

public partial class TrainListDialog : Form
{
    private static readonly string[] DayColumns =
    [
        nameof(TrainResult.Monday),
        nameof(TrainResult.Tuesday),
        nameof(TrainResult.Wednesday),
        nameof(TrainResult.Thursday),
        nameof(TrainResult.Friday),
        nameof(TrainResult.Saturday),
        nameof(TrainResult.Sunday)
    ];

    private static readonly string[] DayNames =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    ];

    private readonly IReadOnlyList<TrainResult> _trains;

    public TrainSelection? SelectedTrain { get; private set; }

    public TrainListDialog(IReadOnlyList<TrainResult> trains, string routeTitle)
    {
        _trains = trains;
        InitializeComponent();
        Text = "Train List";
        routeLabel.Text = routeTitle;
        ConfigureGrid();
        trainGrid.DataSource = _trains.ToList();
        StyleDayCells();
    }

    private void ConfigureGrid()
    {
        trainGrid.AutoGenerateColumns = false;
        trainGrid.Columns.Clear();

        AddCol(nameof(TrainResult.TrainNumber), "Train No", 65);
        AddCol(nameof(TrainResult.TrainName), "Train Name", 120);
        AddCol(nameof(TrainResult.FromStation), "From", 45);
        AddCol(nameof(TrainResult.Departure), "Depart", 55);
        AddCol(nameof(TrainResult.ToStation), "To", 45);
        AddCol(nameof(TrainResult.Arrival), "Arrival", 55);
        AddCol(nameof(TrainResult.TravelTime), "Travel", 55);
        AddCol(nameof(TrainResult.Monday), "M", 28);
        AddCol(nameof(TrainResult.Tuesday), "T", 28);
        AddCol(nameof(TrainResult.Wednesday), "W", 28);
        AddCol(nameof(TrainResult.Thursday), "T", 28);
        AddCol(nameof(TrainResult.Friday), "F", 28);
        AddCol(nameof(TrainResult.Saturday), "S", 28);
        AddCol(nameof(TrainResult.Sunday), "S", 28);
        AddCol(nameof(TrainResult.AvailableClasses), "Classes", 100);
    }

    private void AddCol(string property, string header, int width)
    {
        trainGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void StyleDayCells()
    {
        foreach (DataGridViewRow row in trainGrid.Rows)
        {
            if (row.DataBoundItem is not TrainResult)
            {
                continue;
            }

            for (var index = 0; index < DayColumns.Length; index++)
            {
                var column = trainGrid.Columns[DayColumns[index]];
                if (column is null)
                {
                    continue;
                }

                var cell = row.Cells[column.Index];
                var runs = cell.Value?.ToString() is "X" or "Y";
                cell.Style.ForeColor = runs ? Color.DarkGreen : Color.LightGray;
                cell.Style.Font = runs ? new Font(trainGrid.Font, FontStyle.Bold) : trainGrid.Font;
            }
        }
    }

    private void TrainGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        var column = trainGrid.Columns[e.ColumnIndex];
        var dayIndex = Array.IndexOf(DayColumns, column.DataPropertyName);
        if (dayIndex < 0)
        {
            return;
        }

        if (trainGrid.Rows[e.RowIndex].DataBoundItem is not TrainResult train)
        {
            return;
        }

        var dayValue = column.DataPropertyName switch
        {
            nameof(TrainResult.Monday) => train.Monday,
            nameof(TrainResult.Tuesday) => train.Tuesday,
            nameof(TrainResult.Wednesday) => train.Wednesday,
            nameof(TrainResult.Thursday) => train.Thursday,
            nameof(TrainResult.Friday) => train.Friday,
            nameof(TrainResult.Saturday) => train.Saturday,
            nameof(TrainResult.Sunday) => train.Sunday,
            _ => string.Empty
        };

        if (dayValue is not "X" and not "Y")
        {
            return;
        }

        SelectedTrain = new TrainSelection
        {
            Train = train,
            SelectedDay = DayNames[dayIndex]
        };
        DialogResult = DialogResult.OK;
        Close();
    }
}
