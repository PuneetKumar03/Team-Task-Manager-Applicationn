using TaskManager.Models.Entities;

namespace TaskManager.DataAccess.Repository.Interfaces;

/// <summary>
/// Project-specific queries that go beyond basic CRUD.
/// Inherits all generic methods from IRepository<Project>.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Returns all active projects created by a specific Admin.
    /// Used on the Admin's "My Projects" page.
    /// </summary>
    Task<IEnumerable<Project>> GetProjectsByUserAsync(string userId);

    /// <summary>
    /// Returns all projects where the given user is a member.
    /// Used on the Member's dashboard.
    /// </summary>
    Task<IEnumerable<Project>> GetProjectsForMemberAsync(string userId);

    /// <summary>
    /// Returns a project with all its Tasks and Members eagerly loaded.
    /// Used on the Project Details page — one query instead of three.
    /// </summary>
    Task<Project?> GetProjectWithDetailsAsync(int projectId);

    /// <summary>
    /// Checks whether a user is already a member of a project.
    /// Prevents duplicate entries in the ProjectMembers table.
    /// </summary>
    Task<bool> IsUserMemberOfProjectAsync(int projectId, string userId);

    /// <summary>Adds a user to a project via the ProjectMembers join table.</summary>
    Task AddMemberAsync(ProjectMember member);

    /// <summary>Removes a user from a project.</summary>
    Task RemoveMemberAsync(int projectId, string userId);
}