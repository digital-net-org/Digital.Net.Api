using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityMutationPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Payload",
                schema: "digital_net_cms",
                table: "EntityMutation",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                schema: "digital_net_cms",
                table: "EntityMutation");
        }
    }
}
