![.NET|50](https://img.shields.io/badge/.NET-10.0-blue)  
![C#](https://img.shields.io/badge/C%23-11-informational)
![License](https://img.shields.io/badge/License-MIT-green)
# Task Tracker CLI Tool

A beginner-friendly C# CLI project for managing tasks. This project helped me get familiar with C# syntax, libraries, methods, and coding conventions (e.g., PascalCase for public members, private fields with _ prefix).

---
## ✨ Features

- Create, edit, and delete tasks
- Tasks have a title, note, and status (completed or not)
- Tasks are persisted in JSON files
- CLI commands with simple syntax and optional flags

---
## 🛠️ Tech Stack

- **C# 11** – Main programming language
- **.NET 8** – Runtime & CLI framework
- **JSON** – Task persistence format
- **Visual Studio** – IDE for development

---
## 📂 Project Structure

```
TaskTracker.Cli/  
├── TaskTracker.Cli.csproj  
├── Program.cs                  # Entry point, CLI parsing  
├── Models/  
│   └── TaskItem.cs             # Task model  
├── Services/  
│   └── TaskServices.cs         # Business logic & validation  
├── Persistence/  
│   └── ITaskRepository.cs  
│   └── JsonTaskRepository.cs   # JSON storage  
└── README.md
```

---
## ⚙️ Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/TaskTracker.Cli.git

cd TaskTracker.Cli
```
---
2. Build and run locally:
```bash
dotnet run -- [command] [options]
```

Example:
```bash
dotnet run -- add "Buy groceries" -n "Milk, eggs, bread"  
dotnet run -- list --completed
```
---
3. **(Optional)** Publish as a standalone executable for easy use:
```bash
dotnet publish -c Release
```
✅ You don’t need to add extra flags for self-contained or single-file — it’s already configured in `.csproj`.

- The executable will be in:
```bash
bin/Release/net10.0/win-x64/publish/
```
---
4. **(Optional)** Add a Global Tool option: 
```bash
dotnet tool install --global --add-source ./ TaskTracker.Cli
```

Then you can call your commands globally:
```bash
tasktracker add "Buy fruits"  
tasktracker list --pending
```

- Make sure `tasktracker` (your executable) is in your PATH if you don’t use the global tool.

---
## ⚡ Usage

### Add a task
```bash
tasktracker add "Buy groceries" -n "Milk, eggs, bread"
tasktracker -a "Buy groceries" 
```
### List all tasks
```bash
tasktracker list
tasktracker ls
tasktracker -l
```
### List completed tasks
```bash
tasktracker list --completed
tasktracker list -c
```
### List pending tasks
```bash
tasktracker list --pending
tasktracker list -p
```
### Revert/Undo a task completion
```bash
tasktracker undo 1
tasktracker revert 1
```

### Update a task
```bash
tasktracker update 1 -t "Buy groceries and veggies" -n "Milk, eggs, bread, carrots"
```
### Remove a task
```bash
tasktracker remove 1
```

---
## 🔮 Future Features  
  
- Task priorities (High, Medium, Low)  
- Due dates for tasks  
- Search tasks by keyword  
- Tags / categories for tasks  
- Export / import tasks (CSV/JSON)  
- Undo last action for safety

---
## 🚀 Development Goals

- Practice C# basics and language conventions
- Learn to implement a service layer with validation
- Work with JSON persistence
- Build a real-world CLI tool

---
## 👤 Author
### Khoa Luong

GitHub:
https://github.com/TommyMoonn
