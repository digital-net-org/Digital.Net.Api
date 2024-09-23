using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SafariDigital.Database.Migrations
{
    /// <inheritdoc />
    public partial class ViewsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "view",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    published_frame_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "view_frame",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    view_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "view_content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    props = table.Column<string>(type: "text", nullable: false),
                    view_frame_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "IX_view_published_frame_id",
                table: "view",
                column: "published_frame_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_view_content_view_frame_id",
                table: "view_content",
                column: "view_frame_id");

            migrationBuilder.CreateIndex(
                name: "IX_view_frame_title",
                table: "view_frame",
                column: "title",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_view_view_frame_published_frame_id",
                table: "view");

            migrationBuilder.DropTable(
                name: "view_content");

            migrationBuilder.DropTable(
                name: "view_frame");

            migrationBuilder.DropTable(
                name: "view");
        }
    }
}
