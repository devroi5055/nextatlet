using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deletebehavoirtweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_IndividualProfiles_IndividualProfileId",
                table: "GuardianConsents");

            migrationBuilder.RenameColumn(
                name: "IndividualProfileId",
                table: "GuardianConsents",
                newName: "SiteId");

            migrationBuilder.RenameIndex(
                name: "IX_GuardianConsents_IndividualProfileId",
                table: "GuardianConsents",
                newName: "IX_GuardianConsents_SiteId");

            migrationBuilder.AddColumn<string>(
                name: "Verification_VerifiedByEmail",
                table: "OrganizationProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionTokens_Sites_TargetSiteId",
                table: "ActionTokens",
                column: "TargetSiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianConsents_Sites_SiteId",
                table: "GuardianConsents",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionTokens_Sites_TargetSiteId",
                table: "ActionTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianConsents_Sites_SiteId",
                table: "GuardianConsents");

            migrationBuilder.DropColumn(
                name: "Verification_VerifiedByEmail",
                table: "OrganizationProfiles");

            migrationBuilder.RenameColumn(
                name: "SiteId",
                table: "GuardianConsents",
                newName: "IndividualProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_GuardianConsents_SiteId",
                table: "GuardianConsents",
                newName: "IX_GuardianConsents_IndividualProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianConsents_IndividualProfiles_IndividualProfileId",
                table: "GuardianConsents",
                column: "IndividualProfileId",
                principalTable: "IndividualProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
