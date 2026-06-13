using TaskTracker.Cli.Models;
using TaskTracker.Cli.Services;

namespace TaskTracker.Cli.Ui;

public static class ConsoleUi
{
    private const int DefaultWidth = 96;

    public static void ShowHelpMenu()
    {
        ShowHeader("TaskTracker CLI", "A tiny terminal task manager for quick daily tracking.");

        WriteSection("Usage");
        WriteMuted("  tasktracker <command> [options]");
        Console.WriteLine();

        WriteSection("Commands");
        WriteCommand("list, ls, -l", "[--completed | -c] [--pending | -p]", "List tasks");
        WriteCommand("add, -a", "<title> [--note | -n] <note>", "Add a new task");
        WriteCommand("complete, -c", "<id>", "Mark a task as completed");
        WriteCommand("undo, revert", "<id>", "Undo task completion");
        WriteCommand("update, edit, -u", "<id> [title | -t] <title> [note | -n] <note>", "Update task details");
        WriteCommand("remove, delete, -d", "<id>", "Delete task by ID");

        Console.WriteLine();
        WriteTip("Wrap titles or notes in quotes when they contain spaces.");
    }

    public static void ShowTaskList(IReadOnlyList<TaskItem> tasks, bool? completedFilter)
    {
        string subtitle = completedFilter switch
        {
            true => "Showing completed tasks",
            false => "Showing pending tasks",
            _ => "Showing all tasks"
        };

        ShowHeader("Task Board", subtitle);
        ShowSummary(tasks);

        if (tasks.Count == 0)
        {
            Console.WriteLine();
            WriteEmptyState(completedFilter);
            return;
        }

        Console.WriteLine();
        WriteTaskTable(tasks);
    }

    public static void ShowResult(TaskResult result, string identifier)
    {
        string message = GetMessage(result, identifier);

        switch (result)
        {
            case TaskResult.AddSuccess:
            case TaskResult.UpdateSuccess:
            case TaskResult.RemoveSuccess:
            case TaskResult.MarkCompleted:
            case TaskResult.UndoSuccess:
                WriteStatusBox("Success", message, ConsoleColor.Green);
                break;

            case TaskResult.AlreadyCompleted:
            case TaskResult.NotCompleted:
                WriteStatusBox("No change", message, ConsoleColor.Yellow);
                break;

            default:
                WriteStatusBox("Error", message, ConsoleColor.Red);
                break;
        }
    }

    public static void ShowUsage(string usage)
    {
        WriteStatusBox("Usage", usage, ConsoleColor.Yellow);
    }

    public static void ShowInvalidId()
    {
        WriteStatusBox("Invalid input", "Task id must be a number.", ConsoleColor.Red);
    }

    public static void ShowInvalidCommand(string command)
    {
        ShowHeader("Unknown Command", $"'{command}' is not a valid tasktracker command.");
        WriteMuted("  Usage: tasktracker [command] [options]");
        WriteMuted("  Use:   tasktracker --help");
    }

    private static void ShowHeader(string title, string subtitle)
    {
        int width = GetSafeWidth();
        string line = new('═', width - 2);

        SetColor(ConsoleColor.Cyan);
        Console.WriteLine($"╔{line}╗");
        Console.WriteLine($"║ {Fit(title, width - 4).PadRight(width - 4)} ║");
        SetColor(ConsoleColor.DarkCyan);
        Console.WriteLine($"║ {Fit(subtitle, width - 4).PadRight(width - 4)} ║");
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine($"╚{line}╝");
        ResetColor();
    }

    private static void ShowSummary(IReadOnlyList<TaskItem> tasks)
    {
        int completed = tasks.Count(t => t.IsCompleted);
        int pending = tasks.Count - completed;

        Console.WriteLine();
        WriteMetric("Total", tasks.Count, ConsoleColor.Cyan);
        Console.Write("  ");
        WriteMetric("Done", completed, ConsoleColor.Green);
        Console.Write("  ");
        WriteMetric("Open", pending, ConsoleColor.Yellow);
        Console.WriteLine();
    }

