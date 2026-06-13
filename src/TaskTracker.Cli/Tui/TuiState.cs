namespace TaskTracker.Cli.Tui;

public enum TuiViewMode
{
    Active,
    Archived,
    All
}

public class TuiState
{
    public int SelectedIndex { get; set; }
    public TuiViewMode ViewMode { get; set; } = TuiViewMode.Active;
    public string? SearchQuery { get; set; }
    public string? StatusMessage { get; set; }

    public bool? ArchivedFilter => ViewMode switch
    {
        TuiViewMode.Archived => true,
        TuiViewMode.All => null,
        _ => false
    };

    public string ViewTitle => ViewMode switch
    {
        TuiViewMode.Archived => "Archived",
        TuiViewMode.All => "All Tasks",
        _ => "Active"
    };

    public void MoveSelection(int offset, int itemCount)
    {
        if (itemCount <= 0)
        {
            SelectedIndex = 0;
            return;
        }

        SelectedIndex = Math.Clamp(SelectedIndex + offset, 0, itemCount - 1);
    }

    public void ClampSelection(int itemCount)
    {
        if (itemCount <= 0)
        {
            SelectedIndex = 0;
            return;
        }

        SelectedIndex = Math.Clamp(SelectedIndex, 0, itemCount - 1);
    }

    public void NextView()
    {
        ViewMode = ViewMode switch
        {
            TuiViewMode.Active => TuiViewMode.Archived,
            TuiViewMode.Archived => TuiViewMode.All,
            _ => TuiViewMode.Active
        };

        SelectedIndex = 0;
    }

    public void PreviousView()
    {
        ViewMode = ViewMode switch
        {
            TuiViewMode.Active => TuiViewMode.All,
            TuiViewMode.All => TuiViewMode.Archived,
            _ => TuiViewMode.Active
        };

        SelectedIndex = 0;
    }
}
