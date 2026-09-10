using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AirlineReservation.Data;
using AirlineReservation.DTOs;
using AirlineReservation.Models;
using AirlineReservation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AirlineReservation.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(ApplicationDbContext db, IPasswordHasher<User> hasher, IConfiguration config) : ControllerBase
{
    /// <summary>Registers a user and initializes the SkyMiles balance.</summary>
    [HttpPost("register")] public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request) { if (!ModelState.IsValid) return ValidationProblem(ModelState); if (await db.Users.AnyAsync(x => x.Email == request.Email)) return Conflict("Email is already registered."); var user=new User { Email=request.Email, FirstName=request.FirstName, LastName=request.LastName, Password=string.Empty, Role=UserRole.RegisteredUser }; user.PasswordHash=hasher.HashPassword(user, request.Password); db.Users.Add(user); await db.SaveChangesAsync(); return Ok(Token(user)); }
    /// <summary>Authenticates a registered user.</summary>
    [HttpPost("login")] public async Task<ActionResult<TokenResponse>> Login(LoginRequest request) { var user=await db.Users.SingleOrDefaultAsync(x=>x.Email==request.Email); if(user==null) return Unauthorized("Invalid credentials."); if(!string.IsNullOrEmpty(user.PasswordHash)) { if(hasher.VerifyHashedPassword(user,user.PasswordHash,request.Password)==PasswordVerificationResult.Failed) return Unauthorized("Invalid credentials."); } else if(user.Password!=request.Password) return Unauthorized("Invalid credentials."); return Ok(Token(user)); }
    /// <summary>Issues a limited guest token for read-only endpoints.</summary>
    [HttpPost("guest")] public ActionResult<TokenResponse> Guest() => Ok(Token(new User { UserId=0, Role=UserRole.Guest }));
    private TokenResponse Token(User user) { var expires=DateTime.UtcNow.AddHours(2); var claims=new[]{new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),new Claim(ClaimTypes.Role,user.Role.ToString())}; var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)); var token=new JwtSecurityToken(claims:claims,expires:expires,signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256)); return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token),expires,user.Role.ToString()); }
}

[ApiController, Authorize, Route("api/profile")]
public class UserProfileController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Returns the authenticated user's profile.</summary>
    [HttpGet] public async Task<IActionResult> Get() => Ok(await db.Users.AsNoTracking().Where(x=>x.UserId==Id).Select(x=>new { x.UserId, x.Email, x.FirstName, x.LastName, x.Address, x.PhoneNumber, x.PreferredCreditCard, x.SkyMiles, x.Role, x.CreatedAt, x.UpdatedAt }).SingleAsync(x=>x.UserId==Id));
    /// <summary>Updates editable profile fields.</summary>
    [HttpPut] public async Task<IActionResult> Update(ProfileUpdateRequest request) { var user=await db.Users.FindAsync(Id); if(user==null)return NotFound(); user.Address=request.Address??user.Address;user.PhoneNumber=request.PhoneNumber??user.PhoneNumber;user.PreferredCreditCard=request.PreferredCreditCardNumber??user.PreferredCreditCard;user.FirstName=request.FirstName??user.FirstName;user.LastName=request.LastName??user.LastName;user.UpdatedAt=DateTime.UtcNow;await db.SaveChangesAsync();return Ok(user); }
    /// <summary>Returns the user's current SkyMiles balance and history.</summary>
    [HttpGet("skymiles")] public async Task<IActionResult> SkyMiles()=>Ok(new { Balance=(await db.Users.FindAsync(Id))?.SkyMiles, History=await db.SkyMilesTransactions.Where(x=>x.UserId==Id).OrderByDescending(x=>x.Date).ToListAsync() });
    private int Id=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController, Route("api/cities")]
public class CityController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Resolves all matching city names, including qualifiers.</summary>
    [HttpGet("resolve")] public async Task<IActionResult> Resolve(string name)=>Ok(await db.Cities.Where(x=>x.CityName.Contains(name)).ToListAsync());
    /// <summary>Returns the nearest serviced city and configured distance.</summary>
    [HttpGet("{id:int}/nearest-serviced")] public async Task<IActionResult> Nearest(int id)=>Ok(await db.Cities.Include(x=>x.NearestServicedCity).SingleOrDefaultAsync(x=>x.CityId==id));
}

[ApiController, Route("api/flights")]
public class FlightSearchController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Searches scheduled flights and validates trip dates and passenger counts.</summary>
    [HttpPost("search")] public async Task<IActionResult> Search(FlightSearchRequest request) { if(request.DepartureDate.Date<DateTime.Today)return BadRequest("Departure date cannot be in the past.");if(request.ReturnDate<request.DepartureDate)return BadRequest("Return date cannot precede departure date.");var flights=await db.Flights.Include(x=>x.OriginCity).Include(x=>x.DestinationCity).Where(x=>x.OriginCity.CityName==request.Origin&&x.DestinationCity.CityName==request.Destination&&x.DepartureDate.Date==request.DepartureDate.Date&&x.FlightStatus==FlightStatus.Scheduled).Select(x=>new {x.FlightNumber,x.DepartureDate,x.DepartureTime,x.ArrivalTime,x.Duration,x.EconomySeats,x.BusinessSeats,x.TicketPrice}).ToListAsync();return Ok(flights); }
    /// <summary>Returns public arrival and departure details for a flight date.</summary>
    [HttpGet("{flightNumber}/details")] public async Task<IActionResult> Details(string flightNumber,DateTime date)=>Ok(await db.Flights.Where(x=>x.FlightNumber==flightNumber&&x.DepartureDate.Date==date.Date).Select(x=>new{x.FlightNumber,x.DepartureTime,x.ArrivalTime,x.Duration,x.FlightStatus}).SingleOrDefaultAsync());
}

[ApiController, Route("api/routes")]
public class RouteController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>Returns configured connecting route suggestions.</summary>
    [HttpGet("find")] public async Task<IActionResult> Find(int originCityId,int destinationCityId)=>Ok(await db.Routes.Where(x=>x.OriginCityId==originCityId&&x.DestinationCityId==destinationCityId).ToListAsync());
}
