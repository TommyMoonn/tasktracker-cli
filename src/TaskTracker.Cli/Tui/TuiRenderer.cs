using TaskTracker.Cli.Models;

namespace TaskTracker.Cli.Tui;

public static class TuiRenderer
{
    private const int DefaultWidth = 104;
    private const int DefaultHeight = 30;
    private const int MaxTaskRows = 18;
    private const int MinTaskRows = 5;
    private static int _lastRenderLineCount;

    public static void Draw(TuiState state, IReadOnlyList<TaskItem> tasks)
    {
        int width = GetSafeWidth();
        int pageSize = GetPageSize();
        state.EnsureSelectionVisible(pageSize, tasks.Count);

        BeginFrame();

        WriteHeader(state, tasks, width);
        WriteBlankLine(width);

        if (tasks.Count == 0)
        {
            WriteEmptyState(state, width);
        }
        else
        {
            WriteTaskRows(state, tasks, width, pageSize);
            WriteBlankLine(width);
            WriteSelectedTask(tasks[state.SelectedIndex], width);
        }

        WriteBlankLine(width);
        WriteFooter(width);

        if (!string.IsNullOrWhiteSpace(state.StatusMessage))
        {
            WriteBlankLine(width);
            WriteStatusMessage(state.StatusMessage, width);
        }

        EndFrame(width);
    }

    public static int GetPageSize()
    {
        int height = GetSafeHeight();
        return Math.Clamp(height - 15, MinTaskRows, MaxTaskRows);
    }

    public static void DrawPromptHeader(string title, string subtitle)
    {
        ResetFrame();
        Console.Clear();
        int width = GetSafeWidth();
        WriteBoxLine('┌', '─', '┐', width);
        WriteBoxText(title, width, ConsoleColor.Cyan);
        WriteBoxText(subtitle, width, ConsoleColor.DarkGray);
        WriteBoxLine('└', '─', '┘', width);
        WriteBlankLine(width);
    }

    public static void ShowPause(string message)
    {
        WriteBlankLine(GetSafeWidth());
        WriteMuted(message);
        Console.ReadKey(intercept: true);
    }

    private static void WriteHeader(TuiState state, IReadOnlyList<TaskItem> tasks, int width)
    {
        int open = tasks.Count(t => !t.IsCompleted && !t.IsArchived);
        int done = tasks.Count(t => t.IsCompleted && !t.IsArchived);
        int archived = tasks.Count(t => t.IsArchived);

        string title = $"TaskTracker TUI  |  {BuildViewTabs(state.ViewMode)}";
        string search = string.IsNullOrWhiteSpace(state.SearchQuery) ? "search: off" : $"search: {state.SearchQuery}";
        string position = tasks.Count == 0 ? "0/0" : $"{state.SelectedIndex + 1}/{tasks.Count}";
        string subtitle = $"{position} selected | open {open} | done {done} | archived {archived} | {search}";

        WriteBoxLine('╔', '═', '╗', width, ConsoleColor.Cyan);
        WriteBoxText(title, width, ConsoleColor.White);
        WriteBoxText(subtitle, width, ConsoleColor.DarkGray);
        WriteBoxLine('╚', '═', '╝', width, ConsoleColor.Cyan);
    }

    private static string BuildViewTabs(TuiViewMode viewMode)
    {
        static string Tab(string text, bool active) => active ? $"[{text}]" : $" {text} ";

        return string.Join(" ",
            Tab("Active", viewMode == TuiViewMode.Active),
            Tab("Archived", viewMode == TuiViewMode.Archived),
            Tab("All", viewMode == TuiViewMode.All));
    }

    private static void WriteTaskRows(TuiState state, IReadOnlyList<TaskItem> tasks, int width, int pageSize)
    {
        int markerWidth = 1;
        int idWidth = 4;
        int statusWidth = 9;
        int priorityWidth = 8;
        int dueWidth = 12;

        int fixedWidth = markerWidth + idWidth + statusWidth + priorityWidth + dueWidth + 19;
        int titleWidth = Math.Max(24, width - fixedWidth);

        int startIndex = state.ScrollOffset;
        int endIndex = Math.Min(tasks.Count, startIndex + pageSize);
        bool hasMoreAbove = startIndex > 0;
        bool hasMoreBelow = endIndex < tasks.Count;

        WriteTableBorder('┌', '┬', '┐', markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);
        WriteTableRow("", "ID", "Status", "Priority", "Due", "Title", markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, true);
        WriteTableBorder('├', '┼', '┤', markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);

        if (hasMoreAbove)
            WriteScrollHintRow("more tasks above", markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);

        for (int index = startIndex; index < endIndex; index++)
        {
            TaskItem task = tasks[index];
            bool selected = index == state.SelectedIndex;
            string marker = selected ? ">" : string.Empty;
            string status = task.IsArchived ? "Archived" : task.IsCompleted ? "Done" : "Open";
            string priority = TaskPriority.ToDisplayName(task.Priority);
            string due = TaskDueDate.ToDisplayText(task.DueDate);

            WriteTableRow(marker, task.Id.ToString(), status, priority, due, task.Title, markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, false, selected, task);
        }

        if (hasMoreBelow)
            WriteScrollHintRow("more tasks below", markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);

        WriteTableBorder('└', '┴', '┘', markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);
    }

