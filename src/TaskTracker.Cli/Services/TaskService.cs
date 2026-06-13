using TaskTracker.Cli.Models;
using TaskTracker.Cli.Persistence;

namespace TaskTracker.Cli.Services;

public class TaskService
{
    private readonly ITaskRepository _repo;

    public TaskService(ITaskRepository repo)
    {
        _repo = repo;
    }

    public TaskResult AddTask(string title, string note, string priority = TaskPriority.Normal, DateOnly? dueDate = null)
    {
        var tasks = _repo.GetAll();

        if (string.IsNullOrWhiteSpace(title))
            return TaskResult.EmptyTitle;

        title = title.Trim();
        if (tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
            return TaskResult.DuplicateTitle;

        if (!TaskPriority.TryNormalize(priority, out string normalizedPriority))
            return TaskResult.InvalidPriority;

        note = string.IsNullOrWhiteSpace(note) ? string.Empty : note.Trim();

        int nextId = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;

        var task = new TaskItem
        {
            Id = nextId,
            Title = title,
            Note = note,
            Priority = normalizedPriority,
            DueDate = dueDate,
            IsCompleted = false,
            IsArchived = false
        };

        _repo.Add(task);

        return TaskResult.AddSuccess;
    }

    public TaskResult UpdateTask(int id, string? title, string? note, string? priority = null, DateOnly? dueDate = null, bool updateDueDate = false)
    {
        var task = _repo.GetById(id);
        if (task == null)
            return TaskResult.TaskNotFound;

        var tasks = _repo.GetAll();

        if (!string.IsNullOrWhiteSpace(title))
        {
            title = title.Trim();
            bool duplicate = tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && t.Id != id);
            if (duplicate)
                return TaskResult.DuplicateTitle;

            task.Title = title;
        }

        if (note != null)
            task.Note = note.Trim();

        if (priority != null)
        {
            if (!TaskPriority.TryNormalize(priority, out string normalizedPriority))
                return TaskResult.InvalidPriority;

            task.Priority = normalizedPriority;
        }

        if (updateDueDate)
            task.DueDate = dueDate;

        bool result = _repo.Update(task);
        if (!result)
            return TaskResult.UpdateFailed;

        return TaskResult.UpdateSuccess;
    }

    public TaskResult RemoveTask(int id) => _repo.Remove(id) ? TaskResult.RemoveSuccess : TaskResult.TaskNotFound;

    public TaskItem? GetTaskById(int id) => _repo.GetById(id);

    public List<TaskItem> GetTasks() => _repo.GetAll();

    public List<TaskItem> GetTasksByStatus(bool? isCompleted = null, string? priority = null, string? dueFilter = null, bool? archivedFilter = false)
    {
        IEnumerable<TaskItem> query = _repo.GetAll();
        query = ApplyFilters(query, isCompleted, priority, dueFilter, archivedFilter);
        return query.ToList();
    }

    public List<TaskItem> SearchTasks(string searchText, bool? archivedFilter = false)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return new List<TaskItem>();

        string normalizedSearch = searchText.Trim();
        IEnumerable<TaskItem> query = _repo.GetAll();

        query = ApplyArchiveFilter(query, archivedFilter);
        query = query.Where(task =>
            task.Title.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
            task.Note.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

        return query.ToList();
    }

    public TaskResult UpdateStatus(int id, bool status)
    {
        var task = _repo.GetById(id);
        if (task == null)
            return TaskResult.TaskNotFound;

        if (task.IsCompleted && status)
            return TaskResult.AlreadyCompleted;

        if (!task.IsCompleted && !status)
            return TaskResult.NotCompleted;

        task.IsCompleted = status;

        bool success = _repo.Update(task);
        if (!success)
            return TaskResult.UpdateFailed;

        return status ? TaskResult.MarkCompleted : TaskResult.UndoSuccess;
    }

    public TaskResult ArchiveTask(int id)
    {
        var task = _repo.GetById(id);
        if (task == null)
            return TaskResult.TaskNotFound;

        if (task.IsArchived)
            return TaskResult.AlreadyArchived;

        if (!task.IsCompleted)
            return TaskResult.NotCompleted;

        task.IsArchived = true;

        bool success = _repo.Update(task);
        return success ? TaskResult.ArchiveSuccess : TaskResult.UpdateFailed;
    }

    public int ArchiveCompletedTasks()
    {
        var completedTasks = _repo.GetAll()
            .Where(task => task.IsCompleted && !task.IsArchived)
            .ToList();

        foreach (var task in completedTasks)
        {
            task.IsArchived = true;
            _repo.Update(task);
        }

        return completedTasks.Count;
    }

    public TaskResult RestoreTask(int id)
    {
        var task = _repo.GetById(id);
        if (task == null)
            return TaskResult.TaskNotFound;

        if (!task.IsArchived)
            return TaskResult.NotArchived;

        task.IsArchived = false;

        bool success = _repo.Update(task);
        return success ? TaskResult.RestoreSuccess : TaskResult.UpdateFailed;
    }

    private static IEnumerable<TaskItem> ApplyFilters(IEnumerable<TaskItem> query, bool? isCompleted, string? priority, string? dueFilter, bool? archivedFilter)
    {
        query = ApplyArchiveFilter(query, archivedFilter);

        if (isCompleted != null)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        if (priority != null)
        {
            if (!TaskPriority.TryNormalize(priority, out string normalizedPriority))
                return Enumerable.Empty<TaskItem>();

            query = query.Where(t => TaskPriority.TryNormalize(t.Priority, out string taskPriority) && taskPriority == normalizedPriority);
        }

        if (dueFilter != null)
        {
            if (!TaskDueDate.TryNormalizeFilter(dueFilter, out string normalizedDueFilter))
                return Enumerable.Empty<TaskItem>();

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            query = normalizedDueFilter switch
            {
                TaskDueDate.Today => query.Where(t => t.DueDate == today),
                TaskDueDate.Tomorrow => query.Where(t => t.DueDate == today.AddDays(1)),
                TaskDueDate.Week => query.Where(t => t.DueDate != null && t.DueDate.Value >= today && t.DueDate.Value <= today.AddDays(7)),
                TaskDueDate.Overdue => query.Where(t => TaskDueDate.IsOverdue(t.DueDate, t.IsCompleted)),
                TaskDueDate.None => query.Where(t => t.DueDate == null),
                _ => DateOnly.TryParse(normalizedDueFilter, out DateOnly exactDate)
                    ? query.Where(t => t.DueDate == exactDate)
                    : Enumerable.Empty<TaskItem>()
            };
        }

        return query;
    }

    private static IEnumerable<TaskItem> ApplyArchiveFilter(IEnumerable<TaskItem> query, bool? archivedFilter)
    {
        return archivedFilter switch
        {
            true => query.Where(t => t.IsArchived),
            false => query.Where(t => !t.IsArchived),
            _ => query
        };
    }
}
