using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.Entities;

/// <summary>
/// Represents a top-level project container. 
/// An Admin creates a Project, adds Members to it, and creates Tasks under it.
/// </summary>
public class Project
{
    // ── Primary Key ───────────────────────────────────────────────────────────

    public int Id { get; set; }

    // ── Core Fields ───────────────────────────────────────────────────────────

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Project start date. Used for timeline display on dashboard.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Deadline for the entire project (distinct from individual task deadlines).
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>Soft-delete flag. We never hard-delete projects.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Foreign Keys ──────────────────────────────────────────────────────────

    /// <summary>
    /// The Admin user who created this project.
    /// EF Core uses this int + the navigation property below to build the FK constraint.
    /// </summary>
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    // ── Navigation Properties ─────────────────────────────────────────────────

    /// <summary>The Admin user who owns this project.</summary>
    public ApplicationUser CreatedBy { get; set; } = null!;

    /// <summary>
    /// All tasks that belong to this project.
    /// EF will generate: SELECT * FROM TaskItems WHERE ProjectId = @id
    /// </summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    /// <summary>
    /// Join table rows linking users to this project.
    /// To get the actual users: project.Members.Select(m => m.User)
    /// </summary>
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}