using Microsoft.Extensions.Logging;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services.Implementations;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepo;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepo,
        ILogger<ProjectService> logger)
    {
        _projectRepo = projectRepo;
        _logger = logger;
    }

    // ── Get Projects ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<Project>> GetProjectsAsync(
        string userId, bool isAdmin)
    {
        // Admins see ALL active projects across the system.
        // Members only see projects they belong to.
        if (isAdmin)
            return await _projectRepo.GetAllAsync(
                filter: p => p.IsActive,
                includeProperties: "CreatedBy,Tasks,Members");

        return await _projectRepo.GetProjectsForMemberAsync(userId);
    }

    public async Task<Project?> GetProjectDetailsAsync(int projectId)
        => await _projectRepo.GetProjectWithDetailsAsync(projectId);

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> CreateProjectAsync(
        Project project, string createdByUserId)
    {
        try
        {
            // Business rule: project name must be unique per user
            var existing = await _projectRepo.GetAsync(p =>
                p.CreatedByUserId == createdByUserId &&
                p.Name.ToLower() == project.Name.ToLower() &&
                p.IsActive);

            if (existing != null)
                return (false, $"You already have a project named '{project.Name}'.");

            project.CreatedByUserId = createdByUserId;
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;
            project.IsActive = true;

            await _projectRepo.AddAsync(project);
            await _projectRepo.SaveAsync();

            _logger.LogInformation(
                "Project '{Name}' created by user {UserId}", project.Name, createdByUserId);

            return (true, "Project created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project '{Name}'", project.Name);
            return (false, "An error occurred while creating the project.");
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> UpdateProjectAsync(
        Project project, string requestingUserId)
    {
        try
        {
            var existing = await _projectRepo.GetByIdAsync(project.Id);

            if (existing == null)
                return (false, "Project not found.");

            // Business rule: only the creator can edit the project
            if (existing.CreatedByUserId != requestingUserId)
                return (false, "You don't have permission to edit this project.");

            existing.Name = project.Name;
            existing.Description = project.Description;
            existing.StartDate = project.StartDate;
            existing.Deadline = project.Deadline;
            existing.UpdatedAt = DateTime.UtcNow;

            _projectRepo.Update(existing);
            await _projectRepo.SaveAsync();

            return (true, "Project updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {Id}", project.Id);
            return (false, "An error occurred while updating the project.");
        }
    }

    // ── Delete (Soft) ─────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> DeleteProjectAsync(
        int projectId, string requestingUserId)
    {
        try
        {
            var project = await _projectRepo.GetByIdAsync(projectId);

            if (project == null)
                return (false, "Project not found.");

            if (project.CreatedByUserId != requestingUserId)
                return (false, "You don't have permission to delete this project.");

            // Soft delete — never hard delete projects
            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;

            _projectRepo.Update(project);
            await _projectRepo.SaveAsync();

            _logger.LogInformation(
                "Project {Id} soft-deleted by user {UserId}", projectId, requestingUserId);

            return (true, "Project deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {Id}", projectId);
            return (false, "An error occurred while deleting the project.");
        }
    }

    // ── Member Management ─────────────────────────────────────────────────────

    public async Task<(bool Success, string Message)> AddMemberAsync(
        int projectId, string userId, string requestingUserId)
    {
        try
        {
            var project = await _projectRepo.GetByIdAsync(projectId);

            if (project == null)
                return (false, "Project not found.");

            // Only the project creator (Admin) can add members
            if (project.CreatedByUserId != requestingUserId)
                return (false, "Only the project owner can add members.");

            // Prevent duplicates
            var alreadyMember = await _projectRepo
                .IsUserMemberOfProjectAsync(projectId, userId);

            if (alreadyMember)
                return (false, "This user is already a member of the project.");

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            await _projectRepo.AddMemberAsync(member);
            await _projectRepo.SaveAsync();

            return (true, "Member added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to project {Id}", projectId);
            return (false, "An error occurred while adding the member.");
        }
    }

    public async Task<(bool Success, string Message)> RemoveMemberAsync(
        int projectId, string userId, string requestingUserId)
    {
        try
        {
            var project = await _projectRepo.GetByIdAsync(projectId);

            if (project == null)
                return (false, "Project not found.");

            if (project.CreatedByUserId != requestingUserId)
                return (false, "Only the project owner can remove members.");

            await _projectRepo.RemoveMemberAsync(projectId, userId);
            await _projectRepo.SaveAsync();

            return (true, "Member removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from project {Id}", projectId);
            return (false, "An error occurred while removing the member.");
        }
    }

    public async Task<bool> CanUserAccessProjectAsync(
        int projectId, string userId, bool isAdmin)
    {
        // Admins can access any project
        if (isAdmin) return true;

        // Members can only access projects they belong to
        return await _projectRepo.IsUserMemberOfProjectAsync(projectId, userId);
    }
}