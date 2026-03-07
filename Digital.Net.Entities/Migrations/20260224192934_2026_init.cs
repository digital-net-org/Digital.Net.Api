using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Entities.Migrations
{
    /// <inheritdoc />
    public partial class _2026_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationOption",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "PageAsset",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "PageMeta",
                schema: "digital_net");

            migrationBuilder.DropColumn(
                name: "PuckData",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                schema: "digital_net",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JsonLd",
                schema: "digital_net",
                table: "Page",
                type: "character varying(65535)",
                maxLength: 65535,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PageOpenGraph",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Property = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageOpenGraph", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageOpenGraph_Page_PageId",
                        column: x => x.PageId,
                        principalSchema: "digital_net",
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageOpenGraph_PageId",
                schema: "digital_net",
                table: "PageOpenGraph",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageOpenGraph",
                schema: "digital_net");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                schema: "digital_net",
                table: "User");

            migrationBuilder.DropColumn(
                name: "JsonLd",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.AddColumn<string>(
                name: "PuckData",
                schema: "digital_net",
                table: "Page",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationOption",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageAsset",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Path = table.Column<string>(type: "character varying(2068)", maxLength: 2068, nullable: false),
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
                name: "PageMeta",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageMeta_Page_PageId",
                        column: x => x.PageId,
                        principalSchema: "digital_net",
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOption_Id",
                schema: "digital_net",
                table: "ApplicationOption",
                column: "Id",
                unique: true);

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
                name: "IX_PageMeta_PageId",
                schema: "digital_net",
                table: "PageMeta",
                column: "PageId");
        }
    }
}
