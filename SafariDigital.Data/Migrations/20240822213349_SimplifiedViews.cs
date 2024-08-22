using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafariDigital.Database.Migrations
{
    /// <inheritdoc />
    public partial class SimplifiedViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "view_content");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "view_frame",
                newName: "name");

            migrationBuilder.RenameIndex(
                name: "IX_view_frame_title",
                table: "view_frame",
                newName: "IX_view_frame_name");

            migrationBuilder.AddColumn<string>(
                name: "data",
                table: "view_frame",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data",
                table: "view_frame");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "view_frame",
                newName: "title");

            migrationBuilder.RenameIndex(
                name: "IX_view_frame_name",
                table: "view_frame",
                newName: "IX_view_frame_title");

            migrationBuilder.CreateTable(
                name: "view_content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    view_frame_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    props = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_content", x => x.id);
                    table.ForeignKey(
                        name: "FK_view_content_view_frame_view_frame_id",
                        column: x => x.view_frame_id,
                        principalTable: "view_frame",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_view_content_view_frame_id",
                table: "view_content",
                column: "view_frame_id");
        }
    }
}
