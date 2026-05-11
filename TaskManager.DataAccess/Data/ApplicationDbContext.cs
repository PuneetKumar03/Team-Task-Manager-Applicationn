using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models.Entities;

namespace TaskManager.DataAccess.Data;

/// <summary>
/// The single database session factory for the entire application.
///
/// Inherits from IdentityDbContext so that ASP.NET Core Identity
/// automatically gets its 7 tables (AspNetUsers, AspNetRoles, etc.)
/// We pass ApplicationUser as the generic param so Identity uses OUR
/// extended user class instead of the default IdentityUser.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── DbSets (one per entity = one table in the DB) ─────────────────────────
    // Identity handles AspNetUsers internally — we don't declare it here.
    // We DO declare our custom tables:

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }

    // ── Fluent API Configuration ───────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // MUST call base first — this sets up all Identity tables.
        // Skipping this breaks login/register completely.
        base.OnModelCreating(builder);

        // ── ProjectMember: Composite Primary Key ──────────────────────────────
        // Tells EF the PK is the COMBINATION of ProjectId + UserId.
        // This prevents the same user being added to the same project twice.
        builder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(pm => new { pm.ProjectId, pm.UserId });

            // ProjectMember → Project  (many members belong to one project)
            entity.HasOne(pm => pm.Project)
                  .WithMany(p => p.Members)
                  .HasForeignKey(pm => pm.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Cascade: deleting a Project removes its ProjectMember rows too.

            // ProjectMember → ApplicationUser  (many memberships belong to one user)
            entity.HasOne(pm => pm.User)
                  .WithMany(u => u.ProjectMemberships)
                  .HasForeignKey(pm => pm.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            // Restrict: deleting a User is BLOCKED if they have memberships.
            // This protects data integrity — deactivate users, don't delete them.
        });

        // ── Project → ApplicationUser (CreatedBy) ─────────────────────────────
        builder.Entity<Project>(entity =>
        {
            entity.HasOne(p => p.CreatedBy)
                  .WithMany(u => u.CreatedProjects)
                  .HasForeignKey(p => p.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            // Restrict: can't delete a user who owns projects.

            // Index on CreatedByUserId for fast "show my projects" queries
            entity.HasIndex(p => p.CreatedByUserId);
            entity.HasIndex(p => p.IsActive);
        });

        // ── TaskItem relationships ─────────────────────────────────────────────
        builder.Entity<TaskItem>(entity =>
        {
            // Task → Project
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Cascade: deleting a Project removes all its Tasks.

            // Task → AssignedTo user (nullable — task may be unassigned)
            entity.HasOne(t => t.AssignedTo)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(t => t.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
            // SetNull: if a user is removed, the task becomes unassigned (not deleted).

            // Task → CreatedBy user
            entity.HasOne(t => t.CreatedBy)
                  .WithMany()
                  .HasForeignKey(t => t.CreatedByUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Indexes for common dashboard queries
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.Deadline);
            entity.HasIndex(t => t.AssignedToUserId);
            entity.HasIndex(t => t.ProjectId);
        });
    }

    // ── Auto-timestamp on SaveChanges ─────────────────────────────────────────
    // Every time we save, walk through changed entities and update UpdatedAt.
    // This means we never have to remember to set UpdatedAt manually in services.
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(ct);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Project p) p.UpdatedAt = DateTime.UtcNow;
            if (entry.Entity is TaskItem t) t.UpdatedAt = DateTime.UtcNow;
        }
    }
}