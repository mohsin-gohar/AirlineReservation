using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorize]
public class UsersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await db.Users.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(u => new UserWithStats { User = u, ActiveBookings = u.Bookings.Count(b => b.BookingStatus == BookingStatus.Active) })
            .ToListAsync();
        return View(users);
    }
}
