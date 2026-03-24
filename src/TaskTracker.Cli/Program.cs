using System;
using System.Text.Json.Nodes;
using TaskTracker.Cli.Models;
using TaskTracker.Cli.Persistence;
using TaskTracker.Cli.Services;

public class Program
{
    public static void Main(String[] args)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var filePath = Path.Combine(home, ".tasktracker.json"); 
        var repo = new JsonTaskRepository(filePath);
        var service = new TaskServices(repo);

        if (args.Length == 0)
        {
            ShowHelpMenu();
            return;
        }

        switch (args[0])
        {
            case "help":
            case "--help":
            case "-h":
                ShowHelpMenu();
                break;
            case "list":
            case "ls":
            case "-l":
                List<TaskItem> tasks;

                bool? showCompleted = null;

                if (args.Length > 1)
                {
                    if (args[1].Equals("-c") || args[1].Equals("--completed"))
                        showCompleted = true;
                    else if (args[1].Equals("-p") || args[1].Equals("--pending"))
                        showCompleted = false;
                }

                tasks = service.GetTasksByStatus(showCompleted);

                foreach (var task in tasks)
                {
                    string status = task.IsCompleted ? "Completed" : "Pending";
                    Console.WriteLine($"{task.Id}. {task.Title} - {task.Note} >> {status}");
                }
                break;
            case "add":
            case "-a":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: [add | -a] <title> [--note or -n] <note>");
                    return;
                }

                string title = args[1];

                string note = string.Empty;
                if (args.Length >= 4 && (args[2] == "-n" || args[2] == "--note"))
                {
                    note = args[3];
                }

                TaskResult addResult = service.AddTask(title, note);
                Console.WriteLine(GetMessage(addResult, title));
                break;
            case "complete":
            case "-c":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: [complete | -c] <id>");
                    return;
                }

                if (!int.TryParse(args[1], out int completeId))
                {
                    Console.WriteLine("Invalid id input.");
                    return;
                }

                TaskResult markCompleteResult = service.UpdateStatus(completeId, true);
                Console.WriteLine(GetMessage(markCompleteResult, args[1]));
                break;
            case "undo":
            case "revert":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: [undo | revert] <id>");
                    return;
                }

                if (!int.TryParse(args[1], out int undoId))
                {
                    Console.WriteLine("Invalid id input.");
                    return;
                }

                TaskResult undoResult = service.UpdateStatus(undoId, false);
                Console.WriteLine(GetMessage(undoResult, args[1]));
                break;
            case "update":
            case "edit":
            case "-e":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: [update | -u] <id> [title | -t] <newTitle> [note | -n] <newNote>");
                    return;
                }

                if (!int.TryParse(args[1], out int updateId))
                {
                    Console.WriteLine("Invalid id input.");
                    return;
                }

                string? newTitle = null;
                string? newNote = null;

                for (int i = 2; i < args.Length; i++)
                {
                    if ("title" == args[i].Trim() || "-t" == args[i].Trim())
                    {
                        newTitle = args[++i];
                    }
                    else if ("note" == args[i].Trim() || "-n" == args[i].Trim())
                    {
                        newNote = args[++i];
                    }
                }

                TaskResult updateResult = service.UpdateTask(updateId, newTitle, newNote);
                Console.WriteLine(GetMessage(updateResult, args[1]));
                break;
            case "remove":
            case "delete":
            case "-d":
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: [remove | -d] <id>");
                    return;
                }

                if (!int.TryParse(args[1], out int removeId))
                {
                    Console.WriteLine("Invalid id input.");
                    return;
                }

                TaskResult removeTaskResult = service.RemoveTask(removeId);
                Console.WriteLine(GetMessage(removeTaskResult, args[1]));
                break;
            default:
                string argumentString = string.Join(' ', args);
                Console.WriteLine($"'{argumentString}' is not a valid tasktracker command.");
                Console.WriteLine("Usage: tasktracker [action] [options]");
                Console.WriteLine("use 'tasktracker --help or -h' for additional information.");
                break;
        }

    }

    private static void ShowHelpMenu()
    {
        Console.WriteLine("Usage: tasktracker [command] [options]");
        Console.WriteLine("   ls [--completed | -c] or [--pending | -p]             | -l : List tasks");
        Console.WriteLine("   add <title> [--note | -n <note>] <note>               | -a : Add a new task");
        Console.WriteLine("   complete <id>                                         | -c : Mark a task as completed");
        Console.WriteLine("   undo <id>                                             | revert : Undo a task completion");
        Console.WriteLine("   update <id> -t <title> -n <note>                      | -u: Update task details");
        Console.WriteLine("   remove <id>                                           | -d : Delete task by ID");
    }

    private static string GetMessage(TaskResult result, string identifier)
    {
        return result switch
        {
            TaskResult.AddSuccess => $"Task added successfully.",
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

}

