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
    public int ScrollOffset { get; set; }
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
            ScrollOffset = 0;
            return;
        }

        SelectedIndex = Math.Clamp(SelectedIndex + offset, 0, itemCount - 1);
    }

    public void PageSelection(int offset, int itemCount)
    {
        if (itemCount <= 0)
        {
            SelectedIndex = 0;
            ScrollOffset = 0;
            return;
        }

        SelectedIndex = Math.Clamp(SelectedIndex + offset, 0, itemCount - 1);
    }

    public void MoveToStart()
    {
        SelectedIndex = 0;
        ScrollOffset = 0;
    }

    public void MoveToEnd(int itemCount)
    {
        if (itemCount <= 0)
        {
            SelectedIndex = 0;
            ScrollOffset = 0;
            return;
        }

        SelectedIndex = itemCount - 1;
    }

    public void ClampSelection(int itemCount)
    {
        if (itemCount <= 0)
        {
            SelectedIndex = 0;
            ScrollOffset = 0;
            return;
        }

        SelectedIndex = Math.Clamp(SelectedIndex, 0, itemCount - 1);
        ScrollOffset = Math.Clamp(ScrollOffset, 0, itemCount - 1);
    }

    public void EnsureSelectionVisible(int visibleRowCount, int itemCount)
    {
        if (itemCount <= 0 || visibleRowCount <= 0)
        {
            ScrollOffset = 0;
            return;
        }

        if (SelectedIndex < ScrollOffset)
            ScrollOffset = SelectedIndex;

        if (SelectedIndex >= ScrollOffset + visibleRowCount)
            ScrollOffset = SelectedIndex - visibleRowCount + 1;

        int maxScrollOffset = Math.Max(0, itemCount - visibleRowCount);
        ScrollOffset = Math.Clamp(ScrollOffset, 0, maxScrollOffset);
    }

    public void NextView()
    {
        ViewMode = ViewMode switch
        {
            TuiViewMode.Active => TuiViewMode.Archived,
            TuiViewMode.Archived => TuiViewMode.All,
            _ => TuiViewMode.Active
        };

        ResetListPosition();
    }

    public void PreviousView()
    {
        ViewMode = ViewMode switch
        {
            TuiViewMode.Active => TuiViewMode.All,
            TuiViewMode.All => TuiViewMode.Archived,
            _ => TuiViewMode.Active
        };

        ResetListPosition();
    }

    public void ResetListPosition()
    {
        SelectedIndex = 0;
        ScrollOffset = 0;
    }
}
