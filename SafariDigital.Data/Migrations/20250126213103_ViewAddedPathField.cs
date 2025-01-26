using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafariDigital.Data.Migrations
{
    /// <inheritdoc />
    public partial class ViewAddedPathField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "View",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "View");
        }
    }
}
