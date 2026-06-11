using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Core.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EntityMutation_EntityType_EntityId_CreatedAt",
                schema: "digital_net",
                table: "EntityMutation",
                columns: new[] { "EntityType", "EntityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthEvent_UserId_CreatedAt",
                schema: "digital_net",
                table: "AuthEvent",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntityMutation_EntityType_EntityId_CreatedAt",
                schema: "digital_net",
                table: "EntityMutation");

            migrationBuilder.DropIndex(
                name: "IX_AuthEvent_UserId_CreatedAt",
                schema: "digital_net",
                table: "AuthEvent");
        }
    }
}
