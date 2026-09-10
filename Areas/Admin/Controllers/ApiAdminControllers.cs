using AirlineReservation.Data;
using AirlineReservation.DTOs;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Areas.Admin.Controllers;

[ApiController, Area("Admin"), Authorize(Roles="Admin,Clerk"), Route("api/admin/flights")]
public class AdminFlightController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Lists flights with schedules and fares.</summary>
    [HttpGet] public async Task<IActionResult> List()=>Ok(await db.Flights.Include(x=>x.Schedules).ThenInclude(x=>x.Fares).ToListAsync());
    /// <summary>Creates a flight.</summary>
    [HttpPost] public async Task<IActionResult> Create(Flight flight){if(!ModelState.IsValid)return ValidationProblem(ModelState);db.Flights.Add(flight);await db.SaveChangesAsync();return CreatedAtAction(nameof(List),new{id=flight.FlightId},flight);}
    /// <summary>Updates a flight.</summary>
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,Flight flight){if(id!=flight.FlightId)return BadRequest();db.Entry(flight).State=EntityState.Modified;await db.SaveChangesAsync();return Ok(flight);}
    /// <summary>Deletes a flight.</summary>
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id){var f=await db.Flights.FindAsync(id);if(f==null)return NotFound();db.Remove(f);await db.SaveChangesAsync();return NoContent();}
    /// <summary>Manually adjusts available seats.</summary>
    [HttpPut("{id:int}/seats")] public async Task<IActionResult> Seats(int id,SeatAdjustmentRequest r){var f=await db.Flights.FindAsync(id);if(f==null)return NotFound();f.EconomySeats=r.EconomySeats;f.BusinessSeats=r.BusinessSeats;await db.SaveChangesAsync();return Ok(f);}
}

[ApiController, Area("Admin"), Authorize(Roles="Admin,Clerk"), Route("api/admin/bookings")]
public class AdminBookingController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Lists bookings with optional status filtering.</summary>
    [HttpGet] public async Task<IActionResult> List(BookingStatus? status)=>Ok(await db.Bookings.Include(x=>x.User).Include(x=>x.Flight).Where(x=>status==null||x.BookingStatus==status).ToListAsync());
    /// <summary>Returns full booking details.</summary>
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id)=>Ok(await db.Bookings.Include(x=>x.Passengers).Include(x=>x.Payments).SingleOrDefaultAsync(x=>x.BookingId==id));
    /// <summary>Overrides the status of a booking.</summary>
    [HttpPut("{id:int}/override-status")] public async Task<IActionResult> Override(int id,OverrideStatusRequest request){var b=await db.Bookings.FindAsync(id);if(b==null)return NotFound();b.BookingStatus=request.Status;await db.SaveChangesAsync();return Ok(b);}
}

[ApiController, Area("Admin"), Authorize(Roles="Admin,Clerk"), Route("api/admin/users")]
public class AdminUserController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Lists registered users.</summary>
    [HttpGet] public async Task<IActionResult> List(string? search)=>Ok(await db.Users.Where(x=>search==null||x.Email.Contains(search)||x.LastName.Contains(search)).ToListAsync());
    /// <summary>Returns a user profile, bookings, and SkyMiles history.</summary>
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id)=>Ok(await db.Users.Include(x=>x.Bookings).Include(x=>x.SkyMilesTransactions).SingleOrDefaultAsync(x=>x.UserId==id));
}

[ApiController, Area("Admin"), Authorize(Roles="Admin,Clerk"), Route("api/admin/reports")]
public class AdminReportsController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Returns occupancy by flight.</summary>
    [HttpGet("occupancy")] public async Task<IActionResult> Occupancy()=>Ok(await db.Flights.Select(x=>new{x.FlightId,x.FlightNumber,x.EconomySeats,x.BusinessSeats,Booked=db.Bookings.Where(b=>b.FlightId==x.FlightId&&b.BookingStatus==BookingStatus.Active).Sum(b=>b.NumberOfAdults+b.NumberOfChildren+b.NumberOfSeniors)}).ToListAsync());
    /// <summary>Returns confirmed booking revenue.</summary>
    [HttpGet("revenue")] public async Task<IActionResult> Revenue()=>Ok(await db.Bookings.Where(x=>x.BookingType==BookingType.Confirmed&&x.BookingStatus==BookingStatus.Active).SumAsync(x=>(decimal?)x.TotalPrice)??0);
    /// <summary>Returns flights with available seats.</summary>
    [HttpGet("vacant-seats")] public async Task<IActionResult> VacantSeats()=>Ok(await db.Flights.Where(x=>x.EconomySeats>0||x.BusinessSeats>0).ToListAsync());
    /// <summary>Returns users ranked by SkyMiles.</summary>
    [HttpGet("frequent-travelers")] public async Task<IActionResult> Frequent()=>Ok(await db.Users.OrderByDescending(x=>x.SkyMiles).Take(20).ToListAsync());
    /// <summary>Returns cancellation totals.</summary>
    [HttpGet("cancellations")] public async Task<IActionResult> Cancellations()=>Ok(await db.Bookings.Where(x=>x.BookingStatus==BookingStatus.Cancelled).GroupBy(x=>x.BookingDate.Date).Select(g=>new{Date=g.Key,Count=g.Count(),Refund=g.Sum(x=>x.TotalPrice)}).ToListAsync());
}

[ApiController, Area("Admin"), Authorize(Roles="Admin"), Route("api/admin/cancellation-policies")]
public class AdminCancellationPolicyController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Lists cancellation policies.</summary>
    [HttpGet] public async Task<IActionResult> List()=>Ok(await db.CancellationRules.ToListAsync());
    /// <summary>Creates a cancellation policy.</summary>
    [HttpPost] public async Task<IActionResult> Create(CancellationRule rule){db.CancellationRules.Add(rule);await db.SaveChangesAsync();return Ok(rule);}
    /// <summary>Updates a cancellation policy.</summary>
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,CancellationRule rule){if(id!=rule.CancellationRuleId)return BadRequest();db.Update(rule);await db.SaveChangesAsync();return Ok(rule);}
    /// <summary>Deletes a cancellation policy.</summary>
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id){var r=await db.CancellationRules.FindAsync(id);if(r==null)return NotFound();db.Remove(r);await db.SaveChangesAsync();return NoContent();}
}
