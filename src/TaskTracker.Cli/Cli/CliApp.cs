using TaskTracker.Cli.Models;
using TaskTracker.Cli.Services;
using TaskTracker.Cli.Ui;

namespace TaskTracker.Cli.Cli;

public class CliApp
{
    private readonly TaskService _service;

    public CliApp(TaskService service)
    {
        _service = service;
    }

    public void Run(string[] args)
    {
        if (args.Length == 0)
        {
            ShowTasks(completedFilter: null, priorityFilter: null, dueFilter: null);
            return;
        }

        string command = args[0].Trim().ToLowerInvariant();
        string[] commandArgs = args.Skip(1).ToArray();

        if (CliArguments.IsHelpCommand(command))
        {
            ShowHelp(commandArgs);
            return;
        }

        if (commandArgs.Any(CliArguments.IsHelpOption))
        {
            ConsoleUi.ShowCommandHelp(command);
            return;
        }

        switch (command)
        {
            case "list":
            case "ls":
            case "-l": // Legacy alias. Kept for backwards compatibility.
                HandleList(commandArgs);
                break;

            case "add":
            case "new":
            case "-a": // Legacy alias. Kept for backwards compatibility.
                HandleAdd(commandArgs);
                break;

            case "done":
            case "complete":
            case "finish":
            case "-c": // Legacy alias. Kept for backwards compatibility.
                HandleStatusUpdate(commandArgs, completed: true, usage: "tasktracker done <id>");
                break;

            case "reopen":
            case "undo":
            case "revert":
                HandleStatusUpdate(commandArgs, completed: false, usage: "tasktracker reopen <id>");
                break;

            case "edit":
            case "update":
            case "set":
            case "-e": // Legacy alias. Kept for backwards compatibility.
            case "-u": // Legacy alias. Kept for backwards compatibility.
                HandleEdit(commandArgs);
                break;

            case "delete":
            case "remove":
            case "rm":
            case "del":
            case "-d": // Legacy alias. Kept for backwards compatibility.
                HandleDelete(commandArgs);
                break;

            case "fun":
            case "icon":
            case "logo":
            case "banner":
                ConsoleUi.ShowLogo();
                break;

            case "view":
            case "show":
            case "info":
                HandleView(commandArgs);
                break;

            default:
                ConsoleUi.ShowInvalidCommand(string.Join(' ', args));
                break;
        }
    }

    private void HandleList(string[] args)
    {
        bool? completedFilter = null;
        string? priorityFilter = null;
        string? dueFilter = null;

        for (int index = 0; index < args.Length; index++)
        {
            string option = args[index].Trim().ToLowerInvariant();

            if (option is "--done" or "--completed" or "-c")
            {
                completedFilter = true;
            }
            else if (option is "--open" or "--pending" or "-o")
            {
                completedFilter = false;
            }
            else if (option is "-p")
            {
                if (CliArguments.TryReadOptionValue(args, index + 1, out string? value))
                {
                    if (!TaskPriority.TryNormalize(value, out priorityFilter))
                    {
                        ConsoleUi.ShowInvalidPriority();
                        return;
                    }

                    index++;
                }
                else
                {
                    // Legacy behavior: `tasktracker ls -p` means pending/open.
                    completedFilter = false;
                }
            }
            else if (option is "--priority")
            {
                if (!CliArguments.TryReadOptionValue(args, index + 1, out string? value))
                {
                    ConsoleUi.ShowUsage("tasktracker list [--all | --open | --done] [--priority <low|normal|high>] [--due <filter>]");
                    return;
                }

                if (!TaskPriority.TryNormalize(value, out priorityFilter))
                {
                    ConsoleUi.ShowInvalidPriority();
                    return;
                }

                index++;
            }
            else if (option is "--due")
            {
                if (!CliArguments.TryReadOptionValue(args, index + 1, out string? value))
                {
                    ConsoleUi.ShowUsage("tasktracker list [--all | --open | --done] [--priority <low|normal|high>] [--due <today|tomorrow|week|overdue|none|yyyy-mm-dd>]");
                    return;
                }

                if (!TaskDueDate.TryNormalizeFilter(value, out dueFilter))
                {
                    ConsoleUi.ShowInvalidDueDate();
                    return;
                }

                index++;
            }
            else if (option is "--overdue")
            {
                dueFilter = TaskDueDate.Overdue;
            }
            else if (option is "--all" or "-a")
            {
                completedFilter = null;
            }
            else
            {
                ConsoleUi.ShowUsage("tasktracker list [--all | --open | --done] [--priority <low|normal|high>] [--due <filter>]");
                return;
            }
        }

        ShowTasks(completedFilter, priorityFilter, dueFilter);
    }

