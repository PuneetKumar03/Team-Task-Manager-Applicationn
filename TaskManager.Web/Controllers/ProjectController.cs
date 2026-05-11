using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;
using TaskManager.Services.Interfaces;
using TaskManager.Utility.Constants;
using TaskManager.Web.ViewModels;

namespace TaskManager.Web.Controllers;

[Authorize]
public class ProjectController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectController(
        IProjectService projectService,
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _projectService = projectService;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);
        var projects = await _projectService.GetProjectsAsync(user!.Id, isAdmin);
        return View(projects);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);

        if (!await _projectService.CanUserAccessProjectAsync(id, user!.Id, isAdmin))
        {
            TempData[SD.TempData_Error] = "You do not have access to this project.";
            return RedirectToAction(nameof(Index));
        }

        var project = await _projectService.GetProjectDetailsAsync(id);
        if (project == null) return NotFound();

        var allMembers = await _userRepository.GetAllMembersAsync();
        var currentMemberIds = project.Members.Select(m => m.UserId).ToHashSet();

        var vm = new ProjectDetailsViewModel
        {
            Project = project,
            AvailableMembers = allMembers
                .Where(u => !currentMemberIds.Contains(u.Id))
                .Select(u => new MemberSelectItem
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty
                })
        };

        return View(vm);
    }

    [Authorize(Roles = SD.Role_Admin)]
    public IActionResult Create() => View(new ProjectCreateViewModel
    {
        StartDate = DateTime.Today
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Create(ProjectCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        var project = new Project
        {
            Name = model.Name,
            Description = model.Description,
            StartDate = model.StartDate,
            Deadline = model.Deadline
        };

        var (success, message) = await _projectService.CreateProjectAsync(project, user!.Id);

        if (success)
        {
            TempData[SD.TempData_Success] = message;
            return RedirectToAction(nameof(Index));
        }

        TempData[SD.TempData_Error] = message;
        return View(model);
    }

    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetProjectDetailsAsync(id);
        if (project == null) return NotFound();

        return View(new ProjectEditViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            Deadline = project.Deadline
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Edit(ProjectEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        var project = new Project
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            StartDate = model.StartDate,
            Deadline = model.Deadline
        };

        var (success, message) = await _projectService.UpdateProjectAsync(project, user!.Id);

        if (success)
        {
            TempData[SD.TempData_Success] = message;
            return RedirectToAction(nameof(Index));
        }

        TempData[SD.TempData_Error] = message;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var (success, message) = await _projectService.DeleteProjectAsync(id, user!.Id);

        TempData[success ? SD.TempData_Success : SD.TempData_Error] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> AddMember(AddMemberViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var (success, message) = await _projectService
            .AddMemberAsync(model.ProjectId, model.UserId, user!.Id);

        TempData[success ? SD.TempData_Success : SD.TempData_Error] = message;
        return RedirectToAction(nameof(Details), new { id = model.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> RemoveMember(int projectId, string userId)
    {
        var user = await _userManager.GetUserAsync(User);
        var (success, message) = await _projectService
            .RemoveMemberAsync(projectId, userId, user!.Id);

        TempData[success ? SD.TempData_Success : SD.TempData_Error] = message;
        return RedirectToAction(nameof(Details), new { id = projectId });
    }
}