using AirlineReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Static seed timestamp keeps the HasData model deterministic between builds (see PendingModelChangesWarning).
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public DbSet<User> Users => Set<User>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<CancellationRule> CancellationRules => Set<CancellationRule>();
    public DbSet<Airport> Airports => Set<Airport>();
    public DbSet<FlightSchedule> FlightSchedules => Set<FlightSchedule>();
    public DbSet<SeatClass> SeatClasses => Set<SeatClass>();
    public DbSet<FlightFare> FlightFares => Set<FlightFare>();
    public DbSet<AirlineReservation.Models.Route> Routes => Set<AirlineReservation.Models.Route>();
    public DbSet<Itinerary> Itineraries => Set<Itinerary>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SkyMilesTransaction> SkyMilesTransactions => Set<SkyMilesTransaction>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<City>().HasIndex(x => x.CityName).IsUnique();
        modelBuilder.Entity<Flight>().HasIndex(x => x.FlightNumber).IsUnique();
        modelBuilder.Entity<Booking>().HasIndex(x => x.BlockingNumber).IsUnique().HasFilter("[BlockingNumber] IS NOT NULL");
        modelBuilder.Entity<Booking>().HasIndex(x => x.ConfirmationNumber).IsUnique().HasFilter("[ConfirmationNumber] IS NOT NULL");
        modelBuilder.Entity<Flight>().Property(x => x.TicketPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Booking>().Property(x => x.TotalPrice).HasPrecision(18, 2);

        modelBuilder.Entity<Flight>().HasOne(x => x.OriginCity).WithMany(x => x.OriginFlights).HasForeignKey(x => x.OriginCityId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Flight>().HasOne(x => x.DestinationCity).WithMany(x => x.DestinationFlights).HasForeignKey(x => x.DestinationCityId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Booking>().HasOne(x => x.User).WithMany(x => x.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Booking>().HasOne(x => x.Flight).WithMany(x => x.Bookings).HasForeignKey(x => x.FlightId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Passenger>().HasOne(x => x.Booking).WithMany(x => x.Passengers).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<City>().HasOne(x => x.NearestServicedCity).WithMany(x => x.NearbyCities).HasForeignKey(x => x.NearestServicedCityId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Airport>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Airport>().HasOne(x => x.City).WithMany(x => x.Airports).HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Flight>().HasOne(x => x.OriginAirport).WithMany(x => x.OriginFlights).HasForeignKey(x => x.OriginAirportId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Flight>().HasOne(x => x.DestinationAirport).WithMany(x => x.DestinationFlights).HasForeignKey(x => x.DestinationAirportId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<FlightSchedule>().HasOne(x => x.Flight).WithMany(x => x.Schedules).HasForeignKey(x => x.FlightId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FlightFare>().HasOne(x => x.FlightSchedule).WithMany(x => x.Fares).HasForeignKey(x => x.FlightScheduleId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FlightFare>().HasOne(x => x.SeatClass).WithMany(x => x.Fares).HasForeignKey(x => x.SeatClassId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Itinerary>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Booking>().HasOne(x => x.Itinerary).WithMany(x => x.Bookings).HasForeignKey(x => x.ItineraryId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Payment>().HasOne(x => x.Booking).WithMany(x => x.Payments).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Notification>().HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Notification>().HasOne(x => x.Booking).WithMany(x => x.Notifications).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SkyMilesTransaction>().HasOne(x => x.User).WithMany(x => x.SkyMilesTransactions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<FlightFare>().Property(x => x.BasePrice).HasPrecision(18, 2);
        modelBuilder.Entity<FlightFare>().Property(x => x.CurrentPrice).HasPrecision(18, 2);

        modelBuilder.Entity<CancellationRule>().Property(x => x.RefundPercentage).HasPrecision(5, 2);
        modelBuilder.Entity<City>().Property(x => x.DistanceToNearestServicedCity).HasPrecision(18, 2);
        modelBuilder.Entity<AirlineReservation.Models.Route>().Property(x => x.OriginCityId).IsRequired();
        modelBuilder.Entity<AirlineReservation.Models.Route>().Property(x => x.DestinationCityId).IsRequired();
        modelBuilder.Entity<AirlineReservation.Models.Route>().Property(x => x.IntermediateStops).HasMaxLength(500);
        modelBuilder.Entity<AirlineReservation.Models.Route>().Property(x => x.FlightIds).HasMaxLength(500);
        modelBuilder.Entity<AirlineReservation.Models.Route>().Property(x => x.TotalDuration).HasMaxLength(50);
        modelBuilder.Entity<AirlineReservation.Models.Route>().HasIndex(x => new { x.OriginCityId, x.DestinationCityId }).IsUnique();
        modelBuilder.Entity<AirlineReservation.Models.Route>().HasOne<City>().WithMany().HasForeignKey(x => x.OriginCityId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AirlineReservation.Models.Route>().HasOne<City>().WithMany().HasForeignKey(x => x.DestinationCityId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Booking>().Property(x => x.DepartureDate).IsRequired();
        modelBuilder.Entity<Booking>().HasIndex(x => x.UserId);
        modelBuilder.Entity<Booking>().HasIndex(x => x.FlightId);
        modelBuilder.Entity<Passenger>().Property(x => x.PassengerType).HasMaxLength(30);

        modelBuilder.Entity<NewsletterSubscription>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<NewsletterSubscription>().HasKey(x => x.SubscriptionId);
        modelBuilder.Entity<NewsletterSubscription>().Property(x => x.SubscribedAt).IsRequired();

        modelBuilder.Entity<City>().HasData(
            new City { CityId = 1, CityName = "New York" }, new City { CityId = 2, CityName = "Los Angeles" },
            new City { CityId = 3, CityName = "Chicago" }, new City { CityId = 4, CityName = "Miami" });
        modelBuilder.Entity<User>().HasData(new User { UserId = 1, Email = "demo@example.com", Password = "ChangeMe123!", FirstName = "Demo", LastName = "Traveler", SkyMiles = 2500, Role = UserRole.Admin, CreatedAt = SeedDate, UpdatedAt = SeedDate });
        modelBuilder.Entity<Airport>().HasData(
            new Airport { AirportId = 1, Code = "JFK", Name = "John F. Kennedy International", CityId = 1 },
            new Airport { AirportId = 2, Code = "LAX", Name = "Los Angeles International", CityId = 2 },
            new Airport { AirportId = 3, Code = "ORD", Name = "O'Hare International", CityId = 3 },
            new Airport { AirportId = 4, Code = "MIA", Name = "Miami International", CityId = 4 });
        modelBuilder.Entity<Flight>().HasData(
            new Flight { FlightId = 1, FlightNumber = "AR101", OriginCityId = 1, DestinationCityId = 2, OriginAirportId = 1, DestinationAirportId = 2, DepartureDate = new DateTime(2030, 6, 15), DepartureTime = new TimeSpan(8, 0, 0), ArrivalTime = new TimeSpan(11, 15, 0), Duration = "6h 15m", EconomySeats = 120, BusinessSeats = 20, TicketPrice = 329.99m, FlightStatus = FlightStatus.Scheduled },
            new Flight { FlightId = 2, FlightNumber = "AR202", OriginCityId = 2, DestinationCityId = 4, OriginAirportId = 2, DestinationAirportId = 4, DepartureDate = new DateTime(2030, 7, 20), DepartureTime = new TimeSpan(9, 30, 0), ArrivalTime = new TimeSpan(17, 0, 0), Duration = "4h 30m", EconomySeats = 150, BusinessSeats = 16, TicketPrice = 279.99m, FlightStatus = FlightStatus.Scheduled },
            new Flight { FlightId = 3, FlightNumber = "AR303", OriginCityId = 1, DestinationCityId = 3, OriginAirportId = 1, DestinationAirportId = 3, DepartureDate = new DateTime(2026, 9, 25), DepartureTime = new TimeSpan(7, 30, 0), ArrivalTime = new TimeSpan(9, 5, 0), Duration = "1h 35m", EconomySeats = 140, BusinessSeats = 12, TicketPrice = 189.50m, FlightStatus = FlightStatus.Scheduled },
            new Flight { FlightId = 4, FlightNumber = "AR404", OriginCityId = 3, DestinationCityId = 1, OriginAirportId = 3, DestinationAirportId = 1, DepartureDate = new DateTime(2026, 9, 26), DepartureTime = new TimeSpan(18, 0, 0), ArrivalTime = new TimeSpan(19, 40, 0), Duration = "1h 40m", EconomySeats = 140, BusinessSeats = 12, TicketPrice = 179.00m, FlightStatus = FlightStatus.Scheduled },
            new Flight { FlightId = 5, FlightNumber = "AR505", OriginCityId = 4, DestinationCityId = 1, OriginAirportId = 4, DestinationAirportId = 1, DepartureDate = new DateTime(2026, 10, 2), DepartureTime = new TimeSpan(6, 45, 0), ArrivalTime = new TimeSpan(9, 45, 0), Duration = "3h 00m", EconomySeats = 160, BusinessSeats = 18, TicketPrice = 215.75m, FlightStatus = FlightStatus.Scheduled },
            new Flight { FlightId = 6, FlightNumber = "AR606", OriginCityId = 1, DestinationCityId = 4, OriginAirportId = 1, DestinationAirportId = 4, DepartureDate = new DateTime(2026, 10, 5), DepartureTime = new TimeSpan(10, 15, 0), ArrivalTime = new TimeSpan(13, 30, 0), Duration = "3h 15m", EconomySeats = 160, BusinessSeats = 18, TicketPrice = 229.99m, FlightStatus = FlightStatus.Scheduled });
        modelBuilder.Entity<SeatClass>().HasData(
            new SeatClass { SeatClassId = 1, Name = "Economy" },
            new SeatClass { SeatClassId = 2, Name = "Business" });
        modelBuilder.Entity<FlightSchedule>().HasData(
            new FlightSchedule { FlightScheduleId = 1, FlightId = 1, DepartureDate = new DateTime(2030, 6, 15), AvailableEconomySeats = 120, AvailableBusinessSeats = 20, Status = FlightStatus.Scheduled },
            new FlightSchedule { FlightScheduleId = 2, FlightId = 2, DepartureDate = new DateTime(2030, 7, 20), AvailableEconomySeats = 150, AvailableBusinessSeats = 16, Status = FlightStatus.Scheduled },
            new FlightSchedule { FlightScheduleId = 3, FlightId = 3, DepartureDate = new DateTime(2026, 9, 25), AvailableEconomySeats = 140, AvailableBusinessSeats = 12, Status = FlightStatus.Scheduled },
            new FlightSchedule { FlightScheduleId = 4, FlightId = 4, DepartureDate = new DateTime(2026, 9, 26), AvailableEconomySeats = 140, AvailableBusinessSeats = 12, Status = FlightStatus.Scheduled },
            new FlightSchedule { FlightScheduleId = 5, FlightId = 5, DepartureDate = new DateTime(2026, 10, 2), AvailableEconomySeats = 160, AvailableBusinessSeats = 18, Status = FlightStatus.Scheduled },
            new FlightSchedule { FlightScheduleId = 6, FlightId = 6, DepartureDate = new DateTime(2026, 10, 5), AvailableEconomySeats = 160, AvailableBusinessSeats = 18, Status = FlightStatus.Scheduled });
        modelBuilder.Entity<AirlineReservation.Models.Route>().HasData(
            new AirlineReservation.Models.Route { RouteId = 1, OriginCityId = 1, DestinationCityId = 4, IntermediateStops = "Los Angeles", FlightIds = "1,2", TotalDuration = "3h 15m + 4h 30m (connection)" });
        modelBuilder.Entity<FlightFare>().HasData(
            new FlightFare { FlightFareId = 1, FlightScheduleId = 1, SeatClassId = 1, BasePrice = 329.99m, CurrentPrice = 329.99m },
            new FlightFare { FlightFareId = 2, FlightScheduleId = 1, SeatClassId = 2, BasePrice = 649.99m, CurrentPrice = 649.99m },
            new FlightFare { FlightFareId = 3, FlightScheduleId = 2, SeatClassId = 1, BasePrice = 279.99m, CurrentPrice = 279.99m },
            new FlightFare { FlightFareId = 4, FlightScheduleId = 2, SeatClassId = 2, BasePrice = 549.99m, CurrentPrice = 549.99m },
            new FlightFare { FlightFareId = 5, FlightScheduleId = 3, SeatClassId = 1, BasePrice = 189.50m, CurrentPrice = 189.50m },
            new FlightFare { FlightFareId = 6, FlightScheduleId = 3, SeatClassId = 2, BasePrice = 379.00m, CurrentPrice = 379.00m },
            new FlightFare { FlightFareId = 7, FlightScheduleId = 4, SeatClassId = 1, BasePrice = 179.00m, CurrentPrice = 179.00m },
            new FlightFare { FlightFareId = 8, FlightScheduleId = 4, SeatClassId = 2, BasePrice = 359.00m, CurrentPrice = 359.00m },
            new FlightFare { FlightFareId = 9, FlightScheduleId = 5, SeatClassId = 1, BasePrice = 215.75m, CurrentPrice = 215.75m },
            new FlightFare { FlightFareId = 10, FlightScheduleId = 5, SeatClassId = 2, BasePrice = 429.00m, CurrentPrice = 429.00m },
            new FlightFare { FlightFareId = 11, FlightScheduleId = 6, SeatClassId = 1, BasePrice = 229.99m, CurrentPrice = 229.99m },
            new FlightFare { FlightFareId = 12, FlightScheduleId = 6, SeatClassId = 2, BasePrice = 459.00m, CurrentPrice = 459.00m });
        modelBuilder.Entity<CancellationRule>().HasData(
            new CancellationRule { CancellationRuleId = 1, MinimumDaysBeforeDeparture = 14, MaximumDaysBeforeDeparture = null, RefundPercentage = 100 },
            new CancellationRule { CancellationRuleId = 2, MinimumDaysBeforeDeparture = 7, MaximumDaysBeforeDeparture = 13, RefundPercentage = 75 },
            new CancellationRule { CancellationRuleId = 3, MinimumDaysBeforeDeparture = 0, MaximumDaysBeforeDeparture = 6, RefundPercentage = 25 });
    }
}
