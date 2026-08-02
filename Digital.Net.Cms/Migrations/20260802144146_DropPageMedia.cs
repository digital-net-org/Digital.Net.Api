using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class DropPageMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageMedia",
                schema: "digital_net_cms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageMedia",
                schema: "digital_net_cms",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageMedia", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_PageMedia_Media_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageMedia_Page_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageMedia_ChildId",
                schema: "digital_net_cms",
                table: "PageMedia",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_PageMedia_ParentId_Label",
                schema: "digital_net_cms",
                table: "PageMedia",
                columns: new[] { "ParentId", "Label" },
                unique: true);
        }
    }
}
