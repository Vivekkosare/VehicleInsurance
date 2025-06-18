
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Features.Insurance.Entities;
using InsuranceAPI.Features.Insurance.Configurations;
using InsuranceAPI.Features.FeatureManagement.Entities;
using InsuranceAPI.Features.FeatureManagement.Configurations;

namespace InsuranceAPI.Data;

public class InsuranceDbContext: DbContext
{
    public DbSet<Insurance> Insurances { get; set; }
    public DbSet<InsuranceProduct> InsuranceProducts { get; set; }
    public DbSet<FeatureToggle> FeatureToggles { get; set; }
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration<Insurance>(new InsuranceConfiguration());
        modelBuilder.ApplyConfiguration<InsuranceProduct>(new InsuranceProductConfiguration());
        modelBuilder.ApplyConfiguration<FeatureToggle>(new FeatureToggleConfiguration());
    }
}
