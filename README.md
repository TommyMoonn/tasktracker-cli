![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![C#](https://img.shields.io/badge/C%23-11-informational)
![License](https://img.shields.io/badge/License-MIT-green)

# TaskTracker CLI

A small C# command-line task manager with JSON storage, task priorities, due dates, search, archiving, and a manual terminal UI.

## ✨ Features

- Add, edit, delete, complete, and reopen tasks
- Store task title, note, status, priority, and due date
- Search tasks by title or note
- Archive completed tasks and restore archived tasks
- Filter tasks by status, priority, due date, and archive state
- View task details from the command line
- Use an interactive terminal UI with keyboard navigation
- Show a custom TaskTracker ASCII logo

## 🛠️ Tech Stack

- C#
- .NET
- JSON file persistence
- Manual console rendering

## 📦 Installation

### Clone the repository

```bash
git clone https://github.com/TommyMoonn/tasktracker-cli.git
cd tasktracker-cli/src/TaskTracker.Cli
```

### Run locally

```bash
dotnet run -- list
dotnet run -- add "Buy groceries" --note "carrots potatoes oil"
```

### Build locally

```bash
dotnet build
```

### Pack as a local .NET tool

```bash
dotnet pack -c Release
```

### Install as a global tool

```bash
dotnet tool install --global --add-source ./bin/Release tasktracker
```

### Update the global tool after changes

```bash
dotnet pack -c Release
dotnet tool update --global --add-source ./bin/Release tasktracker
```

### Uninstall the global tool

```bash
dotnet tool uninstall --global tasktracker
```

## ⚡ Usage

### List tasks

```bash
tasktracker
tasktracker list
tasktracker ls
```

### Add a task

```bash
tasktracker add "Submit assignment"
tasktracker add "Submit assignment" --priority high --due tomorrow
tasktracker add "Buy groceries" --note "carrots potatoes oil"
```

### Edit a task

```bash
tasktracker edit 3 --title "Submit database assignment"
tasktracker edit 3 --note "finish ERD first"
tasktracker edit 3 --priority high
tasktracker edit 3 --due 2026-06-20
tasktracker edit 3 --due none
```

### Complete or reopen a task

```bash
tasktracker done 3
tasktracker reopen 3
```

### View task details

```bash
tasktracker view 3
```

### Delete a task

```bash
tasktracker delete 3
```

### Filter tasks

```bash
tasktracker list --open
tasktracker list --done
tasktracker list --priority high
tasktracker list --due today
tasktracker list --due week
tasktracker list --overdue
tasktracker list --archived
tasktracker list --include-archived
```

### Search tasks

```bash
tasktracker search groceries
tasktracker search assignment --include-archived
```

### Archive tasks

```bash
tasktracker archive
tasktracker archive completed
tasktracker archive 3
tasktracker restore 3
```

### Open the interactive TUI

```bash
tasktracker tui
tasktracker ui
```

### Show the logo

```bash
tasktracker fun
tasktracker logo
tasktracker banner
```

## ⌨️ TUI Controls

```text
Up/Down or j/k       Move selection
PageUp/PageDown      Move by page
Home/End             Jump to first or last task
Left/Right or Tab    Switch view
Space                Done or reopen selected task
a                    Add task
e                    Edit selected task
d                    Delete selected task
x                    Archive or restore selected task
/                    Search
Esc                  Clear search
q                    Quit
```

## 📁 Project Structure

```text
TaskTracker.Cli/
├── Cli/
│   ├── CliApp.cs
│   └── CliArguments.cs
├── Models/
│   ├── TaskItem.cs
│   ├── TaskPriority.cs
│   └── TaskDueDate.cs
├── Persistence/
│   ├── ITaskRepository.cs
│   └── JsonTaskRepository.cs
├── Services/
│   ├── TaskResult.cs
│   ├── TaskService.cs
│   └── TaskServices.cs
├── Tui/
│   ├── TuiApp.cs
│   ├── TuiRenderer.cs
│   └── TuiState.cs
├── Ui/
│   └── ConsoleUi.cs
└── Program.cs
```

## 🗃️ Data Storage

Tasks are saved as JSON in the user profile directory.

```text
.tasktracker.json
```

## 👤 Author

Khoa Luong

GitHub: https://github.com/TommyMoonn
