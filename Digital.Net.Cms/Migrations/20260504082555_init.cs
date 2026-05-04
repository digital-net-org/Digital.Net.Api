using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital.Net.Cms.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "digital_net_cms");

            migrationBuilder.CreateTable(
                name: "Article",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Article", x => x.Id);
                });

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
                name: "Media",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Alt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                });

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
                name: "Page",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Path = table.Column<string>(type: "character varying(2068)", maxLength: 2068, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    Indexed = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    JsonLd = table.Column<string>(type: "character varying(65535)", maxLength: 65535, nullable: true),
                    Redirect = table.Column<string>(type: "character varying(2068)", maxLength: 2068, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Page", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sheet",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Published = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "MediaVariant",
                schema: "digital_net_cms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaVariant_Media_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "PageSheet",
                schema: "digital_net_cms",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSheet", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_PageSheet_Page_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageSheet_Sheet_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Sheet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTag",
                schema: "digital_net_cms",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTag", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_ArticleTag_Article_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleTag_Tag_ChildId",
                        column: x => x.ChildId,
                        principalSchema: "digital_net_cms",
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTag_ChildId",
                schema: "digital_net_cms",
                table: "ArticleTag",
                column: "ChildId");

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

            migrationBuilder.CreateIndex(
                name: "IX_MediaVariant_MediaId_Width_Quality",
                schema: "digital_net_cms",
                table: "MediaVariant",
                columns: new[] { "MediaId", "Width", "Quality" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Page_Path",
                schema: "digital_net_cms",
                table: "Page",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageOpenGraph_ChildId",
                schema: "digital_net_cms",
                table: "PageOpenGraph",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_PageSheet_ChildId",
                schema: "digital_net_cms",
                table: "PageSheet",
                column: "ChildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleTag",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "FormField",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "FormSubmission",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "MediaVariant",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "PageOpenGraph",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "PageSheet",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Article",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Form",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Media",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "OpenGraphEntry",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Page",
                schema: "digital_net_cms");

            migrationBuilder.DropTable(
                name: "Sheet",
                schema: "digital_net_cms");
        }
    }
}
