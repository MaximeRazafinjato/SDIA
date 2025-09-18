using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDIA.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicAccessFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Registrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTokenExpiry",
                table: "Registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderSentAt",
                table: "Registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationAttempts",
                table: "Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpiry",
                table: "Registrations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "AccessTokenExpiry",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "LastReminderSentAt",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "VerificationAttempts",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "VerificationCodeExpiry",
                table: "Registrations");
        }
    }
}
