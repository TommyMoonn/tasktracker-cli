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
            IsCompleted = false
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

    public List<TaskItem> GetTasksByStatus(bool? isCompleted = null, string? priority = null, string? dueFilter = null)
    {
        var allTasks = _repo.GetAll();
        IEnumerable<TaskItem> query = allTasks;

        if (isCompleted != null)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        if (priority != null)
        {
            if (!TaskPriority.TryNormalize(priority, out string normalizedPriority))
                return new List<TaskItem>();

            query = query.Where(t => TaskPriority.TryNormalize(t.Priority, out string taskPriority) && taskPriority == normalizedPriority);
        }

        if (dueFilter != null)
        {
            if (!TaskDueDate.TryNormalizeFilter(dueFilter, out string normalizedDueFilter))
                return new List<TaskItem>();

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
}
