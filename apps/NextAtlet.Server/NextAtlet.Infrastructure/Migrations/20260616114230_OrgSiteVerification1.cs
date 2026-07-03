using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrgSiteVerification1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationProfiles_Sites_SiteId",
                table: "OrganizationProfiles");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationProfiles_SiteId",
                table: "OrganizationProfiles");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationProfiles_Slug",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "OrganizationProfiles");

            migrationBuilder.DropColumn(
                name: "VisibilityStateId",
                table: "OrganizationProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "AthleteSlotCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AthleteSlotCount",
                table: "OrganizationProfiles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "OrganizationProfiles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "OrganizationProfiles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisibilityStateId",
                table: "OrganizationProfiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "public");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_SiteId",
                table: "OrganizationProfiles",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationProfiles_Slug",
                table: "OrganizationProfiles",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationProfiles_Sites_SiteId",
                table: "OrganizationProfiles",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
