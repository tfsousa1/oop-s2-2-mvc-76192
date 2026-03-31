using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Controllers
{
    [Authorize]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InspectionsController> _logger;

        public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Admin and Inspector can view inspections.
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Index()
        {
            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .OrderByDescending(i => i.InspectionDate)
                .ToListAsync();

            return View(inspections);
        }

        // Admin and Inspector can view inspection details.
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Inspection details requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null)
            {
                _logger.LogWarning("Inspection details requested for missing InspectionId {InspectionId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(inspection);
        }

        // Inspector can create inspections and Admin has full access.
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create()
        {
            await PopulatePremisesDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            var premisesExists = await _context.Premises.AnyAsync(p => p.Id == inspection.PremisesId);

            if (!premisesExists)
            {
                ModelState.AddModelError("PremisesId", "Please select a valid premises.");
                _logger.LogWarning("Attempt to create inspection with invalid PremisesId {PremisesId} by {UserName}",
                    inspection.PremisesId, User.Identity?.Name ?? "Anonymous");
            }

            if (inspection.Outcome == "Pass" && inspection.Score < 70)
            {
                ModelState.AddModelError("Outcome", "A pass outcome should normally have a score of 70 or above.");
                _logger.LogWarning("Inspection outcome validation warning during create. PremisesId {PremisesId}, Score {Score}, Outcome {Outcome}",
                    inspection.PremisesId, inspection.Score, inspection.Outcome);
            }

            if (inspection.Outcome == "Fail" && inspection.Score >= 70)
            {
                ModelState.AddModelError("Outcome", "A fail outcome should normally have a score below 70.");
                _logger.LogWarning("Inspection outcome validation warning during create. PremisesId {PremisesId}, Score {Score}, Outcome {Outcome}",
                    inspection.PremisesId, inspection.Score, inspection.Outcome);
            }

            if (ModelState.IsValid)
            {
                _context.Add(inspection);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Inspection created. InspectionId {InspectionId}, PremisesId {PremisesId}, UserName {UserName}",
                    inspection.Id, inspection.PremisesId, User.Identity?.Name ?? "Anonymous");

                return RedirectToAction(nameof(Index));
            }

            await PopulatePremisesDropDownList(inspection.PremisesId);
            return View(inspection);
        }

        // Only Admin can edit inspections.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Inspection edit requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
            {
                _logger.LogWarning("Inspection edit requested for missing InspectionId {InspectionId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            await PopulatePremisesDropDownList(inspection.PremisesId);
            return View(inspection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PremisesId,InspectionDate,Score,Outcome,Notes")] Inspection inspection)
        {
            if (id != inspection.Id)
            {
                _logger.LogWarning("Inspection edit id mismatch. RouteId {RouteId}, ModelId {ModelId}, UserName {UserName}",
                    id, inspection.Id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var premisesExists = await _context.Premises.AnyAsync(p => p.Id == inspection.PremisesId);

            if (!premisesExists)
            {
                ModelState.AddModelError("PremisesId", "Please select a valid premises.");
                _logger.LogWarning("Attempt to edit inspection with invalid PremisesId {PremisesId} by {UserName}",
                    inspection.PremisesId, User.Identity?.Name ?? "Anonymous");
            }

            if (inspection.Outcome == "Pass" && inspection.Score < 70)
            {
                ModelState.AddModelError("Outcome", "A pass outcome should normally have a score of 70 or above.");
                _logger.LogWarning("Inspection outcome validation warning during edit. InspectionId {InspectionId}, Score {Score}, Outcome {Outcome}",
                    inspection.Id, inspection.Score, inspection.Outcome);
            }

            if (inspection.Outcome == "Fail" && inspection.Score >= 70)
            {
                ModelState.AddModelError("Outcome", "A fail outcome should normally have a score below 70.");
                _logger.LogWarning("Inspection outcome validation warning during edit. InspectionId {InspectionId}, Score {Score}, Outcome {Outcome}",
                    inspection.Id, inspection.Score, inspection.Outcome);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Inspection updated. InspectionId {InspectionId}, PremisesId {PremisesId}, UserName {UserName}",
                        inspection.Id, inspection.PremisesId, User.Identity?.Name ?? "Anonymous");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!InspectionExists(inspection.Id))
                    {
                        _logger.LogWarning("Inspection update failed because InspectionId {InspectionId} no longer exists", inspection.Id);
                        return NotFound();
                    }

                    _logger.LogError(ex, "Concurrency error while updating InspectionId {InspectionId}", inspection.Id);
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulatePremisesDropDownList(inspection.PremisesId);
            return View(inspection);
        }

        // Only Admin can delete inspections.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Inspection delete requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null)
            {
                _logger.LogWarning("Inspection delete requested for missing InspectionId {InspectionId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(inspection);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);

            if (inspection == null)
            {
                _logger.LogWarning("Inspection delete confirmed but InspectionId {InspectionId} was not found", id);
                return RedirectToAction(nameof(Index));
            }

            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Inspection deleted. InspectionId {InspectionId}, UserName {UserName}",
                id, User.Identity?.Name ?? "Anonymous");

            return RedirectToAction(nameof(Index));
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }

        private async Task PopulatePremisesDropDownList(object? selectedPremises = null)
        {
            var premises = await _context.Premises
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    DisplayText = p.Name + " - " + p.Town
                })
                .ToListAsync();

            ViewData["PremisesId"] = new SelectList(premises, "Id", "DisplayText", selectedPremises);
        }
    }
}