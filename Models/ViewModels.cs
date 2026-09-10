using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirlineReservation.Models;

public class FlightSearchViewModel
{
    [Required] public int OriginCityId { get; set; }
    [Required] public int DestinationCityId { get; set; }
    [Required, DataType(DataType.Date)] public DateTime DepartureDate { get; set; }
    [DataType(DataType.Date)] public DateTime? ReturnDate { get; set; }
    [Range(1, 99)] public int Adults { get; set; } = 1;
    [Range(0, 99)] public int Children { get; set; }
    [Range(0, 99)] public int Seniors { get; set; }
    [NotMapped] public int TravelerCount => Adults + Children + Seniors;
}


public class BookingRequest
{
    [Required] public int FlightId { get; set; }
    [Range(1, 99)] public int NumberOfAdults { get; set; } = 1;
    [Range(0, 99)] public int NumberOfChildren { get; set; }
    [Range(0, 99)] public int NumberOfSeniors { get; set; }
    public List<PassengerInput> Passengers { get; set; } = new();
    public string? CreditCardNumber { get; set; }
    [StringLength(150)] public string? EmergencyContactName { get; set; }
    [Phone, StringLength(25)] public string? EmergencyContactPhone { get; set; }
    [NotMapped] public int PassengerCount => NumberOfAdults + NumberOfChildren + NumberOfSeniors;
}
public class PassengerInput
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string PassengerType { get; set; } = "Adult";
    public string? PassportNumber { get; set; }
}

public class RegisterViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
}

public class ProfileEditViewModel
{
    public int UserId { get; set; }
    [Required, StringLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(50)] public string LastName { get; set; } = string.Empty;
    [EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Phone, StringLength(25)] public string? PhoneNumber { get; set; }
    [StringLength(250)] public string? Address { get; set; }
}

public class RescheduleViewModel
{
    public int BookingId { get; set; }
    public string? Reference { get; set; }
    public string? Route { get; set; }
    public DateTime CurrentDeparture { get; set; }
    public decimal CurrentTotal { get; set; }
    public List<AirlineReservation.Models.Flight> AlternativeFlights { get; set; } = new();
}

public class UserWithStats
{
    public User User { get; set; } = null!;
    public int ActiveBookings { get; set; }
}

public class FlightStatusViewModel
{
    [StringLength(20)] public string? FlightNumber { get; set; }
    [DataType(DataType.Date)] public DateTime? FlightDate { get; set; }
    public Flight? Flight { get; set; }
    [NotMapped] public string Indicator => Flight?.FlightStatus switch
    {
        Models.FlightStatus.Delayed => "Delayed",
        Models.FlightStatus.Cancelled => "Cancelled",
        Models.FlightStatus.Completed => "Arrived",
        _ => "On Time"
    };
}
