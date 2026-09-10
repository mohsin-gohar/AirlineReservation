using AirlineReservation.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorize]
public class BookingsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Bookings.Include(x => x.User).Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity).OrderByDescending(x => x.BookingDate).ToListAsync());
}
