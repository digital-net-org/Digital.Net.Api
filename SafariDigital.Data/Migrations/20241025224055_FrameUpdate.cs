using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SafariDigital.Database.Migrations
{
    /// <inheritdoc />
    public partial class FrameUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_view_view_frame_published_frame_id",
                table: "view");

            migrationBuilder.DropTable(
                name: "view_frame");

            migrationBuilder.DropColumn(
                name: "type",
                table: "view");

            migrationBuilder.RenameColumn(
                name: "published_frame_id",
                table: "view",
                newName: "frame_id");

            migrationBuilder.RenameIndex(
                name: "IX_view_published_frame_id",
                table: "view",
                newName: "IX_view_frame_id");

            migrationBuilder.CreateTable(
                name: "frame",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    data = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_frame_name",
                table: "frame",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_view_frame_frame_id",
                table: "view",
                column: "frame_id",
                principalTable: "frame",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_view_frame_frame_id",
                table: "view");

            migrationBuilder.DropTable(
                name: "frame");

            migrationBuilder.RenameColumn(
                name: "frame_id",
                table: "view",
                newName: "published_frame_id");

            migrationBuilder.RenameIndex(
                name: "IX_view_frame_id",
                table: "view",
                newName: "IX_view_published_frame_id");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "view",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "view_frame",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    view_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_frame", x => x.id);
                    table.ForeignKey(
                        name: "FK_view_frame_view_view_id",
                        column: x => x.view_id,
                        principalTable: "view",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_view_frame_name",
                table: "view_frame",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_view_frame_view_id",
                table: "view_frame",
                column: "view_id");

            migrationBuilder.AddForeignKey(
                name: "FK_view_view_frame_published_frame_id",
                table: "view",
                column: "published_frame_id",
                principalTable: "view_frame",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