    private static void WriteSelectedTask(TaskItem task, int width)
    {
        string status = task.IsArchived ? "Archived" : task.IsCompleted ? "Done" : "Open";
        string note = string.IsNullOrWhiteSpace(task.Note) ? "-" : task.Note;
        string title = Fit(task.Title, width - 20);
        string noteLine = Fit(note, width - 10);

        WriteBoxLine('┌', '─', '┐', width);
        WriteBoxText($"Selected #{task.Id}: {title}", width, ConsoleColor.Cyan);
        WriteBoxText($"Status: {status} | Priority: {TaskPriority.ToDisplayName(task.Priority)} | Due: {TaskDueDate.ToDisplayText(task.DueDate)}", width, ConsoleColor.Gray);
        WriteBoxText($"Note: {noteLine}", width, ConsoleColor.DarkGray);
        WriteBoxLine('└', '─', '┘', width);
    }

    private static void WriteEmptyState(TuiState state, int width)
    {
        string message = string.IsNullOrWhiteSpace(state.SearchQuery)
            ? "No tasks in this view. Press 'a' to add a task or use Left/Right to switch views."
            : "No tasks matched the current search. Press Esc to clear search.";

        WriteBoxLine('┌', '─', '┐', width);
        WriteBoxText("Empty", width, ConsoleColor.Yellow);
        WriteBoxText(message, width, ConsoleColor.DarkGray);
        WriteBoxLine('└', '─', '┘', width);
    }

    private static void WriteFooter(int width)
    {
        WriteMuted(Fit("[↑/↓ or j/k] Move  [PgUp/PgDn] Page  [Home/End] Jump  [←/→ or Tab] View  [Space] Done/Reopen", width));
        WriteMuted(Fit("[a] Add  [e] Edit  [d] Delete  [x] Archive/Restore  [/] Search  [Esc] Clear search  [q] Quit", width));
    }

