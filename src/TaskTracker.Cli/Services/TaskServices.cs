using TaskTracker.Cli.Persistence;

namespace TaskTracker.Cli.Services;

// Compatibility wrapper for older code that still references TaskServices.
// New code should use TaskService.
public class TaskServices : TaskService
{
    public TaskServices(ITaskRepository repo) : base(repo)
    {
    }
}
