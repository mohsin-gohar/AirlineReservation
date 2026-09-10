using AirlineReservation.Data;
using AirlineReservation.Models;
using AirlineReservation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;
public class BookingController(ApplicationDbContext db, ReservationService reservations) : Controller
{
    private int? UserId => HttpContext.Session.GetInt32("UserId");
    private IActionResult Login() => RedirectToAction("Login", "Account");

    private async Task<Flight?> GetFlightAsync(int flightId) =>
        await db.Flights.Include(x => x.OriginCity).Include(x => x.DestinationCity).SingleOrDefaultAsync(x => x.FlightId == flightId);

    // Step 2 of 4 — passenger details (Stitch "Passenger Telemetry & Travel Documents").
    public async Task<IActionResult> Create(int flightId, int adults = 1, int children = 0, int seniors = 0)
    {
        if (UserId is null) return Login();
        var flight = await GetFlightAsync(flightId);
        if (flight is null) return NotFound();
        if (flight.FlightStatus != FlightStatus.Scheduled)
        {
            TempData["Error"] = $"Flight {flight.FlightNumber} is {flight.FlightStatus.ToString().ToLowerInvariant()} and cannot be booked.";
            return RedirectToAction(nameof(Results), "Flight", new { OriginCityId = flight.OriginCityId, DestinationCityId = flight.DestinationCityId, DepartureDate = flight.DepartureDate.ToString("yyyy-MM-dd") });
        }
        var request = new BookingRequest { FlightId = flightId, NumberOfAdults = Math.Max(1, adults), NumberOfChildren = children, NumberOfSeniors = seniors };
        request.Passengers = Enumerable.Range(0, request.PassengerCount).Select(i => new PassengerInput()).ToList();
        return View((request, flight));
    }

    // Validate passenger details, park the draft in session, continue to Block vs Buy.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PassengerDetails(BookingRequest request)
    {
        if (UserId is null) return Login();
        var flight = await GetFlightAsync(request.FlightId);
        if (flight is null) return NotFound();
        if (flight.Departure <= DateTime.Now) ModelState.AddModelError("", "This flight has already departed.");
        if (flight.Departure <= DateTime.Now.AddDays(14)) TempData["BlockNotice"] = "Departure is within 14 days — ticket blocking is disabled for this flight. Purchase is required.";
        if (request.Passengers.Count > request.PassengerCount) request.Passengers = request.Passengers.Take(request.PassengerCount).ToList();
        if (!ModelState.IsValid) return View("Create", (request, flight));
        HttpContext.Session.SetString("BookingDraft", System.Text.Json.JsonSerializer.Serialize(request));
        return RedirectToAction(nameof(TicketOptions));
    }

    // Step 3 of 4 — Block vs Purchase (Stitch "Choose Ticket Option").
    [HttpGet]
    public async Task<IActionResult> TicketOptions()
    {
        if (UserId is null) return Login();
        var draft = HttpContext.Session.GetString("BookingDraft");
        if (draft is null) return RedirectToAction("Search", "Flight");
        var request = System.Text.Json.JsonSerializer.Deserialize<BookingRequest>(draft);
        if (request is null) return RedirectToAction("Search", "Flight");
        var flight = await GetFlightAsync(request.FlightId);
        if (flight is null) return RedirectToAction("Search", "Flight");
        return View((request, flight));
    }

    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Block() => await Save(true);
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Buy() => await Save(false);

