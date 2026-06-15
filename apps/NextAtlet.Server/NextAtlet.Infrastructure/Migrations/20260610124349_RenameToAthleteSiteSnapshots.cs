using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameToAthleteSiteSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteConfigs");

            migrationBuilder.DropColumn(
                name: "MinimumCapability",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "ConsentedUtc",
                table: "GuardianConsents");

            migrationBuilder.DropColumn(
                name: "ConsentCapturedUtc",
                table: "AthleteProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentDraftSnapshotId",
                table: "AthleteProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentPublishedSnapshotId",
                table: "AthleteProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AthleteSiteSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Layout = table.Column<string>(type: "jsonb", nullable: false),
                    GlobalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteSiteSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteSiteSnapshots_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthleteSiteSnapshots_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Manifest",
                value: "{\"supportedSectionTypes\":[\"hero\",\"bio\"],\"colorSlots\":[\"primary\",\"accent\",\"background\"],\"fontSlots\":[\"heading\",\"body\"]}");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteSiteSnapshots_AthleteProfileId",
                table: "AthleteSiteSnapshots",
                column: "AthleteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteSiteSnapshots_CreatedUtc",
                table: "AthleteSiteSnapshots",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteSiteSnapshots_ThemeId",
                table: "AthleteSiteSnapshots",
                column: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AthleteSiteSnapshots");

            migrationBuilder.DropColumn(
                name: "CurrentDraftSnapshotId",
                table: "AthleteProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentPublishedSnapshotId",
                table: "AthleteProfiles");

            migrationBuilder.AddColumn<string>(
                name: "MinimumCapability",
                table: "Themes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "Themes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentedUtc",
                table: "GuardianConsents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentCapturedUtc",
                table: "AthleteProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SiteConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GlobalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    IsDraft = table.Column<bool>(type: "boolean", nullable: false),
                    Layout = table.Column<string>(type: "jsonb", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ThemeVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteConfigs_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteConfigs_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Manifest", "MinimumCapability", "UpdatedUtc" },
                values: new object[] { "{\"supportedSectionTypes\":[\"hero\",\"bio\"],\"colorSlots\":[\"primary\",\"secondary\",\"accent\"],\"fontSlots\":[\"headingFont\",\"bodyFont\"]}", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_SiteConfigs_AthleteProfileId_IsDraft",
                table: "SiteConfigs",
                columns: new[] { "AthleteProfileId", "IsDraft" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteConfigs_ThemeId",
                table: "SiteConfigs",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteConfigs_UpdatedUtc",
                table: "SiteConfigs",
                column: "UpdatedUtc",
                descending: new bool[0]);
        }
    }
}
