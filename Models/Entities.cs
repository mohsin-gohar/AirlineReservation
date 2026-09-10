using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineReservation.Models;

public enum BookingType { Blocked, Confirmed }
public enum BookingStatus { Active, Cancelled, Rescheduled }
public enum FlightStatus { Scheduled, Delayed, Cancelled, Completed }
public enum UserRole { Guest, RegisteredUser, Admin, Clerk }
public enum TripType { OneWay, RoundTrip }
public enum PassengerType { Adult, Child, Senior }
public enum PaymentTransactionType { Charge, Refund }
public enum PaymentStatus { Pending, Completed, Failed }
public enum NotificationType { BlockingReminder, ScheduleChange, ConfirmationNotice }

public class User
{
    public int UserId { get; set; }
    [StringLength(50)] public string? LoginId { get; set; }
    [StringLength(200)] public string? PasswordHash { get; set; }
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, StringLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(50)] public string LastName { get; set; } = string.Empty;
    [StringLength(250)] public string? Address { get; set; }
    [Phone, StringLength(25)] public string? PhoneNumber { get; set; }
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [StringLength(20)] public string? Gender { get; set; }
    [Range(0, 120)] public int? Age { get; set; }
    [StringLength(50)] public string? PreferredCreditCard { get; set; }
    [Range(0, int.MaxValue)] public int SkyMiles { get; set; }
    public UserRole Role { get; set; } = UserRole.RegisteredUser;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SkyMilesTransaction> SkyMilesTransactions { get; set; } = new List<SkyMilesTransaction>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class City
{
    public int CityId { get; set; }
    [Required, StringLength(100)] public string CityName { get; set; } = string.Empty;
    [StringLength(100)] public string? StateOrRegion { get; set; }
    [StringLength(100)] public string? Country { get; set; }
    public int? NearestServicedCityId { get; set; }
    public decimal? DistanceToNearestServicedCity { get; set; }
    public City? NearestServicedCity { get; set; }
    public ICollection<City> NearbyCities { get; set; } = new List<City>();
    public ICollection<Airport> Airports { get; set; } = new List<Airport>();
    public ICollection<Flight> OriginFlights { get; set; } = new List<Flight>();
    public ICollection<Flight> DestinationFlights { get; set; } = new List<Flight>();
}

