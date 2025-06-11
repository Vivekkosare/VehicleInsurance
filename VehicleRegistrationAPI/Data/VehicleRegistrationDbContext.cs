using Microsoft.EntityFrameworkCore;
using VehicleRegistrationAPI.Configurations;
using VehicleRegistrationAPI.Entities;

namespace VehicleRegistrationAPI.Data;

public class VehicleRegistrationDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public VehicleRegistrationDbContext(DbContextOptions<VehicleRegistrationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration<Customer>(new CustomerConfiguration());
        builder.ApplyConfiguration<Vehicle>(new VehicleConfiguration());
    }
}
