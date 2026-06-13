using TaskTracker.Cli.Models;
using TaskTracker.Cli.Services;

namespace TaskTracker.Cli.Tui;

public class TuiApp
{
    private readonly TaskService _service;
    private readonly TuiState _state = new();
    private bool _running = true;

    public TuiApp(TaskService service)
    {
        _service = service;
    }

    public void Run()
    {
        Console.CursorVisible = false;

        try
        {
            while (_running)
            {
                List<TaskItem> tasks = GetVisibleTasks();
                _state.ClampSelection(tasks.Count);
                TuiRenderer.Draw(_state, tasks);

                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                HandleKey(key, tasks);
            }
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.Clear();
        }
    }

    private void HandleKey(ConsoleKeyInfo key, IReadOnlyList<TaskItem> tasks)
    {
        _state.StatusMessage = null;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _state.MoveSelection(-1, tasks.Count);
                break;

            case ConsoleKey.DownArrow:
                _state.MoveSelection(1, tasks.Count);
                break;

            case ConsoleKey.LeftArrow:
                _state.PreviousView();
                break;

            case ConsoleKey.RightArrow:
                _state.NextView();
                break;

            case ConsoleKey.Spacebar:
                ToggleSelectedTask(tasks);
                break;

            case ConsoleKey.A:
                AddTaskPrompt();
                break;

            case ConsoleKey.E:
                EditSelectedTaskPrompt(tasks);
                break;

            case ConsoleKey.D:
                DeleteSelectedTaskPrompt(tasks);
                break;

            case ConsoleKey.X:
                ArchiveOrRestoreSelectedTask(tasks);
                break;

            case ConsoleKey.Divide:
            case ConsoleKey.Oem2:
                SearchPrompt();
                break;

            case ConsoleKey.Escape:
                _state.SearchQuery = null;
                _state.StatusMessage = "Search cleared.";
                break;

            case ConsoleKey.Q:
                _running = false;
                break;
        }
    }

    private List<TaskItem> GetVisibleTasks()
    {
        if (!string.IsNullOrWhiteSpace(_state.SearchQuery))
            return _service.SearchTasks(_state.SearchQuery, _state.ArchivedFilter);

        return _service.GetTasksByStatus(isCompleted: null, priority: null, dueFilter: null, archivedFilter: _state.ArchivedFilter);
    }

    private TaskItem? GetSelectedTask(IReadOnlyList<TaskItem> tasks)
    {
        if (tasks.Count == 0 || _state.SelectedIndex < 0 || _state.SelectedIndex >= tasks.Count)
            return null;

        return tasks[_state.SelectedIndex];
    }

    private void ToggleSelectedTask(IReadOnlyList<TaskItem> tasks)
    {
        TaskItem? task = GetSelectedTask(tasks);
        if (task == null)
        {
            _state.StatusMessage = "No task selected.";
            return;
        }

        TaskResult result = _service.UpdateStatus(task.Id, !task.IsCompleted);
        _state.StatusMessage = ToStatusMessage(result, task.Id);
    }

    private void ArchiveOrRestoreSelectedTask(IReadOnlyList<TaskItem> tasks)
    {
        TaskItem? task = GetSelectedTask(tasks);
        if (task == null)
        {
            _state.StatusMessage = "No task selected.";
            return;
        }

        TaskResult result = task.IsArchived
            ? _service.RestoreTask(task.Id)
            : _service.ArchiveTask(task.Id);

        _state.StatusMessage = ToStatusMessage(result, task.Id);
    }

    private void AddTaskPrompt()
    {
        Console.CursorVisible = true;
        TuiRenderer.DrawPromptHeader("Add Task", "Leave title empty to cancel.");

        string? title = Prompt("Title");
        if (string.IsNullOrWhiteSpace(title))
        {
            _state.StatusMessage = "Add cancelled.";
            Console.CursorVisible = false;
            return;
        }

        string note = Prompt("Note") ?? string.Empty;
        string priority = ReadPriority(defaultValue: TaskPriority.Normal);
        DateOnly? dueDate = ReadDueDate(allowClear: false, keepExisting: false, existingValue: null, updateDueDate: out _);

        TaskResult result = _service.AddTask(title, note, priority, dueDate);
        _state.StatusMessage = result == TaskResult.AddSuccess
            ? "Task added."
            : ToStatusMessage(result, 0);

        _state.SearchQuery = null;
        Console.CursorVisible = false;
    }

    private void EditSelectedTaskPrompt(IReadOnlyList<TaskItem> tasks)
    {
        TaskItem? task = GetSelectedTask(tasks);
        if (task == null)
        {
            _state.StatusMessage = "No task selected.";
            return;
        }

        Console.CursorVisible = true;
        TuiRenderer.DrawPromptHeader($"Edit Task #{task.Id}", "Leave a field empty to keep the current value. Use 'none' to clear due date.");

        Console.WriteLine($"Current title: {task.Title}");
        string? title = Prompt("New title");
        if (string.IsNullOrWhiteSpace(title))
            title = null;

        Console.WriteLine($"Current note: {(string.IsNullOrWhiteSpace(task.Note) ? "-" : task.Note)}");
        string? note = Prompt("New note");
        if (note == null)
            note = null;

        Console.WriteLine($"Current priority: {TaskPriority.ToDisplayName(task.Priority)}");
        string? priorityInput = Prompt("New priority [low/normal/high]");
        string? priority = null;
        if (!string.IsNullOrWhiteSpace(priorityInput))
        {
            if (!TaskPriority.TryNormalize(priorityInput, out priority))
            {
                _state.StatusMessage = "Invalid priority. Edit cancelled.";
                Console.CursorVisible = false;
                return;
            }
        }

        Console.WriteLine($"Current due date: {TaskDueDate.ToDisplayText(task.DueDate)}");
        DateOnly? dueDate = ReadDueDate(allowClear: true, keepExisting: true, existingValue: task.DueDate, updateDueDate: out bool updateDueDate);

        TaskResult result = _service.UpdateTask(task.Id, title, note, priority, dueDate, updateDueDate);
        _state.StatusMessage = ToStatusMessage(result, task.Id);
        Console.CursorVisible = false;
    }

    private void DeleteSelectedTaskPrompt(IReadOnlyList<TaskItem> tasks)
    {
        TaskItem? task = GetSelectedTask(tasks);
        if (task == null)
        {
            _state.StatusMessage = "No task selected.";
            return;
        }

        Console.CursorVisible = true;
        TuiRenderer.DrawPromptHeader($"Delete Task #{task.Id}", task.Title);
        string? confirm = Prompt("Type y to delete");

        if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
        {
            _state.StatusMessage = "Delete cancelled.";
            Console.CursorVisible = false;
            return;
        }

        TaskResult result = _service.RemoveTask(task.Id);
        _state.StatusMessage = ToStatusMessage(result, task.Id);
        Console.CursorVisible = false;
    }

    private void SearchPrompt()
    {
        Console.CursorVisible = true;
        TuiRenderer.DrawPromptHeader("Search Tasks", "Search checks titles and notes. Leave empty to clear search.");
        string? query = Prompt("Search");

        _state.SearchQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        _state.SelectedIndex = 0;
        _state.StatusMessage = _state.SearchQuery == null ? "Search cleared." : $"Searching for '{_state.SearchQuery}'.";
        Console.CursorVisible = false;
    }

    private static string ReadPriority(string defaultValue)
    {
        string? input = Prompt($"Priority [low/normal/high, empty = {defaultValue}]");

        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;

        return TaskPriority.TryNormalize(input, out string priority)
            ? priority
            : defaultValue;
    }

    private static DateOnly? ReadDueDate(bool allowClear, bool keepExisting, DateOnly? existingValue, out bool updateDueDate)
    {
        string label = allowClear
            ? "Due [today/tomorrow/yyyy-mm-dd/none]"
            : "Due [today/tomorrow/yyyy-mm-dd, empty = none]";

        string? input = Prompt(label);

        if (string.IsNullOrWhiteSpace(input))
        {
            updateDueDate = !keepExisting;
            return keepExisting ? existingValue : null;
        }

        if (!TaskDueDate.TryParse(input, out DateOnly? dueDate, out bool clearDueDate))
        {
            updateDueDate = false;
            return keepExisting ? existingValue : null;
        }

        updateDueDate = true;
        return clearDueDate ? null : dueDate;
    }

    private static string? Prompt(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine();
    }

    private static string ToStatusMessage(TaskResult result, int id)
    {
        return result switch
        {
            TaskResult.AddSuccess => "Task added.",
            TaskResult.UpdateSuccess => $"Task {id} updated.",
            TaskResult.RemoveSuccess => $"Task {id} deleted.",
            TaskResult.MarkCompleted => $"Task {id} marked done.",
            TaskResult.UndoSuccess => $"Task {id} reopened.",
            TaskResult.ArchiveSuccess => $"Task {id} archived.",
            TaskResult.RestoreSuccess => $"Task {id} restored.",
            TaskResult.AlreadyCompleted => $"Task {id} is already done.",
            TaskResult.NotCompleted => $"Task {id} must be completed first.",
            TaskResult.AlreadyArchived => $"Task {id} is already archived.",
            TaskResult.NotArchived => $"Task {id} is not archived.",
            TaskResult.TaskNotFound => $"Task {id} was not found.",
            TaskResult.DuplicateTitle => "A task with that title already exists.",
            TaskResult.EmptyTitle => "Title is required.",
            TaskResult.InvalidPriority => "Priority must be low, normal, or high.",
            TaskResult.InvalidDueDate => "Due date must be today, tomorrow, yyyy-mm-dd, or none.",
            _ => "Action failed."
        };
    }
}
