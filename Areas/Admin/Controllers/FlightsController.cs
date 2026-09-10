using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorize]
public class FlightsController(ApplicationDbContext db) : Controller
{
    private async Task LoadCitiesAsync() => ViewBag.Cities = await db.Cities.OrderBy(x => x.CityName).ToListAsync();

    public async Task<IActionResult> Index() => View(await db.Flights.Include(x => x.OriginCity).Include(x => x.DestinationCity).OrderBy(x => x.DepartureDate).ToListAsync());

    public async Task<IActionResult> Create()
    {
        await LoadCitiesAsync();
        return View(new Flight { DepartureDate = DateTime.Today.AddDays(7), DepartureTime = new TimeSpan(9, 0, 0), ArrivalTime = new TimeSpan(11, 0, 0) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Flight flight)
    {
        await ValidateFlight(flight, isNew: true);
        if (!ModelState.IsValid) { await LoadCitiesAsync(); return View(flight); }
        db.Add(flight);
        await db.SaveChangesAsync();
        TempData["Message"] = $"Flight {flight.FlightNumber} created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var flight = await db.Flights.FindAsync(id);
        if (flight is null) return NotFound();
        await LoadCitiesAsync();
        return View(flight);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Flight flight)
    {
        if (!await db.Flights.AnyAsync(x => x.FlightId == flight.FlightId)) return NotFound();
        await ValidateFlight(flight, isNew: false);
        if (!ModelState.IsValid) { await LoadCitiesAsync(); return View(flight); }
        try
        {
            db.Update(flight);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Flight {flight.FlightNumber} updated.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "The flight was modified by someone else. Reload and try again.";
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var flight = await db.Flights.FindAsync(id);
        if (flight is null) return NotFound();
        try
        {
            db.Remove(flight);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Flight {flight.FlightNumber} deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = $"Cannot delete flight {flight.FlightNumber}: bookings reference it. Cancel those bookings first.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateFlight(Flight flight, bool isNew)
    {
        if (flight.OriginCityId == flight.DestinationCityId) ModelState.AddModelError("DestinationCityId", "Destination must differ from origin.");
        if (isNew && await db.Flights.AnyAsync(x => x.FlightNumber == flight.FlightNumber)) ModelState.AddModelError("FlightNumber", "That flight number already exists.");
        if (flight.DepartureDate.Date < DateTime.Today) ModelState.AddModelError("DepartureDate", "Departure date cannot be in the past.");
        if (flight.EconomySeats <= 0 && flight.BusinessSeats <= 0) ModelState.AddModelError("EconomySeats", "Provide at least one seat.");
        if (flight.TicketPrice <= 0) ModelState.AddModelError("TicketPrice", "Ticket price must be greater than zero.");
    }
}
