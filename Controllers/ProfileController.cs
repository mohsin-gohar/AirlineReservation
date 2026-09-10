using AirlineReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;
using AirlineReservation.Models;
public class ProfileController(ApplicationDbContext db) : Controller
{
    private int? UserId => HttpContext.Session.GetInt32("UserId");
    public async Task<IActionResult> Index()
    {
        if (UserId is not int id) return RedirectToAction("Login", "Account");
        var user = await db.Users.FindAsync(id);
        return user is null ? RedirectToAction("Login", "Account") : View(user);
    }
    public async Task<IActionResult> Edit()
    {
        if (UserId is not int id) return RedirectToAction("Login", "Account");
        var user = await db.Users.FindAsync(id);
        if (user is null) return RedirectToAction("Login", "Account");
        return View(new ProfileEditViewModel { UserId = user.UserId, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, PhoneNumber = user.PhoneNumber, Address = user.Address });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileEditViewModel model)
    {
        if (UserId is not int id || id != model.UserId) return Forbid();
        if (await db.Users.AnyAsync(x => x.Email == model.Email && x.UserId != id)) ModelState.AddModelError("Email", "That email is already in use.");
        if (!ModelState.IsValid) return View("Edit", model);
        var user = await db.Users.FindAsync(id);
        if (user is null) return RedirectToAction("Login", "Account");
        // Update only the editable fields — never touch credentials or miles here.
        user.FirstName = model.FirstName; user.LastName = model.LastName; user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber; user.Address = model.Address; user.UpdatedAt = DateTime.UtcNow;
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
        await db.SaveChangesAsync();
        TempData["Message"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
