using InsuranceAPI.Configurations;
using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Features.Insurance.Entities;

namespace InsuranceAPI.Data;

public class InsuranceDbContext: DbContext
{
    public DbSet<Insurance> Insurances { get; set; }
    public DbSet<InsuranceProduct> InsuranceProducts { get; set; }
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration<Insurance>(new InsuranceConfiguration());
        modelBuilder.ApplyConfiguration<InsuranceProduct>(new InsuranceProductConfiguration());
    }
}
