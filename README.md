![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![C#](https://img.shields.io/badge/C%23-11-informational)
![License](https://img.shields.io/badge/License-MIT-green)

# TaskTracker CLI

TaskTracker CLI is a small C# command-line task manager with JSON storage, priorities, due dates, search, archiving, and an optional terminal UI.

## Features

- Manage tasks from the command line
- Add titles, notes, priorities, due dates, and status changes
- Search, filter, archive, and restore tasks
- Store tasks locally as JSON
- Open an interactive terminal UI when preferred

## Tech Stack

- C#
- .NET
- JSON file persistence
- Manual console rendering for the TUI

## Installation

### Option 1: Run from source

```bash
git clone https://github.com/TommyMoonn/tasktracker-cli.git
cd tasktracker-cli
dotnet run --project src/TaskTracker.Cli -- list
```

### Option 2: Install as a local .NET tool package

From the repository root:

```bash
dotnet pack src/TaskTracker.Cli -c Release
dotnet tool install --global --add-source ./src/TaskTracker.Cli/bin/Release tasktracker
```

After installing:

```bash
tasktracker list
```

To update after local changes:

```bash
dotnet pack src/TaskTracker.Cli -c Release
dotnet tool update --global --add-source ./src/TaskTracker.Cli/bin/Release tasktracker
```

To uninstall:

```bash
dotnet tool uninstall --global tasktracker
```

### Option 3: Run with Docker

Build the image from the repository root:

```bash
docker build -t tasktracker-cli .
```

Run the CLI:

```bash
docker run --rm -it tasktracker-cli list
```

To persist task data between container runs, mount a Docker volume:

```bash
docker volume create tasktracker-data
docker run --rm -it -v tasktracker-data:/root tasktracker-cli list
```

## Usage

### List tasks

```bash
tasktracker list
```

### Add a task

```bash
tasktracker add "Submit assignment" --priority high --due tomorrow
```

### Edit a task

```bash
tasktracker edit 3 --title "Submit database assignment"
```

### Complete or reopen a task

```bash
tasktracker done 3
tasktracker reopen 3
```

### Search tasks

```bash
tasktracker search groceries
```

### Archive and restore tasks

```bash
tasktracker archive completed
tasktracker restore 3
```

### Open the terminal UI

```bash
tasktracker tui
```

## Project Structure

```text
TaskTracker.Cli/
├── Cli/
├── Models/
├── Persistence/
├── Services/
├── Tui/
├── Ui/
└── Program.cs
```

## Data Storage

Tasks are saved as JSON in the user profile directory:

```text
.tasktracker.json
```

When running with Docker, use a mounted volume if you want the task file to survive after the container exits.

## Author

Khoa Luong

GitHub: https://github.com/TommyMoonn
