using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;

public class NewsletterController(ApplicationDbContext db) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email)
    {
        email = email?.Trim() ?? "";
        if (email.Length < 5 || !email.Contains("@") || !email.Contains("."))
        {
            TempData["Error"] = "Please enter a valid email address to subscribe.";
            return Redirect("/");
        }
        var existing = await db.NewsletterSubscriptions.SingleOrDefaultAsync(x => x.Email == email);
        if (existing is null)
        {
            db.NewsletterSubscriptions.Add(new NewsletterSubscription { Email = email });
            TempData["Message"] = "Welcome aboard! You're subscribed to SkyPulse offers and fare alerts.";
        }
        else
        {
            existing.Active = true;
            existing.SubscribedAt = DateTime.UtcNow;
            TempData["Message"] = "You're already subscribed — new SkyPulse offers will keep coming your way.";
        }
        await db.SaveChangesAsync();
        return Redirect("/");
    }
}