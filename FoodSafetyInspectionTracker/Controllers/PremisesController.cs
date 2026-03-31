using FoodSafetyInspectionTracker.Data;
using FoodSafetyInspectionTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspectionTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PremisesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PremisesController> _logger;

        public PremisesController(ApplicationDbContext context, ILogger<PremisesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Only Admin can manage premises because they are the base records for inspections and follow-ups.
        public async Task<IActionResult> Index()
        {
            var premisesList = await _context.Premises
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(premisesList);
        }

        // GET: Premises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises details requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var premises = await _context.Premises
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises details requested for missing PremisesId {PremisesId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(premises);
        }

        // GET: Premises/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Premises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (await _context.Premises.AnyAsync(p => p.Name == premises.Name && p.Address == premises.Address))
            {
                ModelState.AddModelError("", "A premises with the same name and address already exists.");
                _logger.LogWarning("Duplicate premises create attempt. Name {Name}, Address {Address}, UserName {UserName}",
                    premises.Name, premises.Address, User.Identity?.Name ?? "Anonymous");
            }

            if (ModelState.IsValid)
            {
                _context.Add(premises);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Premises created. PremisesId {PremisesId}, Name {Name}, UserName {UserName}",
                    premises.Id, premises.Name, User.Identity?.Name ?? "Anonymous");

                return RedirectToAction(nameof(Index));
            }

            return View(premises);
        }

        // GET: Premises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises edit requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var premises = await _context.Premises.FindAsync(id);
            if (premises == null)
            {
                _logger.LogWarning("Premises edit requested for missing PremisesId {PremisesId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(premises);
        }

        // POST: Premises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Town,RiskRating")] Premises premises)
        {
            if (id != premises.Id)
            {
                _logger.LogWarning("Premises edit id mismatch. RouteId {RouteId}, ModelId {ModelId}, UserName {UserName}",
                    id, premises.Id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            if (await _context.Premises.AnyAsync(p =>
                p.Id != premises.Id &&
                p.Name == premises.Name &&
                p.Address == premises.Address))
            {
                ModelState.AddModelError("", "Another premises with the same name and address already exists.");
                _logger.LogWarning("Duplicate premises edit attempt. PremisesId {PremisesId}, Name {Name}, Address {Address}, UserName {UserName}",
                    premises.Id, premises.Name, premises.Address, User.Identity?.Name ?? "Anonymous");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(premises);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Premises updated. PremisesId {PremisesId}, Name {Name}, UserName {UserName}",
                        premises.Id, premises.Name, User.Identity?.Name ?? "Anonymous");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!PremisesExists(premises.Id))
                    {
                        _logger.LogWarning("Premises update failed because PremisesId {PremisesId} no longer exists", premises.Id);
                        return NotFound();
                    }

                    _logger.LogError(ex, "Concurrency error while updating PremisesId {PremisesId}", premises.Id);
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(premises);
        }

        // GET: Premises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Premises delete requested with null id by {UserName}", User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            var premises = await _context.Premises
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises delete requested for missing PremisesId {PremisesId} by {UserName}", id, User.Identity?.Name ?? "Anonymous");
                return NotFound();
            }

            return View(premises);
        }

        // POST: Premises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premises = await _context.Premises
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (premises == null)
            {
                _logger.LogWarning("Premises delete confirmed but PremisesId {PremisesId} was not found", id);
                return RedirectToAction(nameof(Index));
            }

            // Prevent deleting premises that already have inspections linked to them.
            if (premises.Inspections.Any())
            {
                TempData["ErrorMessage"] = "This premises cannot be deleted because it already has inspections linked to it.";

                _logger.LogWarning("Delete blocked for PremisesId {PremisesId} because related inspections exist. UserName {UserName}",
                    premises.Id, User.Identity?.Name ?? "Anonymous");

                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Premises.Remove(premises);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Premises deleted. PremisesId {PremisesId}, Name {Name}, UserName {UserName}",
                premises.Id, premises.Name, User.Identity?.Name ?? "Anonymous");

            return RedirectToAction(nameof(Index));
        }

        private bool PremisesExists(int id)
        {
            return _context.Premises.Any(e => e.Id == id);
        }
    }
}