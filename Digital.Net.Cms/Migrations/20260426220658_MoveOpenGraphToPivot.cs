using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class MoveOpenGraphToPivot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenGraph",
                schema: "digital_net_cms",
                table: "Page");

            migrationBuilder.CreateTable(
                name: "OpenGraphEntry",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Property = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Content = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenGraphEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageOpenGraph",
                schema: "digital_net_cms",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageOpenGraph", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_PageOpenGraph_OpenGraphEntry_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "digital_net_cms",
                        principalTable: "OpenGraphEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageOpenGraph_Page_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageOpenGraph_ChildId",
                schema: "digital_net_cms",
                table: "PageOpenGraph",
                column: "ChildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageOpenGraph",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "OpenGraphEntry",
                schema: "digital_net_cms");

            migrationBuilder.AddColumn<string>(
                name: "OpenGraph",
                schema: "digital_net_cms",
                table: "Page",
                type: "character varying(65535)",
                maxLength: 65535,
                nullable: true);
        }
    }
}
