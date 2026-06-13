using TaskTracker.Cli.Models;

namespace TaskTracker.Cli.Tui;

public static class TuiRenderer
{
    private const int DefaultWidth = 104;

    public static void Draw(TuiState state, IReadOnlyList<TaskItem> tasks)
    {
        Console.Clear();
        int width = GetSafeWidth();

        WriteHeader(state, tasks, width);
        Console.WriteLine();

        if (tasks.Count == 0)
        {
            WriteEmptyState(state, width);
        }
        else
        {
            WriteTaskRows(state, tasks, width);
            Console.WriteLine();
            WriteSelectedTask(tasks[state.SelectedIndex], width);
        }

        Console.WriteLine();
        WriteFooter(width);

        if (!string.IsNullOrWhiteSpace(state.StatusMessage))
        {
            Console.WriteLine();
            WriteMuted(Fit(state.StatusMessage, width));
        }
    }

    public static void DrawPromptHeader(string title, string subtitle)
    {
        Console.Clear();
        int width = GetSafeWidth();
        WriteBoxLine('┌', '─', '┐', width);
        WriteBoxText(title, width, ConsoleColor.Cyan);
        WriteBoxText(subtitle, width, ConsoleColor.DarkGray);
        WriteBoxLine('└', '─', '┘', width);
        Console.WriteLine();
    }

    public static void ShowPause(string message)
    {
        Console.WriteLine();
        WriteMuted(message);
        Console.ReadKey(intercept: true);
    }

    private static void WriteHeader(TuiState state, IReadOnlyList<TaskItem> tasks, int width)
    {
        int open = tasks.Count(t => !t.IsCompleted && !t.IsArchived);
        int done = tasks.Count(t => t.IsCompleted && !t.IsArchived);
        int archived = tasks.Count(t => t.IsArchived);

        string title = $"TaskTracker TUI - {state.ViewTitle}";
        string search = string.IsNullOrWhiteSpace(state.SearchQuery) ? "no search" : $"search: {state.SearchQuery}";
        string subtitle = $"{tasks.Count} shown | open {open} | done {done} | archived {archived} | {search}";

        WriteBoxLine('╔', '═', '╗', width, ConsoleColor.Cyan);
        WriteBoxText(title, width, ConsoleColor.White);
        WriteBoxText(subtitle, width, ConsoleColor.DarkGray);
        WriteBoxLine('╚', '═', '╝', width, ConsoleColor.Cyan);
    }

    private static void WriteTaskRows(TuiState state, IReadOnlyList<TaskItem> tasks, int width)
    {
        int markerWidth = 1;
        int idWidth = 4;
        int statusWidth = 9;
        int priorityWidth = 8;
        int dueWidth = 12;

        int fixedWidth = markerWidth + idWidth + statusWidth + priorityWidth + dueWidth + 21;
        int titleWidth = Math.Max(24, width - fixedWidth);

        WriteTableBorder('┌', '┬', '┐', markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);
        WriteTableRow("", "ID", "Status", "Priority", "Due", "Title", markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, true);
        WriteTableBorder('├', '┼', '┤', markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);

        for (int index = 0; index < tasks.Count; index++)
        {
            TaskItem task = tasks[index];
            bool selected = index == state.SelectedIndex;
            string marker = selected ? ">" : "";
            string status = task.IsArchived ? "Archived" : task.IsCompleted ? "Done" : "Open";
            string priority = TaskPriority.ToDisplayName(task.Priority);
            string due = TaskDueDate.ToDisplayText(task.DueDate);

            WriteTableRow(marker, task.Id.ToString(), status, priority, due, task.Title, markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, false, selected, task);
        }

        WriteTableBorder('└', '┴', '┘', markerWidth, idWidth, statusWidth, priorityWidth, dueWidth, titleWidth);
    }

    private static void WriteSelectedTask(TaskItem task, int width)
    {
        string status = task.IsArchived ? "Archived" : task.IsCompleted ? "Done" : "Open";
        string note = string.IsNullOrWhiteSpace(task.Note) ? "-" : task.Note;

        WriteBoxLine('┌', '─', '┐', width);
        WriteBoxText($"Selected: #{task.Id} - {task.Title}", width, ConsoleColor.Cyan);
        WriteBoxText($"Status: {status} | Priority: {TaskPriority.ToDisplayName(task.Priority)} | Due: {TaskDueDate.ToDisplayText(task.DueDate)}", width, ConsoleColor.Gray);
        WriteBoxText($"Note: {note}", width, ConsoleColor.DarkGray);
        WriteBoxLine('└', '─', '┘', width);
    }

    private static void WriteEmptyState(TuiState state, int width)
    {
        string message = string.IsNullOrWhiteSpace(state.SearchQuery)
            ? "No tasks to show in this view. Press 'a' to add a task."
            : "No tasks matched the current search. Press Esc to clear search.";

        WriteBoxLine('┌', '─', '┐', width);
        WriteBoxText("Empty", width, ConsoleColor.Yellow);
        WriteBoxText(message, width, ConsoleColor.DarkGray);
        WriteBoxLine('└', '─', '┘', width);
    }

    private static void WriteFooter(int width)
    {
        WriteMuted(Fit("[↑/↓] Move  [←/→] View  [Space] Done/Reopen  [a] Add  [e] Edit  [d] Delete  [x] Archive/Restore  [/] Search  [Esc] Clear  [q] Quit", width));
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
        Console.WriteLine(" │");
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

        Console.WriteLine();
        ResetColor();
    }

    private static void WriteBoxLine(char left, char fill, char right, int width, ConsoleColor color = ConsoleColor.DarkGray)
    {
        SetColor(color);
        Console.WriteLine($"{left}{new string(fill, width - 2)}{right}");
        ResetColor();
    }

    private static void WriteBoxText(string text, int width, ConsoleColor color)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("│ ");
        SetColor(color);
        Console.Write(Fit(text, width - 4).PadRight(width - 4));
        SetColor(ConsoleColor.DarkGray);
        Console.WriteLine(" │");
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
        Console.WriteLine(message);
        ResetColor();
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
