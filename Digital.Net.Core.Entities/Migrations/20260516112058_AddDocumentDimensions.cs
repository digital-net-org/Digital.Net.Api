using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Core.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                schema: "digital_net",
                table: "Document",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                schema: "digital_net",
                table: "Document",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                schema: "digital_net",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "Width",
                schema: "digital_net",
                table: "Document");
        }
    }
}
