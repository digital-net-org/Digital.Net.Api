using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafariDigital.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAvatarPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PosX",
                table: "Avatar");

            migrationBuilder.DropColumn(
                name: "PosY",
                table: "Avatar");

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "Avatar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "Avatar",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "X",
                table: "Avatar");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Avatar");

            migrationBuilder.AddColumn<int>(
                name: "PosX",
                table: "Avatar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PosY",
                table: "Avatar",
                type: "integer",
                nullable: true);
        }
    }
}
