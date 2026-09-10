using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Services;

public class ReservationService(ApplicationDbContext db)
{
    public async Task<(bool Success, string Message, Booking? Booking)> CreateAsync(int userId, BookingRequest request, bool block, CancellationToken ct = default)
    {
        var flight = await db.Flights.SingleOrDefaultAsync(x => x.FlightId == request.FlightId, ct);
        if (flight is null) return (false, "Flight was not found.", null);
        var count = request.PassengerCount;
        if (count <= 0) return (false, "At least one passenger is required.", null);
        if (block && flight.Departure <= DateTime.Now.AddDays(14)) return (false, "Tickets cannot be blocked within two weeks of departure.", null);
        if (flight.Departure <= DateTime.Now) return (false, "This flight has already departed.", null);
        if (flight.EconomySeats < count) return (false, "There are not enough available economy seats.", null);
        // Require a name for every traveller on the manifest.
        if (request.Passengers.Count < count) return (false, "Please provide details for every passenger.", null);

        flight.EconomySeats -= count;
        var booking = new Booking { UserId = userId, FlightId = flight.FlightId, BookingType = block ? BookingType.Blocked : BookingType.Confirmed,
            BlockingNumber = block ? await NewBlockingNumberAsync(ct) : null, ConfirmationNumber = block ? null : await NewConfirmationNumberAsync(ct),
            NumberOfAdults = request.NumberOfAdults, NumberOfChildren = request.NumberOfChildren, NumberOfSeniors = request.NumberOfSeniors,
            TotalPrice = flight.TicketPrice * count, BookingDate = DateTime.UtcNow, DepartureDate = flight.Departure,
            EmergencyContactName = request.EmergencyContactName, EmergencyContactPhone = request.EmergencyContactPhone };
        foreach (var p in request.Passengers.Take(count)) booking.Passengers.Add(new Passenger { FirstName = p.FirstName.Trim(), LastName = p.LastName.Trim(), PassengerType = p.PassengerType, PassportNumber = p.PassportNumber });
        db.Bookings.Add(booking);
        if (!block)
        {
            db.Payments.Add(new Payment { Booking = booking, Amount = booking.TotalPrice, CreditCardNumberMasked = Mask(request.CreditCardNumber), TransactionType = PaymentTransactionType.Charge, Status = PaymentStatus.Completed });
            await AddMilesAsync(userId, booking, ct);
            db.Notifications.Add(new Notification { UserId = userId, Booking = booking, Type = NotificationType.ConfirmationNotice, ScheduledSendDate = DateTime.UtcNow });
        }
        else if (flight.Departure > DateTime.Now.AddDays(21))
        {
            db.Notifications.Add(new Notification { UserId = userId, Booking = booking, Type = NotificationType.BlockingReminder, ScheduledSendDate = flight.Departure.ToUniversalTime().AddDays(-21) });
        }
        await db.SaveChangesAsync(ct);
        return (true, "Booking created.", booking);
    }

    public async Task<(bool Success, string Message)> ConfirmAsync(int userId, string blockingNumber, CancellationToken ct = default)
    {
        var booking = await db.Bookings.Include(x => x.Flight).SingleOrDefaultAsync(x => x.UserId == userId && x.BlockingNumber == blockingNumber && x.BookingStatus == BookingStatus.Active, ct);
        if (booking is null || booking.BookingType != BookingType.Blocked) return (false, "Valid blocked booking not found.");
        if (booking.Flight.Departure <= DateTime.Now) return (false, "This flight has already departed; the blocked ticket can no longer be confirmed.");
        booking.BookingType = BookingType.Confirmed; booking.ConfirmationNumber = await NewConfirmationNumberAsync(ct); booking.UpdatedAt = DateTime.UtcNow;
        db.Payments.Add(new Payment { BookingId = booking.BookingId, Amount = booking.TotalPrice, CreditCardNumberMasked = "****", TransactionType = PaymentTransactionType.Charge, Status = PaymentStatus.Completed });
        await AddMilesAsync(userId, booking, ct);
        db.Notifications.Add(new Notification { UserId = userId, BookingId = booking.BookingId, Type = NotificationType.ConfirmationNotice, ScheduledSendDate = DateTime.UtcNow });
        await db.SaveChangesAsync(ct); return (true, "Booking confirmed.");
    }

