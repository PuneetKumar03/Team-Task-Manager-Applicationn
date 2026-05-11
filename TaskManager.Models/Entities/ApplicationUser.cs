using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models.Entities;

public class ApplicationUser : IdentityUser
{
    // ── Profile ──────────────────────────────────────────────────────────────

    /// <summary>Full display name shown in the UI.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Optional profile picture URL (e.g. stored in blob storage).</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>When the user account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ─────────────────────────────────────────────────
    // These tell EF Core about the relationships. EF uses these to generate
    // JOIN queries — we never write SQL by hand.

    /// <summary>Projects this user has created (Admin creates projects).</summary>
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();

    /// <summary>Tasks assigned to this user.</summary>
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

    /// <summary>Projects this user is a member of (via ProjectMember join table).</summary>
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
}