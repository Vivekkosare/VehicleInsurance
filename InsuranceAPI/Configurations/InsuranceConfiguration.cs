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
    }
}
