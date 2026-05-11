using System.ComponentModel.DataAnnotations;
using TaskManager.Models.Entities;

namespace TaskManager.Web.ViewModels;

public class ProjectCreateViewModel
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "Deadline")]
    [DataType(DataType.Date)]
    public DateTime? Deadline { get; set; }
}

public class ProjectEditViewModel : ProjectCreateViewModel
{
    public int Id { get; set; }
}

public class ProjectDetailsViewModel
{
    public Project Project { get; set; } = null!;

    // All members available to add (not already in project)
    public IEnumerable<MemberSelectItem> AvailableMembers { get; set; }
        = new List<MemberSelectItem>();
}

public class MemberSelectItem
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class AddMemberViewModel
{
    public int ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
}