using TaskManager.Models.Entities;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Interfaces;

/// <summary>
/// Defines all task-related business operations.
/// Key rule enforced here: Members can only update tasks assigned to them.
/// Admins can do everything.
/// </summary>
public interface ITaskService
{
    /// <summary>Get all tasks for a project.</summary>
    Task<IEnumerable<TaskItem>> GetProjectTasksAsync(int projectId);

    /// <summary>Get all tasks assigned to a specific user.</summary>
    Task<IEnumerable<TaskItem>> GetMyTasksAsync(string userId);

    /// <summary>Get a single task by ID.</summary>
    Task<TaskItem?> GetTaskByIdAsync(int taskId);

    /// <summary>
    /// Create a new task inside a project.
    /// Only Admins can create and assign tasks.
    /// </summary>
    Task<(bool Success, string Message)> CreateTaskAsync(
        TaskItem task, string createdByUserId);

    /// <summary>
    /// Full task update — Admin only.
    /// Admins can change title, description, assignee, priority, deadline.
    /// </summary>
    Task<(bool Success, string Message)> UpdateTaskAsync(
        TaskItem task, string requestingUserId, bool isAdmin);

    /// <summary>
    /// Status + progress update — Members can do this for their own tasks.
    /// Members cannot change title, assignee, or priority.
    /// </summary>
    Task<(bool Success, string Message)> UpdateTaskStatusAsync(
        int taskId,
        TaskItemStatus newStatus,
        int progressPercentage,
        string requestingUserId,
        bool isAdmin);

    /// <summary>Delete a task. Admin only.</summary>
    Task<(bool Success, string Message)> DeleteTaskAsync(
        int taskId, string requestingUserId);

    /// <summary>Get all overdue tasks, optionally filtered by user.</summary>
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(string? userId = null);
}