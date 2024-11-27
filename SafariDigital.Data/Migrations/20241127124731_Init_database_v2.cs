using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafariDigital.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init_database_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_key",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_key", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "frame",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    data = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recorded_login",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recorded_login", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "view",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    frame_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view", x => x.id);
                    table.ForeignKey(
                        name: "FK_view_frame_frame_id",
                        column: x => x.frame_id,
                        principalTable: "frame",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "avatar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pos_x = table.Column<int>(type: "integer", nullable: true),
                    pos_y = table.Column<int>(type: "integer", nullable: true),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avatar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    password = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    avatar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_avatar_avatar_id",
                        column: x => x.avatar_id,
                        principalTable: "avatar",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    file_type = table.Column<int>(type: "integer", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                name: "recorded_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recorded_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_recorded_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_key_key",
                table: "api_key",
                column: "key",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_frame_name",
                table: "frame",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recorded_token_user_id",
                table: "recorded_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_avatar_id",
                table: "user",
                column: "avatar_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_username_email",
                table: "user",
                columns: new[] { "username", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_view_frame_id",
                table: "view",
                column: "frame_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_view_title",
                table: "view",
                column: "title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_avatar_document_document_id",
                table: "avatar",
                column: "document_id",
                principalTable: "document",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_avatar_document_document_id",
                table: "avatar");

            migrationBuilder.DropTable(
                name: "api_key");

            migrationBuilder.DropTable(
                name: "recorded_login");

            migrationBuilder.DropTable(
                name: "recorded_token");

            migrationBuilder.DropTable(
                name: "view");

            migrationBuilder.DropTable(
                name: "frame");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "avatar");
        }
    }
}
