using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedmanifest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AthleteSiteSnapshots_AthleteProfiles_AthleteProfileId",
                table: "AthleteSiteSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_AthleteProfiles_AthleteProfileId",
                table: "GuardianConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AthleteProfiles_TargetProfileId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_AthleteProfiles_AthleteProfileId",
                table: "MediaAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileLogins_AthleteProfiles_AthleteProfileId",
                table: "ProfileLogins");

            migrationBuilder.DropTable(
                name: "AthleteProfiles");

            migrationBuilder.RenameColumn(
                name: "AthleteProfileId",
                table: "MediaAssets",
                newName: "AthleteSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_MediaAssets_AthleteProfileId",
                table: "MediaAssets",
                newName: "IX_MediaAssets_AthleteSiteId");

            migrationBuilder.CreateTable(
                name: "AthleteSites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "judo"),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    DefaultLocaleId = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "da"),
                    VisibilityStateId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Public"),
                    ControlMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "AthleteControlled"),
                    ConsentState = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "NotRequired"),
                    SelfTierId = table.Column<string>(type: "text", nullable: true),
                    CurrentDraftSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentPublishedSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteSites", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Manifest",
                value: "{\"colors\":{\"primary\":\"#BA4336\",\"secondary\":\"#874942\",\"accent\":\"#EC2A15\",\"background\":\"#FAF8F7\",\"surface\":\"#FFFFFF\",\"text\":\"#332E2D\"},\"typography\":{\"headingFont\":\"Sora\",\"bodyFont\":\"Inter\",\"headingWeight\":\"700\",\"bodyWeight\":\"400\"},\"components\":{\"buttons\":{\"overrides\":{\"radius\":\"rounded\"},\"options\":[{\"key\":\"sharp\",\"displayName\":\"Sharp Edges\",\"styles\":{\"radius\":\"none\"}}]},\"cards\":{\"overrides\":{\"radius\":\"medium\"},\"options\":[]}},\"sectionStyles\":{}}");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteSites_CreatedUtc",
                table: "AthleteSites",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteSites_Slug",
                table: "AthleteSites",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteSites_SportId",
                table: "AthleteSites",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_AthleteSiteSnapshots_AthleteSites_AthleteProfileId",
                table: "AthleteSiteSnapshots",
                column: "AthleteProfileId",
                principalTable: "AthleteSites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianConsents_AthleteSites_AthleteProfileId",
                table: "GuardianConsents",
                column: "AthleteProfileId",
                principalTable: "AthleteSites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_AthleteSites_TargetProfileId",
                table: "Invitations",
                column: "TargetProfileId",
                principalTable: "AthleteSites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssets_AthleteSites_AthleteSiteId",
                table: "MediaAssets",
                column: "AthleteSiteId",
                principalTable: "AthleteSites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileLogins_AthleteSites_AthleteProfileId",
                table: "ProfileLogins",
                column: "AthleteProfileId",
                principalTable: "AthleteSites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AthleteSiteSnapshots_AthleteSites_AthleteProfileId",
                table: "AthleteSiteSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_AthleteSites_AthleteProfileId",
                table: "GuardianConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AthleteSites_TargetProfileId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_AthleteSites_AthleteSiteId",
                table: "MediaAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileLogins_AthleteSites_AthleteProfileId",
                table: "ProfileLogins");

            migrationBuilder.DropTable(
                name: "AthleteSites");

            migrationBuilder.RenameColumn(
                name: "AthleteSiteId",
                table: "MediaAssets",
                newName: "AthleteProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_MediaAssets_AthleteSiteId",
                table: "MediaAssets",
                newName: "IX_MediaAssets_AthleteProfileId");

            migrationBuilder.CreateTable(
                name: "AthleteProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentState = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "NotRequired"),
                    ControlMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "AthleteControlled"),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentDraftSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentPublishedSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    DefaultLocaleId = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "da"),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SelfTierId = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "judo"),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VisibilityStateId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Public")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteProfiles", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "Manifest",
                value: "{\"supportedSectionTypes\":[\"hero\",\"bio\"],\"colorSlots\":[\"primary\",\"accent\",\"background\"],\"fontSlots\":[\"heading\",\"body\"]}");

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_CreatedUtc",
                table: "AthleteProfiles",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_Slug",
                table: "AthleteProfiles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_SportId",
                table: "AthleteProfiles",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_AthleteSiteSnapshots_AthleteProfiles_AthleteProfileId",
                table: "AthleteSiteSnapshots",
                column: "AthleteProfileId",
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
                name: "FK_Invitations_AthleteProfiles_TargetProfileId",
                table: "Invitations",
                column: "TargetProfileId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssets_AthleteProfiles_AthleteProfileId",
                table: "MediaAssets",
                column: "AthleteProfileId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileLogins_AthleteProfiles_AthleteProfileId",
                table: "ProfileLogins",
                column: "AthleteProfileId",
                principalTable: "AthleteProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
