using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Digital.Net.Api.Entities.Migrations
{
    /// <inheritdoc />
    public partial class PageMeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Page_Title",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                schema: "digital_net",
                table: "PageAsset",
                type: "character varying(2068)",
                maxLength: 2068,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "digital_net",
                table: "Page",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                schema: "digital_net",
                table: "Page",
                type: "character varying(2068)",
                maxLength: 2068,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "digital_net",
                table: "Page",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsIndexed",
                schema: "digital_net",
                table: "Page",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PageMeta",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Property = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Content = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "IX_PageMeta_PageId",
                schema: "digital_net",
                table: "PageMeta",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageMeta",
                schema: "digital_net");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.DropColumn(
                name: "IsIndexed",
                schema: "digital_net",
                table: "Page");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                schema: "digital_net",
                table: "PageAsset",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2068)",
                oldMaxLength: 2068);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "digital_net",
                table: "Page",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                schema: "digital_net",
                table: "Page",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2068)",
                oldMaxLength: 2068);

            migrationBuilder.CreateIndex(
                name: "IX_Page_Title",
                schema: "digital_net",
                table: "Page",
                column: "Title",
                unique: true);
        }
    }
}
