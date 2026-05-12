# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskManager.sln .
COPY TaskManager.Web/TaskManager.Web.csproj             TaskManager.Web/
COPY TaskManager.Models/TaskManager.Models.csproj       TaskManager.Models/
COPY TaskManager.DataAccess/TaskManager.DataAccess.csproj TaskManager.DataAccess/
COPY TaskManager.Services/TaskManager.Services.csproj   TaskManager.Services/
COPY TaskManager.Utility/TaskManager.Utility.csproj     TaskManager.Utility/

RUN dotnet restore

COPY . .

RUN dotnet publish TaskManager.Web/TaskManager.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManager.Web.dll"]