public class Flight
{
    public int FlightId { get; set; }
    [Required, StringLength(20)] public string FlightNumber { get; set; } = string.Empty;
    public int OriginCityId { get; set; }
    public int DestinationCityId { get; set; }
    public int? OriginAirportId { get; set; }
    public int? DestinationAirportId { get; set; }
    [Required] public DateTime DepartureDate { get; set; }
    [Required] public TimeSpan DepartureTime { get; set; }
    [Required] public TimeSpan ArrivalTime { get; set; }
    [Required, StringLength(30)] public string Duration { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int EconomySeats { get; set; }
    [Range(0, int.MaxValue)] public int BusinessSeats { get; set; }
    [Range(0, 1000000)] public decimal TicketPrice { get; set; }
    public FlightStatus FlightStatus { get; set; } = FlightStatus.Scheduled;
    public City OriginCity { get; set; } = null!;
    public City DestinationCity { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    [NotMapped] public DateTime Departure => DepartureDate.Date + DepartureTime;
    public Airport? OriginAirport { get; set; }
    public Airport? DestinationAirport { get; set; }
    public ICollection<FlightSchedule> Schedules { get; set; } = new List<FlightSchedule>();
}

public class Booking
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int FlightId { get; set; }
    public int? ItineraryId { get; set; }
    public BookingType BookingType { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Active;
    [StringLength(30)] public string? BlockingNumber { get; set; }
    [StringLength(30)] public string? ConfirmationNumber { get; set; }
    [StringLength(30)] public string? CancellationNumber { get; set; }
    [Range(0, 99)] public int NumberOfAdults { get; set; }
    [Range(0, 99)] public int NumberOfChildren { get; set; }
    [Range(0, 99)] public int NumberOfSeniors { get; set; }
    [Range(0, 1000000)] public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public DateTime DepartureDate { get; set; }
    [StringLength(150)] public string? EmergencyContactName { get; set; }
    [StringLength(25)] public string? EmergencyContactPhone { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public Flight Flight { get; set; } = null!;
    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    public Itinerary? Itinerary { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    [NotMapped] public int PassengerCount => NumberOfAdults + NumberOfChildren + NumberOfSeniors;
    [NotMapped] public bool IsCheckedIn => CheckedInAt != null;
}

public class Airport
{
    public int AirportId { get; set; }
    [Required, StringLength(3, MinimumLength = 3)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    public int CityId { get; set; }
    public City City { get; set; } = null!;
    public ICollection<Flight> OriginFlights { get; set; } = new List<Flight>();
    public ICollection<Flight> DestinationFlights { get; set; } = new List<Flight>();
}

public class FlightSchedule
{
    public int FlightScheduleId { get; set; }
    public int FlightId { get; set; }
    public DateTime DepartureDate { get; set; }
    public int AvailableEconomySeats { get; set; }
    public int AvailableBusinessSeats { get; set; }
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public Flight Flight { get; set; } = null!;
    public ICollection<FlightFare> Fares { get; set; } = new List<FlightFare>();
}

public class SeatClass
{
    public int SeatClassId { get; set; }
    [Required, StringLength(40)] public string Name { get; set; } = string.Empty;
    public ICollection<FlightFare> Fares { get; set; } = new List<FlightFare>();
}

public class FlightFare
{
    public int FlightFareId { get; set; }
    public int FlightScheduleId { get; set; }
    public int SeatClassId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public FlightSchedule FlightSchedule { get; set; } = null!;
    public SeatClass SeatClass { get; set; } = null!;
}

public class Route
{
    public int RouteId { get; set; }
    public int OriginCityId { get; set; }
    public int DestinationCityId { get; set; }
    public string IntermediateStops { get; set; } = string.Empty;
    public string FlightIds { get; set; } = string.Empty;
    public string TotalDuration { get; set; } = string.Empty;
}

public class Itinerary
{
    public int ItineraryId { get; set; }
    public int? UserId { get; set; }
    public TripType TripType { get; set; }
    public string OnwardLegIds { get; set; } = string.Empty;
    public string? ReturnLegIds { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Seniors { get; set; }
    public int SeatClassId { get; set; }
    public User? User { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public string CreditCardNumberMasked { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentTransactionType TransactionType { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public Booking Booking { get; set; } = null!;
}

public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public int BookingId { get; set; }
    public NotificationType Type { get; set; }
    public DateTime ScheduledSendDate { get; set; }
    public bool Sent { get; set; }
    public DateTime? SentAt { get; set; }
    public User User { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
}

public class SkyMilesTransaction
{
    public int SkyMilesTransactionId { get; set; }
    public int UserId { get; set; }
    public int? BookingId { get; set; }
    public int MilesChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public Booking? Booking { get; set; }
}

public class Passenger
{
    public int PassengerId { get; set; }
    public int BookingId { get; set; }
    [Required, StringLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(50)] public string LastName { get; set; } = string.Empty;
    [Required, StringLength(30)] public string PassengerType { get; set; } = "Adult";
    [StringLength(30)] public string? PassportNumber { get; set; }
    public Booking Booking { get; set; } = null!;
}

public class CancellationRule
{
    public int CancellationRuleId { get; set; }
    [Required] public int MinimumDaysBeforeDeparture { get; set; }
    public int? MaximumDaysBeforeDeparture { get; set; }
    [Range(0, 100)] public decimal RefundPercentage { get; set; }
}

public class NewsletterSubscription
{
    [Key]
    public int SubscriptionId { get; set; }
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;
}
