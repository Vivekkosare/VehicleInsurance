using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedInsuredItemIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InsuredItem",
                table: "Insurances",
                newName: "InsuredItemIdentity");

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("a1e1b2c3-d4e5-4f6a-8b7c-9d0e1f2a3b4c"),
                column: "InsuredItemIdentity",
                value: "ABC1234");

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("b2e2c3d4-e5f6-4a8b-7c9d-0e1f2a3b4c5d"),
                column: "InsuredItemIdentity",
                value: "Bob Smith");

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a8b7-4c9d-0e1f-2a3b4c5d6e7f"),
                column: "InsuredItemIdentity",
                value: "Bruno the Dog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InsuredItemIdentity",
                table: "Insurances",
                newName: "InsuredItem");

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("a1e1b2c3-d4e5-4f6a-8b7c-9d0e1f2a3b4c"),
                column: "InsuredItem",
                value: "Car");

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("b2e2c3d4-e5f6-4a8b-7c9d-0e1f2a3b4c5d"),
                column: "InsuredItem",
                value: "Health");

            migrationBuilder.UpdateData(
                table: "Insurances",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a8b7-4c9d-0e1f-2a3b4c5d6e7f"),
                column: "InsuredItem",
                value: "Pet");
        }
    }
}
