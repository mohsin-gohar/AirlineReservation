using System.Security.Claims;
using AirlineReservation.Data;
using AirlineReservation.DTOs;
using AirlineReservation.Models;
using AirlineReservation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Controllers;

[ApiController, Authorize(Roles="RegisteredUser,Admin,Clerk"), Route("api/bookings")]
public class ApiBookingController(ApplicationDbContext db, ReservationService service) : ControllerBase
{
    /// <summary>Blocks a ticket when departure is more than fourteen days away.</summary>
    [HttpPost("block")] public async Task<IActionResult> Block(BookingRequestDto r){var result=await service.CreateAsync(Id,Map(r),true);return result.Success?Ok(result.Booking):BadRequest(result.Message);}
    /// <summary>Purchases a ticket, charges payment, and awards SkyMiles.</summary>
    [HttpPost("buy")] public async Task<IActionResult> Buy(BookingRequestDto r){var result=await service.CreateAsync(Id,Map(r),false);return result.Success?Ok(result.Booking):BadRequest(result.Message);}
    /// <summary>Maps the DTO and synthesizes a passenger manifest when names are not supplied.</summary>
    private static BookingRequest Map(BookingRequestDto r)
    {
        var request = new BookingRequest { FlightId = r.FlightId, NumberOfAdults = r.Adults, NumberOfChildren = r.Children, NumberOfSeniors = r.Seniors, CreditCardNumber = r.CreditCardNumber };
        request.Passengers = r.Passengers is { Count: > 0 } ? r.Passengers
            : Enumerable.Range(0, request.PassengerCount).Select(i => new PassengerInput { FirstName = $"Guest {i + 1}", LastName = "TBA", PassengerType = i < request.NumberOfAdults ? "Adult" : i < request.NumberOfAdults + request.NumberOfChildren ? "Child" : "Senior" }).ToList();
        return request;
    }
    /// <summary>Confirms a blocked booking using its blocking number.</summary>
    [HttpPost("{id:int}/confirm")] public async Task<IActionResult> Confirm(int id,[FromQuery] string blockingNumber){var result=await service.ConfirmAsync(Id,blockingNumber);return result.Success?Ok():BadRequest(result.Message);}
    /// <summary>Cancels a booking, restores seats, and calculates a policy refund.</summary>
    [HttpDelete("{id:int}/cancel")] public async Task<IActionResult> Cancel(int id){var result=await service.CancelAsync(Id,id);return result.Success?Ok(new{result.Refund}):NotFound(result.Message);}
    /// <summary>Returns booking status by booking identifier.</summary>
    [HttpGet("{id:int}/status")] public async Task<IActionResult> Status(int id)=>Ok(await db.Bookings.Include(x=>x.Flight).Include(x=>x.Passengers).SingleOrDefaultAsync(x=>x.BookingId==id&&x.UserId==Id));
    /// <summary>Reschedules a confirmed booking after checking the replacement flight.</summary>
    [HttpPut("{id:int}/reschedule")] public async Task<IActionResult> Reschedule(int id,[FromQuery]int newFlightId){var booking=await db.Bookings.Include(x=>x.Flight).SingleOrDefaultAsync(x=>x.BookingId==id&&x.UserId==Id);var flight=await db.Flights.FindAsync(newFlightId);if(booking==null||flight==null)return NotFound();if(booking.BookingType!=BookingType.Confirmed)return BadRequest("Only confirmed bookings can be rescheduled.");if(booking.BookingStatus!=BookingStatus.Active)return BadRequest("Only active bookings can be rescheduled.");if(flight.FlightStatus!=FlightStatus.Scheduled||flight.Departure<=DateTime.Now)return BadRequest("That flight is not available for rescheduling.");var count=booking.NumberOfAdults+booking.NumberOfChildren+booking.NumberOfSeniors;if(flight.EconomySeats<count)return BadRequest("Insufficient seats.");booking.Flight.EconomySeats+=count;flight.EconomySeats-=count;booking.FlightId=flight.FlightId;booking.DepartureDate=flight.Departure;booking.ConfirmationNumber=$"CNF-{Guid.NewGuid():N}"[..20].ToUpperInvariant();booking.TotalPrice=flight.TicketPrice*count;booking.UpdatedAt=DateTime.UtcNow;db.Notifications.Add(new Notification{UserId=booking.UserId,BookingId=booking.BookingId,Type=NotificationType.ScheduleChange,ScheduledSendDate=DateTime.UtcNow});await db.SaveChangesAsync();return Ok(booking);}
    private int Id=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController, Authorize, Route("api/payments")]
public class PaymentController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Records a charge against a booking using a masked card number.</summary>
    [HttpPost("charge")] public async Task<IActionResult> Charge(int bookingId,decimal amount,string creditCardNumber){var b=await db.Bookings.FindAsync(bookingId);if(b==null)return NotFound();db.Payments.Add(new Payment{BookingId=bookingId,Amount=amount,CreditCardNumberMasked=Mask(creditCardNumber),TransactionType=PaymentTransactionType.Charge,Status=PaymentStatus.Completed});await db.SaveChangesAsync();return Ok();}
    /// <summary>Records a refund against a booking.</summary>
    [HttpPost("refund")] public async Task<IActionResult> Refund(int bookingId,decimal amount){if(await db.Bookings.FindAsync(bookingId)==null)return NotFound();db.Payments.Add(new Payment{BookingId=bookingId,Amount=amount,TransactionType=PaymentTransactionType.Refund,Status=PaymentStatus.Completed});await db.SaveChangesAsync();return Ok();}
    private static string Mask(string value)=>value.Length<4?"****":$"****{value[^4..]}";
}

[ApiController, Authorize, Route("api/notifications")]
public class NotificationController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Returns pending notifications for administrative processing.</summary>
    [HttpGet("pending")] public async Task<IActionResult> Pending()=>Ok(await db.Notifications.Where(x=>!x.Sent&&x.ScheduledSendDate<=DateTime.UtcNow).ToListAsync());
}
