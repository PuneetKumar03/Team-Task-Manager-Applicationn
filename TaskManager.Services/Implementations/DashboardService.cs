using Microsoft.EntityFrameworkCore;
using TaskManager.DataAccess.Data;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    // Dashboard needs complex aggregations across multiple tables.
    // Using DbContext directly here is intentional — it avoids
    // multiple repository round-trips and keeps the query in one place.
    public DashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(
        string? userId = null, bool isAdmin = false)
    {
        var now = DateTime.UtcNow;

        // ── Base query with optional user filter ──────────────────────────────
        var taskQuery = _db.TaskItems.AsNoTracking();
        var projectQuery = _db.Projects.AsNoTracking().Where(p => p.IsActive);

        if (!isAdmin && userId != null)
        {
            // Members only see their own tasks and their projects
            taskQuery = taskQuery.Where(t => t.AssignedToUserId == userId);
            projectQuery = projectQuery.Where(p =>
                p.Members.Any(m => m.UserId == userId));
        }

        // ── Task counts (parallel async for performance) ───────────────────
        var totalTasks = await taskQuery.CountAsync();
        var completedTasks = await taskQuery.CountAsync(t =>
            t.Status == TaskItemStatus.Completed);
        var pendingTasks = await taskQuery.CountAsync(t =>
            t.Status != TaskItemStatus.Completed);
        var overdueTasks = await taskQuery.CountAsync(t =>
            t.Deadline < now && t.Status != TaskItemStatus.Completed);

        // ── Project counts ─────────────────────────────────────────────────
        var totalProjects = await projectQuery.CountAsync();
        var activeProjects = await projectQuery.CountAsync(p =>
            p.Tasks.Any(t => t.Status != TaskItemStatus.Completed));

        // ── Recent tasks (last 5 updated) ──────────────────────────────────
        var recentTasks = await taskQuery
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(5)
            .Select(t => new RecentTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                ProjectName = t.Project.Name,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                Deadline = t.Deadline,
                AssigneeName = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned"
            })
            .ToListAsync();

        // ── Upcoming deadlines (next 7 days, not completed) ────────────────
        var upcomingDeadlines = await taskQuery
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Where(t => t.Deadline >= now
                     && t.Deadline <= now.AddDays(7)
                     && t.Status != TaskItemStatus.Completed)
            .OrderBy(t => t.Deadline)
            .Take(5)
            .Select(t => new RecentTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                ProjectName = t.Project.Name,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                Deadline = t.Deadline,
                AssigneeName = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned"
            })
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            OverdueTasks = overdueTasks,
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            RecentTasks = recentTasks,
            UpcomingDeadlines = upcomingDeadlines
        };
    }
}