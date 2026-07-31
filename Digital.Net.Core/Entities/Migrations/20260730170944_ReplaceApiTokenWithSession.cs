using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Core.Entities.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceApiTokenWithSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiToken",
                schema: "digital_net");

            migrationBuilder.CreateTable(
                name: "Session",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AbsoluteExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "digital_net",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_Key",
                schema: "digital_net",
                table: "Session",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_UserId",
                schema: "digital_net",
                table: "Session",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Session",
                schema: "digital_net");

            migrationBuilder.CreateTable(
                name: "ApiToken",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiToken_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "digital_net",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiToken_Key",
                schema: "digital_net",
                table: "ApiToken",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiToken_UserId",
                schema: "digital_net",
                table: "ApiToken",
                column: "UserId");
        }
    }
}
