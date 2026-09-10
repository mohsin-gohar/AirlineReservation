using AirlineReservation.Data;
using AirlineReservation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Services;

/// <summary>
/// Applies pending EF Core migrations at startup and upgrades the seeded demo
/// account to a hashed password so the JWT API can authenticate it as well.
/// Both operations are idempotent and safe to run on every boot.
/// </summary>
public class DbInitializer(IServiceScopeFactory scopes, ILogger<DbInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync(stoppingToken);
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var demo = await db.Users.FirstOrDefaultAsync(x => x.Email == "demo@example.com", stoppingToken);
            if (demo is not null && string.IsNullOrEmpty(demo.PasswordHash))
            {
                // The HasData seed ships a plain-text demo password; hash it once so both
                // the MVC session login and the JWT API accept the same credentials.
                demo.PasswordHash = hasher.HashPassword(demo, demo.Password);
                demo.Password = string.Empty;
                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Demo account password was hashed for secure login.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Database initialization failed; the app will start but database-backed pages may error.");
        }
    }
}
