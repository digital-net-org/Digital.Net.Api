using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Core.Entities.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEventWithAuthEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Event",
                schema: "digital_net");

            migrationBuilder.CreateTable(
                name: "AuthEvent",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Login = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthEvent_IpAddress_Type_Success_CreatedAt",
                schema: "digital_net",
                table: "AuthEvent",
                columns: new[] { "IpAddress", "Type", "Success", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthEvent",
                schema: "digital_net");

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorTrace = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    HasError = table.Column<bool>(type: "boolean", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Event_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "digital_net",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_UserId",
                schema: "digital_net",
                table: "Event",
                column: "UserId");
        }
    }
}
