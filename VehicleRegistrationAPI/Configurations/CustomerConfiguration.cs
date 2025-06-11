using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<VehicleRegistrationAPI.Entities.Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");


        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15);
        builder.Property(c => c.PersonalIdentificationNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnUpdate();

        builder.HasMany(c => c.Vehicles)
            .WithOne(v => v.Owner)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.PersonalIdentificationNumber)
            .IsUnique()
            .HasDatabaseName("IX_Customers_PersonalIdentificationNumber");

            builder.HasData(
                new Customer
                {
                    Id = Guid.Parse("ad14152a-55b9-43f1-ac68-e87ecaef702a"),
                    Name = "Alice Johnson",
                    Email = "alice.johnson@example.com",
                    PhoneNumber = "1234567890",
                    PersonalIdentificationNumber = "PIN1001"
                },
                new Customer
                {
                    Id = Guid.Parse("445bdbe0-b8b0-4efb-9540-e5b38ff07a86"),
                    Name = "Bob Smith",
                    Email = "bob.smith@example.com",
                    PhoneNumber = "2345678901",
                    PersonalIdentificationNumber = "PIN1002"
                },
                new Customer
                {
                    Id = Guid.Parse("763ff2cd-55b9-4c0f-ac6c-ff94bc7abdd0"),
                    Name = "Charlie Brown",
                    Email = "charlie.brown@example.com",
                    PhoneNumber = "3456789012",
                    PersonalIdentificationNumber = "PIN1003"
                },
                new Customer
                {
                    Id = Guid.Parse("1efd72fa-e9ef-4415-b669-445dc43c4f8c"),
                    Name = "Diana Prince",
                    Email = "diana.prince@example.com",
                    PhoneNumber = "4567890123",
                    PersonalIdentificationNumber = "PIN1004"
                },
                new Customer
                {
                    Id = Guid.Parse("b85483e5-25d2-479a-9d0b-8e80a57484de"),
                    Name = "Ethan Hunt",
                    Email = "ethan.hunt@example.com",
                    PhoneNumber = "5678901234",
                    PersonalIdentificationNumber = "PIN1005"
                }
            );

    }
}
