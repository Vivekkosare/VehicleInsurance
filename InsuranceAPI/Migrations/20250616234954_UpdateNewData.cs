using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsuranceAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsuranceProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insurances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonalIdentificationNumber = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    InsuredItem = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insurances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Insurances_InsuranceProducts_InsuranceProductId",
                        column: x => x.InsuranceProductId,
                        principalTable: "InsuranceProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InsuranceProducts",
                columns: new[] { "Id", "Code", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("b43c53a0-1c57-4c9c-94a1-673d7db31fcf"), "PET", "Pet insurance", 10m },
                    { new Guid("ca536771-42b8-4f55-8014-7e98c6c7b060"), "CAR", "Car insurance", 30m },
                    { new Guid("def55e24-40c9-4234-825a-bbf4319fc79b"), "HEALTH", "Personal health insurance", 20m }
                });

            migrationBuilder.InsertData(
                table: "Insurances",
                columns: new[] { "Id", "CreatedAt", "EndDate", "InsuranceProductId", "InsuredItem", "PersonalIdentificationNumber", "StartDate" },
                values: new object[,]
                {
                    { new Guid("a1e1b2c3-d4e5-4f6a-8b7c-9d0e1f2a3b4c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("ca536771-42b8-4f55-8014-7e98c6c7b060"), "Car", "PIN1001", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2e2c3d4-e5f6-4a8b-7c9d-0e1f2a3b4c5d"), new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("def55e24-40c9-4234-825a-bbf4319fc79b"), "Health", "PIN1002", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c3d4e5f6-a8b7-4c9d-0e1f-2a3b4c5d6e7f"), new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("b43c53a0-1c57-4c9c-94a1-673d7db31fcf"), "Pet", "PIN1003", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceProducts_Code",
                table: "InsuranceProducts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceProducts_Name",
                table: "InsuranceProducts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Insurances_InsuranceProductId",
                table: "Insurances",
                column: "InsuranceProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Insurances");

            migrationBuilder.DropTable(
                name: "InsuranceProducts");
        }
    }
}
