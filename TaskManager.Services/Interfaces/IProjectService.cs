using TaskManager.Models.Entities;

namespace TaskManager.Services.Interfaces;

/// <summary>
/// Defines all project-related business operations.
/// Controllers depend on this interface, never on the concrete class.
/// This makes the code testable — you can mock IProjectService in unit tests.
/// </summary>
public interface IProjectService
{
    // ── Project CRUD ──────────────────────────────────────────────────────────

    /// <summary>Get all active projects. Admins see all, Members see only theirs.</summary>
    Task<IEnumerable<Project>> GetProjectsAsync(string userId, bool isAdmin);

    /// <summary>Get a single project with all tasks and members loaded.</summary>
    Task<Project?> GetProjectDetailsAsync(int projectId);

    /// <summary>Create a new project. Only Admins can do this.</summary>
    Task<(bool Success, string Message)> CreateProjectAsync(
        Project project, string createdByUserId);

    /// <summary>Edit an existing project.</summary>
    Task<(bool Success, string Message)> UpdateProjectAsync(
        Project project, string requestingUserId);

    /// <summary>Soft-delete a project (sets IsActive = false).</summary>
    Task<(bool Success, string Message)> DeleteProjectAsync(
        int projectId, string requestingUserId);

    // ── Member Management ─────────────────────────────────────────────────────

    /// <summary>Add a user to a project as a Member.</summary>
    Task<(bool Success, string Message)> AddMemberAsync(
        int projectId, string userId, string requestingUserId);

    /// <summary>Remove a user from a project.</summary>
    Task<(bool Success, string Message)> RemoveMemberAsync(
        int projectId, string userId, string requestingUserId);

    /// <summary>Check if a user has access to view a project.</summary>
    Task<bool> CanUserAccessProjectAsync(int projectId, string userId, bool isAdmin);
}