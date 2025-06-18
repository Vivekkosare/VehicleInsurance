using InsuranceAPI.Features.Insurance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAPI.Features.Insurance.Configurations;

public class InsuranceProductConfiguration : IEntityTypeConfiguration<InsuranceProduct>
{
    public void Configure(EntityTypeBuilder<InsuranceProduct> builder)
    {
        builder.ToTable("InsuranceProducts");
        builder.HasKey(ip => ip.Id);

        builder.Property(ip => ip.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(ip => ip.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ip => ip.Code)
            .HasColumnName("Code")
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(ip => ip.Price)
            .HasColumnName("Price")
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(ip => ip.Discount)
            .HasColumnName("Discount")
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnUpdate();

        builder.HasIndex(ip => ip.Code)
            .IsUnique()
            .HasDatabaseName("IX_InsuranceProducts_Code");

        builder.HasIndex(ip => ip.Name)
            .HasDatabaseName("IX_InsuranceProducts_Name");

        builder.HasData(
            new InsuranceProduct
            {
                Id = Guid.Parse("b43c53a0-1c57-4c9c-94a1-673d7db31fcf"),
                Name = "Pet insurance",
                Code = "PET",
                Price = 10,
                Discount = 15
            },
            new InsuranceProduct
            {
                Id = Guid.Parse("def55e24-40c9-4234-825a-bbf4319fc79b"),
                Name = "Personal health insurance",
                Code = "HEALTH",
                Price = 20,
                Discount = 10
            },
            new InsuranceProduct
            {
                Id = Guid.Parse("ca536771-42b8-4f55-8014-7e98c6c7b060"),
                Name = "Car insurance",
                Code = "CAR",
                Price = 30,
                Discount = 5
            }
        );
    }
}
