using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddControlMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentCapturedUtc",
                table: "AthleteProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ControlMode",
                table: "AthleteProfiles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "AthleteControlled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentCapturedUtc",
                table: "AthleteProfiles");

            migrationBuilder.DropColumn(
                name: "ControlMode",
                table: "AthleteProfiles");
        }
    }
}
