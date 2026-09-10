using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineReservation.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CancellationRules",
                columns: table => new
                {
                    CancellationRuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinimumDaysBeforeDeparture = table.Column<int>(type: "int", nullable: false),
                    MaximumDaysBeforeDeparture = table.Column<int>(type: "int", nullable: true),
                    RefundPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationRules", x => x.CancellationRuleId);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StateOrRegion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NearestServicedCityId = table.Column<int>(type: "int", nullable: true),
                    DistanceToNearestServicedCity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityId);
                    table.ForeignKey(
                        name: "FK_Cities_Cities_NearestServicedCityId",
                        column: x => x.NearestServicedCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeatClasses",
                columns: table => new
                {
                    SeatClassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatClasses", x => x.SeatClassId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoginId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    PreferredCreditCard = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SkyMiles = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    AirportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.AirportId);
                    table.ForeignKey(
                        name: "FK_Airports_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginCityId = table.Column<int>(type: "int", nullable: false),
                    DestinationCityId = table.Column<int>(type: "int", nullable: false),
                    IntermediateStops = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FlightIds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TotalDuration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Routes_Cities_DestinationCityId",
                        column: x => x.DestinationCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routes_Cities_OriginCityId",
                        column: x => x.OriginCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Itineraries",
                columns: table => new
                {
                    ItineraryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TripType = table.Column<int>(type: "int", nullable: false),
                    OnwardLegIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnLegIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adults = table.Column<int>(type: "int", nullable: false),
                    Children = table.Column<int>(type: "int", nullable: false),
                    Seniors = table.Column<int>(type: "int", nullable: false),
                    SeatClassId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraries", x => x.ItineraryId);
                    table.ForeignKey(
                        name: "FK_Itineraries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SkyMilesTransactions",
                columns: table => new
                {
                    SkyMilesTransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    MilesChange = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkyMilesTransactions", x => x.SkyMilesTransactionId);
                    table.ForeignKey(
                        name: "FK_SkyMilesTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    FlightId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OriginCityId = table.Column<int>(type: "int", nullable: false),
                    DestinationCityId = table.Column<int>(type: "int", nullable: false),
                    OriginAirportId = table.Column<int>(type: "int", nullable: true),
                    DestinationAirportId = table.Column<int>(type: "int", nullable: true),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EconomySeats = table.Column<int>(type: "int", nullable: false),
                    BusinessSeats = table.Column<int>(type: "int", nullable: false),
                    TicketPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FlightStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.FlightId);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DestinationAirportId",
                        column: x => x.DestinationAirportId,
                        principalTable: "Airports",
                        principalColumn: "AirportId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_OriginAirportId",
                        column: x => x.OriginAirportId,
                        principalTable: "Airports",
                        principalColumn: "AirportId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Cities_DestinationCityId",
                        column: x => x.DestinationCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Cities_OriginCityId",
                        column: x => x.OriginCityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FlightId = table.Column<int>(type: "int", nullable: false),
                    ItineraryId = table.Column<int>(type: "int", nullable: true),
                    BookingType = table.Column<int>(type: "int", nullable: false),
                    BookingStatus = table.Column<int>(type: "int", nullable: false),
                    BlockingNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CancellationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NumberOfAdults = table.Column<int>(type: "int", nullable: false),
                    NumberOfChildren = table.Column<int>(type: "int", nullable: false),
                    NumberOfSeniors = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "FlightId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "ItineraryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightSchedules",
                columns: table => new
                {
                    FlightScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightId = table.Column<int>(type: "int", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableEconomySeats = table.Column<int>(type: "int", nullable: false),
                    AvailableBusinessSeats = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSchedules", x => x.FlightScheduleId);
                    table.ForeignKey(
                        name: "FK_FlightSchedules_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "FlightId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ScheduledSendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sent = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Passengers",
                columns: table => new
                {
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PassengerType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passengers", x => x.PassengerId);
                    table.ForeignKey(
                        name: "FK_Passengers_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CreditCardNumberMasked = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlightFares",
                columns: table => new
                {
                    FlightFareId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightScheduleId = table.Column<int>(type: "int", nullable: false),
                    SeatClassId = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightFares", x => x.FlightFareId);
                    table.ForeignKey(
                        name: "FK_FlightFares_FlightSchedules_FlightScheduleId",
                        column: x => x.FlightScheduleId,
                        principalTable: "FlightSchedules",
                        principalColumn: "FlightScheduleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightFares_SeatClasses_SeatClassId",
                        column: x => x.SeatClassId,
                        principalTable: "SeatClasses",
                        principalColumn: "SeatClassId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CancellationRules",
                columns: new[] { "CancellationRuleId", "MaximumDaysBeforeDeparture", "MinimumDaysBeforeDeparture", "RefundPercentage" },
                values: new object[,]
                {
                    { 1, null, 14, 100m },
                    { 2, 13, 7, 75m },
                    { 3, 6, 0, 25m }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "CityId", "CityName", "Country", "DistanceToNearestServicedCity", "NearestServicedCityId", "StateOrRegion" },
                values: new object[,]
                {
                    { 1, "New York", null, null, null, null },
                    { 2, "Los Angeles", null, null, null, null },
                    { 3, "Chicago", null, null, null, null },
                    { 4, "Miami", null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "Age", "CreatedAt", "Email", "FirstName", "Gender", "LastName", "LoginId", "Password", "PasswordHash", "PhoneNumber", "PreferredCreditCard", "Role", "SkyMiles", "UpdatedAt" },
                values: new object[] { 1, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@example.com", "Demo", null, "Traveler", null, "ChangeMe123!", null, null, null, 2, 2500, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "FlightId", "ArrivalTime", "BusinessSeats", "DepartureDate", "DepartureTime", "DestinationAirportId", "DestinationCityId", "Duration", "EconomySeats", "FlightNumber", "FlightStatus", "OriginAirportId", "OriginCityId", "TicketPrice" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 11, 15, 0, 0), 20, new DateTime(2030, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0), null, 2, "6h 15m", 120, "AR101", 0, null, 1, 329.99m },
                    { 2, new TimeSpan(0, 17, 0, 0, 0), 16, new DateTime(2030, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 9, 30, 0, 0), null, 4, "4h 30m", 150, "AR202", 0, null, 2, 279.99m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airports_CityId",
                table: "Airports",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_Code",
                table: "Airports",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BlockingNumber",
                table: "Bookings",
                column: "BlockingNumber",
                unique: true,
                filter: "[BlockingNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConfirmationNumber",
                table: "Bookings",
                column: "ConfirmationNumber",
                unique: true,
                filter: "[ConfirmationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FlightId",
                table: "Bookings",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ItineraryId",
                table: "Bookings",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CityName",
                table: "Cities",
                column: "CityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_NearestServicedCityId",
                table: "Cities",
                column: "NearestServicedCityId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightFares_FlightScheduleId",
                table: "FlightFares",
                column: "FlightScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightFares_SeatClassId",
                table: "FlightFares",
                column: "SeatClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationAirportId",
                table: "Flights",
                column: "DestinationAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationCityId",
                table: "Flights",
                column: "DestinationCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights",
                column: "FlightNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginAirportId",
                table: "Flights",
                column: "OriginAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginCityId",
                table: "Flights",
                column: "OriginCityId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightSchedules_FlightId",
                table: "FlightSchedules",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_UserId",
                table: "Itineraries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BookingId",
                table: "Notifications",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_BookingId",
                table: "Passengers",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DestinationCityId",
                table: "Routes",
                column: "DestinationCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_OriginCityId_DestinationCityId",
                table: "Routes",
                columns: new[] { "OriginCityId", "DestinationCityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkyMilesTransactions_UserId",
                table: "SkyMilesTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancellationRules");

            migrationBuilder.DropTable(
                name: "FlightFares");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Passengers");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "SkyMilesTransactions");

            migrationBuilder.DropTable(
                name: "FlightSchedules");

            migrationBuilder.DropTable(
                name: "SeatClasses");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Itineraries");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
