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
            ConsoleUi.ShowHelpMenu();
            return;
        }

        switch (args[0])
        {
            case "help":
            case "--help":
            case "-h":
                ConsoleUi.ShowHelpMenu();
                break;

            case "list":
            case "ls":
            case "-l":
                bool? showCompleted = null;

                if (args.Length > 1)
                {
                    if (args[1].Equals("-c") || args[1].Equals("--completed"))
                        showCompleted = true;
                    else if (args[1].Equals("-p") || args[1].Equals("--pending"))
                        showCompleted = false;
                }

                List<TaskItem> tasks = service.GetTasksByStatus(showCompleted);
                ConsoleUi.ShowTaskList(tasks, showCompleted);
                break;

            case "add":
            case "-a":
                if (args.Length < 2)
                {
                    ConsoleUi.ShowUsage("tasktracker add <title> [--note | -n] <note>");
                    return;
                }

                string title = args[1];
                string note = string.Empty;

                if (args.Length >= 4 && (args[2] == "-n" || args[2] == "--note"))
                    note = args[3];

                TaskResult addResult = service.AddTask(title, note);
                ConsoleUi.ShowResult(addResult, title);
                break;

            case "complete":
            case "-c":
                if (args.Length < 2)
                {
                    ConsoleUi.ShowUsage("tasktracker complete <id>");
                    return;
                }

                if (!int.TryParse(args[1], out int completeId))
                {
                    ConsoleUi.ShowInvalidId();
                    return;
                }

                TaskResult markCompleteResult = service.UpdateStatus(completeId, true);
                ConsoleUi.ShowResult(markCompleteResult, args[1]);
                break;

            case "undo":
            case "revert":
                if (args.Length < 2)
                {
                    ConsoleUi.ShowUsage("tasktracker undo <id>");
                    return;
                }

                if (!int.TryParse(args[1], out int undoId))
                {
                    ConsoleUi.ShowInvalidId();
                    return;
                }

                TaskResult undoResult = service.UpdateStatus(undoId, false);
                ConsoleUi.ShowResult(undoResult, args[1]);
                break;

            case "update":
            case "edit":
            case "-e":
            case "-u":
                if (args.Length < 2)
                {
                    ConsoleUi.ShowUsage("tasktracker update <id> [title | -t] <newTitle> [note | -n] <newNote>");
                    return;
                }

                if (!int.TryParse(args[1], out int updateId))
                {
                    ConsoleUi.ShowInvalidId();
                    return;
                }

                string? newTitle = null;
                string? newNote = null;

                for (int i = 2; i < args.Length; i++)
                {
                    string option = args[i].Trim();

                    if ((option == "title" || option == "-t") && i + 1 < args.Length)
                    {
                        newTitle = args[++i];
                    }
                    else if ((option == "note" || option == "-n") && i + 1 < args.Length)
                    {
                        newNote = args[++i];
                    }
                }

                TaskResult updateResult = service.UpdateTask(updateId, newTitle, newNote);
                ConsoleUi.ShowResult(updateResult, args[1]);
                break;

            case "remove":
            case "delete":
            case "-d":
                if (args.Length < 2)
                {
                    ConsoleUi.ShowUsage("tasktracker remove <id>");
                    return;
                }

                if (!int.TryParse(args[1], out int removeId))
                {
                    ConsoleUi.ShowInvalidId();
                    return;
                }

                TaskResult removeTaskResult = service.RemoveTask(removeId);
                ConsoleUi.ShowResult(removeTaskResult, args[1]);
                break;

            default:
                string argumentString = string.Join(' ', args);
                ConsoleUi.ShowInvalidCommand(argumentString);
                break;
        }
    }
}
