using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAthleteToIndividual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeRequests_AthleteProfiles_TargetProfileId",
                table: "ChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_AthleteProfiles_AthleteProfileId",
                table: "GuardianConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AthleteProfiles_TargetSiteId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_AthleteProfiles_AthleteProfileId",
                table: "Memberships");

            migrationBuilder.DropTable(
                name: "AthleteProfiles");

            migrationBuilder.DropColumn(
                name: "SiteProfileId",
                table: "Sites");

            migrationBuilder.RenameColumn(
                name: "AthleteProfileId",
                table: "Memberships",
                newName: "IndividualProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_AthleteProfileId",
                table: "Memberships",
                newName: "IX_Memberships_IndividualProfileId");

            migrationBuilder.RenameColumn(
                name: "AthleteProfileId",
                table: "GuardianConsents",
                newName: "IndividualProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_GuardianConsents_AthleteProfileId",
                table: "GuardianConsents",
                newName: "IX_GuardianConsents_IndividualProfileId");

            migrationBuilder.AddColumn<string>(
                name: "SiteTypeId",
                table: "Sites",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "individual");

            migrationBuilder.CreateTable(
                name: "IndividualProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "judo"),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    ControlModeId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "athlete_controlled"),
                    ConsentStateId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "not_required"),
                    SelfTierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndividualProfiles_CreatedUtc",
                table: "IndividualProfiles",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_IndividualProfiles_SportId",
                table: "IndividualProfiles",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeRequests_IndividualProfiles_TargetProfileId",
                table: "ChangeRequests",
                column: "TargetProfileId",
                principalTable: "IndividualProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianConsents_IndividualProfiles_IndividualProfileId",
                table: "GuardianConsents",
                column: "IndividualProfileId",
                principalTable: "IndividualProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_IndividualProfiles_TargetSiteId",
                table: "Invitations",
                column: "TargetSiteId",
                principalTable: "IndividualProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_IndividualProfiles_IndividualProfileId",
                table: "Memberships",
                column: "IndividualProfileId",
                principalTable: "IndividualProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeRequests_IndividualProfiles_TargetProfileId",
                table: "ChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_IndividualProfiles_IndividualProfileId",
                table: "GuardianConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_IndividualProfiles_TargetSiteId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_IndividualProfiles_IndividualProfileId",
                table: "Memberships");

            migrationBuilder.DropTable(
                name: "IndividualProfiles");

            migrationBuilder.DropColumn(
                name: "SiteTypeId",
                table: "Sites");

            migrationBuilder.RenameColumn(
                name: "IndividualProfileId",
                table: "Memberships",
                newName: "AthleteProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_IndividualProfileId",
                table: "Memberships",
                newName: "IX_Memberships_AthleteProfileId");

            migrationBuilder.RenameColumn(
                name: "IndividualProfileId",
                table: "GuardianConsents",
                newName: "AthleteProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_GuardianConsents_IndividualProfileId",
                table: "GuardianConsents",
                newName: "IX_GuardianConsents_AthleteProfileId");

            migrationBuilder.AddColumn<string>(
                name: "SiteProfileId",
                table: "Sites",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "athlete");

            migrationBuilder.CreateTable(
                name: "AthleteProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentStateId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "not_required"),
                    ControlModeId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "athlete_controlled"),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SelfTierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "judo"),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_CreatedUtc",
                table: "AthleteProfiles",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_SportId",
                table: "AthleteProfiles",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeRequests_AthleteProfiles_TargetProfileId",
                table: "ChangeRequests",
                column: "TargetProfileId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianConsents_AthleteProfiles_AthleteProfileId",
                table: "GuardianConsents",
                column: "AthleteProfileId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_AthleteProfiles_TargetSiteId",
                table: "Invitations",
                column: "TargetSiteId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_AthleteProfiles_AthleteProfileId",
                table: "Memberships",
                column: "AthleteProfileId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