    private async Task<IActionResult> Save(bool block)
    {
        if (UserId is null) return Login();
        var draft = HttpContext.Session.GetString("BookingDraft");
        if (draft is null) { TempData["Error"] = "Your booking session expired. Please search again."; return RedirectToAction("Search", "Flight"); }
        var request = System.Text.Json.JsonSerializer.Deserialize<BookingRequest>(draft);
        if (request is null) return RedirectToAction("Search", "Flight");
        HttpContext.Session.Remove("BookingDraft");
        var result = await reservations.CreateAsync(UserId.Value, request, block);
        if (!result.Success || result.Booking is null)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(TicketOptions));
        }
        TempData["Message"] = block ? "Your fare has been blocked. Confirm it any time before the hold expires." : "Your e-ticket has been issued. SkyMiles earned!";
        return RedirectToAction(nameof(Status), new { id = result.Booking.BookingId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(string blockingNumber)
    {
        if (UserId is null) return Login();
        var result = await reservations.ConfirmAsync(UserId.Value, blockingNumber);
        TempData[result.Success ? "Message" : "Error"] = result.Message;
        return RedirectToAction(nameof(MyBookings));
    }

    public async Task<IActionResult> MyBookings()
    {
        if (UserId is null) return Login();
        var bookings = await db.Bookings.Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity)
            .Where(x => x.UserId == UserId).OrderByDescending(x => x.BookingDate).ToListAsync();
        return View(bookings);
    }

    // Booking / ticket status page. Supports deep-link by id, lookup by reference (guests allowed
    // for read-only reference lookups), or latest booking for signed-in members.
    public async Task<IActionResult> Status(int? id, string? reference)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        Booking? booking = null;
        if (!string.IsNullOrWhiteSpace(reference))
        {
            var r = reference.Trim();
            var q = db.Bookings.Include(x => x.Flight).ThenInclude(f => f.OriginCity)
                .Include(x => x.Flight).ThenInclude(f => f.DestinationCity).Include(x => x.Passengers)
                .Where(x => x.ConfirmationNumber == r || x.BlockingNumber == r);
            if (userId.HasValue) q = q.Where(x => x.UserId == userId.Value);
            booking = await q.FirstOrDefaultAsync();
            if (booking is null) ModelState.AddModelError("", $"No booking found for reference '{r}'.");
        }
        else if (id.HasValue)
        {
            if (!userId.HasValue) return Login();
            booking = await db.Bookings.Include(x => x.Flight).ThenInclude(f => f.OriginCity)
                .Include(x => x.Flight).ThenInclude(f => f.DestinationCity).Include(x => x.Passengers)
                .FirstOrDefaultAsync(x => x.BookingId == id.Value && x.UserId == userId.Value);
        }
        else if (userId.HasValue)
        {
            booking = await db.Bookings.Include(x => x.Flight).ThenInclude(f => f.OriginCity)
                .Include(x => x.Flight).ThenInclude(f => f.DestinationCity).Include(x => x.Passengers)
                .Where(x => x.UserId == userId.Value).OrderByDescending(x => x.BookingDate).FirstOrDefaultAsync();
        }
        return View(booking);
    }

    // Check-in — list active upcoming bookings and check travelers in on the day of departure.
    public async Task<IActionResult> CheckIn()
    {
        if (UserId is null) return Login();
        var bookings = await db.Bookings
            .Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity)
            .Include(x => x.Flight).ThenInclude(f => f.OriginAirport).Include(x => x.Flight).ThenInclude(f => f.DestinationAirport)
            .Where(x => x.UserId == UserId && x.BookingStatus == BookingStatus.Active)
            .OrderBy(x => x.DepartureDate).ThenBy(x => x.CreatedAt).ToListAsync();
        return View(bookings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id)
    {
        if (UserId is null) return Login();
        var booking = await db.Bookings.Include(x => x.Flight).Include(x => x.Passengers)
            .FirstOrDefaultAsync(x => x.BookingId == id && x.UserId == UserId && x.BookingStatus == BookingStatus.Active);
        if (booking is null) { TempData["Error"] = "Active booking not found."; return RedirectToAction(nameof(CheckIn)); }
        if (booking.BookingType != BookingType.Confirmed) { TempData["Error"] = "Only confirmed e-tickets can be checked in. Confirm your fare hold first."; return RedirectToAction(nameof(CheckIn)); }
        if (booking.Flight.Departure <= DateTime.Now) { TempData["Error"] = "This flight has already departed."; return RedirectToAction(nameof(CheckIn)); }
        if (booking.IsCheckedIn) { TempData["Message"] = $"You are already checked in for SkyPulse {booking.Flight.FlightNumber}."; return RedirectToAction(nameof(CheckIn)); }
        booking.CheckedInAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Message"] = $"Checked in. Welcome aboard SkyPulse {booking.Flight.FlightNumber}.";
        return RedirectToAction(nameof(Status), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        if (UserId is null) return Login();
        var result = await reservations.CancelAsync(UserId.Value, id);
        TempData[result.Success ? "Message" : "Error"] = $"{result.Message} Refund: {result.Refund:C}";
        return RedirectToAction(nameof(MyBookings));
    }

    // GET — reschedule options for an active booking.
    public async Task<IActionResult> Reschedule(int id)
    {
                if (UserId is null) return Login();
        var booking = await db.Bookings.Include(x => x.Flight).ThenInclude(f => f.OriginCity).Include(x => x.Flight).ThenInclude(f => f.DestinationCity)
            .FirstOrDefaultAsync(x => x.BookingId == id && x.UserId == UserId && x.BookingStatus == BookingStatus.Active);
        if (booking is null) { TempData["Error"] = "Active booking not found."; return RedirectToAction(nameof(MyBookings)); }
        // Flight.Departure and Booking.PassengerCount are [NotMapped] computed properties and
        // therefore cannot appear inside a SQL query. Capture their underlying mapped column
        // values into local scalars so EF Core can translate the predicate to SQL.
        var now = DateTime.Now;
        var originCityId = booking.Flight.OriginCityId;
        var destinationCityId = booking.Flight.DestinationCityId;
        var flightId = booking.FlightId;
        var passengerCount = booking.NumberOfAdults + booking.NumberOfChildren + booking.NumberOfSeniors;
        var alternatives = await db.Flights.Include(x => x.OriginCity).Include(x => x.DestinationCity)
            .Where(x => x.OriginCityId == originCityId && x.DestinationCityId == destinationCityId
                        && x.FlightId != flightId && x.FlightStatus == FlightStatus.Scheduled
                        // Equivalent to the unmapped (DepartureDate.Date + DepartureTime) > now.
                        && (x.DepartureDate.Date > now.Date
                            || (x.DepartureDate.Date == now.Date && x.DepartureTime > now.TimeOfDay))
                        && x.EconomySeats >= passengerCount)
            .OrderBy(x => x.DepartureDate).ThenBy(x => x.DepartureTime).ToListAsync();
        var model = new RescheduleViewModel
        {
            BookingId = booking.BookingId,
            Reference = booking.ConfirmationNumber ?? booking.BlockingNumber,
            Route = $"{booking.Flight.OriginCity.CityName} → {booking.Flight.DestinationCity.CityName}",
            CurrentDeparture = booking.Flight.Departure,
            CurrentTotal = booking.TotalPrice,
            AlternativeFlights = alternatives
        };
        return View(model);
    }

    // POST — move the booking to the selected alternative flight, adjusting seats and price.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(int id, int newFlightId)
    {
        if (UserId is null) return Login();
        var booking = await db.Bookings.Include(x => x.Flight).Include(x => x.Passengers)
            .FirstOrDefaultAsync(x => x.BookingId == id && x.UserId == UserId && x.BookingStatus == BookingStatus.Active);
        if (booking is null) { TempData["Error"] = "Active booking not found."; return RedirectToAction(nameof(MyBookings)); }
        var newFlight = await db.Flights.FindAsync(newFlightId);
        if (newFlight is null || newFlight.FlightId == booking.FlightId) { TempData["Error"] = "Please choose a different flight."; return RedirectToAction(nameof(Reschedule), new { id }); }
        if (newFlight.FlightStatus != FlightStatus.Scheduled || newFlight.Departure <= DateTime.Now) { TempData["Error"] = "That flight is not available for rescheduling."; return RedirectToAction(nameof(Reschedule), new { id }); }
        if (newFlight.EconomySeats < booking.PassengerCount) { TempData["Error"] = "Not enough economy seats on the selected flight."; return RedirectToAction(nameof(Reschedule), new { id }); }

        // Release the old seats and mark the original booking rescheduled.
        booking.Flight.EconomySeats += booking.PassengerCount;
        booking.BookingStatus = BookingStatus.Rescheduled;
        booking.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Carry the passengers to a fresh record on the new flight.
        newFlight.EconomySeats -= booking.PassengerCount;
        var carried = new Booking
        {
            UserId = booking.UserId, FlightId = newFlight.FlightId, BookingType = booking.BookingType,
            ConfirmationNumber = booking.BookingType == BookingType.Confirmed ? await reservations.NewConfirmationNumberAsync() : null,
            BlockingNumber = booking.BookingType == BookingType.Blocked ? await reservations.NewBlockingNumberAsync() : null,
            NumberOfAdults = booking.NumberOfAdults, NumberOfChildren = booking.NumberOfChildren, NumberOfSeniors = booking.NumberOfSeniors,
            TotalPrice = newFlight.TicketPrice * booking.PassengerCount, DepartureDate = newFlight.Departure, BookingDate = DateTime.UtcNow
        };
        foreach (var p in booking.Passengers) carried.Passengers.Add(new Passenger { FirstName = p.FirstName, LastName = p.LastName, PassengerType = p.PassengerType, PassportNumber = p.PassportNumber });
        db.Bookings.Add(carried);
        db.Notifications.Add(new Notification { UserId = booking.UserId, Booking = carried, Type = NotificationType.ScheduleChange, ScheduledSendDate = DateTime.UtcNow });
        await db.SaveChangesAsync();
        TempData["Message"] = "Ticket rescheduled. Your new itinerary is confirmed below.";
        return RedirectToAction(nameof(Status), new { id = carried.BookingId });
    }
}
