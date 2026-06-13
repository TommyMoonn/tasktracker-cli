using TaskTracker.Cli.Models;
using TaskTracker.Cli.Services;

namespace TaskTracker.Cli.Ui;

public static class ConsoleUi
{
    private const int DefaultWidth = 96;

    public static void ShowHelpMenu()
    {
        ShowHeader("TaskTracker CLI", "Fast task tracking from the terminal.");

        WriteSection("Usage");
        WriteMuted("  tasktracker [command] [options]");
        WriteMuted("  tasktracker                 List active tasks");
        Console.WriteLine();

        WriteSection("Core commands");
        WriteCommand("list", "[--all | --open | --done] [--archived] [--priority <priority>] [--due <filter>]", "List tasks");
        WriteCommand("search", "<text> [--archived | --include-archived]", "Search task titles and notes");
        WriteCommand("tui", "", "Open the interactive terminal UI");
        WriteCommand("add", "<title> [--note <note>] [--priority <priority>] [--due <date>]", "Add a task");
        WriteCommand("view", "<id>", "Show one task");
        WriteCommand("fun", "", "Show the TaskTracker ASCII banner");
        WriteCommand("done", "<id>", "Mark a task as done");
        WriteCommand("reopen", "<id>", "Move a task back to open");
        WriteCommand("archive", "[id]", "Archive one completed task, or all completed tasks");
        WriteCommand("restore", "<id>", "Restore an archived task");
        WriteCommand("edit", "<id> [--title <title>] [--note <note>] [--priority <priority>] [--due <date|none>]", "Edit task details");
        WriteCommand("delete", "<id>", "Delete a task");

        Console.WriteLine();
        WriteSection("Archive filters");
        WriteMuted("  Normal lists hide archived tasks.");
        WriteMuted("  Use --archived to show archived tasks only.");
        WriteMuted("  Use --include-archived to search/list both active and archived tasks.");

        Console.WriteLine();
        WriteSection("Priority values");
        WriteMuted("  low, normal, high");
        WriteMuted("  Short forms: l, n, h");
        WriteMuted("  Also accepted: medium, med, m -> normal");

        Console.WriteLine();
        WriteSection("Due date values");
        WriteMuted("  Add/edit: today, tomorrow, yyyy-mm-dd");
        WriteMuted("  Clear due date: none, clear, remove");
        WriteMuted("  List filters: today, tomorrow, week, overdue, none, yyyy-mm-dd");

        Console.WriteLine();
        WriteSection("Aliases");
        WriteMuted("  list: ls");
        WriteMuted("  search: find");
        WriteMuted("  add: new");
        WriteMuted("  done: complete, finish");
        WriteMuted("  reopen: undo, revert");
        WriteMuted("  restore: unarchive");
        WriteMuted("  edit: update, set");
        WriteMuted("  delete: remove, rm, del");
        WriteMuted("  fun: icon, logo, banner");

        Console.WriteLine();
        WriteSection("Examples");
        WriteMuted("  tasktracker add Buy groceries --priority high --due tomorrow --note carrots potatoes oil");
        WriteMuted("  tasktracker list --open --priority high --due week");
        WriteMuted("  tasktracker search groceries");
        WriteMuted("  tasktracker archive");
        WriteMuted("  tasktracker list --archived");
        WriteMuted("  tasktracker restore 3");
        WriteMuted("  tasktracker fun");

        Console.WriteLine();
        WriteTip("Quotes are optional for simple input, but still useful when your shell needs exact spacing.");
    }

