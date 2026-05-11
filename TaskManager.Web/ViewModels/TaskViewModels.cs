using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Models.Enums;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Web.ViewModels;

public class TaskCreateViewModel
{
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public string? AssignedToUserId { get; set; }

    [Display(Name = "Priority")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Display(Name = "Deadline")]
    [DataType(DataType.Date)]
    public DateTime? Deadline { get; set; }

    // Populated in controller for dropdowns
    public IEnumerable<SelectListItem> ProjectList { get; set; }
        = new List<SelectListItem>();
    public IEnumerable<SelectListItem> MemberList { get; set; }
        = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PriorityList { get; set; }
        = new List<SelectListItem>();
}

public class TaskEditViewModel : TaskCreateViewModel
{
    public int Id { get; set; }

    [Display(Name = "Status")]
    public TaskItemStatus Status { get; set; }

    [Display(Name = "Progress (%)")]
    [Range(0, 100)]
    public int ProgressPercentage { get; set; }

    public IEnumerable<SelectListItem> StatusList { get; set; }
        = new List<SelectListItem>();
}

public class TaskUpdateStatusViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public TaskItemStatus CurrentStatus { get; set; }
    public TaskItemStatus NewStatus { get; set; }

    [Range(0, 100)]
    [Display(Name = "Progress (%)")]
    public int ProgressPercentage { get; set; }

    public IEnumerable<SelectListItem> StatusList { get; set; }
        = new List<SelectListItem>();
}