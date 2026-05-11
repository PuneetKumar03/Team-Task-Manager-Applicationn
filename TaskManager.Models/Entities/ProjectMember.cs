namespace TaskManager.Models.Entities;

/// <summary>
/// Join table connecting ApplicationUser ↔ Project.
/// 
/// This replaces a "Team" entity. Instead of Team → Members,
/// we directly model the many-to-many: a User can be in many Projects,
/// a Project can have many Users.
/// 
/// Example row: { ProjectId: 5, UserId: "abc-123", Role: "Member", JoinedAt: ... }
/// </summary>
public class ProjectMember
{
    // ── Composite Primary Key ─────────────────────────────────────────────────
    // EF Core will be told (in ApplicationDbContext) that the PK is (ProjectId, UserId).
    // This prevents adding the same user to the same project twice.

    public int ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // ── Additional Fields ─────────────────────────────────────────────────────

    /// <summary>
    /// Role within this specific project. Different from global Identity role.
    /// A user could be "Lead" in Project A and "Member" in Project B.
    /// Stored as a string for flexibility.
    /// </summary>
    public string Role { get; set; } = "Member";

    /// <summary>When the user was added to the project.</summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ─────────────────────────────────────────────────

    /// <summary>The project this membership belongs to.</summary>
    public Project Project { get; set; } = null!;

    /// <summary>The user who is a member.</summary>
    public ApplicationUser User { get; set; } = null!;
}