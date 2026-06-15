using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsentViaEndpointNotInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConsentRequest",
                table: "Invitations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConsentRequest",
                table: "Invitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
