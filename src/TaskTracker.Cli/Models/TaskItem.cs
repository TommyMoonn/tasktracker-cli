using System.Globalization;
using System.Text.Json.Serialization;

namespace TaskTracker.Cli.Models
{
    public static class TaskPriority
    {
        public const string Low = "low";
        public const string Normal = "normal";
        public const string High = "high";

        public static bool TryNormalize(string? value, out string priority)
        {
            priority = Normal;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string normalized = value.Trim().ToLowerInvariant();

            switch (normalized)
            {
                case "l":
                case Low:
                    priority = Low;
                    return true;

                case "n":
                case "medium":
                case "med":
                case "m":
                case Normal:
                    priority = Normal;
                    return true;

                case "h":
                case High:
                    priority = High;
                    return true;

                default:
                    return false;
            }
        }

        public static string ToDisplayName(string? value)
        {
            if (!TryNormalize(value, out string priority))
                priority = Normal;

            return priority switch
            {
                Low => "Low",
                High => "High",
                _ => "Normal"
            };
        }
    }


    public static class TaskDueDate
    {
        public const string Today = "today";
        public const string Tomorrow = "tomorrow";
        public const string Week = "week";
        public const string Overdue = "overdue";
        public const string None = "none";

        public static bool TryParse(string? value, out DateOnly? dueDate, out bool clearDueDate)
        {
            dueDate = null;
            clearDueDate = false;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string normalized = value.Trim().ToLowerInvariant();

            if (normalized is "none" or "no" or "clear" or "remove" or "-" or "null")
            {
                clearDueDate = true;
                return true;
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (normalized == "today")
            {
                dueDate = today;
                return true;
            }

            if (normalized == "tomorrow")
            {
                dueDate = today.AddDays(1);
                return true;
            }

            if (DateOnly.TryParseExact(normalized, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsed))
            {
                dueDate = parsed;
                return true;
            }

            return false;
        }

        public static string ToDisplayText(DateOnly? dueDate)
        {
            if (dueDate == null)
                return "-";

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (dueDate.Value == today)
                return "Today";

            if (dueDate.Value == today.AddDays(1))
                return "Tomorrow";

            return dueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static bool IsOverdue(DateOnly? dueDate, bool isCompleted)
        {
            if (dueDate == null || isCompleted)
                return false;

            return dueDate.Value < DateOnly.FromDateTime(DateTime.Today);
        }

        public static bool TryNormalizeFilter(string? value, out string dueFilter)
        {
            dueFilter = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string normalized = value.Trim().ToLowerInvariant();

            switch (normalized)
            {
                case Today:
                case "td":
                    dueFilter = Today;
                    return true;

                case Tomorrow:
                case "tmr":
                    dueFilter = Tomorrow;
                    return true;

                case Week:
                case "this-week":
                case "weekly":
                    dueFilter = Week;
                    return true;

                case Overdue:
                case "late":
                    dueFilter = Overdue;
                    return true;

                case None:
                case "no":
                case "empty":
                    dueFilter = None;
                    return true;
            }

            if (DateOnly.TryParseExact(normalized, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsed))
            {
                dueFilter = parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return true;
            }

            return false;
        }

        public static string ToFilterDisplayText(string? dueFilter)
        {
            if (string.IsNullOrWhiteSpace(dueFilter))
                return string.Empty;

            return dueFilter switch
            {
                Today => "due today",
                Tomorrow => "due tomorrow",
                Week => "due this week",
                Overdue => "overdue",
                None => "without due dates",
                _ => $"due on {dueFilter}"
            };
        }

    }

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

        public TaskItem() { }

        public TaskItem(int id, string title, string note, bool isCompleted, string priority = TaskPriority.Normal, DateOnly? dueDate = null)
        {
            Id = id;
            Title = title;
            Note = note;
            IsCompleted = isCompleted;
            DueDate = dueDate;
            Priority = TaskPriority.TryNormalize(priority, out string normalizedPriority)
                ? normalizedPriority
                : TaskPriority.Normal;
        }
    }
}
