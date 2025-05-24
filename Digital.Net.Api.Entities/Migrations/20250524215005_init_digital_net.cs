using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Digital.Net.Api.Entities.Migrations
{
    /// <inheritdoc />
    public partial class init_digital_net : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "digital_net");

            migrationBuilder.CreateTable(
                name: "ApplicationOption",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuckConfig",
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
                    table.PrimaryKey("PK_PuckConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "View",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    PuckConfigId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Page",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Path = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    ViewId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Page", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Page_View_ViewId",
                        column: x => x.ViewId,
                        principalSchema: "digital_net",
                        principalTable: "View",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApiKey",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiToken",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Avatar",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avatar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    AvatarId = table.Column<Guid>(type: "uuid", nullable: true),
                    Password = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Login = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Avatar_AvatarId",
                        column: x => x.AvatarId,
                        principalSchema: "digital_net",
                        principalTable: "Avatar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_User_UploaderId",
                        column: x => x.UploaderId,
                        principalSchema: "digital_net",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "digital_net",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    HasError = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorTrace = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Event_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "digital_net",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_Key",
                schema: "digital_net",
                table: "ApiKey",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_UserId",
                schema: "digital_net",
                table: "ApiKey",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiToken_UserId",
                schema: "digital_net",
                table: "ApiToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationOption_Id",
                schema: "digital_net",
                table: "ApplicationOption",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Avatar_DocumentId",
                schema: "digital_net",
                table: "Avatar",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_FileName",
                schema: "digital_net",
                table: "Document",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Document_UploaderId",
                schema: "digital_net",
                table: "Document",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_UserId",
                schema: "digital_net",
                table: "Event",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Page_Path",
                schema: "digital_net",
                table: "Page",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Page_Title",
                schema: "digital_net",
                table: "Page",
                column: "Title",
                unique: true);

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
                name: "IX_User_AvatarId",
                schema: "digital_net",
                table: "User",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username_Email",
                schema: "digital_net",
                table: "User",
                columns: new[] { "Username", "Email" },
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
                name: "FK_ApiKey_User_UserId",
                schema: "digital_net",
                table: "ApiKey",
                column: "UserId",
                principalSchema: "digital_net",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApiToken_User_UserId",
                schema: "digital_net",
                table: "ApiToken",
                column: "UserId",
                principalSchema: "digital_net",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Avatar_Document_DocumentId",
                schema: "digital_net",
                table: "Avatar",
                column: "DocumentId",
                principalSchema: "digital_net",
                principalTable: "Document",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_User_UploaderId",
                schema: "digital_net",
                table: "Document");

            migrationBuilder.DropTable(
                name: "ApiKey",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "ApiToken",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "ApplicationOption",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "Event",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "Page",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "View",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "PuckConfig",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "User",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "Avatar",
                schema: "digital_net");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "digital_net");
        }
    }
}
