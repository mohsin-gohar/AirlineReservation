using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.EntityFrameworkCore;
namespace AirlineReservation.Services;
public class ReservationBackgroundService(IServiceScopeFactory scopes, ILogger<ReservationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var now=DateTime.UtcNow;
            // Flight.Departure is [NotMapped], so it cannot run inside a SQL query — evaluate it in memory.
            var candidates=await db.Bookings.Include(x=>x.Flight).Where(x=>x.BookingType==BookingType.Blocked&&x.BookingStatus==BookingStatus.Active).ToListAsync(stoppingToken);
            var blocked=candidates.Where(x=>x.Flight.Departure<=now.AddDays(14)).ToList();
            foreach(var b in blocked){b.BookingStatus=BookingStatus.Cancelled;b.CancellationNumber=$"CAN-{DateTime.UtcNow:HHmmssfff}";b.UpdatedAt=now;b.Flight.EconomySeats+=b.NumberOfAdults+b.NumberOfChildren+b.NumberOfSeniors;}
            var pending=await db.Notifications.Where(x=>!x.Sent&&x.ScheduledSendDate<=now).ToListAsync(stoppingToken);foreach(var n in pending){n.Sent=true;n.SentAt=now;}await db.SaveChangesAsync(stoppingToken); } catch(Exception e){logger.LogError(e,"Reservation background processing failed.");} await Task.Delay(TimeSpan.FromMinutes(15),stoppingToken);
        }
    }
}