    private static void WriteMetric(string label, int value, ConsoleColor color)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("[");
        SetColor(color);
        Console.Write($"{label}: {value}");
        SetColor(ConsoleColor.DarkGray);
        Console.Write("]");
        ResetColor();
    }

    private static void WriteTaskTable(IReadOnlyList<TaskItem> tasks)
    {
        int width = GetSafeWidth();
        int idWidth = 4;
        int statusWidth = 11;
        int fixedWidth = idWidth + statusWidth + 9;
        int remaining = Math.Max(30, width - fixedWidth);
        int titleWidth = Math.Max(18, (int)(remaining * 0.42));
        int noteWidth = Math.Max(20, remaining - titleWidth);

        WriteTableBorder('┌', '┬', '┐', idWidth, statusWidth, titleWidth, noteWidth);
        WriteTableRow("ID", "Status", "Title", "Note", idWidth, statusWidth, titleWidth, noteWidth, isHeader: true);
        WriteTableBorder('├', '┼', '┤', idWidth, statusWidth, titleWidth, noteWidth);

        foreach (var task in tasks)
        {
            string statusText = task.IsCompleted ? "✓ Done" : "○ Open";
            string note = string.IsNullOrWhiteSpace(task.Note) ? "—" : task.Note;
            WriteTableRow(task.Id.ToString(), statusText, task.Title, note, idWidth, statusWidth, titleWidth, noteWidth, isCompleted: task.IsCompleted);
        }

        WriteTableBorder('└', '┴', '┘', idWidth, statusWidth, titleWidth, noteWidth);
    }

    private static void WriteTableRow(
        string id,
        string status,
        string title,
        string note,
        int idWidth,
        int statusWidth,
        int titleWidth,
        int noteWidth,
        bool isHeader = false,
        bool isCompleted = false)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("│ ");

        SetColor(isHeader ? ConsoleColor.Cyan : ConsoleColor.Gray);
        Console.Write(Fit(id, idWidth).PadRight(idWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");

        SetColor(isHeader ? ConsoleColor.Cyan : isCompleted ? ConsoleColor.Green : ConsoleColor.Yellow);
        Console.Write(Fit(status, statusWidth).PadRight(statusWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");

        SetColor(isHeader ? ConsoleColor.Cyan : ConsoleColor.White);
        Console.Write(Fit(title, titleWidth).PadRight(titleWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");

        SetColor(isHeader ? ConsoleColor.Cyan : ConsoleColor.Gray);
        Console.Write(Fit(note, noteWidth).PadRight(noteWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.WriteLine(" │");
        ResetColor();
    }

    private static void WriteTableBorder(char left, char middle, char right, params int[] widths)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write(left);
        for (int i = 0; i < widths.Length; i++)
        {
            Console.Write(new string('─', widths[i] + 2));
            Console.Write(i == widths.Length - 1 ? right : middle);
        }
        Console.WriteLine();
        ResetColor();
    }

    private static void WriteStatusBox(string title, string message, ConsoleColor color)
    {
        int width = Math.Min(GetSafeWidth(), DefaultWidth);
        string content = $"{title}: {message}";
        string line = new('─', width - 2);

        SetColor(color);
        Console.WriteLine($"┌{line}┐");
        Console.Write("│ ");
        Console.Write(Fit(content, width - 4).PadRight(width - 4));
        Console.WriteLine(" │");
        Console.WriteLine($"└{line}┘");
        ResetColor();
    }

    private static void WriteEmptyState(bool? completedFilter)
    {
        string message = completedFilter switch
        {
            true => "No completed tasks yet.",
            false => "No pending tasks. Nice work.",
            _ => "No tasks found. Add one with: tasktracker add \"Task title\""
        };

        WriteStatusBox("Empty", message, ConsoleColor.DarkGray);
    }

    private static void WriteSection(string text)
    {
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine(text);
        ResetColor();
    }

    private static void WriteCommand(string command, string args, string description)
    {
        SetColor(ConsoleColor.Green);
        Console.Write($"  {command.PadRight(18)}");
        SetColor(ConsoleColor.Yellow);
        Console.Write($" {args.PadRight(48)}");
        SetColor(ConsoleColor.Gray);
        Console.WriteLine(description);
        ResetColor();
    }

    private static void WriteTip(string message)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("Tip: ");
        ResetColor();
        Console.WriteLine(message);
    }

    private static void WriteMuted(string message)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.WriteLine(message);
        ResetColor();
    }

    private static string GetMessage(TaskResult result, string identifier)
    {
        return result switch
        {
            TaskResult.AddSuccess => "Task added successfully.",
            TaskResult.UpdateSuccess => $"Task {identifier} updated successfully.",
            TaskResult.RemoveSuccess => $"Task {identifier} removed.",
            TaskResult.MarkCompleted => $"Task {identifier} marked completed.",
            TaskResult.UndoSuccess => $"Task {identifier} completion undone.",
            TaskResult.UpdateFailed => $"Failed to update task {identifier}.",
            TaskResult.AlreadyCompleted => $"Task {identifier} is already completed.",
            TaskResult.NotCompleted => $"Task {identifier} is not completed, nothing to undo.",
            TaskResult.TaskNotFound => $"Task {identifier} not found.",
            TaskResult.DuplicateTitle => $"Task title '{identifier}' already exists.",
            TaskResult.EmptyTitle => "Task title cannot be empty.",
            _ => "Unknown error."
        };
    }

    private static string Fit(string value, int width)
    {
        if (width <= 0)
            return string.Empty;

        value = value.ReplaceLineEndings(" ").Trim();

        if (value.Length <= width)
            return value;

        if (width <= 1)
            return "…";

        return value[..(width - 1)] + "…";
    }

    private static int GetSafeWidth()
    {
        try
        {
            if (Console.IsOutputRedirected)
                return DefaultWidth;

            return Math.Clamp(Console.WindowWidth - 1, 72, 120);
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
