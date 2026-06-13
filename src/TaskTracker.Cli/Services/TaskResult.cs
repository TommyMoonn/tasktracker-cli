namespace TaskTracker.Cli.Services;

public enum TaskResult
{
    AddSuccess,
    UpdateSuccess,
    RemoveSuccess,
    MarkCompleted,
    TaskNotFound,
    EmptyTitle,
    DuplicateTitle,
    InvalidPriority,
    InvalidDueDate,
    UpdateFailed,
    AlreadyCompleted,
    NotCompleted,
    UndoSuccess
}
