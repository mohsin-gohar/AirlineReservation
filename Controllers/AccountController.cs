using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;
public class AccountController(ApplicationDbContext db, IPasswordHasher<User> hasher) : Controller
{
    public IActionResult Register() => View();
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await db.Users.AnyAsync(x => x.Email == model.Email)) { ModelState.AddModelError("Email", "Email is already registered."); return View(model); }
        var user = new User { Email = model.Email, FirstName = model.FirstName, LastName = model.LastName };
        user.PasswordHash = hasher.HashPassword(user, model.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        StartSession(user);
        return RedirectToAction("Index", "Home");
    }
    public IActionResult Login() => View();
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Login(string email, string password)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
        if (user is null || !VerifyPassword(user, password)) { ModelState.AddModelError("", "Invalid email or password."); return View(); }
        StartSession(user);
        return user.Role == UserRole.Admin ? RedirectToAction("Index", "Dashboard", new { area = "Admin" }) : RedirectToAction("Index", "Home");
    }
    private bool VerifyPassword(User user, string password)
    {
        if (!string.IsNullOrEmpty(user.PasswordHash)) return hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
        // Legacy plain-text accounts (seeded demo data) — verified directly.
        return user.Password == password;
    }
    private void StartSession(User user)
    {
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
        HttpContext.Session.SetInt32("UserRole", (int)user.Role);
    }
    public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction(nameof(Login)); }
}
