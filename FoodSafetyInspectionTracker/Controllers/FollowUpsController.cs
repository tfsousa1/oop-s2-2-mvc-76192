using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Controllers
{
    [Authorize]
    public class FollowUpsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FollowUpsController> _logger;

        public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Admin and Inspector can view follow-ups.
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Index()
        {
            var followUps = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i!.Premises)
                .OrderBy(f => f.DueDate)
                .ToListAsync();

            return View(followUps);
        }

        // Admin and Inspector can view follow-up details.
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Follow-up details requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i!.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null)
            {
                _logger.LogWarning("Follow-up details requested for missing FollowUpId {FollowUpId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(followUp);
        }

        // Inspector can create follow-ups and Admin has full access.
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create()
        {
            await PopulateInspectionDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Inspector")]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            var inspection = await _context.Inspections
                .Include(i => i.Premises)
                .FirstOrDefaultAsync(i => i.Id == followUp.InspectionId);

            if (inspection == null)
            {
                ModelState.AddModelError("InspectionId", "Please select a valid inspection.");
                _logger.LogWarning("Attempt to create follow-up with invalid InspectionId {InspectionId} by {UserName}",
                    followUp.InspectionId, User.Identity?.Name ?? "Anonymous");
            }
            else
            {
                // A follow-up should not be due before the inspection took place.
                if (followUp.DueDate.Date < inspection.InspectionDate.Date)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be earlier than the inspection date.");
                    _logger.LogWarning("Invalid follow-up due date. InspectionId {InspectionId}, DueDate {DueDate}, InspectionDate {InspectionDate}",
                        followUp.InspectionId, followUp.DueDate, inspection.InspectionDate);
                }
            }

            // If the follow-up is closed, a closed date must be recorded.
            if (followUp.Status == "Closed" && followUp.ClosedDate == null)
            {
                ModelState.AddModelError("ClosedDate", "Closed Date is required when the follow-up status is Closed.");
                _logger.LogWarning("Attempt to create closed follow-up without ClosedDate for InspectionId {InspectionId}",
                    followUp.InspectionId);
            }

            // If the follow-up is still open, ClosedDate should not be stored.
            if (followUp.Status == "Open")
            {
                followUp.ClosedDate = null;
            }

            if (ModelState.IsValid)
            {
                _context.Add(followUp);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Follow-up created. FollowUpId {FollowUpId}, InspectionId {InspectionId}, UserName {UserName}",
                    followUp.Id, followUp.InspectionId, User.Identity?.Name ?? "Anonymous");

                return RedirectToAction(nameof(Index));
            }

            await PopulateInspectionDropDownList(followUp.InspectionId);
            return View(followUp);
        }

        // Only Admin can edit existing follow-ups.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Follow-up edit requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null)
            {
                _logger.LogWarning("Follow-up edit requested for missing FollowUpId {FollowUpId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            await PopulateInspectionDropDownList(followUp.InspectionId);
            return View(followUp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,DueDate,Status,ClosedDate")] FollowUp followUp)
        {
            if (id != followUp.Id)
            {
                _logger.LogWarning("Follow-up edit id mismatch. RouteId {RouteId}, ModelId {ModelId}, UserName {UserName}",
                    id, followUp.Id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var inspection = await _context.Inspections.FirstOrDefaultAsync(i => i.Id == followUp.InspectionId);

            if (inspection == null)
            {
                ModelState.AddModelError("InspectionId", "Please select a valid inspection.");
                _logger.LogWarning("Attempt to edit follow-up with invalid InspectionId {InspectionId} by {UserName}",
                    followUp.InspectionId, User.Identity?.Name ?? "Anonymous");
            }
            else if (followUp.DueDate.Date < inspection.InspectionDate.Date)
            {
                ModelState.AddModelError("DueDate", "Due date cannot be earlier than the inspection date.");
                _logger.LogWarning("Invalid follow-up due date during edit. FollowUpId {FollowUpId}, InspectionId {InspectionId}",
                    followUp.Id, followUp.InspectionId);
            }

            if (followUp.Status == "Closed" && followUp.ClosedDate == null)
            {
                ModelState.AddModelError("ClosedDate", "Closed Date is required when the follow-up status is Closed.");
                _logger.LogWarning("Attempt to edit closed follow-up without ClosedDate. FollowUpId {FollowUpId}",
                    followUp.Id);
            }

            if (followUp.Status == "Open")
            {
                followUp.ClosedDate = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(followUp);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Follow-up updated. FollowUpId {FollowUpId}, InspectionId {InspectionId}, UserName {UserName}",
                        followUp.Id, followUp.InspectionId, User.Identity?.Name ?? "Anonymous");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!FollowUpExists(followUp.Id))
                    {
                        _logger.LogWarning("Follow-up update failed because FollowUpId {FollowUpId} no longer exists", followUp.Id);
                        return NotFound();
                    }

                    _logger.LogError(ex, "Concurrency error while updating FollowUpId {FollowUpId}", followUp.Id);
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateInspectionDropDownList(followUp.InspectionId);
            return View(followUp);
        }

        // Only Admin can delete follow-ups.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Follow-up delete requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var followUp = await _context.FollowUps
                .Include(f => f.Inspection)
                .ThenInclude(i => i!.Premises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (followUp == null)
            {
                _logger.LogWarning("Follow-up delete requested for missing FollowUpId {FollowUpId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(followUp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followUp = await _context.FollowUps.FindAsync(id);

            if (followUp == null)
            {
                _logger.LogWarning("Follow-up delete confirmed but FollowUpId {FollowUpId} was not found", id);
                return RedirectToAction(nameof(Index));
            }

            _context.FollowUps.Remove(followUp);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Follow-up deleted. FollowUpId {FollowUpId}, UserName {UserName}",
                id, User.Identity?.Name ?? "Anonymous");

            return RedirectToAction(nameof(Index));
        }

        private bool FollowUpExists(int id)
        {
            return _context.FollowUps.Any(e => e.Id == id);
        }

        private async Task PopulateInspectionDropDownList(object? selectedInspection = null)
        {
            var inspections = await _context.Inspections
                .Include(i => i.Premises)
                .OrderByDescending(i => i.InspectionDate)
                .Select(i => new
                {
                    i.Id,
                    DisplayText = "Inspection #" + i.Id + " - " + (i.Premises != null ? i.Premises.Name : "Unknown Premises")
                })
                .ToListAsync();

            ViewData["InspectionId"] = new SelectList(inspections, "Id", "DisplayText", selectedInspection);
        }
    }
}