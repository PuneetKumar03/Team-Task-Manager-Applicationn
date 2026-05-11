using TaskManager.Models.Entities;

namespace TaskManager.DataAccess.Repository.Interfaces;

/// <summary>
/// User-specific queries. Note: authentication itself is handled by
/// ASP.NET Core Identity (UserManager / SignInManager) — this repo
/// handles domain-level user lookups only.
/// </summary>
public interface IUserRepository
{
    /// <summary>Returns all users with the "Member" role for the assign-task dropdown.</summary>
    Task<IEnumerable<ApplicationUser>> GetAllMembersAsync();

    /// <summary>Returns members of a specific project for the assign-task dropdown.</summary>
    Task<IEnumerable<ApplicationUser>> GetProjectMembersAsync(int projectId);

    /// <summary>User profile lookup with memberships eagerly loaded.</summary>
    Task<ApplicationUser?> GetUserWithDetailsAsync(string userId);
}