    public async Task<(bool Success, string Message, decimal Refund)> CancelAsync(int userId, int bookingId, CancellationToken ct = default)
    {
        var booking = await db.Bookings.Include(x => x.Flight).SingleOrDefaultAsync(x => x.BookingId == bookingId && x.UserId == userId, ct);
        if (booking is null || booking.BookingStatus != BookingStatus.Active) return (false, "Active booking not found.", 0);
        var days = (booking.Flight.Departure - DateTime.Now).TotalDays;
        var rule = await db.CancellationRules.Where(x => days >= x.MinimumDaysBeforeDeparture && (x.MaximumDaysBeforeDeparture == null || days <= x.MaximumDaysBeforeDeparture)).OrderByDescending(x => x.MinimumDaysBeforeDeparture).FirstOrDefaultAsync(ct);
        // Blocked tickets were never paid for — nothing to refund.
        var refund = booking.BookingType == BookingType.Confirmed ? booking.TotalPrice * (rule?.RefundPercentage ?? 0) / 100m : 0m;
        booking.BookingStatus = BookingStatus.Cancelled;
        booking.CancellationNumber = await NewCancellationNumberAsync(ct);
        booking.UpdatedAt = DateTime.UtcNow;
        booking.Flight.EconomySeats += booking.PassengerCount;
        if (booking.BookingType == BookingType.Confirmed) await DeductMilesAsync(userId, booking, ct);
        // Record the policy refund so admin revenue/refund reports stay accurate.
        if (refund > 0m) db.Payments.Add(new Payment { BookingId = booking.BookingId, Amount = refund, CreditCardNumberMasked = "****", TransactionType = PaymentTransactionType.Refund, Status = PaymentStatus.Completed });
        await db.SaveChangesAsync(ct);
        return (true, "Booking cancelled.", refund);
    }

    // Adjust the SkyMiles balance and keep an auditable transaction entry for the member's history.
    private async Task AddMilesAsync(int userId, Booking booking, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([userId], ct);
        if (u is null) return;
        var miles = (int)booking.TotalPrice;
        u.SkyMiles += miles;
        db.SkyMilesTransactions.Add(new SkyMilesTransaction { UserId = userId, Booking = booking, MilesChange = miles, Reason = "SkyMiles earned for ticket purchase" });
    }
    private async Task DeductMilesAsync(int userId, Booking booking, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([userId], ct);
        if (u is null) return;
        var miles = (int)booking.TotalPrice;
        u.SkyMiles = Math.Max(0, u.SkyMiles - miles);
        db.SkyMilesTransactions.Add(new SkyMilesTransaction { UserId = userId, Booking = booking, MilesChange = -miles, Reason = "SkyMiles deducted on cancellation" });
    }

    // Human-friendly reference numbers, e.g. AW-847291 / BLK-492017 / CAN-883104, with collision-safe retries.
    private async Task<string> NumberAsync(string prefix, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"{prefix}-{Random.Shared.Next(100000, 999999)}";
            var taken = await db.Bookings.AnyAsync(x => x.ConfirmationNumber == candidate || x.BlockingNumber == candidate || x.CancellationNumber == candidate, ct);
            if (!taken) return candidate;
        }
        return $"{prefix}-{DateTime.UtcNow:HHmmssfff}";
    }
    public Task<string> NewConfirmationNumberAsync(CancellationToken ct = default) => NumberAsync("AW", ct);
    public Task<string> NewBlockingNumberAsync(CancellationToken ct = default) => NumberAsync("BLK", ct);
    public Task<string> NewCancellationNumberAsync(CancellationToken ct = default) => NumberAsync("CAN", ct);

    private static string Mask(string? card) => string.IsNullOrWhiteSpace(card) || card.Length < 4 ? "****" : $"****{card[^4..]}";
}
