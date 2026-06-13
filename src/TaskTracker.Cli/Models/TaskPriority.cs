namespace TaskTracker.Cli.Models;

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
