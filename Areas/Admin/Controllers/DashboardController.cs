using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Areas.Admin.Controllers;

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
        var recent = await db.Bookings.Include(x => x.User).Include(x => x.Flight).OrderByDescending(x => x.BookingDate).Take(6).ToListAsync();
        return View(recent);
    }
}