    public static void ShowCommandHelp(string command)
    {
        command = NormalizeCommand(command);

        switch (command)
        {
            case "list":
                ShowHeader("tasktracker list", "List tasks with optional status, archive, priority, and due date filters.");
                WriteMuted("Usage: tasktracker list [--all | --open | --done] [--archived | --include-archived] [--priority <low|normal|high>] [--due <filter>]");
                WriteMuted("Alias: ls");
                WriteMuted("Examples:");
                WriteMuted("  tasktracker list");
                WriteMuted("  tasktracker list --open");
                WriteMuted("  tasktracker list --priority high");
                WriteMuted("  tasktracker list --due today");
                WriteMuted("  tasktracker list --archived");
                WriteMuted("  tasktracker list --include-archived");
                break;

            case "search":
                ShowHeader("tasktracker search", "Search task titles and notes.");
                WriteMuted("Usage: tasktracker search <text> [--archived | --include-archived]");
                WriteMuted("Alias: find");
                WriteMuted("Examples:");
                WriteMuted("  tasktracker search groceries");
                WriteMuted("  tasktracker search report draft");
                WriteMuted("  tasktracker search groceries --archived");
                break;

            case "tui":
                ShowHeader("tasktracker tui", "Open the interactive terminal UI.");
                WriteMuted("Usage: tasktracker tui");
                WriteMuted("Alias: ui");
                WriteMuted("Keys:");
                WriteMuted("  Up/Down    Move selection");
                WriteMuted("  Left/Right Switch Active, Archived, and All views");
                WriteMuted("  Space      Mark selected task done or reopen it");
                WriteMuted("  a          Add task");
                WriteMuted("  e          Edit selected task");
                WriteMuted("  d          Delete selected task");
                WriteMuted("  x          Archive completed task or restore archived task");
                WriteMuted("  /          Search");
                WriteMuted("  Esc        Clear search");
                WriteMuted("  q          Quit");
                break;

            case "add":
                ShowHeader("tasktracker add", "Create a new task.");
                WriteMuted("Usage: tasktracker add <title> [--note <note>] [--priority <low|normal|high>] [--due <today|tomorrow|yyyy-mm-dd>]");
                WriteMuted("Alias: new");
                WriteMuted("Default priority: normal");
                WriteMuted("Examples:");
                WriteMuted("  tasktracker add Buy groceries");
                WriteMuted("  tasktracker add Buy groceries --priority high");
                WriteMuted("  tasktracker add Submit report --due 2026-06-20");
                WriteMuted("  tasktracker add Buy groceries --priority high --due tomorrow --note carrots potatoes oil");
                break;

            case "view":
                ShowHeader("tasktracker view", "Show the full details for one task.");
                WriteMuted("Usage: tasktracker view <id>");
                WriteMuted("Aliases: show, info");
                WriteMuted("Example:");
                WriteMuted("  tasktracker view 2");
                break;

            case "archive":
                ShowHeader("tasktracker archive", "Archive completed tasks so they disappear from normal lists.");
                WriteMuted("Usage: tasktracker archive [id]");
                WriteMuted("Examples:");
                WriteMuted("  tasktracker archive");
                WriteMuted("  tasktracker archive 2");
                WriteMuted("Notes:");
                WriteMuted("  Without an id, archives all completed active tasks.");
                WriteMuted("  A single task must be completed before it can be archived.");
                break;

            case "restore":
                ShowHeader("tasktracker restore", "Restore an archived task back to normal lists.");
                WriteMuted("Usage: tasktracker restore <id>");
                WriteMuted("Alias: unarchive");
                WriteMuted("Example:");
                WriteMuted("  tasktracker restore 2");
                break;

            case "fun":
                ShowHeader("tasktracker fun", "Show the TaskTracker ASCII banner.");
                WriteMuted("Usage: tasktracker fun");
                WriteMuted("Aliases: icon, logo, banner");
                WriteMuted("Example:");
                WriteMuted("  tasktracker icon");
                break;

            case "done":
                ShowHeader("tasktracker done", "Mark a task as completed.");
                WriteMuted("Usage: tasktracker done <id>");
                WriteMuted("Aliases: complete, finish");
                WriteMuted("Example:");
                WriteMuted("  tasktracker done 2");
                break;

            case "reopen":
                ShowHeader("tasktracker reopen", "Move a completed task back to open.");
                WriteMuted("Usage: tasktracker reopen <id>");
                WriteMuted("Aliases: undo, revert");
                WriteMuted("Example:");
                WriteMuted("  tasktracker reopen 2");
                break;

            case "edit":
                ShowHeader("tasktracker edit", "Update a task title, note, priority, or due date.");
                WriteMuted("Usage: tasktracker edit <id> [--title <title>] [--note <note>] [--priority <low|normal|high>] [--due <date|none>]");
                WriteMuted("Aliases: update, set");
                WriteMuted("Examples:");
                WriteMuted("  tasktracker edit 2 --title Buy groceries today");
                WriteMuted("  tasktracker edit 2 --note carrots potatoes oil");
                WriteMuted("  tasktracker edit 2 --priority high");
                WriteMuted("  tasktracker edit 2 --due tomorrow");
                WriteMuted("  tasktracker edit 2 --due none");
                break;

            case "delete":
                ShowHeader("tasktracker delete", "Delete a task by ID.");
                WriteMuted("Usage: tasktracker delete <id>");
                WriteMuted("Aliases: remove, rm, del");
                WriteMuted("Example:");
                WriteMuted("  tasktracker delete 2");
                break;

            default:
                ShowInvalidCommand(command);
                break;
        }
    }

