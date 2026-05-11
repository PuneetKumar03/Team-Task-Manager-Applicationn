using TaskManager.Models.Entities;
using TaskManager.Models.Enums;

// Alias to avoid clash with System.Threading.Tasks.TaskStatus
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.DataAccess.Repository.Interfaces;

/// <summary>
/// Task-specific queries for dashboards, filtering, and status updates.
/// </summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    /// <summary>All tasks in a project, with AssignedTo user eagerly loaded.</summary>
    Task<IEnumerable<TaskItem>> GetTasksByProjectAsync(int projectId);

    /// <summary>All tasks assigned to a specific user, across all projects.</summary>
    Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string userId);

    /// <summary>
    /// Tasks that are overdue: Deadline has passed AND status is not Completed.
    /// Used for the overdue counter on the dashboard.
    /// </summary>
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(string? userId = null);

    /// <summary>Tasks filtered by status. Used for Kanban-style views.</summary>
    Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(
        TaskItemStatus status,
        int? projectId = null);

    /// <summary>
    /// Dashboard aggregate counts — one DB roundtrip instead of four.
    /// Returns (Total, Completed, Pending, Overdue).
    /// </summary>
    Task<(int Total, int Completed, int Pending, int Overdue)>
        GetTaskCountsAsync(string? userId = null, int? projectId = null);
}