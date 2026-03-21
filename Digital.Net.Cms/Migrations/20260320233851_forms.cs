using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class forms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Form",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    SubmitLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormField",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ValidationJson = table.Column<string>(type: "text", nullable: true),
                    OptionsJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormField_Form_FormId",
                        column: x => x.FormId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormSubmission",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValuesJson = table.Column<string>(type: "text", nullable: false),
                    SubmitterIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmission_Form_FormId",
                        column: x => x.FormId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormField_FormId",
                schema: "digital_net_cms",
                table: "FormField",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmission_FormId",
                schema: "digital_net_cms",
                table: "FormSubmission",
                column: "FormId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormField",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "FormSubmission",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Form",
                schema: "digital_net_cms");
        }
    }
}
