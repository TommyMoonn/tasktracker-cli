using System.Text.Json;
using TaskTracker.Cli.Models;

namespace TaskTracker.Cli.Persistence;

public class JsonTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks;
    private readonly string _filePath;

    public JsonTaskRepository(string filePath)
    {
        _filePath = filePath;
        _tasks = Load();
    }

    private void Save()
    {
        string json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        File.WriteAllText(_filePath, json);
    }

    private List<TaskItem> Load()
    {
        if (!File.Exists(_filePath))
            return new List<TaskItem>();

        string json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<TaskItem>();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json, options) ?? new List<TaskItem>();

        foreach (var task in tasks)
        {
            task.Priority = TaskPriority.TryNormalize(task.Priority, out string priority)
                ? priority
                : TaskPriority.Normal;
        }

        return tasks;
    }

    public void Add(TaskItem task)
    {
        _tasks.Add(task);
        Save();
    }

    public bool Update(TaskItem task)
    {
        var existing = GetById(task.Id);

        if (existing == null)
            return false;

        existing.Title = task.Title;
        existing.Note = task.Note;
        existing.Priority = TaskPriority.TryNormalize(task.Priority, out string priority)
            ? priority
            : TaskPriority.Normal;
        existing.DueDate = task.DueDate;
        existing.IsCompleted = task.IsCompleted;
        existing.IsArchived = task.IsArchived;

        Save();
        return true;
    }

    public bool Remove(int id)
    {
        var existing = GetById(id);

        if (existing == null)
            return false;

        _tasks.Remove(existing);
        Save();
        return true;
    }

    public TaskItem? GetById(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    public List<TaskItem> GetAll() => new List<TaskItem>(_tasks);
}
