using Microsoft.EntityFrameworkCore;
using TaskManager.DataAccess.Data;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.DataAccess.Repository.Implementations;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<TaskItem>> GetTasksByProjectAsync(int projectId)
        => await _dbSet
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .OrderBy(t => t.Deadline)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetTasksByAssigneeAsync(string userId)
        => await _dbSet
            .Where(t => t.AssignedToUserId == userId)
            .Include(t => t.Project)
            .OrderBy(t => t.Deadline)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync(string? userId = null)
    {
        var now = DateTime.UtcNow;

        var query = _dbSet
            .Where(t => t.Deadline < now
                     && t.Status != TaskItemStatus.Completed);

        if (userId != null)
            query = query.Where(t => t.AssignedToUserId == userId);

        return await query
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(
        TaskItemStatus status,
        int? projectId = null)
    {
        var query = _dbSet.Where(t => t.Status == status);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        return await query
            .Include(t => t.AssignedTo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(int Total, int Completed, int Pending, int Overdue)>
        GetTaskCountsAsync(string? userId = null, int? projectId = null)
    {
        var now = DateTime.UtcNow;

        // Build the base query with optional filters
        IQueryable<TaskItem> query = _dbSet;

        if (userId != null)
            query = query.Where(t => t.AssignedToUserId == userId);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        // All four counts in a single DB round-trip using EF aggregations.
        // EF translates each Count() call into a subquery or CASE WHEN.
        var total = await query.CountAsync();
        var completed = await query.CountAsync(t =>
            t.Status == TaskItemStatus.Completed);
        var pending = await query.CountAsync(t =>
            t.Status != TaskItemStatus.Completed);
        var overdue = await query.CountAsync(t =>
            t.Deadline < now && t.Status != TaskItemStatus.Completed);

        return (total, completed, pending, overdue);
    }
}