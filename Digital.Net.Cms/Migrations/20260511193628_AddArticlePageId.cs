using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class AddArticlePageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PageId",
                schema: "digital_net_cms",
                table: "Article",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Article_PageId",
                schema: "digital_net_cms",
                table: "Article",
                column: "PageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Article_Page_PageId",
                schema: "digital_net_cms",
                table: "Article",
                column: "PageId",
                principalSchema: "digital_net_cms",
                principalTable: "Page",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Article_Page_PageId",
                schema: "digital_net_cms",
                table: "Article");

            migrationBuilder.DropIndex(
                name: "IX_Article_PageId",
                schema: "digital_net_cms",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "PageId",
                schema: "digital_net_cms",
                table: "Article");
        }
    }
}
