using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Cities = await db.Cities.OrderBy(x => x.CityName).ToListAsync();
        ViewBag.Featured = await db.Flights.Include(x => x.OriginCity).Include(x => x.DestinationCity)
            .Where(x => x.FlightStatus == FlightStatus.Scheduled && x.DepartureDate.Date > DateTime.Today)
            .OrderBy(x => x.TicketPrice).Take(4).ToListAsync();
        return View();
    }
    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
    public async Task<IActionResult> Destinations()
    {
        ViewBag.Cities = await db.Cities.Include(x => x.Airports).OrderBy(x => x.CityName).ToListAsync();
        return View();
    }
    public async Task<IActionResult> Offers()
    {
        var flights = await db.Flights.Include(x => x.OriginCity).Include(x => x.DestinationCity)
            .Include(x => x.OriginAirport).Include(x => x.DestinationAirport)
            .Where(x => x.FlightStatus == FlightStatus.Scheduled && x.DepartureDate.Date > DateTime.Today)
            .OrderBy(x => x.TicketPrice).ToListAsync();
        return View(flights);
    }
    public async Task<IActionResult> Dashboard()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null) return RedirectToAction("Login", "Account");
        var user = await db.Users.FindAsync(userId.Value);
        if (user is null) return RedirectToAction("Login", "Account");
        var bookings = await db.Bookings.Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity)
            .Where(x => x.UserId == userId).OrderByDescending(x => x.BookingDate).Take(6).ToListAsync();
        return View((user, bookings));
    }
    public IActionResult Error() => View(new Models.ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
