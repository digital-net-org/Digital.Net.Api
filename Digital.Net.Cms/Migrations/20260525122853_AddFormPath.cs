using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddFormPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                schema: "digital_net_cms",
                table: "Form",
                type: "character varying(2068)",
                maxLength: 2068,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                schema: "digital_net_cms",
                table: "Form");
        }
    }
}
