using InsuranceAPI.Features.Insurance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAPI.Configurations;

public class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
{
    public void Configure(EntityTypeBuilder<Insurance> builder)
    {
        builder.ToTable("Insurances");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(i => i.PersonalIdentificationNumber)
            .HasColumnName("PersonalIdentificationNumber")
            .IsRequired()
            .HasMaxLength(12);

        builder.Property(i => i.StartDate)
            .HasColumnName("StartDate")
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(i => i.EndDate)
            .HasColumnName("EndDate")
            .IsRequired()
            .HasColumnType("timestamp with time zone");

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

        builder.HasOne(i => i.InsuranceProduct)
            .WithMany()
            .HasForeignKey(i => i.InsuranceProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new Insurance
            {
                Id = Guid.Parse("a1e1b2c3-d4e5-4f6a-8b7c-9d0e1f2a3b4c"),
                PersonalIdentificationNumber = "PIN1001", // Alice Johnson
                InsuranceProductId = Guid.Parse("ca536771-42b8-4f55-8014-7e98c6c7b060"), // Car insurance
                StartDate = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc),
                CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
                InsuredItem = "Car"
            },
            new Insurance
            {
                Id = Guid.Parse("b2e2c3d4-e5f6-4a8b-7c9d-0e1f2a3b4c5d"),
                PersonalIdentificationNumber = "PIN1002", // Bob Smith
                InsuranceProductId = Guid.Parse("def55e24-40c9-4234-825a-bbf4319fc79b"), // Personal health insurance
                StartDate = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(2025, 2, 1), DateTimeKind.Utc),
                CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(new DateTime(2024, 2, 1), DateTimeKind.Utc),
                InsuredItem = "Health"
            },
            new Insurance
            {
                Id = Guid.Parse("c3d4e5f6-a8b7-4c9d-0e1f-2a3b4c5d6e7f"),
                PersonalIdentificationNumber = "PIN1003", // Charlie Brown
                InsuranceProductId = Guid.Parse("b43c53a0-1c57-4c9c-94a1-673d7db31fcf"), // Pet insurance
                StartDate = DateTime.SpecifyKind(new DateTime(2024, 3, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(2025, 3, 1), DateTimeKind.Utc),
                CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 3, 1), DateTimeKind.Utc),
                UpdatedAt = DateTime.SpecifyKind(new DateTime(2024, 3, 1), DateTimeKind.Utc),
                InsuredItem = "Pet"
            }
        );
    }
}
