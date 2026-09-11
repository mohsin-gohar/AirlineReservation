using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorize]
public class CancellationPolicyController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var rules = await db.CancellationRules
            .OrderByDescending(r => r.MinimumDaysBeforeDeparture)
            .ToListAsync();
        return View(rules);
    }

    public IActionResult Create()
    {
        return View(new CancellationRule { MinimumDaysBeforeDeparture = 0, RefundPercentage = 100 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CancellationRule rule)
    {
        if (rule.RefundPercentage < 0 || rule.RefundPercentage > 100)
            ModelState.AddModelError("RefundPercentage", "Refund percentage must be between 0 and 100.");
        if (rule.MinimumDaysBeforeDeparture < 0)
            ModelState.AddModelError("MinimumDaysBeforeDeparture", "Minimum days must be 0 or greater.");
        if (rule.MaximumDaysBeforeDeparture.HasValue && rule.MaximumDaysBeforeDeparture < rule.MinimumDaysBeforeDeparture)
            ModelState.AddModelError("MaximumDaysBeforeDeparture", "Maximum days cannot be less than minimum days.");

        if (!ModelState.IsValid) return View(rule);

        db.CancellationRules.Add(rule);
        await db.SaveChangesAsync();
        TempData["Message"] = "Cancellation rule added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var rule = await db.CancellationRules.FindAsync(id);
        if (rule is null) return NotFound();
        return View(rule);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CancellationRule rule)
    {
        if (!await db.CancellationRules.AnyAsync(r => r.CancellationRuleId == rule.CancellationRuleId))
            return NotFound();

        if (rule.RefundPercentage < 0 || rule.RefundPercentage > 100)
            ModelState.AddModelError("RefundPercentage", "Refund percentage must be between 0 and 100.");
        if (rule.MinimumDaysBeforeDeparture < 0)
            ModelState.AddModelError("MinimumDaysBeforeDeparture", "Minimum days must be 0 or greater.");
        if (rule.MaximumDaysBeforeDeparture.HasValue && rule.MaximumDaysBeforeDeparture < rule.MinimumDaysBeforeDeparture)
            ModelState.AddModelError("MaximumDaysBeforeDeparture", "Maximum days cannot be less than minimum days.");

        if (!ModelState.IsValid) return View(rule);

        db.CancellationRules.Update(rule);
        await db.SaveChangesAsync();
        TempData["Message"] = "Cancellation rule updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var rule = await db.CancellationRules.FindAsync(id);
        if (rule is null) return NotFound();

        db.CancellationRules.Remove(rule);
        await db.SaveChangesAsync();
        TempData["Message"] = "Cancellation rule deleted.";
        return RedirectToAction(nameof(Index));
    }
}
