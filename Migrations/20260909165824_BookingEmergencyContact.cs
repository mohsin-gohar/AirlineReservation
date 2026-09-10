﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineReservation.Migrations
{
    /// <inheritdoc />
    public partial class BookingEmergencyContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Bookings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Bookings",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Bookings");
        }
    }
}
