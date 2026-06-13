namespace TaskTracker.Cli.Cli;

public static class CliArguments
{
    public static bool TryReadId(string[] args, out int id)
    {
        id = 0;
        return args.Length > 0 && int.TryParse(args[0], out id);
    }

    public static bool TryReadOptionValue(string[] args, int valueIndex, out string? value)
    {
        value = null;

        if (valueIndex >= args.Length || IsOption(args[valueIndex]))
            return false;

        value = args[valueIndex];
        return true;
    }

    public static string? ReadValueUntilOption(string[] args, int startIndex, out int nextIndex)
    {
        List<string> values = new();
        nextIndex = startIndex;

        while (nextIndex < args.Length && !IsOption(args[nextIndex]))
        {
            values.Add(args[nextIndex]);
            nextIndex++;
        }

        return values.Count == 0 ? null : string.Join(' ', values).Trim();
    }

    public static bool IsOption(string value) => value.StartsWith('-');

    public static bool IsHelpCommand(string command) => command is "help" or "--help" or "-h";

    public static bool IsHelpOption(string arg) => arg is "--help" or "-h";

    public static bool IsNamedEditOption(string value)
    {
        string option = value.Trim().ToLowerInvariant();
        return option is "title" or "note" or "priority" or "due";
    }
}
