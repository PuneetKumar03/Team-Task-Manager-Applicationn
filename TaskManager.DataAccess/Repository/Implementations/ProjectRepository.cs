using Microsoft.EntityFrameworkCore;
using TaskManager.DataAccess.Data;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;

namespace TaskManager.DataAccess.Repository.Implementations;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Project>> GetProjectsByUserAsync(string userId)
        => await _dbSet
            .Where(p => p.CreatedByUserId == userId && p.IsActive)
            .Include(p => p.Tasks)
            .Include(p => p.Members)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Project>> GetProjectsForMemberAsync(string userId)
        => await _db.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Include(pm => pm.Project)
                .ThenInclude(p => p.Tasks)
            .Select(pm => pm.Project)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    // Note: we query from ProjectMembers (the join table) and navigate
    // to Project. This is cleaner than the reverse direction.

    public async Task<Project?> GetProjectWithDetailsAsync(int projectId)
        => await _dbSet
            .Where(p => p.Id == projectId)
            .Include(p => p.CreatedBy)
            .Include(p => p.Tasks)
                .ThenInclude(t => t.AssignedTo)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync();
    // ThenInclude: goes one level deeper.
    // Members → User loads the actual ApplicationUser for each ProjectMember.

    public async Task<bool> IsUserMemberOfProjectAsync(int projectId, string userId)
        => await _db.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

    public async Task AddMemberAsync(ProjectMember member)
        => await _db.ProjectMembers.AddAsync(member);

    public async Task RemoveMemberAsync(int projectId, string userId)
    {
        var member = await _db.ProjectMembers
            .FindAsync(projectId, userId);
        // FindAsync with composite key: pass values in the same order
        // as declared in HasKey(pm => new { pm.ProjectId, pm.UserId })

        if (member != null)
            _db.ProjectMembers.Remove(member);
    }
}