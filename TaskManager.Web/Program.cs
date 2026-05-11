using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.DataAccess;
using TaskManager.DataAccess.Data;
using TaskManager.DataAccess.Repository.Implementations;
using TaskManager.DataAccess.Repository.Interfaces;
using TaskManager.Models.Entities;
using TaskManager.Services.Implementations;
using TaskManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("TaskManager.DataAccess")));

// ── Identity ───────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Cookie ─────────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// ── Repositories ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ── MVC + API ──────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Seed ───────────────────────────────────────────────────────────────────
await DbInitializer.InitializeAsync(app.Services);

// ── Middleware ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// API routes
app.MapControllers();

app.Run();