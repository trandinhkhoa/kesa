using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kesa.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "profile_field_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DataType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OptionsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_field_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "candidate_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Sex = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CustomFields = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_profiles_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_candidate_profiles_users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "profile_field_definitions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DataType", "IsActive", "IsRequired", "Key", "Name", "OptionsJson", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("4e32edf0-8be8-4d83-aec8-244fdb6aa8c5"), new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "Enum", true, false, "marriage", "Marriage", "[\"no\",\"married\",\"divoced\",\"widowed\"]", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("620f56ef-edf6-4a18-84fd-72a19f887cbc"), new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "Enum", true, false, "religion", "Religion", "[\"buddism\",\"christian\",\"others\"]", new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8ddc8f74-4d9d-4623-96f6-4adfa1a6ea09"), new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "String", true, false, "address", "Address", null, new DateTime(2026, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_BirthDate",
                table: "candidate_profiles",
                column: "BirthDate");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_CreatedByUserId",
                table: "candidate_profiles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_CustomFields",
                table: "candidate_profiles",
                column: "CustomFields")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "jsonb_path_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_Name",
                table: "candidate_profiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_Sex",
                table: "candidate_profiles",
                column: "Sex");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_UpdatedByUserId",
                table: "candidate_profiles",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_profile_field_definitions_Key",
                table: "profile_field_definitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidate_profiles");

            migrationBuilder.DropTable(
                name: "profile_field_definitions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
