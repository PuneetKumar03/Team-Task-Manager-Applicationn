using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models.Entities;
using TaskManager.Services.Interfaces;
using TaskManager.Utility.Constants;

namespace TaskManager.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        IDashboardService dashboardService,
        UserManager<ApplicationUser> userManager)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole(SD.Role_Admin);

        var stats = await _dashboardService.GetDashboardStatsAsync(
            userId: user?.Id,
            isAdmin: isAdmin);

        return View(stats);
    }
}