using System.ComponentModel.DataAnnotations;
using TaskManager.Models.Enums;

// Alias to avoid name clash with System.Threading.Tasks.TaskStatus
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Models.Entities;

/// <summary>
/// Represents a unit of work within a Project.
/// 
/// Relationships:
///   TaskItem → Project       (belongs to one project)
///   TaskItem → ApplicationUser (assigned to one member, nullable)
/// </summary>
public class TaskItem
{
    // ── Primary Key ───────────────────────────────────────────────────────────

    public int Id { get; set; }

    // ── Core Fields ───────────────────────────────────────────────────────────

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Current lifecycle stage of the task.
    /// EF stores this as int (0=Todo, 1=InProgress, 2=InReview, 3=Completed).
    /// </summary>
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    /// <summary>Urgency level — drives visual badges in the UI.</summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// When this task must be completed. 
    /// Dashboard flags tasks where Deadline < DateTime.UtcNow AND Status != Completed.
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>Tracks completion percentage (0-100). Updated by assigned member.</summary>
    [Range(0, 100)]
    public int ProgressPercentage { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Foreign Keys ──────────────────────────────────────────────────────────

    /// <summary>The project this task belongs to. Required — tasks can't be orphaned.</summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// The user this task is assigned to. 
    /// Nullable — a task can exist unassigned until an Admin assigns it.
    /// </summary>
    public string? AssignedToUserId { get; set; }

    /// <summary>The Admin who created this task.</summary>
    public string CreatedByUserId { get; set; } = string.Empty;

    // ── Navigation Properties ─────────────────────────────────────────────────

    public Project Project { get; set; } = null!;
    public ApplicationUser? AssignedTo { get; set; }
    public ApplicationUser CreatedBy { get; set; } = null!;
}