    private void HandleAdd(string[] args)
    {
        const string usage = "tasktracker add <title> [--note <note>] [--priority <low|normal|high>] [--due <today|tomorrow|yyyy-mm-dd>]";

        if (args.Length == 0)
        {
            ConsoleUi.ShowUsage(usage);
            return;
        }

        string? title = CliArguments.ReadValueUntilOption(args, 0, out int nextIndex);
        string note = string.Empty;
        string priority = TaskPriority.Normal;
        DateOnly? dueDate = null;

        while (nextIndex < args.Length)
        {
            string option = args[nextIndex].Trim().ToLowerInvariant();

            if (option is "--note" or "-n")
            {
                note = CliArguments.ReadValueUntilOption(args, nextIndex + 1, out nextIndex) ?? string.Empty;
            }
            else if (option is "--priority" or "-p")
            {
                if (!CliArguments.TryReadOptionValue(args, nextIndex + 1, out string? value))
                {
                    ConsoleUi.ShowUsage(usage);
                    return;
                }

                if (!TaskPriority.TryNormalize(value, out priority))
                {
                    ConsoleUi.ShowInvalidPriority();
                    return;
                }

                nextIndex += 2;
            }
            else if (option is "--due")
            {
                if (!CliArguments.TryReadOptionValue(args, nextIndex + 1, out string? value))
                {
                    ConsoleUi.ShowUsage(usage);
                    return;
                }

                if (!TaskDueDate.TryParse(value, out dueDate, out bool clearDueDate) || clearDueDate)
                {
                    ConsoleUi.ShowInvalidDueDate();
                    return;
                }

                nextIndex += 2;
            }
            else
            {
                ConsoleUi.ShowUsage(usage);
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            ConsoleUi.ShowUsage(usage);
            return;
        }

        TaskResult result = _service.AddTask(title, note, priority, dueDate);
        ConsoleUi.ShowResult(result, title);
    }

    private void HandleStatusUpdate(string[] args, bool completed, string usage)
    {
        if (!TryReadIdOrShowUsage(args, usage, out int id))
            return;

        TaskResult result = _service.UpdateStatus(id, completed);
        ConsoleUi.ShowResult(result, id.ToString());
    }

    private void HandleEdit(string[] args)
    {
        const string usage = "tasktracker edit <id> [--title <title>] [--note <note>] [--priority <low|normal|high>] [--due <today|tomorrow|yyyy-mm-dd|none>]";

        if (!TryReadIdOrShowUsage(args, usage, out int id))
            return;

        string? newTitle = null;
        string? newNote = null;
        string? newPriority = null;
        DateOnly? newDueDate = null;
        bool updateDueDate = false;
        int index = 1;

        if (index < args.Length && !CliArguments.IsOption(args[index]) && !CliArguments.IsNamedEditOption(args[index]))
            newTitle = CliArguments.ReadValueUntilOption(args, index, out index);

        while (index < args.Length)
        {
            string option = args[index].Trim().ToLowerInvariant();

            if (option is "--title" or "-t" or "title")
            {
                newTitle = CliArguments.ReadValueUntilOption(args, index + 1, out index);
            }
            else if (option is "--note" or "-n" or "note")
            {
                newNote = CliArguments.ReadValueUntilOption(args, index + 1, out index) ?? string.Empty;
            }
            else if (option is "--priority" or "-p" or "priority")
            {
                if (!CliArguments.TryReadOptionValue(args, index + 1, out string? value))
                {
                    ConsoleUi.ShowUsage(usage);
                    return;
                }

                if (!TaskPriority.TryNormalize(value, out newPriority))
                {
                    ConsoleUi.ShowInvalidPriority();
                    return;
                }

                index += 2;
            }
            else if (option is "--due" or "due")
            {
                if (!CliArguments.TryReadOptionValue(args, index + 1, out string? value))
                {
                    ConsoleUi.ShowUsage(usage);
                    return;
                }

                if (!TaskDueDate.TryParse(value, out newDueDate, out bool clearDueDate))
                {
                    ConsoleUi.ShowInvalidDueDate();
                    return;
                }

                if (clearDueDate)
                    newDueDate = null;

                updateDueDate = true;
                index += 2;
            }
            else
            {
                ConsoleUi.ShowUsage(usage);
                return;
            }
        }

        if (newTitle == null && newNote == null && newPriority == null && !updateDueDate)
        {
            ConsoleUi.ShowUsage(usage);
            return;
        }

        TaskResult result = _service.UpdateTask(id, newTitle, newNote, newPriority, newDueDate, updateDueDate);
        ConsoleUi.ShowResult(result, id.ToString());
    }

    private void HandleDelete(string[] args)
    {
        if (!TryReadIdOrShowUsage(args, "tasktracker delete <id>", out int id))
            return;

        TaskResult result = _service.RemoveTask(id);
        ConsoleUi.ShowResult(result, id.ToString());
    }

    private void HandleView(string[] args)
    {
        if (!TryReadIdOrShowUsage(args, "tasktracker view <id>", out int id))
            return;

        TaskItem? task = _service.GetTaskById(id);

        if (task == null)
        {
            ConsoleUi.ShowResult(TaskResult.TaskNotFound, id.ToString());
            return;
        }

        ConsoleUi.ShowTaskDetails(task);
    }

    private void ShowTasks(bool? completedFilter, string? priorityFilter, string? dueFilter)
    {
        List<TaskItem> tasks = _service.GetTasksByStatus(completedFilter, priorityFilter, dueFilter);
        ConsoleUi.ShowTaskList(tasks, completedFilter, priorityFilter, dueFilter);
    }

    private static void ShowHelp(string[] args)
    {
        if (args.Length == 0)
            ConsoleUi.ShowHelpMenu();
        else
            ConsoleUi.ShowCommandHelp(args[0]);
    }

    private static bool TryReadIdOrShowUsage(string[] args, string usage, out int id)
    {
        if (args.Length == 0)
        {
            id = 0;
            ConsoleUi.ShowUsage(usage);
            return false;
        }

        if (!CliArguments.TryReadId(args, out id))
        {
            ConsoleUi.ShowInvalidId();
            return false;
        }

        return true;
    }
}
