using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VehicleRegistrationAPI;
using VehicleRegistrationAPI.Data;

namespace InsuranceAPI.Tests.Integration
{
    // Custom factory to seed test data
    public class TestVehicleRegistrationApiFactory : WebApplicationFactory<VehicleRegistrationAPI.Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            // Ensure the in-memory DB is used for tests
            Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "true");
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration(s)
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<VehicleRegistrationDbContext>));
                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(VehicleRegistrationDbContext));
                if (contextDescriptor != null)
                    services.Remove(contextDescriptor);

                // Remove all DbContextOptions and DbContext registrations for VehicleRegistrationDbContext
                var dbContextDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<VehicleRegistrationDbContext>)).ToList();
                foreach (var descriptor in dbContextDescriptors)
                    services.Remove(descriptor);
                    
                var contextDescriptors = services.Where(d => d.ServiceType == typeof(VehicleRegistrationDbContext)).ToList();
                foreach (var descriptor in contextDescriptors)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<VehicleRegistrationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Seed the database
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<VehicleRegistrationDbContext>();
                    db.Database.EnsureCreated();
                    DataSeeder.SeedTestData(db);
                }
            });
        }
    }
}
