using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Controllers
{
    [Authorize(Roles = "Admin,Inspector,Viewer")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? town)
        {
            IQueryable<Premises> premisesQuery = _context.Premises;

            if (!string.IsNullOrEmpty(town))
            {
                premisesQuery = premisesQuery.Where(p => p.Town == town);
                _logger.LogInformation("Dashboard filtered by town: {Town}", town);
            }
            else
            {
                _logger.LogInformation("Dashboard loaded without town filter");
            }

            var premisesIds = await premisesQuery.Select(p => p.Id).ToListAsync();

            var inspectionsQuery = _context.Inspections
                .Include(i => i.Premises)
                .Where(i => premisesIds.Contains(i.PremisesId));

            var followUpsQuery = _context.FollowUps
                .Where(f => _context.Inspections
                    .Where(i => premisesIds.Contains(i.PremisesId))
                    .Select(i => i.Id)
                    .Contains(f.InspectionId));

            var model = new DashboardViewModel
            {
                TotalPremises = await premisesQuery.CountAsync(),
                TotalInspections = await inspectionsQuery.CountAsync(),
                OpenFollowUps = await followUpsQuery.CountAsync(f => f.Status == "Open"),
                OverdueFollowUps = await followUpsQuery.CountAsync(f => f.Status == "Open" && f.DueDate < DateTime.Now),
                SelectedTown = town,
                Towns = await _context.Premises
                    .Select(p => p.Town)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync(),
                RecentInspections = await inspectionsQuery
                    .OrderByDescending(i => i.InspectionDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}