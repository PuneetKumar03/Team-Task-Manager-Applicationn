using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManager.DataAccess.Data;
using TaskManager.Models.Entities;
using TaskManager.Utility.Constants;

namespace TaskManager.DataAccess;

/// <summary>
/// Runs once at startup to ensure the database is ready.
///
/// Responsibilities:
///   1. Apply any pending EF migrations (creates tables if they don't exist)
///   2. Seed the two roles: Admin and Member
///   3. Seed a default Admin user so the app is usable immediately
///
/// Called from Program.cs AFTER the DI container is built.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Resolve services from the DI container.
        // We use a scope because DbContext is scoped (one per request).
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // ── Step 1: Apply migrations ──────────────────────────────────────
            // MigrateAsync() runs any unapplied migrations.
            // On a fresh database this creates all tables.
            // On an existing database with new migrations it alters tables.
            // It is SAFE to call on every startup.
            logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();

            // ── Step 2: Seed Roles ────────────────────────────────────────────
            // SD.Role_Admin and SD.Role_Member are constants ("Admin", "Member")
            // defined in TaskManager.Utility — no magic strings here.
            await SeedRoleAsync(roleManager, SD.Role_Admin);
            await SeedRoleAsync(roleManager, SD.Role_Member);

            // ── Step 3: Seed default Admin user ───────────────────────────────
            const string adminEmail = "admin@taskmanager.com";
            const string adminPassword = "Admin@123!";
            // In production, pull these from environment variables / secrets.

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true   // Skip email verification for seeded user
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                    logger.LogInformation("Default admin user seeded: {Email}", adminEmail);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Failed to seed admin user: {Errors}", errors);
                }
            }

            logger.LogInformation("Database initialization complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization.");
            throw; // Re-throw so the app crashes loudly on startup failure
        }
    }

    private static async Task SeedRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
        // If role already exists, we do nothing — idempotent on every restart.
    }
}