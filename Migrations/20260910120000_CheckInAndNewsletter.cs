using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineReservation.Migrations
{
    /// <inheritdoc />
    public partial class CheckInAndNewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NewsletterSubscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptions", x => x.SubscriptionId);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__NewsletterSubscriptions__Email",
                table: "NewsletterSubscriptions",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscriptions");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Bookings");
        }
    }
}