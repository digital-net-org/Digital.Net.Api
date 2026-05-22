using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class UniqueMediaLabelPerParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reconcile pre-constraint data: previous schema let two rows share (ParentId, Label),
            // so adding the unique index could conflict with existing duplicates (including the
            // empty-string defaults seeded by `MakeMediaLabelRequired`). Drop the surplus rows
            // (keep one per group). Only the pivot binding is removed — the underlying Media
            // entities are untouched.
            migrationBuilder.Sql(@"
                DELETE FROM digital_net_cms.""ArticleMedia"" a
                USING digital_net_cms.""ArticleMedia"" b
                WHERE a.ctid > b.ctid
                  AND a.""ParentId"" = b.""ParentId""
                  AND a.""Label"" = b.""Label"";
            ");
            migrationBuilder.Sql(@"
                DELETE FROM digital_net_cms.""PageMedia"" a
                USING digital_net_cms.""PageMedia"" b
                WHERE a.ctid > b.ctid
                  AND a.""ParentId"" = b.""ParentId""
                  AND a.""Label"" = b.""Label"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_PageMedia_ParentId_Label",
                schema: "digital_net_cms",
                table: "PageMedia",
                columns: new[] { "ParentId", "Label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleMedia_ParentId_Label",
                schema: "digital_net_cms",
                table: "ArticleMedia",
                columns: new[] { "ParentId", "Label" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PageMedia_ParentId_Label",
                schema: "digital_net_cms",
                table: "PageMedia");

            migrationBuilder.DropIndex(
                name: "IX_ArticleMedia_ParentId_Label",
                schema: "digital_net_cms",
                table: "ArticleMedia");
        }
    }
}
