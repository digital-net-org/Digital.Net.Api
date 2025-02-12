using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Pages.Data.Migrations
{
    /// <inheritdoc />
    public partial class ViewUniquePathField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_View_Path",
                table: "View",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_View_Path",
                table: "View");
        }
    }
}
