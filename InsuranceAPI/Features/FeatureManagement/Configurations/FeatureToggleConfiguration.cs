using InsuranceAPI.Features.FeatureManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceAPI.Features.FeatureManagement.Configurations;

public class FeatureToggleConfiguration : IEntityTypeConfiguration<FeatureToggle>
{
    public void Configure(EntityTypeBuilder<FeatureToggle> builder)
    {
        builder.ToTable("FeatureToggles");
        builder.HasKey(ft => ft.Id);

        builder.Property(ft => ft.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(ft => ft.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ft => ft.Description)
            .HasColumnName("Description")
            .HasMaxLength(200);

        builder.Property(ft => ft.IsEnabled)
            .HasColumnName("IsEnabled")
            .IsRequired()
            .HasDefaultValue(false);

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
        builder.HasIndex(ft => ft.Name)
            .IsUnique()
            .HasDatabaseName("IX_FeatureToggles_Name");
        builder.HasData(
            new FeatureToggle
            {
                Id = Guid.Parse("d1e2f3a4-b5c6-7d8e-9f0a-b1c2d3e4f5a6"),
                Name = "ShowCarDetails",
                Description = "Enable or disable the Car details on car insurance display",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new FeatureToggle
            {
                Id = Guid.Parse("e2f3a4b5-c6d7-8e9f-0a1b-2c3d4e5f6a7b"),
                Name = "ApplyDiscounts",
                Description = "Enable or disable discounts on insurance products",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
