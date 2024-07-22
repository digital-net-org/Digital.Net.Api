using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SafariDigital.Database.Migrations
{
    /// <inheritdoc />
    public partial class created_document_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "avatar_id",
                table: "user",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    mime_type = table.Column<int>(type: "integer", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploader_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_user_uploader_id",
                        column: x => x.uploader_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "avatar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pos_x = table.Column<int>(type: "integer", nullable: true),
                    pos_y = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avatar", x => x.id);
                    table.ForeignKey(
                        name: "FK_avatar_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_avatar_id",
                table: "user",
                column: "avatar_id");

            migrationBuilder.CreateIndex(
                name: "IX_avatar_document_id",
                table: "avatar",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_file_name",
                table: "document",
                column: "file_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_uploader_id",
                table: "document",
                column: "uploader_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_avatar_avatar_id",
                table: "user",
                column: "avatar_id",
                principalTable: "avatar",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_avatar_avatar_id",
                table: "user");

            migrationBuilder.DropTable(
                name: "avatar");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropIndex(
                name: "IX_user_avatar_id",
                table: "user");

            migrationBuilder.DropColumn(
                name: "avatar_id",
                table: "user");
        }
    }
}
