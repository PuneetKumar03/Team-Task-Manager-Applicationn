using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.DataAccess.Data;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;

namespace TaskManager.DataAccess.Repository.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllMembersAsync()
    {
        var members = await _userManager.GetUsersInRoleAsync("Member");
        return members;
    }

    public async Task<IEnumerable<ApplicationUser>> GetProjectMembersAsync(int projectId)
        => await _db.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .Select(pm => pm.User)
            .AsNoTracking()
            .ToListAsync();

    public async Task<ApplicationUser?> GetUserWithDetailsAsync(string userId)
        => await _db.Users
            .Include(u => u.ProjectMemberships)
            .Include(u => u.AssignedTasks)
            .FirstOrDefaultAsync(u => u.Id == userId);
}