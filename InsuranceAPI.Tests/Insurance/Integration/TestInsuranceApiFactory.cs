using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InsuranceAPI.Data;

namespace InsuranceAPI.Tests.Insurance.Integration
{
    public class TestInsuranceApiFactory : WebApplicationFactory<Program>
    {
        public TestInsuranceApiFactory()
        {
            // Ensure the environment variable is set before the host is built
            Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "true");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove all DbContextOptions and DbContext registrations for InsuranceDbContext
                var dbContextDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<InsuranceDbContext>)).ToList();
                foreach (var descriptor in dbContextDescriptors)
                    services.Remove(descriptor);

                var contextDescriptors = services.Where(d => d.ServiceType == typeof(InsuranceDbContext)).ToList();
                foreach (var descriptor in contextDescriptors)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<InsuranceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestInsuranceDb");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Ensure the database is created
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<InsuranceDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}
