using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafariDigital.Database.Migrations
{
    /// <inheritdoc />
    public partial class DocumentMimeTypeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "mime_type",
                table: "document",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "mime_type",
                table: "document",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
