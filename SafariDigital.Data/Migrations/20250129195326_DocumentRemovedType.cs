using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafariDigital.Data.Migrations
{
    /// <inheritdoc />
    public partial class DocumentRemovedType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Document");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileType",
                table: "Document",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
