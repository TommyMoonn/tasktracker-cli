using System.Globalization;

namespace TaskTracker.Cli.Models;

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

        if (normalized == Today)
        {
            dueDate = today;
            return true;
        }

        if (normalized == Tomorrow)
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
