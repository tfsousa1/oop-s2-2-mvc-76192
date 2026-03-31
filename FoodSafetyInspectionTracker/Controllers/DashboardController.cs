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

        // Dashboard shows reporting-style summary information with optional filters.
        public async Task<IActionResult> Index(string? town, string? riskRating)
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // Start from premises because both filters belong to the premises entity.
            IQueryable<Premises> premisesQuery = _context.Premises;

            if (!string.IsNullOrWhiteSpace(town))
            {
                premisesQuery = premisesQuery.Where(p => p.Town == town);
            }

            if (!string.IsNullOrWhiteSpace(riskRating))
            {
                premisesQuery = premisesQuery.Where(p => p.RiskRating == riskRating);
            }

            _logger.LogInformation(
                "Dashboard loaded with filters. Town: {Town}, RiskRating: {RiskRating}, UserName: {UserName}",
                town ?? "All",
                riskRating ?? "All",
                User.Identity?.Name ?? "Anonymous");

            var filteredPremisesIds = await premisesQuery
                .Select(p => p.Id)
                .ToListAsync();

            var inspectionsQuery = _context.Inspections
                .Include(i => i.Premises)
                .Where(i => filteredPremisesIds.Contains(i.PremisesId));

            var followUpsQuery = _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i!.Premises)
                .Where(f => filteredPremisesIds.Contains(f.Inspection!.PremisesId));

            var model = new DashboardViewModel
            {
                SelectedTown = town,
                SelectedRiskRating = riskRating,

                Towns = await _context.Premises
                    .Select(p => p.Town)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync(),

                RiskRatings = await _context.Premises
                    .Select(p => p.RiskRating)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToListAsync(),

                TotalPremises = await premisesQuery.CountAsync(),

                // Assessment requirement: inspections this month.
                InspectionsThisMonth = await inspectionsQuery
                    .CountAsync(i => i.InspectionDate >= firstDayOfMonth),

                // Assessment requirement: failed inspections this month.
                FailedInspectionsThisMonth = await inspectionsQuery
                    .CountAsync(i => i.InspectionDate >= firstDayOfMonth && i.Outcome == "Fail"),

                // Extra reporting value: total inspections under current filters.
                TotalInspections = await inspectionsQuery.CountAsync(),

                OpenFollowUps = await followUpsQuery
                    .CountAsync(f => f.Status == "Open"),

                // Assessment requirement: overdue open follow-ups.
                OverdueFollowUps = await followUpsQuery
                    .CountAsync(f => f.Status == "Open" && f.DueDate < today),

                RecentInspections = await inspectionsQuery
                    .OrderByDescending(i => i.InspectionDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}