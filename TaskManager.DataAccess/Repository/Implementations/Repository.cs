using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskManager.DataAccess.Data;
using TaskManager.DataAccess.Repository.Interfaces;

namespace TaskManager.DataAccess.Repository.Implementations;

/// <summary>
/// Concrete implementation of IRepository<T>.
///
/// Every specific repository (ProjectRepository, TaskRepository)
/// inherits from this class. They get all CRUD for free and only
/// add their own domain-specific methods on top.
///
/// The T : class constraint matches EF Core's requirement —
/// entities must be reference types.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _db;

    // DbSet<T> is EF's handle to the table for this entity.
    // "protected" so child repos can use it (e.g. _dbSet.Include(...))
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _dbSet = db.Set<T>();
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        string? includeProperties = null)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
            query = query.Where(filter);

        query = ApplyIncludes(query, includeProperties);

        return await query.AsNoTracking().ToListAsync();
        // AsNoTracking(): read-only queries don't need change tracking.
        // This is a significant performance win for list pages.
    }

    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> filter,
        string? includeProperties = null)
    {
        IQueryable<T> query = _dbSet;
        query = query.Where(filter);
        query = ApplyIncludes(query, includeProperties);
        return await query.FirstOrDefaultAsync();
    }

    // ── Write ─────────────────────────────────────────────────────────────────

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public async Task SaveAsync()
        => await _db.SaveChangesAsync();

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Parses a comma-separated list of nav property names and applies
    /// .Include() for each one. This is the engine behind eager loading.
    ///
    /// Example: includeProperties = "Project,AssignedTo"
    /// Becomes: query.Include("Project").Include("AssignedTo")
    /// </summary>
    private static IQueryable<T> ApplyIncludes(
        IQueryable<T> query,
        string? includeProperties)
    {
        if (string.IsNullOrWhiteSpace(includeProperties))
            return query;

        foreach (var prop in includeProperties
                     .Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(prop.Trim());
        }

        return query;
    }
}