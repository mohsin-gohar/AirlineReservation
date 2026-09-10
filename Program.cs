using AirlineReservation.Data;
using AirlineReservation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; });
builder.Services.AddScoped<ReservationService>();
builder.Services.AddHostedService<ReservationBackgroundService>();
builder.Services.AddHostedService<DbInitializer>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<AirlineReservation.Models.User>, Microsoft.AspNetCore.Identity.PasswordHasher<AirlineReservation.Models.User>>();
var jwtKey = builder.Configuration["Jwt:Key"] ?? "development-only-change-this-key-to-a-long-random-value";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = true, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) });
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseSession(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();