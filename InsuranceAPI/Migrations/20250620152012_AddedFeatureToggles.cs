using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedFeatureToggles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DiscountApplied",
                table: "Insurances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Insurances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "InsuranceProducts",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FeatureToggles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureToggles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FeatureToggles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsEnabled", "Name" },
                values: new object[] { new Guid("d1e2f3a4-b5c6-7d8e-9f0a-b1c2d3e4f5a6"), new DateTime(2025, 6, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Enable or disable the Car details on car insurance display", true, "ShowCarDetails" });

            migrationBuilder.InsertData(
                table: "FeatureToggles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[] { new Guid("e2f3a4b5-c6d7-8e9f-0a1b-2c3d4e5f6a7b"), new DateTime(2025, 6, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Enable or disable discounts on insurance products", "ApplyDiscounts" });

            migrationBuilder.UpdateData(
                table: "InsuranceProducts",
                keyColumn: "Id",
                keyValue: new Guid("b43c53a0-1c57-4c9c-94a1-673d7db31fcf"),
                column: "Discount",
                value: 15m);

            migrationBuilder.UpdateData(
                table: "InsuranceProducts",
                keyColumn: "Id",
                keyValue: new Guid("ca536771-42b8-4f55-8014-7e98c6c7b060"),
                column: "Discount",
                value: 5m);

            migrationBuilder.UpdateData(
                table: "InsuranceProducts",
                keyColumn: "Id",
                keyValue: new Guid("def55e24-40c9-4234-825a-bbf4319fc79b"),
                column: "Discount",
                value: 10m);

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("a1e1b2c3-d4e5-4f6a-8b7c-9d0e1f2a3b4c"),
                columns: new[] { "DiscountApplied", "Price" },
                values: new object[] { false, 30m });

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("b2e2c3d4-e5f6-4a8b-7c9d-0e1f2a3b4c5d"),
                columns: new[] { "DiscountApplied", "Price" },
                values: new object[] { false, 20m });

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a8b7-4c9d-0e1f-2a3b4c5d6e7f"),
                columns: new[] { "DiscountApplied", "Price" },
                values: new object[] { false, 10m });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureToggles_Name",
                table: "FeatureToggles",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureToggles");

            migrationBuilder.DropColumn(
                name: "DiscountApplied",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "InsuranceProducts");
        }
    }
}
