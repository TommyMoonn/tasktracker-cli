FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY TaskTrackerCliTool.slnx ./
COPY src/TaskTracker.Cli/TaskTracker.Cli.csproj src/TaskTracker.Cli/

RUN dotnet restore TaskTrackerCliTool.slnx

COPY . .
RUN dotnet publish src/TaskTracker.Cli/TaskTracker.Cli.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskTracker.Cli.dll"]
