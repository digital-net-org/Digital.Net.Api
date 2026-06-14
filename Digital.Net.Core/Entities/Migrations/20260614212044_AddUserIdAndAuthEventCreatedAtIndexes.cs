using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Core.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdAndAuthEventCreatedAtIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EntityMutation_UserId_CreatedAt",
                schema: "digital_net",
                table: "EntityMutation",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthEvent_CreatedAt_Id",
                schema: "digital_net",
                table: "AuthEvent",
                columns: new[] { "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntityMutation_UserId_CreatedAt",
                schema: "digital_net",
                table: "EntityMutation");

            migrationBuilder.DropIndex(
                name: "IX_AuthEvent_CreatedAt_Id",
                schema: "digital_net",
                table: "AuthEvent");
        }
    }
}