    private static void WriteStatusMessage(string message, int width)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("Status: ");
        SetColor(ConsoleColor.Gray);
        Console.Write(Fit(message, width - 8));
        FinishLine();
        ResetColor();
    }

    private static void WriteTableRow(
        string marker,
        string id,
        string status,
        string priority,
        string due,
        string title,
        int markerWidth,
        int idWidth,
        int statusWidth,
        int priorityWidth,
        int dueWidth,
        int titleWidth,
        bool isHeader,
        bool selected = false,
        TaskItem? task = null)
    {
        SetColor(selected ? ConsoleColor.Cyan : ConsoleColor.DarkGray);
        Console.Write("│ ");

        WriteCell(marker, markerWidth, selected ? ConsoleColor.White : ConsoleColor.DarkGray);
        WriteSeparator();
        WriteCell(id, idWidth, isHeader ? ConsoleColor.Cyan : ConsoleColor.Gray);
        WriteSeparator();
        WriteCell(status, statusWidth, isHeader ? ConsoleColor.Cyan : GetStatusColor(task));
        WriteSeparator();
        WriteCell(priority, priorityWidth, isHeader ? ConsoleColor.Cyan : GetPriorityColor(task?.Priority));
        WriteSeparator();
        WriteCell(due, dueWidth, isHeader ? ConsoleColor.Cyan : GetDueColor(task));
        WriteSeparator();
        WriteCell(title, titleWidth, isHeader ? ConsoleColor.Cyan : selected ? ConsoleColor.White : ConsoleColor.Gray);

        SetColor(selected ? ConsoleColor.Cyan : ConsoleColor.DarkGray);
        Console.Write(" │");
        FinishLine();
        ResetColor();
    }

    private static void WriteScrollHintRow(string message, int markerWidth, int idWidth, int statusWidth, int priorityWidth, int dueWidth, int titleWidth)
    {
        int contentWidth = markerWidth + idWidth + statusWidth + priorityWidth + dueWidth + titleWidth + 15;

        SetColor(ConsoleColor.DarkGray);
        Console.Write("│ ");
        Console.Write(Fit($"... {message} ...", contentWidth).PadRight(contentWidth));
        Console.Write(" │");
        FinishLine();
        ResetColor();
    }

    private static void WriteCell(string value, int width, ConsoleColor color)
    {
        SetColor(color);
        Console.Write(Fit(value, width).PadRight(width));
    }

    private static void WriteSeparator()
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");
    }

    private static void WriteTableBorder(char left, char middle, char right, params int[] widths)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write(left);
        Console.Write(new string('─', 2));

        for (int i = 0; i < widths.Length; i++)
        {
            Console.Write(new string('─', widths[i]));
            Console.Write(i == widths.Length - 1 ? right : middle);
            if (i < widths.Length - 1)
                Console.Write(new string('─', 2));
        }

        FinishLine();
        ResetColor();
    }

    private static void WriteBoxLine(char left, char fill, char right, int width, ConsoleColor color = ConsoleColor.DarkGray)
    {
        SetColor(color);
        Console.Write($"{left}{new string(fill, width - 2)}{right}");
        FinishLine();
        ResetColor();
    }

    private static void WriteBoxText(string text, int width, ConsoleColor color)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("│ ");
        SetColor(color);
        Console.Write(Fit(text, width - 4).PadRight(width - 4));
        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │");
        FinishLine();
        ResetColor();
    }

    private static ConsoleColor GetStatusColor(TaskItem? task)
    {
        if (task == null)
            return ConsoleColor.Gray;

        if (task.IsArchived)
            return ConsoleColor.DarkGray;

        return task.IsCompleted ? ConsoleColor.Green : ConsoleColor.Yellow;
    }

    private static ConsoleColor GetPriorityColor(string? priority)
    {
        if (!TaskPriority.TryNormalize(priority, out string normalizedPriority))
            normalizedPriority = TaskPriority.Normal;

        return normalizedPriority switch
        {
            TaskPriority.High => ConsoleColor.Red,
            TaskPriority.Low => ConsoleColor.DarkGray,
            _ => ConsoleColor.Blue
        };
    }

    private static ConsoleColor GetDueColor(TaskItem? task)
    {
        if (task?.DueDate == null)
            return ConsoleColor.DarkGray;

        return TaskDueDate.IsOverdue(task.DueDate, task.IsCompleted) ? ConsoleColor.Magenta : ConsoleColor.Gray;
    }

    private static void WriteMuted(string message)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write(Fit(message, GetSafeWidth()));
        FinishLine();
        ResetColor();
    }


    private static void WriteBlankLine(int width)
    {
        Console.Write(new string(' ', Math.Max(0, Math.Min(width, GetClearWidth()))));
        FinishLine();
    }

    private static void FinishLine()
    {
        if (!Console.IsOutputRedirected)
        {
            try
            {
                int remaining = GetClearWidth() - Console.CursorLeft;
                if (remaining > 0)
                    Console.Write(new string(' ', remaining));
            }
            catch
            {
                // Ignore cursor issues in unusual terminal hosts.
            }
        }

        Console.WriteLine();
    }

    private static int GetClearWidth()
    {
        try
        {
            if (Console.IsOutputRedirected)
                return DefaultWidth;

            return Math.Max(1, Console.WindowWidth - 1);
        }
        catch
        {
            return DefaultWidth;
        }
    }

    private static string Fit(string value, int width)
    {
        if (width <= 0)
            return string.Empty;

        value = value.ReplaceLineEndings(" ").Trim();

        if (value.Length <= width)
            return value;

        if (width <= 3)
            return value[..width];

        return value[..(width - 3)] + "...";
    }

    private static void BeginFrame()
    {
        if (Console.IsOutputRedirected)
        {
            Console.Clear();
            return;
        }

        try
        {
            Console.SetCursorPosition(0, 0);
        }
        catch
        {
            Console.Clear();
        }
    }

    private static void EndFrame(int width)
    {
        if (Console.IsOutputRedirected)
            return;

        try
        {
            int currentLine = Console.CursorTop;
            int clearWidth = GetClearWidth();

            for (int line = currentLine; line < _lastRenderLineCount; line++)
            {
                Console.SetCursorPosition(0, line);
                Console.Write(new string(' ', clearWidth));
            }

            Console.SetCursorPosition(0, currentLine);
            _lastRenderLineCount = currentLine;
        }
        catch
        {
            _lastRenderLineCount = 0;
        }
    }

    private static void ResetFrame()
    {
        _lastRenderLineCount = 0;
    }

    private static int GetSafeWidth()
    {
        try
        {
            if (Console.IsOutputRedirected)
                return DefaultWidth;

            return Math.Clamp(Console.WindowWidth - 1, 80, 120);
        }
        catch
        {
            return DefaultWidth;
        }
    }

    private static int GetSafeHeight()
    {
        try
        {
            if (Console.IsOutputRedirected)
                return DefaultHeight;

            return Math.Clamp(Console.WindowHeight, 22, 44);
        }
        catch
        {
            return DefaultHeight;
        }
    }

    private static void SetColor(ConsoleColor color)
    {
        if (!Console.IsOutputRedirected)
            Console.ForegroundColor = color;
    }

    private static void ResetColor()
    {
        if (!Console.IsOutputRedirected)
            Console.ResetColor();
    }
}
