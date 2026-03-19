using System;
using System.Text.Json.Serialization;

namespace TaskTracker.Cli.Models
{
    public class TaskItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty;
        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        public TaskItem() { }

        public TaskItem(int id, string title, string note, bool isCompleted)
        {
            this.Id = id;
            this.Title = title;
            this.Note = note;
            this.IsCompleted = isCompleted;
        }

    }
}