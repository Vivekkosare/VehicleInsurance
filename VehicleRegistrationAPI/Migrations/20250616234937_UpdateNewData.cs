using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VehicleRegistrationAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PersonalIdentificationNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Make = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "integer", maxLength: 4, nullable: false),
                    Color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Customers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Email", "Name", "PersonalIdentificationNumber", "PhoneNumber" },
                values: new object[,]
                {
                    { new Guid("1efd72fa-e9ef-4415-b669-445dc43c4f8c"), "diana.prince@example.com", "Diana Prince", "PIN1004", "4567890123" },
                    { new Guid("445bdbe0-b8b0-4efb-9540-e5b38ff07a86"), "bob.smith@example.com", "Bob Smith", "PIN1002", "2345678901" },
                    { new Guid("763ff2cd-55b9-4c0f-ac6c-ff94bc7abdd0"), "charlie.brown@example.com", "Charlie Brown", "PIN1003", "3456789012" },
                    { new Guid("ad14152a-55b9-43f1-ac68-e87ecaef702a"), "alice.johnson@example.com", "Alice Johnson", "PIN1001", "1234567890" },
                    { new Guid("b85483e5-25d2-479a-9d0b-8e80a57484de"), "ethan.hunt@example.com", "Ethan Hunt", "PIN1005", "5678901234" }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Color", "Make", "Model", "Name", "OwnerId", "RegistrationDate", "RegistrationNumber", "Year" },
                values: new object[,]
                {
                    { new Guid("5fa8cca5-9612-4cd4-9c7d-a85afdd1c37f"), "Red", "Ford", "Mustang", "Ford Mustang", new Guid("763ff2cd-55b9-4c0f-ac6c-ff94bc7abdd0"), new DateTime(2021, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), "MUS2021", 2021 },
                    { new Guid("f0f23bde-c9e3-4cc3-b999-7681a15c66f1"), "Black", "Honda", "Accord", "Honda Accord", new Guid("445bdbe0-b8b0-4efb-9540-e5b38ff07a86"), new DateTime(2019, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), "XYZ5678", 2019 },
                    { new Guid("f28d7ed3-e0e6-42d7-8381-f5acf7fc10ef"), "White", "Toyota", "Camry", "Toyota Camry", new Guid("ad14152a-55b9-43f1-ac68-e87ecaef702a"), new DateTime(2020, 5, 10, 0, 0, 0, 0, DateTimeKind.Utc), "ABC1234", 2020 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PersonalIdentificationNumber",
                table: "Customers",
                column: "PersonalIdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_OwnerId",
                table: "Vehicles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_RegistrationNumber",
                table: "Vehicles",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
