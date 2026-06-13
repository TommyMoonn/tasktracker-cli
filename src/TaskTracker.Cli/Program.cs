using System.Text;
using TaskTracker.Cli.Models;
using TaskTracker.Cli.Persistence;
using TaskTracker.Cli.Services;
using TaskTracker.Cli.Ui;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var filePath = Path.Combine(home, ".tasktracker.json");
        var repo = new JsonTaskRepository(filePath);
        var service = new TaskServices(repo);

        if (args.Length == 0)
        {
            ShowTasks(service, completedFilter: null);
            return;
        }

        string command = args[0].Trim().ToLowerInvariant();
        string[] commandArgs = args.Skip(1).ToArray();

        if (IsHelpCommand(command))
        {
            ShowHelp(commandArgs);
            return;
        }

        if (commandArgs.Any(IsHelpOption))
        {
            ConsoleUi.ShowCommandHelp(command);
            return;
        }

        switch (command)
        {
            case "list":
            case "ls":
            case "-l": // Legacy alias. Kept for backwards compatibility.
                HandleList(service, commandArgs);
                break;

            case "add":
            case "new":
            case "-a": // Legacy alias. Kept for backwards compatibility.
                HandleAdd(service, commandArgs);
                break;

            case "done":
            case "complete":
            case "finish":
            case "-c": // Legacy alias. Kept for backwards compatibility.
                HandleStatusUpdate(service, commandArgs, completed: true, usage: "tasktracker done <id>");
                break;

            case "reopen":
            case "undo":
            case "revert":
                HandleStatusUpdate(service, commandArgs, completed: false, usage: "tasktracker reopen <id>");
                break;

            case "edit":
            case "update":
            case "set":
            case "-e": // Legacy alias. Kept for backwards compatibility.
            case "-u": // Legacy alias. Kept for backwards compatibility.
                HandleEdit(service, commandArgs);
                break;

            case "delete":
            case "remove":
            case "rm":
            case "del":
            case "-d": // Legacy alias. Kept for backwards compatibility.
                HandleDelete(service, commandArgs);
                break;

            case "view":
            case "show":
            case "info":
                HandleView(service, commandArgs);
                break;

            default:
                ConsoleUi.ShowInvalidCommand(string.Join(' ', args));
                break;
        }
    }

    private static void HandleList(TaskServices service, string[] args)
    {
        bool? completedFilter = null;

        foreach (string arg in args)
        {
            string option = arg.Trim().ToLowerInvariant();

            if (option is "--done" or "--completed" or "-c")
                completedFilter = true;
            else if (option is "--open" or "--pending" or "-o" or "-p")
                completedFilter = false;
            else if (option is "--all" or "-a")
                completedFilter = null;
            else
            {
                ConsoleUi.ShowUsage("tasktracker list [--all | --open | --done]");
                return;
            }
        }

        ShowTasks(service, completedFilter);
    }

    private static void HandleAdd(TaskServices service, string[] args)
    {
        if (args.Length == 0)
        {
            ConsoleUi.ShowUsage("tasktracker add <title> [--note <note>]");
            return;
        }

        string? title = ReadValueUntilOption(args, 0, out int nextIndex);
        string note = string.Empty;

        while (nextIndex < args.Length)
        {
            string option = args[nextIndex].Trim().ToLowerInvariant();

            if (option is "--note" or "-n")
            {
                note = ReadValueUntilOption(args, nextIndex + 1, out nextIndex) ?? string.Empty;
            }
            else
            {
                ConsoleUi.ShowUsage("tasktracker add <title> [--note <note>]");
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            ConsoleUi.ShowUsage("tasktracker add <title> [--note <note>]");
            return;
        }

        TaskResult result = service.AddTask(title, note);
        ConsoleUi.ShowResult(result, title);
    }

    private static void HandleStatusUpdate(TaskServices service, string[] args, bool completed, string usage)
    {
        if (!TryReadId(args, usage, out int id))
            return;

        TaskResult result = service.UpdateStatus(id, completed);
        ConsoleUi.ShowResult(result, id.ToString());
    }

    private static void HandleEdit(TaskServices service, string[] args)
    {
        if (!TryReadId(args, "tasktracker edit <id> [--title <title>] [--note <note>]", out int id))
            return;

        string? newTitle = null;
        string? newNote = null;
        int index = 1;

        if (index < args.Length && !IsOption(args[index]) && !IsNamedEditOption(args[index]))
            newTitle = ReadValueUntilOption(args, index, out index);

        while (index < args.Length)
        {
            string option = args[index].Trim().ToLowerInvariant();

            if (option is "--title" or "-t" or "title")
            {
                newTitle = ReadValueUntilOption(args, index + 1, out index);
            }
            else if (option is "--note" or "-n" or "note")
            {
                newNote = ReadValueUntilOption(args, index + 1, out index) ?? string.Empty;
            }
            else
            {
                ConsoleUi.ShowUsage("tasktracker edit <id> [--title <title>] [--note <note>]");
                return;
            }
        }

        if (newTitle == null && newNote == null)
        {
            ConsoleUi.ShowUsage("tasktracker edit <id> [--title <title>] [--note <note>]");
            return;
        }

        TaskResult result = service.UpdateTask(id, newTitle, newNote);
        ConsoleUi.ShowResult(result, id.ToString());
    }

    private static void HandleDelete(TaskServices service, string[] args)
    {
        if (!TryReadId(args, "tasktracker delete <id>", out int id))
            return;

        TaskResult result = service.RemoveTask(id);
        ConsoleUi.ShowResult(result, id.ToString());
    }

    private static void HandleView(TaskServices service, string[] args)
    {
        if (!TryReadId(args, "tasktracker view <id>", out int id))
            return;

        TaskItem? task = service.GetTasks().FirstOrDefault(t => t.Id == id);

        if (task == null)
        {
            ConsoleUi.ShowResult(TaskResult.TaskNotFound, id.ToString());
            return;
        }

        ConsoleUi.ShowTaskDetails(task);
    }

    private static void ShowTasks(TaskServices service, bool? completedFilter)
    {
        List<TaskItem> tasks = service.GetTasksByStatus(completedFilter);
        ConsoleUi.ShowTaskList(tasks, completedFilter);
    }

    private static void ShowHelp(string[] args)
    {
        if (args.Length == 0)
            ConsoleUi.ShowHelpMenu();
        else
            ConsoleUi.ShowCommandHelp(args[0]);
    }

    private static bool TryReadId(string[] args, string usage, out int id)
    {
        id = 0;

        if (args.Length == 0)
        {
            ConsoleUi.ShowUsage(usage);
            return false;
        }

        if (!int.TryParse(args[0], out id))
        {
            ConsoleUi.ShowInvalidId();
            return false;
        }

        return true;
    }

    private static string? ReadValueUntilOption(string[] args, int startIndex, out int nextIndex)
    {
        List<string> values = new();
        nextIndex = startIndex;

        while (nextIndex < args.Length && !IsOption(args[nextIndex]))
        {
            values.Add(args[nextIndex]);
            nextIndex++;
        }

        return values.Count == 0 ? null : string.Join(' ', values).Trim();
    }

    private static bool IsOption(string value) => value.StartsWith('-');

    private static bool IsNamedEditOption(string value)
    {
        string option = value.Trim().ToLowerInvariant();
        return option is "title" or "note";
    }

    private static bool IsHelpCommand(string command) => command is "help" or "--help" or "-h";

    private static bool IsHelpOption(string arg) => arg is "--help" or "-h";
}
