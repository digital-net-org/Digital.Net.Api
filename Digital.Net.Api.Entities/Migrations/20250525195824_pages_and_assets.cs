using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Digital.Net.Api.Entities.Migrations
{
    /// <inheritdoc />
    public partial class pages_and_assets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_View_ViewId",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.DropTable(
                name: "View",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "PuckConfig",
                schema: "digital_net");

            migrationBuilder.DropIndex(
                name: "IX_Page_ViewId",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.DropColumn(
                name: "ViewId",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.AddColumn<int>(
                name: "PuckConfigId",
                schema: "digital_net",
                table: "Page",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PuckData",
                schema: "digital_net",
                table: "Page",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PageAsset",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Path = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageAsset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageAsset_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "digital_net",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagePuckConfig",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "IX_PageAsset_DocumentId",
                schema: "digital_net",
                table: "PageAsset",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PageAsset_Path",
                schema: "digital_net",
                table: "PageAsset",
                column: "Path",
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Page_PagePuckConfig_PuckConfigId",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.DropTable(
                name: "PageAsset",
                schema: "digital_net");

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

            migrationBuilder.DropColumn(
                name: "PuckData",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.AddColumn<Guid>(
                name: "ViewId",
                schema: "digital_net",
                table: "Page",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PuckConfig",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuckConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "View",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PuckConfigId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_View", x => x.Id);
                    table.ForeignKey(
                        name: "FK_View_PuckConfig_PuckConfigId",
                        column: x => x.PuckConfigId,
                        principalSchema: "digital_net",
                        principalTable: "PuckConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Page_ViewId",
                schema: "digital_net",
                table: "Page",
                column: "ViewId");

            migrationBuilder.CreateIndex(
                name: "IX_PuckConfig_Version",
                schema: "digital_net",
                table: "PuckConfig",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_View_Name",
                schema: "digital_net",
                table: "View",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_View_PuckConfigId",
                schema: "digital_net",
                table: "View",
                column: "PuckConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Page_View_ViewId",
                schema: "digital_net",
                table: "Page",
                column: "ViewId",
                principalSchema: "digital_net",
                principalTable: "View",
                principalColumn: "Id");
        }
    }
}
