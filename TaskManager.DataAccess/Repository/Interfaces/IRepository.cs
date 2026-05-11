using System.Linq.Expressions;

namespace TaskManager.DataAccess.Repository.Interfaces;

/// <summary>
/// Generic repository contract. T is any entity class (Project, TaskItem, etc.)
///
/// Why generic? Instead of repeating GetById / GetAll / Add / Remove
/// in every repository, we define them once here. Concrete repos
/// inherit this and only add their domain-specific queries on top.
/// </summary>
public interface IRepository<T> where T : class
{
    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a single entity by primary key.
    /// Returns null if not found — callers must handle null.
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Returns all rows. Use sparingly — prefer filtered queries.
    /// includeProperties: comma-separated nav property names to eager-load.
    /// Example: "Project,AssignedTo"
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        string? includeProperties = null);

    /// <summary>
    /// Returns the first entity matching the filter, or null.
    /// Example: GetAsync(u => u.Email == email)
    /// </summary>
    Task<T?> GetAsync(
        Expression<Func<T, bool>> filter,
        string? includeProperties = null);

    // ── Write ─────────────────────────────────────────────────────────────────

    /// <summary>Stages an INSERT. Call UnitOfWork.SaveAsync() to commit.</summary>
    Task AddAsync(T entity);

    /// <summary>Stages an UPDATE.</summary>
    void Update(T entity);

    /// <summary>Stages a DELETE.</summary>
    void Remove(T entity);

    /// <summary>Commits all staged changes to the database in one transaction.</summary>
    Task SaveAsync();
}