using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextAtlet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActionToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ActionTokens",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionTokens_Type_AcceptedUtc",
                table: "ActionTokens",
                newName: "IX_ActionTokens_TypeId_AcceptedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "ActionTokens",
                newName: "Type");

            migrationBuilder.RenameIndex(
                name: "IX_ActionTokens_TypeId_AcceptedUtc",
                table: "ActionTokens",
                newName: "IX_ActionTokens_Type_AcceptedUtc");
        }
    }
}
