using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;
public class FlightController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Search() { ViewBag.Cities = await db.Cities.OrderBy(x=>x.CityName).ToListAsync(); return View(new FlightSearchViewModel { DepartureDate=DateTime.Today }); }
    [HttpGet]
    public async Task<IActionResult> Results(FlightSearchViewModel model)
    {
        if (model.DestinationCityId == model.OriginCityId && model.OriginCityId != 0) ModelState.AddModelError("DestinationCityId", "Destination must differ from origin.");
        if (model.ReturnDate.HasValue && model.ReturnDate.Value.Date < model.DepartureDate.Date) ModelState.AddModelError("ReturnDate", "Return date cannot be earlier than departure date.");
        if (model.DepartureDate.Date < DateTime.Today) ModelState.AddModelError("DepartureDate", "Departure date cannot be in the past.");
        if (!ModelState.IsValid)
        {
            ViewBag.Cities = await db.Cities.OrderBy(x => x.CityName).ToListAsync();
            return View("Search", model); // Search view is typed to FlightSearchViewModel.
        }
        var flights = await db.Flights.Include(x=>x.OriginCity).Include(x=>x.DestinationCity).Include(x=>x.OriginAirport).Include(x=>x.DestinationAirport)
            .Where(x=>x.OriginCityId==model.OriginCityId && x.DestinationCityId==model.DestinationCityId && x.DepartureDate.Date==model.DepartureDate.Date && x.FlightStatus==FlightStatus.Scheduled)
            .OrderBy(x => x.DepartureTime).ToListAsync();
        // Real business-class fares (FlightFare -> FlightSchedule) for every returned flight.
        var bizId = await db.SeatClasses.Where(x => x.Name == "Business").Select(x => x.SeatClassId).FirstOrDefaultAsync();
        if (bizId <= 0) bizId = 2;
        ViewBag.BusinessFares = await db.FlightFares.Include(x => x.FlightSchedule).Where(x => x.SeatClassId == bizId).ToListAsync();
        ViewBag.OriginCity = (await db.Cities.FindAsync(model.OriginCityId))?.CityName;
        ViewBag.DestinationCity = (await db.Cities.FindAsync(model.DestinationCityId))?.CityName;
        ViewBag.Search = model;
        return View(flights);
    }
    public async Task<IActionResult> Details(int id)
    {
        var flight = await db.Flights.Include(x=>x.OriginCity).Include(x=>x.DestinationCity).SingleOrDefaultAsync(x=>x.FlightId==id);
        return flight is null ? NotFound() : View(flight);
    }

    // Flight Status — public lookup by flight number and date.
    public IActionResult Status() => View(new FlightStatusViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(FlightStatusViewModel model)
    {
        var number = model.FlightNumber?.Trim().ToUpperInvariant() ?? "";
        if (number.Length < 3) ModelState.AddModelError("FlightNumber", "Enter a valid flight number, e.g. AR101.");
        if (model.FlightDate is null) ModelState.AddModelError("FlightDate", "Choose a departure date.");
        Flight? flight = null;
        if (ModelState.IsValid)
        {
            var date = (model.FlightDate ?? DateTime.Today).Date;
            flight = await db.Flights.Include(x => x.OriginAirport).Include(x => x.DestinationAirport)
                .Include(x => x.OriginCity).Include(x => x.DestinationCity)
                .FirstOrDefaultAsync(x => x.FlightNumber == number && x.DepartureDate.Date == date);
            if (flight is null) ModelState.AddModelError("", $"No flight {number} was scheduled for {(model.FlightDate ?? DateTime.Today).ToString("MMMM d, yyyy")}.");
        }
        model.FlightNumber = number;
        model.Flight = flight;
        return View(model);
    }
}
