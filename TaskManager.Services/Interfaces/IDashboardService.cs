namespace TaskManager.Services.Interfaces;

/// <summary>
/// Provides aggregated statistics for the dashboard page.
/// Keeping this in its own service means the DashboardController
/// stays thin — it just asks for data, never computes it.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns all dashboard stats in one call.
    /// userId = null means global stats (Admin view).
    /// userId = specific user means that user's personal stats (Member view).
    /// </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync(
        string? userId = null,
        bool isAdmin = false);
}

/// <summary>
/// Data Transfer Object carrying all dashboard numbers.
/// A DTO is just a plain container — no logic, no database access.
/// We use it to pass data from Service → Controller → View cleanly.
/// </summary>
public class DashboardStatsDto
{
    // ── Task Counts ───────────────────────────────────────────────────────────
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }

    // ── Project Counts ────────────────────────────────────────────────────────
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }

    // ── Recent Activity ───────────────────────────────────────────────────────

    /// <summary>5 most recently updated tasks for the activity feed.</summary>
    public IEnumerable<RecentTaskDto> RecentTasks { get; set; }
        = new List<RecentTaskDto>();

    /// <summary>Tasks due within the next 7 days.</summary>
    public IEnumerable<RecentTaskDto> UpcomingDeadlines { get; set; }
        = new List<RecentTaskDto>();

    // ── Computed Properties (used by View directly) ───────────────────────────
    public double CompletionRate => TotalTasks == 0
        ? 0
        : Math.Round((double)CompletedTasks / TotalTasks * 100, 1);
}

/// <summary>Lightweight task summary for dashboard lists.</summary>
public class RecentTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public string? AssigneeName { get; set; }
    public bool IsOverdue => Deadline.HasValue
        && Deadline.Value < DateTime.UtcNow
        && Status != "Completed";
}