using Microsoft.Extensions.Logging;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;
using TaskManager.Services.Interfaces;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepo,
        IProjectRepository projectRepo,
        ILogger<TaskService> logger)
    {
        _taskRepo = taskRepo;
        _projectRepo = projectRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetProjectTasksAsync(int projectId)
        => await _taskRepo.GetTasksByProjectAsync(projectId);

    public async Task<IEnumerable<TaskItem>> GetMyTasksAsync(string userId)
        => await _taskRepo.GetTasksByAssigneeAsync(userId);

    public async Task<TaskItem?> GetTaskByIdAsync(int taskId)
        => await _taskRepo.GetAsync(
            t => t.Id == taskId,
            includeProperties: "Project,AssignedTo,CreatedBy");

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> CreateTaskAsync(
        TaskItem task, string createdByUserId)
    {
        try
        {
            // Verify the project exists and is active
            var project = await _projectRepo.GetByIdAsync(task.ProjectId);

            if (project == null || !project.IsActive)
                return (false, "Project not found or is inactive.");

            task.CreatedByUserId = createdByUserId;
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            task.Status = TaskItemStatus.Todo;
            task.ProgressPercentage = 0;

            await _taskRepo.AddAsync(task);
            await _taskRepo.SaveAsync();

            _logger.LogInformation(
                "Task '{Title}' created in project {ProjectId}", task.Title, task.ProjectId);

            return (true, "Task created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task '{Title}'", task.Title);
            return (false, "An error occurred while creating the task.");
        }
    }

    // ── Full Update (Admin only) ───────────────────────────────────────────────

    public async Task<(bool Success, string Message)> UpdateTaskAsync(
        TaskItem task, string requestingUserId, bool isAdmin)
    {
        try
        {
            var existing = await _taskRepo.GetByIdAsync(task.Id);

            if (existing == null)
                return (false, "Task not found.");

            // Business rule: only Admin can do a full update
            if (!isAdmin)
                return (false, "Only administrators can edit task details.");

            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.Priority = task.Priority;
            existing.Deadline = task.Deadline;
            existing.AssignedToUserId = task.AssignedToUserId;
            existing.Status = task.Status;
            existing.ProgressPercentage = task.ProgressPercentage;
            existing.UpdatedAt = DateTime.UtcNow;

            _taskRepo.Update(existing);
            await _taskRepo.SaveAsync();

            return (true, "Task updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", task.Id);
            return (false, "An error occurred while updating the task.");
        }
    }

    // ── Status Update (Member can do this for their own tasks) ────────────────

    public async Task<(bool Success, string Message)> UpdateTaskStatusAsync(
        int taskId,
        TaskItemStatus newStatus,
        int progressPercentage,
        string requestingUserId,
        bool isAdmin)
    {
        try
        {
            var task = await _taskRepo.GetByIdAsync(taskId);

            if (task == null)
                return (false, "Task not found.");

            // Business rule: Members can only update tasks assigned to them
            if (!isAdmin && task.AssignedToUserId != requestingUserId)
                return (false, "You can only update tasks assigned to you.");

            // Business rule: progress must match status logically
            if (newStatus == TaskItemStatus.Completed)
                progressPercentage = 100;

            if (newStatus == TaskItemStatus.Todo)
                progressPercentage = 0;

            // Clamp progress to valid range
            progressPercentage = Math.Clamp(progressPercentage, 0, 100);

            task.Status = newStatus;
            task.ProgressPercentage = progressPercentage;
            task.UpdatedAt = DateTime.UtcNow;

            _taskRepo.Update(task);
            await _taskRepo.SaveAsync();

            return (true, "Task status updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for task {Id}", taskId);
            return (false, "An error occurred while updating the task status.");
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> DeleteTaskAsync(
        int taskId, string requestingUserId)
    {
        try
        {
            var task = await _taskRepo.GetByIdAsync(taskId);

            if (task == null)
                return (false, "Task not found.");

            _taskRepo.Remove(task);
            await _taskRepo.SaveAsync();

            _logger.LogInformation(
                "Task {Id} deleted by user {UserId}", taskId, requestingUserId);

            return (true, "Task deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {Id}", taskId);
            return (false, "An error occurred while deleting the task.");
        }
    }

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(string? userId = null)
        => await _taskRepo.GetOverdueTasksAsync(userId);
}