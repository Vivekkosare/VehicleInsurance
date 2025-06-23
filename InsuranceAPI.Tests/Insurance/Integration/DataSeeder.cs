using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Entities;

namespace InsuranceAPI.Tests.Insurance.Integration
{
    public static class DataSeeder
    {
        public static void SeedTestData(VehicleRegistrationDbContext db)
        {
            // Seed customers
            db.Customers.AddRange(new[]
            {
                new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Owner",
                    PersonalIdentificationNumber = "1234567890",
                    Email = "owner@example.com",
                    PhoneNumber = "1234567890"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = "Cached Owner",
                    PersonalIdentificationNumber = "9999999999",
                    Email = "cached@example.com",
                    PhoneNumber = "9999999999"
                }
            });
            db.SaveChanges();

            // Seed vehicles
            var owner1 = db.Customers.First(c => c.PersonalIdentificationNumber == "1234567890");
            var owner2 = db.Customers.First(c => c.PersonalIdentificationNumber == "9999999999");
            db.Vehicles.AddRange(new[]
            {
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = "XYZ789",
                    OwnerId = owner1.Id,
                    Owner = owner1,
                    Color = "Blue",
                    Make = "Honda",
                    Model = "Civic",
                    Name = "TestCar2",
                    Year = 2020,
                    RegistrationDate = DateTime.UtcNow
                },
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    RegistrationNumber = "CACHED123",
                    OwnerId = owner2.Id,
                    Owner = owner2,
                    Color = "Green",
                    Make = "Ford",
                    Model = "Focus",
                    Name = "CachedCar",
                    Year = 2021,
                    RegistrationDate = DateTime.UtcNow
                }
            });
            db.SaveChanges();
        }
    }
}
