using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Core.Entities.Migrations
{
    /// <inheritdoc />
    public partial class UserUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Username_Email",
                schema: "digital_net",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "digital_net",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                schema: "digital_net",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                schema: "digital_net",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_Username",
                schema: "digital_net",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username_Email",
                schema: "digital_net",
                table: "User",
                columns: new[] { "Username", "Email" },
                unique: true);
        }
    }
}
