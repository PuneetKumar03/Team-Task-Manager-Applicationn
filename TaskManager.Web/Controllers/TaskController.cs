using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Models.Entities;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.Utility.Constants;
using TaskManager.Web.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Web.Controllers;

[Authorize]
public class TaskController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly DataAccess.Repository.Interfaces.IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaskController(
        ITaskService taskService,
        IProjectService projectService,
        DataAccess.Repository.Interfaces.IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _taskService = taskService;
        _projectService = projectService;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    // ── Index ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(int? projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);

        IEnumerable<TaskItem> tasks;

        if (projectId.HasValue)
            tasks = await _taskService.GetProjectTasksAsync(projectId.Value);
        else if (isAdmin)
            tasks = await _taskService.GetProjectTasksAsync(0);
        else
            tasks = await _taskService.GetMyTasksAsync(user!.Id);

        ViewBag.ProjectId = projectId;
        return View(tasks);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Create(int? projectId)
    {
        var vm = await BuildCreateViewModel(projectId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Create(TaskCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(await BuildCreateViewModel(model.ProjectId));

        var user = await _userManager.GetUserAsync(User);

        var task = new TaskItem
        {
            Title = model.Title,
            Description = model.Description,
            ProjectId = model.ProjectId,
            AssignedToUserId = model.AssignedToUserId,
            Priority = model.Priority,
            Deadline = model.Deadline
        };

        var (success, message) = await _taskService.CreateTaskAsync(task, user!.Id);

        if (success)
        {
            TempData[SD.TempData_Success] = message;
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        TempData[SD.TempData_Error] = message;
        return View(await BuildCreateViewModel(model.ProjectId));
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();

        var vm = await BuildEditViewModel(task);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Edit(TaskEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var existing = await _taskService.GetTaskByIdAsync(model.Id);
            return View(await BuildEditViewModel(existing!));
        }

        var user = await _userManager.GetUserAsync(User);

        var task = new TaskItem
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            ProjectId = model.ProjectId,
            AssignedToUserId = model.AssignedToUserId,
            Priority = model.Priority,
            Deadline = model.Deadline,
            Status = model.Status,
            ProgressPercentage = model.ProgressPercentage
        };

        var (success, message) = await _taskService
            .UpdateTaskAsync(task, user!.Id, isAdmin: true);

        if (success)
        {
            TempData[SD.TempData_Success] = message;
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        TempData[SD.TempData_Error] = message;
        return View(await BuildEditViewModel(task));
    }

    // ── Update Status (Members) ───────────────────────────────────────────────

    public async Task<IActionResult> UpdateStatus(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);

        // Members can only update their own tasks
        if (!isAdmin && task.AssignedToUserId != user!.Id)
        {
            TempData[SD.TempData_Error] = "You can only update tasks assigned to you.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new TaskUpdateStatusViewModel
        {
            Id = task.Id,
            Title = task.Title,
            ProjectName = task.Project?.Name ?? string.Empty,
            CurrentStatus = task.Status,
            NewStatus = task.Status,
            ProgressPercentage = task.ProgressPercentage,
            StatusList = GetStatusSelectList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(TaskUpdateStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.StatusList = GetStatusSelectList();
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);

        var (success, message) = await _taskService.UpdateTaskStatusAsync(
            model.Id,
            model.NewStatus,
            model.ProgressPercentage,
            user!.Id,
            isAdmin);

        if (success)
        {
            TempData[SD.TempData_Success] = message;
            return RedirectToAction(nameof(Index));
        }

        TempData[SD.TempData_Error] = message;
        model.StatusList = GetStatusSelectList();
        return View(model);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        var (success, message) = await _taskService.DeleteTaskAsync(id, user!.Id);

        TempData[success ? SD.TempData_Success : SD.TempData_Error] = message;
        return RedirectToAction(nameof(Index), new { projectId });
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private async Task<TaskCreateViewModel> BuildCreateViewModel(int? projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        var projects = await _projectService.GetProjectsAsync(user!.Id, isAdmin: true);
        var members = await _userRepository.GetAllMembersAsync();

        return new TaskCreateViewModel
        {
            ProjectId = projectId ?? 0,
            ProjectList = projects.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = p.Id == projectId
            }),
            MemberList = members.Select(m => new SelectListItem
            {
                Value = m.Id,
                Text = $"{m.FullName} ({m.Email})"
            }).Prepend(new SelectListItem { Value = "", Text = "-- Unassigned --" }),
            PriorityList = GetPrioritySelectList()
        };
    }

    private async Task<TaskEditViewModel> BuildEditViewModel(TaskItem task)
    {
        var user = await _userManager.GetUserAsync(User);
        var projects = await _projectService.GetProjectsAsync(user!.Id, isAdmin: true);
        var members = await _userRepository.GetAllMembersAsync();

        return new TaskEditViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            AssignedToUserId = task.AssignedToUserId,
            Priority = task.Priority,
            Deadline = task.Deadline,
            Status = task.Status,
            ProgressPercentage = task.ProgressPercentage,
            ProjectList = projects.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }),
            MemberList = members.Select(m => new SelectListItem
            {
                Value = m.Id,
                Text = $"{m.FullName} ({m.Email})"
            }).Prepend(new SelectListItem { Value = "", Text = "-- Unassigned --" }),
            PriorityList = GetPrioritySelectList(),
            StatusList = GetStatusSelectList()
        };
    }

    private static IEnumerable<SelectListItem> GetStatusSelectList()
        => Enum.GetValues<TaskItemStatus>()
               .Select(s => new SelectListItem
               {
                   Value = s.ToString(),
                   Text = s.ToString()
               });

    private static IEnumerable<SelectListItem> GetPrioritySelectList()
        => Enum.GetValues<TaskPriority>()
               .Select(p => new SelectListItem
               {
                   Value = p.ToString(),
                   Text = p.ToString()
               });
}