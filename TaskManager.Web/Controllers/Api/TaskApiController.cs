using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models.Entities;
using TaskManager.Services.Interfaces;
using TaskManager.Utility.Constants;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Web.Controllers.Api;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TaskApiController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TaskApiController(
        ITaskService taskService,
        UserManager<ApplicationUser> userManager)
    {
        _taskService = taskService;
        _userManager = userManager;
    }

    // GET /api/tasks?projectId=1
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] int? projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);

        IEnumerable<TaskItem> tasks = projectId.HasValue
            ? await _taskService.GetProjectTasksAsync(projectId.Value)
            : await _taskService.GetMyTasksAsync(user!.Id);

        var result = tasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            t.Deadline,
            t.ProgressPercentage,
            ProjectName = t.Project?.Name,
            AssigneeName = t.AssignedTo?.FullName ?? "Unassigned"
        });

        return Ok(result);
    }

    // POST /api/tasks
    [HttpPost]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> CreateTask([FromBody] TaskApiRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId,
            Deadline = request.Deadline
        };

        var (success, message) = await _taskService.CreateTaskAsync(task, user!.Id);

        if (success)
            return Ok(new { message });

        return BadRequest(new { message });
    }

    // PUT /api/tasks/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskApiRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        var task = new TaskItem
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId,
            Deadline = request.Deadline
        };

        var (success, message) = await _taskService
            .UpdateTaskAsync(task, user!.Id, isAdmin: true);

        if (success) return Ok(new { message });
        return BadRequest(new { message });
    }

    // DELETE /api/tasks/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = SD.Role_Admin)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var (success, message) = await _taskService.DeleteTaskAsync(id, user!.Id);

        if (success) return Ok(new { message });
        return BadRequest(new { message });
    }
}

public class TaskApiRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime? Deadline { get; set; }
}