using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Api.Entities.Migrations
{
    /// <inheritdoc />
    public partial class PageMetaFieldsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Property",
                schema: "digital_net",
                table: "PageMeta",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "digital_net",
                table: "PageMeta",
                newName: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                schema: "digital_net",
                table: "PageMeta",
                newName: "Property");

            migrationBuilder.RenameColumn(
                name: "Key",
                schema: "digital_net",
                table: "PageMeta",
                newName: "Name");
        }
    }
}
