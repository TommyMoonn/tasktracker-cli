using TaskTracker.Cli.Models;
using TaskTracker.Cli.Persistence;

namespace TaskTracker.Cli.Services
{
    public enum TaskResult
    {
        AddSuccess,
        UpdateSuccess,
        RemoveSuccess,
        MarkCompleted,
        TaskNotFound,
        EmptyTitle,
        DuplicateTitle,
        InvalidPriority,
        UpdateFailed,
        AlreadyCompleted,
        NotCompleted,
        UndoSuccess
    }

    public class TaskServices
    {
        private readonly ITaskRepository _repo;

        public TaskServices(ITaskRepository repo)
        {
            _repo = repo;
        }

        public TaskResult AddTask(string title, string note, string priority = TaskPriority.Normal)
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
                IsCompleted = false
            };

            _repo.Add(task);

            return TaskResult.AddSuccess;
        }

        public TaskResult UpdateTask(int id, string? title, string? note, string? priority = null)
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

            bool result = _repo.Update(task);
            if (!result)
                return TaskResult.UpdateFailed;

            return TaskResult.UpdateSuccess;
        }

        public TaskResult RemoveTask(int id) => _repo.Remove(id) ? TaskResult.RemoveSuccess : TaskResult.TaskNotFound;

        public List<TaskItem> GetTasks() => _repo.GetAll();

        public List<TaskItem> GetTasksByStatus(bool? isCompleted = null, string? priority = null)
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
}
