using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Areas.Admin.Controllers;

public class OccupancyReportItem
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int BookedSeats { get; set; }
    public int OccupancyPercentage => TotalCapacity > 0 ? (int)Math.Min(100, Math.Round((double)BookedSeats / TotalCapacity * 100)) : 0;
}

public class CancellationReportItem
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal TotalRefund { get; set; }
}

[Area("Admin")]
[AdminAuthorize]
public class DashboardController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Flights = await db.Flights.CountAsync();
        ViewBag.ActiveBookings = await db.Bookings.CountAsync(x => x.BookingStatus == BookingStatus.Active);
        ViewBag.Users = await db.Users.CountAsync();
        ViewBag.BlockedHolds = await db.Bookings.CountAsync(x => x.BookingType == BookingType.Blocked && x.BookingStatus == BookingStatus.Active);
        ViewBag.Revenue = await db.Payments.Where(x => x.TransactionType == PaymentTransactionType.Charge && x.Status == PaymentStatus.Completed).SumAsync(x => (decimal?)x.Amount) ?? 0m;
        ViewBag.Refunds = await db.Payments.Where(x => x.TransactionType == PaymentTransactionType.Refund).SumAsync(x => (decimal?)x.Amount) ?? 0m;

        // 1. Occupancy report
        var flightsWithBookings = await db.Flights
            .Include(x => x.OriginCity)
            .Include(x => x.DestinationCity)
            .Select(x => new OccupancyReportItem
            {
                FlightNumber = x.FlightNumber,
                Route = $"{x.OriginCity.CityName} → {x.DestinationCity.CityName}",
                TotalCapacity = x.EconomySeats + x.BusinessSeats,
                BookedSeats = db.Bookings.Where(b => b.FlightId == x.FlightId && b.BookingStatus == BookingStatus.Active).Sum(b => b.NumberOfAdults + b.NumberOfChildren + b.NumberOfSeniors)
            }).ToListAsync();
        ViewBag.OccupancyReport = flightsWithBookings;

        // 2. Vacant seats report
        var vacantSeatsFlights = await db.Flights
            .Include(x => x.OriginCity)
            .Include(x => x.DestinationCity)
            .Where(x => x.EconomySeats > 0 || x.BusinessSeats > 0)
            .OrderByDescending(x => x.EconomySeats + x.BusinessSeats)
            .Take(5)
            .ToListAsync();
        ViewBag.VacantSeatsReport = vacantSeatsFlights;

        // 3. Frequent travelers
        var frequentTravelers = await db.Users
            .OrderByDescending(x => x.SkyMiles)
            .Take(5)
            .ToListAsync();
        ViewBag.FrequentTravelers = frequentTravelers;

        // 4. Cancellations report
        var cancellations = await db.Bookings
            .Where(x => x.BookingStatus == BookingStatus.Cancelled)
            .GroupBy(x => x.BookingDate.Date)
            .Select(g => new CancellationReportItem
            {
                Date = g.Key,
                Count = g.Count(),
                TotalRefund = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.Date)
            .Take(5)
            .ToListAsync();
        ViewBag.CancellationsReport = cancellations;

        var recent = await db.Bookings.Include(x => x.User).Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity).OrderByDescending(x => x.BookingDate).Take(6).ToListAsync();
        return View(recent);
    }
}
