using System.Text;
using TaskTracker.Cli.Cli;
using TaskTracker.Cli.Persistence;
using TaskTracker.Cli.Services;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var filePath = Path.Combine(home, ".tasktracker.json");

        var repo = new JsonTaskRepository(filePath);
        var service = new TaskService(repo);
        var app = new CliApp(service);

        app.Run(args);
    }
}