    public static void ShowLogo()
    {
        Console.WriteLine();
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine(@"   _______        _    _______             _             ");
        Console.WriteLine(@"  |__   __|      | |  |__   __|           | |            ");
        Console.WriteLine(@"     | | __ _ ___| | __  | |_ __ __ _  ___| | _____ _ __ ");
        Console.WriteLine(@"     | |/ _` / __| |/ /  | | '__/ _` |/ __| |/ / _ \ '__|");
        Console.WriteLine(@"     | | (_| \__ \   <   | | | | (_| | (__|   <  __/ |   ");
        Console.WriteLine(@"     |_|\__,_|___/_|\_\  |_|_|  \__,_|\___|_|\_\___|_|   ");
        ResetColor();

        Console.WriteLine();
        SetColor(ConsoleColor.DarkGray);
        Console.Write("     [ ");
        ResetColor();
        SetColor(ConsoleColor.Green);
        Console.Write("track tasks");
        ResetColor();
        SetColor(ConsoleColor.DarkGray);
        Console.Write(" | ");
        ResetColor();
        SetColor(ConsoleColor.Yellow);
        Console.Write("clear backlog");
        ResetColor();
        SetColor(ConsoleColor.DarkGray);
        Console.Write(" | ");
        ResetColor();
        SetColor(ConsoleColor.Red);
        Console.Write("ship work");
        ResetColor();
        SetColor(ConsoleColor.DarkGray);
        Console.WriteLine(" ]");
        ResetColor();
        Console.WriteLine();
    }

    public static void ShowTaskList(IReadOnlyList<TaskItem> tasks, bool? completedFilter, string? priorityFilter, string? dueFilter, bool? archivedFilter)
    {
        string statusText = completedFilter switch
        {
            true => "completed",
            false => "open",
            _ => "all"
        };

        List<string> filterParts = new() { $"Showing {statusText} tasks" };

        filterParts.Add(archivedFilter switch
        {
            true => "archived only",
            false => "active only",
            _ => "including archived"
        });

        if (priorityFilter != null)
            filterParts.Add($"{TaskPriority.ToDisplayName(priorityFilter).ToLowerInvariant()} priority");

        if (dueFilter != null)
            filterParts.Add(TaskDueDate.ToFilterDisplayText(dueFilter));

        string subtitle = string.Join(" | ", filterParts);

        ShowHeader("Task Board", subtitle);
        ShowSummary(tasks);

        if (tasks.Count == 0)
        {
            Console.WriteLine();
            WriteEmptyState(completedFilter, priorityFilter, dueFilter, archivedFilter);
            return;
        }

        Console.WriteLine();
        WriteTaskTable(tasks);
    }

