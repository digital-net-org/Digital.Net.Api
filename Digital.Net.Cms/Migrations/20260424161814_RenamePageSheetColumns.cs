using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class RenamePageSheetColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageSheet_Page_PageId",
                schema: "digital_net_cms",
                table: "PageSheet");

            migrationBuilder.DropForeignKey(
                name: "FK_PageSheet_Sheet_SheetId",
                schema: "digital_net_cms",
                table: "PageSheet");

            migrationBuilder.RenameColumn(
                name: "LoadOrder",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "SheetId",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "ChildId");

            migrationBuilder.RenameColumn(
                name: "PageId",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_PageSheet_SheetId",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "IX_PageSheet_ChildId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageSheet_Page_ParentId",
                schema: "digital_net_cms",
                table: "PageSheet",
                column: "ParentId",
                principalSchema: "digital_net_cms",
                principalTable: "Page",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageSheet_Sheet_ChildId",
                schema: "digital_net_cms",
                table: "PageSheet",
                column: "ChildId",
                principalSchema: "digital_net_cms",
                principalTable: "Sheet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PageSheet_Page_ParentId",
                schema: "digital_net_cms",
                table: "PageSheet");

            migrationBuilder.DropForeignKey(
                name: "FK_PageSheet_Sheet_ChildId",
                schema: "digital_net_cms",
                table: "PageSheet");

            migrationBuilder.RenameColumn(
                name: "Order",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "LoadOrder");

            migrationBuilder.RenameColumn(
                name: "ChildId",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "SheetId");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "PageId");

            migrationBuilder.RenameIndex(
                name: "IX_PageSheet_ChildId",
                schema: "digital_net_cms",
                table: "PageSheet",
                newName: "IX_PageSheet_SheetId");

            migrationBuilder.AddForeignKey(
                name: "FK_PageSheet_Page_PageId",
                schema: "digital_net_cms",
                table: "PageSheet",
                column: "PageId",
                principalSchema: "digital_net_cms",
                principalTable: "Page",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PageSheet_Sheet_SheetId",
                schema: "digital_net_cms",
                table: "PageSheet",
                column: "SheetId",
                principalSchema: "digital_net_cms",
                principalTable: "Sheet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
