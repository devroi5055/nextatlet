using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SiteAbstractionRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_AthleteSites_AthleteProfileId",
                table: "GuardianConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AthleteSites_TargetProfileId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_AthleteSites_AthleteSiteId",
                table: "MediaAssets");

            migrationBuilder.DropTable(
                name: "AthleteSiteSnapshots");

            migrationBuilder.DropTable(
                name: "ProfileLogins");

            migrationBuilder.DropTable(
                name: "AthleteSites");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Themes");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "MediaAssets",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "TargetProfileId",
                table: "Invitations",
                newName: "TargetSiteId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Invitations",
                newName: "StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_TargetProfileId",
                table: "Invitations",
                newName: "IX_Invitations_TargetSiteId");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_Email_Status",
                table: "Invitations",
                newName: "IX_Invitations_Email_StatusId");

            migrationBuilder.RenameColumn(
                name: "Method",
                table: "GuardianConsents",
                newName: "MethodId");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewImageUrl",
                table: "Themes",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetiredUtc",
                table: "Themes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AthleteProfiles",
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
                    table.PrimaryKey("PK_AthleteProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Layout = table.Column<string>(type: "jsonb", nullable: false),
                    GlobalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSnapshots_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentDraftSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentPublishedSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VisibilityStateId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "public"),
                    VerificationStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    DefaultLocaleId = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "en"),
                    SiteProfileId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "athlete"),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_SiteSnapshots_CurrentDraftSnapshotId",
                        column: x => x.CurrentDraftSnapshotId,
                        principalTable: "SiteSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sites_SiteSnapshots_CurrentPublishedSnapshotId",
                        column: x => x.CurrentPublishedSnapshotId,
                        principalTable: "SiteSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationTypeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsServerManaged = table.Column<bool>(type: "boolean", nullable: false),
                    OrganizationTierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "free"),
                    AthleteSlotCount = table.Column<int>(type: "integer", nullable: true),
                    VisibilityStateId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "public"),
                    VerificationStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    Verification_VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Verification_MethodId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Verification_CVR = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Verification_VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationProfiles_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteRoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Permissions = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteLogins_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposingOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedLayout = table.Column<string>(type: "jsonb", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PreviewImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_AthleteProfiles_TargetProfileId",
                        column: x => x.TargetProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_OrganizationProfiles_ProposingOrganizationId",
                        column: x => x.ProposingOrganizationId,
                        principalTable: "OrganizationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Users_ProposedByUserId",
                        column: x => x.ProposedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    statusId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    OccupiesSlot = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memberships_AthleteProfiles_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_OrganizationProfiles_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "OrganizationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "Manifest", "RetiredUtc" },
                values: new object[] { "{\"colors\":{\"primary\":\"#BA4336\",\"secondary\":\"#874942\",\"accent\":\"#EC2A15\",\"background\":\"#FAF8F7\",\"surface\":\"#FFFFFF\",\"text\":\"#332E2D\"},\"typography\":{\"headingFont\":\"Sora\",\"bodyFont\":\"Inter\",\"headingWeight\":\"700\",\"bodyWeight\":\"400\"},\"componentStyles\":{\"buttons\":{\"overrides\":{\"radius\":\"rounded\"},\"options\":[{\"key\":\"sharp\",\"displayName\":\"Sharp Edges\",\"styles\":{\"radius\":\"none\"}}]},\"cards\":{\"overrides\":{\"radius\":\"medium\"},\"options\":[]}},\"sectionStyles\":{}}", null });

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_CreatedUtc",
                table: "AthleteProfiles",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AthleteProfiles_SportId",
                table: "AthleteProfiles",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ProposedByUserId",
                table: "ChangeRequests",
                column: "ProposedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ProposingOrganizationId",
                table: "ChangeRequests",
                column: "ProposingOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_TargetProfileId",
                table: "ChangeRequests",
                column: "TargetProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ThemeId",
                table: "ChangeRequests",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_AthleteProfileId",
                table: "Memberships",
                column: "AthleteProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_OrganizationId",
                table: "Memberships",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_OrganizationTypeId",
                table: "OrganizationProfiles",
                column: "OrganizationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_SiteId",
                table: "OrganizationProfiles",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_Slug",
                table: "OrganizationProfiles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteLogins_SiteId",
                table: "SiteLogins",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteLogins_UserId",
                table: "SiteLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteLogins_UserId_SiteId",
                table: "SiteLogins",
                columns: new[] { "UserId", "SiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CurrentDraftSnapshotId",
                table: "Sites",
                column: "CurrentDraftSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_CurrentPublishedSnapshotId",
                table: "Sites",
                column: "CurrentPublishedSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Slug",
                table: "Sites",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSnapshots_CreatedUtc",
                table: "SiteSnapshots",
                column: "CreatedUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSnapshots_SiteId",
                table: "SiteSnapshots",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSnapshots_ThemeId",
                table: "SiteSnapshots",
                column: "ThemeId");

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
                name: "FK_MediaAssets_Sites_AthleteSiteId",
                table: "MediaAssets",
                column: "AthleteSiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_AthleteProfiles_AthleteProfileId",
                table: "GuardianConsents");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AthleteProfiles_TargetSiteId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_Sites_AthleteSiteId",
                table: "MediaAssets");

            migrationBuilder.DropTable(
                name: "ChangeRequests");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "SiteLogins");

            migrationBuilder.DropTable(
                name: "AthleteProfiles");

            migrationBuilder.DropTable(
                name: "OrganizationProfiles");

            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "SiteSnapshots");

            migrationBuilder.DropColumn(
                name: "RetiredUtc",
                table: "Themes");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "MediaAssets",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "TargetSiteId",
                table: "Invitations",
                newName: "TargetProfileId");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Invitations",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_TargetSiteId",
                table: "Invitations",
                newName: "IX_Invitations_TargetProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_Email_StatusId",
                table: "Invitations",
                newName: "IX_Invitations_Email_Status");

            migrationBuilder.RenameColumn(
                name: "MethodId",
                table: "GuardianConsents",
                newName: "Method");

            migrationBuilder.AlterColumn<string>(
                name: "PreviewImageUrl",
                table: "Themes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Themes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Themes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "AthleteSites",
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
                    table.PrimaryKey("PK_AthleteSites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AthleteSiteSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GlobalSettings = table.Column<string>(type: "jsonb", nullable: true),
                    Layout = table.Column<string>(type: "jsonb", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ThemeVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AthleteSiteSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AthleteSiteSnapshots_AthleteSites_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AthleteSiteSnapshots_Themes_ThemeId",
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
                    AthleteProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Permissions = table.Column<string>(type: "jsonb", nullable: true),
                    RoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileLogins_AthleteSites_AthleteProfileId",
                        column: x => x.AthleteProfileId,
                        principalTable: "AthleteSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "IsActive", "Manifest", "Version" },
                values: new object[] { true, "{\"colors\":{\"primary\":\"#BA4336\",\"secondary\":\"#874942\",\"accent\":\"#EC2A15\",\"background\":\"#FAF8F7\",\"surface\":\"#FFFFFF\",\"text\":\"#332E2D\"},\"typography\":{\"headingFont\":\"Sora\",\"bodyFont\":\"Inter\",\"headingWeight\":\"700\",\"bodyWeight\":\"400\"},\"components\":{\"buttons\":{\"overrides\":{\"radius\":\"rounded\"},\"options\":[{\"key\":\"sharp\",\"displayName\":\"Sharp Edges\",\"styles\":{\"radius\":\"none\"}}]},\"cards\":{\"overrides\":{\"radius\":\"medium\"},\"options\":[]}},\"sectionStyles\":{}}", 1 });

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
        }
    }
}
