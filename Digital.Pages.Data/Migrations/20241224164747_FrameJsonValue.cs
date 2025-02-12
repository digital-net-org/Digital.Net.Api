using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Pages.Data.Migrations
{
    /// <inheritdoc />
    public partial class FrameJsonValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Frame"" ALTER COLUMN ""Data"" TYPE jsonb USING ""Data""::jsonb;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Frame"" ALTER COLUMN ""Data"" TYPE text USING ""Data""::text;");
        }
    }
}
