using System;
using System.Collections.Generic;
using System.Text;
using TaskTracker.Cli.Models;

namespace TaskTracker.Cli.Persistence
{
    public interface ITaskRepository
    {
        void Add(TaskItem task);
        bool Update(TaskItem task);
        bool Remove(int id);
        TaskItem? GetById(int id);
        List<TaskItem> GetAll();
    }
}
