using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineReservation.Migrations
{
    /// <inheritdoc />
    public partial class SeedReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Airports",
                columns: new[] { "AirportId", "CityId", "Code", "Name" },
                values: new object[,]
                {
                    { 1, 1, "JFK", "John F. Kennedy International" },
                    { 2, 2, "LAX", "Los Angeles International" },
                    { 3, 3, "ORD", "O'Hare International" },
                    { 4, 4, "MIA", "Miami International" }
                });

            migrationBuilder.InsertData(
                table: "FlightSchedules",
                columns: new[] { "FlightScheduleId", "AvailableBusinessSeats", "AvailableEconomySeats", "DepartureDate", "FlightId", "Status" },
                values: new object[,]
                {
                    { 1, 20, 120, new DateTime(2030, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0 },
                    { 2, 16, 150, new DateTime(2030, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 0 }
                });

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 1,
                columns: new[] { "DestinationAirportId", "OriginAirportId" },
                values: new object[] { 2, 1 });

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 2,
                columns: new[] { "DestinationAirportId", "OriginAirportId" },
                values: new object[] { 4, 2 });

            migrationBuilder.InsertData(
                table: "Routes",
                columns: new[] { "RouteId", "DestinationCityId", "FlightIds", "IntermediateStops", "OriginCityId", "TotalDuration" },
                values: new object[] { 1, 4, "1,2", "Los Angeles", 1, "3h 15m + 4h 30m (connection)" });

            migrationBuilder.InsertData(
                table: "SeatClasses",
                columns: new[] { "SeatClassId", "Name" },
                values: new object[,]
                {
                    { 1, "Economy" },
                    { 2, "Business" }
                });

            migrationBuilder.InsertData(
                table: "FlightFares",
                columns: new[] { "FlightFareId", "BasePrice", "CurrentPrice", "FlightScheduleId", "SeatClassId" },
                values: new object[,]
                {
                    { 1, 329.99m, 329.99m, 1, 1 },
                    { 2, 649.99m, 649.99m, 1, 2 },
                    { 3, 279.99m, 279.99m, 2, 1 },
                    { 4, 549.99m, 549.99m, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "FlightId", "ArrivalTime", "BusinessSeats", "DepartureDate", "DepartureTime", "DestinationAirportId", "DestinationCityId", "Duration", "EconomySeats", "FlightNumber", "FlightStatus", "OriginAirportId", "OriginCityId", "TicketPrice" },
                values: new object[,]
                {
                    { 3, new TimeSpan(0, 9, 5, 0, 0), 12, new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 7, 30, 0, 0), 3, 3, "1h 35m", 140, "AR303", 0, 1, 1, 189.50m },
                    { 4, new TimeSpan(0, 19, 40, 0, 0), 12, new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 18, 0, 0, 0), 1, 1, "1h 40m", 140, "AR404", 0, 3, 3, 179.00m },
                    { 5, new TimeSpan(0, 9, 45, 0, 0), 18, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 6, 45, 0, 0), 1, 1, "3h 00m", 160, "AR505", 0, 4, 4, 215.75m },
                    { 6, new TimeSpan(0, 13, 30, 0, 0), 18, new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 10, 15, 0, 0), 4, 4, "3h 15m", 160, "AR606", 0, 1, 1, 229.99m }
                });

            migrationBuilder.InsertData(
                table: "FlightSchedules",
                columns: new[] { "FlightScheduleId", "AvailableBusinessSeats", "AvailableEconomySeats", "DepartureDate", "FlightId", "Status" },
                values: new object[,]
                {
                    { 3, 12, 140, new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 0 },
                    { 4, 12, 140, new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 0 },
                    { 5, 18, 160, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 0 },
                    { 6, 18, 160, new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 0 }
                });

            migrationBuilder.InsertData(
                table: "FlightFares",
                columns: new[] { "FlightFareId", "BasePrice", "CurrentPrice", "FlightScheduleId", "SeatClassId" },
                values: new object[,]
                {
                    { 5, 189.50m, 189.50m, 3, 1 },
                    { 6, 379.00m, 379.00m, 3, 2 },
                    { 7, 179.00m, 179.00m, 4, 1 },
                    { 8, 359.00m, 359.00m, 4, 2 },
                    { 9, 215.75m, 215.75m, 5, 1 },
                    { 10, 429.00m, 429.00m, 5, 2 },
                    { 11, 229.99m, 229.99m, 6, 1 },
                    { 12, 459.00m, 459.00m, 6, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkyMilesTransactions_BookingId",
                table: "SkyMilesTransactions",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkyMilesTransactions_Bookings_BookingId",
                table: "SkyMilesTransactions",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkyMilesTransactions_Bookings_BookingId",
                table: "SkyMilesTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SkyMilesTransactions_BookingId",
                table: "SkyMilesTransactions");

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "AirportId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "FlightFares",
                keyColumn: "FlightFareId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Routes",
                keyColumn: "RouteId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FlightSchedules",
                keyColumn: "FlightScheduleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FlightSchedules",
                keyColumn: "FlightScheduleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FlightSchedules",
                keyColumn: "FlightScheduleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FlightSchedules",
                keyColumn: "FlightScheduleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FlightSchedules",
                keyColumn: "FlightScheduleId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "FlightSchedules",
                keyColumn: "FlightScheduleId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "SeatClasses",
                keyColumn: "SeatClassId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SeatClasses",
                keyColumn: "SeatClassId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "AirportId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "AirportId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "AirportId",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 1,
                columns: new[] { "DestinationAirportId", "OriginAirportId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Flights",
                keyColumn: "FlightId",
                keyValue: 2,
                columns: new[] { "DestinationAirportId", "OriginAirportId" },
                values: new object[] { null, null });
        }
    }
}
