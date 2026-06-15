using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuardianConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConsentRequest",
                table: "Invitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConsentState",
                table: "AthleteProfiles",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "NotRequired");

            migrationBuilder.CreateTable(
                name: "GuardianConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardianUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TermsVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConsentedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuardianConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuardianConsents_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuardianConsents_Users_GuardianUserId",
                        column: x => x.GuardianUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuardianConsents_AthleteProfileId",
                table: "GuardianConsents",
                column: "AthleteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_GuardianConsents_GuardianUserId",
                table: "GuardianConsents",
                column: "GuardianUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuardianConsents");

            migrationBuilder.DropColumn(
                name: "IsConsentRequest",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ConsentState",
                table: "AthleteProfiles");
        }
    }
}
