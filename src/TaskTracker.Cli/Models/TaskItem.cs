using System.Text.Json.Serialization;

namespace TaskTracker.Cli.Models;

public class TaskItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = TaskPriority.Normal;

    [JsonPropertyName("dueDate")]
    public DateOnly? DueDate { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [JsonPropertyName("isArchived")]
    public bool IsArchived { get; set; } = false;

    public TaskItem() { }

    public TaskItem(int id, string title, string note, bool isCompleted, string priority = TaskPriority.Normal, DateOnly? dueDate = null, bool isArchived = false)
    {
        Id = id;
        Title = title;
        Note = note;
        IsCompleted = isCompleted;
        DueDate = dueDate;
        IsArchived = isArchived;
        Priority = TaskPriority.TryNormalize(priority, out string normalizedPriority)
            ? normalizedPriority
            : TaskPriority.Normal;
    }
}