    public static void ShowSearchResults(IReadOnlyList<TaskItem> tasks, string searchText, bool? archivedFilter)
    {
        string archiveText = archivedFilter switch
        {
            true => "archived only",
            false => "active only",
            _ => "including archived"
        };

        ShowHeader("Search Results", $"Query: {searchText} | {archiveText}");
        ShowSummary(tasks);

        if (tasks.Count == 0)
        {
            Console.WriteLine();
            WriteStatusBox("Empty", $"No tasks matched '{searchText}'.", ConsoleColor.DarkGray);
            return;
        }

        Console.WriteLine();
        WriteTaskTable(tasks);
    }

    public static void ShowTaskDetails(TaskItem task)
    {
        string status = task.IsArchived ? "Archived" : task.IsCompleted ? "Done" : "Open";
        ShowHeader($"Task #{task.Id}", $"Status: {status}");

        WriteSection("Title");
        Console.WriteLine($"  {task.Title}");
        Console.WriteLine();

        WriteSection("Priority");
        WritePriorityLine(task.Priority);
        Console.WriteLine();

        WriteSection("Due Date");
        WriteDueDateLine(task.DueDate, task.IsCompleted);
        Console.WriteLine();

        WriteSection("Archived");
        Console.WriteLine(task.IsArchived ? "  Yes" : "  No");
        Console.WriteLine();

        WriteSection("Note");
        string note = string.IsNullOrWhiteSpace(task.Note) ? "-" : task.Note;
        Console.WriteLine($"  {note}");
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
            case TaskResult.ArchiveSuccess:
            case TaskResult.RestoreSuccess:
                WriteStatusBox("Success", message, ConsoleColor.Green);
                break;

            case TaskResult.AlreadyCompleted:
            case TaskResult.NotCompleted:
            case TaskResult.AlreadyArchived:
            case TaskResult.NotArchived:
            case TaskResult.NoCompletedTasksToArchive:
                WriteStatusBox("No change", message, ConsoleColor.Yellow);
                break;

            default:
                WriteStatusBox("Error", message, ConsoleColor.Red);
                break;
        }
    }

    public static void ShowArchiveCompletedResult(int archivedCount)
    {
        if (archivedCount == 0)
        {
            ShowResult(TaskResult.NoCompletedTasksToArchive, string.Empty);
            return;
        }

        string label = archivedCount == 1 ? "task" : "tasks";
        WriteStatusBox("Success", $"Archived {archivedCount} completed {label}.", ConsoleColor.Green);
    }

    public static void ShowUsage(string usage)
    {
        WriteStatusBox("Usage", usage, ConsoleColor.Yellow);
    }

    public static void ShowInvalidId()
    {
        WriteStatusBox("Invalid input", "Task id must be a number.", ConsoleColor.Red);
    }

    public static void ShowInvalidPriority()
    {
        WriteStatusBox("Invalid input", "Priority must be low, normal, or high.", ConsoleColor.Red);
    }

    public static void ShowInvalidDueDate()
    {
        WriteStatusBox("Invalid input", "Due date must be today, tomorrow, yyyy-mm-dd, or none when editing.", ConsoleColor.Red);
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
        int pending = tasks.Count(t => !t.IsCompleted);
        int archived = tasks.Count(t => t.IsArchived);
        int highPriority = tasks.Count(t => TaskPriority.TryNormalize(t.Priority, out string priority) && priority == TaskPriority.High);
        int overdue = tasks.Count(t => TaskDueDate.IsOverdue(t.DueDate, t.IsCompleted));

        Console.WriteLine();
        WriteMetric("Total", tasks.Count.ToString(), ConsoleColor.Cyan);
        Console.Write("  ");
        WriteMetric("Done", completed.ToString(), ConsoleColor.Green);
        Console.Write("  ");
        WriteMetric("Open", pending.ToString(), ConsoleColor.Yellow);
        Console.Write("  ");
        WriteMetric("Archived", archived.ToString(), ConsoleColor.DarkGray);

        if (highPriority > 0)
        {
            Console.Write("  ");
            WriteMetric("High", highPriority.ToString(), ConsoleColor.Red);
        }

        if (overdue > 0)
        {
            Console.Write("  ");
            WriteMetric("Overdue", overdue.ToString(), ConsoleColor.Magenta);
        }

        Console.WriteLine();
    }

    private static void WriteMetric(string label, string value, ConsoleColor valueColor)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("[");
        ResetColor();
        SetColor(ConsoleColor.Gray);
        Console.Write($"{label}: ");
        SetColor(valueColor);
        Console.Write(value);
        SetColor(ConsoleColor.DarkGray);
        Console.Write("]");
        ResetColor();
    }

    private static void WriteTaskTable(IReadOnlyList<TaskItem> tasks)
    {
        int width = GetSafeWidth();

        int idWidth = 4;
        int statusWidth = 9;
        int priorityWidth = 8;
        int dueWidth = 12;

        int fixedWidth = idWidth + statusWidth + priorityWidth + dueWidth + 15;
        int remaining = Math.Max(30, width - fixedWidth);
        int titleWidth = Math.Max(18, (int)(remaining * 0.48));
        int noteWidth = Math.Max(16, remaining - titleWidth);

        WriteTableBorder('┌', '┬', '┐', idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, noteWidth);
        WriteTableRow("ID", "Status", "Priority", "Due", "Title", "Note", idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, noteWidth, isHeader: true);
        WriteTableBorder('├', '┼', '┤', idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, noteWidth);

        foreach (var task in tasks)
        {
            string statusText = task.IsArchived ? "Archived" : task.IsCompleted ? "Done" : "Open";
            string priorityText = TaskPriority.ToDisplayName(task.Priority);
            string dueText = TaskDueDate.ToDisplayText(task.DueDate);
            string note = string.IsNullOrWhiteSpace(task.Note) ? "-" : task.Note;

            WriteTableRow(
                task.Id.ToString(),
                statusText,
                priorityText,
                dueText,
                task.Title,
                note,
                idWidth,
                statusWidth,
                priorityWidth,
                dueWidth,
                titleWidth,
                noteWidth,
                isCompleted: task.IsCompleted,
                isArchived: task.IsArchived,
                priority: task.Priority,
                dueDate: task.DueDate);
        }

        WriteTableBorder('└', '┴', '┘', idWidth, statusWidth, priorityWidth, dueWidth, titleWidth, noteWidth);
    }

    private static void WriteTableRow(
        string id,
        string status,
        string priorityText,
        string dueText,
        string title,
        string note,
        int idWidth,
        int statusWidth,
        int priorityWidth,
        int dueWidth,
        int titleWidth,
        int noteWidth,
        bool isHeader = false,
        bool isCompleted = false,
        bool isArchived = false,
        string? priority = null,
        DateOnly? dueDate = null)
    {
        SetColor(ConsoleColor.DarkGray);
        Console.Write("│ ");

        SetColor(isHeader ? ConsoleColor.Cyan : ConsoleColor.Gray);
        Console.Write(Fit(id, idWidth).PadRight(idWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");

        SetColor(isHeader ? ConsoleColor.Cyan : GetStatusColor(isCompleted, isArchived));
        Console.Write(Fit(status, statusWidth).PadRight(statusWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");

        SetColor(isHeader ? ConsoleColor.Cyan : GetPriorityColor(priority));
        Console.Write(Fit(priorityText, priorityWidth).PadRight(priorityWidth));

        SetColor(ConsoleColor.DarkGray);
        Console.Write(" │ ");

        SetColor(isHeader ? ConsoleColor.Cyan : GetDueDateColor(dueDate, isCompleted));
        Console.Write(Fit(dueText, dueWidth).PadRight(dueWidth));

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

    private static void WriteEmptyState(bool? completedFilter, string? priorityFilter, string? dueFilter, bool? archivedFilter)
    {
        string message;

        if (archivedFilter == true)
        {
            message = "No archived tasks found.";
        }
        else if (dueFilter != null)
        {
            message = $"No tasks found that are {TaskDueDate.ToFilterDisplayText(dueFilter)}.";
        }
        else if (priorityFilter != null)
        {
            string priorityText = TaskPriority.ToDisplayName(priorityFilter).ToLowerInvariant();
            message = $"No tasks found with {priorityText} priority.";
        }
        else
        {
            message = completedFilter switch
            {
                true => "No completed tasks yet.",
                false => "No pending tasks. Nice work.",
                _ => "No active tasks found. Add one with: tasktracker add \"Task title\""
            };
        }

        WriteStatusBox("Empty", message, ConsoleColor.DarkGray);
    }

    private static void WritePriorityLine(string priority)
    {
        SetColor(GetPriorityColor(priority));
        Console.WriteLine($"  {TaskPriority.ToDisplayName(priority)}");
        ResetColor();
    }

    private static void WriteDueDateLine(DateOnly? dueDate, bool isCompleted)
    {
        SetColor(GetDueDateColor(dueDate, isCompleted));
        Console.WriteLine($"  {TaskDueDate.ToDisplayText(dueDate)}");
        ResetColor();
    }

    private static ConsoleColor GetStatusColor(bool isCompleted, bool isArchived)
    {
        if (isArchived)
            return ConsoleColor.DarkGray;

        return isCompleted ? ConsoleColor.Green : ConsoleColor.Yellow;
    }

    private static ConsoleColor GetDueDateColor(DateOnly? dueDate, bool isCompleted)
    {
        if (dueDate == null)
            return ConsoleColor.DarkGray;

        if (TaskDueDate.IsOverdue(dueDate, isCompleted))
            return ConsoleColor.Magenta;

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if (dueDate == today)
            return ConsoleColor.Yellow;

        return ConsoleColor.Gray;
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
        Console.Write($" {Fit(args, 66).PadRight(66)}");
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

    private static string NormalizeCommand(string command)
    {
        return command.Trim().ToLowerInvariant() switch
        {
            "ls" or "-l" => "list",
            "find" => "search",
            "ui" => "tui",
            "new" or "-a" => "add",
            "complete" or "finish" or "-c" => "done",
            "undo" or "revert" => "reopen",
            "unarchive" => "restore",
            "update" or "set" or "-e" or "-u" => "edit",
            "remove" or "rm" or "del" or "-d" => "delete",
            "show" or "info" => "view",
            "icon" or "logo" or "banner" => "fun",
            _ => command.Trim().ToLowerInvariant()
        };
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
            TaskResult.ArchiveSuccess => $"Task {identifier} archived.",
            TaskResult.RestoreSuccess => $"Task {identifier} restored.",
            TaskResult.UpdateFailed => $"Failed to update task {identifier}.",
            TaskResult.AlreadyCompleted => $"Task {identifier} is already completed.",
            TaskResult.NotCompleted => $"Task {identifier} is not completed.",
            TaskResult.AlreadyArchived => $"Task {identifier} is already archived.",
            TaskResult.NotArchived => $"Task {identifier} is not archived.",
            TaskResult.NoCompletedTasksToArchive => "No completed active tasks to archive.",
            TaskResult.TaskNotFound => $"Task {identifier} not found.",
            TaskResult.DuplicateTitle => $"Task title '{identifier}' already exists.",
            TaskResult.InvalidPriority => "Priority must be low, normal, or high.",
            TaskResult.InvalidDueDate => "Due date must be today, tomorrow, yyyy-mm-dd, or none when editing.",
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
