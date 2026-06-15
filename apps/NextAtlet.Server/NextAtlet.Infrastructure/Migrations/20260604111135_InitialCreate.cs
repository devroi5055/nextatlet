using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AthleteProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "judo"),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    DefaultLocaleId = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "da"),
                    VisibilityStateId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Public"),
                    SelfTierId = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    MinimumCapability = table.Column<string>(type: "jsonb", nullable: true),
                    Manifest = table.Column<string>(type: "jsonb", nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AuthProviderId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "self_upload"),
                    IsClubBranding = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    AltText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAssets_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDraft = table.Column<bool>(type: "boolean", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Layout = table.Column<string>(type: "jsonb", nullable: false),
                    GlobalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ProfileLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Permissions = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileLogins_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "CreatedUtc", "IsActive", "Manifest", "MinimumCapability", "Name", "PreviewImageUrl", "UpdatedUtc", "Version" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "{\"supportedSectionTypes\":[\"hero\",\"bio\"],\"colorSlots\":[\"primary\",\"secondary\",\"accent\"],\"fontSlots\":[\"headingFont\",\"bodyFont\"]}", null, "Classic", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

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

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_AthleteProfileId",
                table: "MediaAssets",
                column: "AthleteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileLogins_AthleteProfileId",
                table: "ProfileLogins",
                column: "AthleteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileLogins_UserId",
                table: "ProfileLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileLogins_UserId_AthleteProfileId",
                table: "ProfileLogins",
                columns: new[] { "UserId", "AthleteProfileId" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthProviderId",
                table: "Users",
                column: "AuthProviderId",
                unique: true);

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
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "ProfileLogins");

            migrationBuilder.DropTable(
                name: "SiteConfigs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AthleteProfiles");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
