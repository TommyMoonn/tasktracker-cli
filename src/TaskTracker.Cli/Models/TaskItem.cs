using System;

namespace TaskTracker.Cli.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } 
        public string Note { get; set; } = string.Empty;
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