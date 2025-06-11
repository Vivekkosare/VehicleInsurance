using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<VehicleRegistrationAPI.Entities.Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(v => v.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(15);
        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(v => v.Year)
            .IsRequired()
            .HasMaxLength(4);
        builder.Property(v => v.Color)
            .IsRequired()
            .HasMaxLength(30);
        builder.Property(v => v.RegistrationDate)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnUpdate();

        builder.HasOne(v => v.Owner)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.RegistrationNumber)
            .IsUnique()
            .HasDatabaseName("IX_Vehicles_RegistrationNumber");
            
            builder.HasData(
                new Vehicle
                {
                    Id = Guid.Parse("f28d7ed3-e0e6-42d7-8381-f5acf7fc10ef"),
                    Name = "Toyota Camry",
                    RegistrationNumber = "ABC1234",
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2020,
                    Color = "White",
                    RegistrationDate = DateTime.SpecifyKind(new DateTime(2020, 5, 10), DateTimeKind.Utc),
                    OwnerId = Guid.Parse("ad14152a-55b9-43f1-ac68-e87ecaef702a")
                },
                new Vehicle
                {
                    Id = Guid.Parse("f0f23bde-c9e3-4cc3-b999-7681a15c66f1"),
                    Name = "Honda Accord",
                    RegistrationNumber = "XYZ5678",
                    Make = "Honda",
                    Model = "Accord",
                    Year = 2019,
                    Color = "Black",
                    RegistrationDate = DateTime.SpecifyKind(new DateTime(2019, 3, 15), DateTimeKind.Utc),
                    OwnerId = Guid.Parse("445bdbe0-b8b0-4efb-9540-e5b38ff07a86")
                },
                new Vehicle
                {
                    Id = Guid.Parse("5fa8cca5-9612-4cd4-9c7d-a85afdd1c37f"),
                    Name = "Ford Mustang",
                    RegistrationNumber = "MUS2021",
                    Make = "Ford",
                    Model = "Mustang",
                    Year = 2021,
                    Color = "Red",
                    RegistrationDate = DateTime.SpecifyKind(new DateTime(2021, 7, 20), DateTimeKind.Utc),
                    OwnerId = Guid.Parse("763ff2cd-55b9-4c0f-ac6c-ff94bc7abdd0")
                }
            );
    }
}
