using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorize]
public class BookingsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Bookings.Include(x => x.User).Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity).OrderByDescending(x => x.BookingDate).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> OverrideStatus(int id, BookingStatus newStatus)
    {
        var booking = await db.Bookings.Include(x => x.Flight).FirstOrDefaultAsync(x => x.BookingId == id);
        if (booking is null) return NotFound();

        var oldStatus = booking.BookingStatus;
        if (oldStatus != newStatus)
        {
            booking.BookingStatus = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            // Handle seat releasing if booking was active and is now cancelled
            if (oldStatus == BookingStatus.Active && newStatus == BookingStatus.Cancelled)
            {
                booking.Flight.EconomySeats += booking.PassengerCount;
                booking.CancellationNumber ??= $"CAN-{DateTime.UtcNow:HHmmssfff}";
            }

            await db.SaveChangesAsync();
            TempData["Message"] = $"Booking {(booking.ConfirmationNumber ?? booking.BlockingNumber)} status updated to {newStatus}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
