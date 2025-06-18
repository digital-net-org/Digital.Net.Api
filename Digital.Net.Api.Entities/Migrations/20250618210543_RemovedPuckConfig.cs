using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Digital.Net.Api.Entities.Migrations
{
    /// <inheritdoc />
    public partial class RemovedPuckConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_PagePuckConfig_PuckConfigId",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.DropTable(
                name: "PagePuckConfig",
                schema: "digital_net");

            migrationBuilder.DropIndex(
                name: "IX_Page_PuckConfigId",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.DropColumn(
                name: "PuckConfigId",
                schema: "digital_net",
                table: "Page");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PuckConfigId",
                schema: "digital_net",
                table: "Page",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PagePuckConfig",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagePuckConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagePuckConfig_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "digital_net",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Page_PuckConfigId",
                schema: "digital_net",
                table: "Page",
                column: "PuckConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_PagePuckConfig_DocumentId",
                schema: "digital_net",
                table: "PagePuckConfig",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PagePuckConfig_Version",
                schema: "digital_net",
                table: "PagePuckConfig",
                column: "Version",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Page_PagePuckConfig_PuckConfigId",
                schema: "digital_net",
                table: "Page",
                column: "PuckConfigId",
                principalSchema: "digital_net",
                principalTable: "PagePuckConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
