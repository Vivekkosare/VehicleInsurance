using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleRegistrationAPI.Data;
using VehicleRegistrationAPI.Entities;
using VehicleRegistrationAPI.Features.Vehicles.Repositories;
using VehicleInsurance.Shared.DTOs;
using Xunit;

namespace VehicleRegistrationAPI.Tests.VehicleTests
{
    public class VehicleRepositoryTests
    {
        private readonly DbContextOptions<VehicleRegistrationDbContext> _dbContextOptions;
        private readonly Mock<VehicleInsurance.Shared.Services.ICacheService> _cacheMock = new();

        public VehicleRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<VehicleRegistrationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddVehicleAsync_ShouldReturnSuccess_WhenVehicleIsNew()
        {
            // Arrange
            using var dbContext = new VehicleRegistrationDbContext(_dbContextOptions);
            var repository = new VehicleRepository(dbContext, Mock.Of<Microsoft.Extensions.Logging.ILogger<VehicleRepository>>(),
                    Mock.Of<VehicleInsurance.Shared.Services.ICacheService>());
            var vehicle = new Vehicle {
                Id = Guid.NewGuid(),
                RegistrationNumber = "ABC123",
                OwnerId = Guid.NewGuid(),
                Color = "Red",
                Make = "Toyota",
                Model = "Corolla",
                Name = "TestCar"
            };

            // Act
            var result = await repository.AddVehicleAsync(vehicle);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("ABC123", result.Value.RegistrationNumber);
        }

        [Fact]
        public async Task GetVehicleByIdAsync_ShouldReturnVehicle_WhenExists()
        {
            // Arrange
            using var dbContext = new VehicleRegistrationDbContext(_dbContextOptions);
            var repository = new VehicleRepository(dbContext, Mock.Of<Microsoft.Extensions.Logging.ILogger<VehicleRepository>>(), _cacheMock.Object);
            var owner = new VehicleRegistrationAPI.Entities.Customer {
                Id = Guid.NewGuid(),
                Name = "Test Owner",
                PersonalIdentificationNumber = "1234567890",
                Email = "owner@example.com",
                PhoneNumber = "1234567890"
            };
            dbContext.Customers.Add(owner);
            var vehicle = new Vehicle {
                Id = Guid.NewGuid(),
                RegistrationNumber = "XYZ789",
                OwnerId = owner.Id,
                Owner = owner,
                Color = "Blue",
                Make = "Honda",
                Model = "Civic",
                Name = "TestCar2",
                Year = 2020,
                RegistrationDate = DateTime.UtcNow
            };
            dbContext.Vehicles.Add(vehicle);
            dbContext.SaveChanges();

            // Setup cache miss (returns Result<T>.Failure or null)
            _cacheMock.Setup(c => c.GetAsync<Vehicle>(It.IsAny<string>()))
                .ReturnsAsync(Result<Vehicle?>.Failure("Not found"));

            // Act
            var result = await repository.GetVehicleByIdAsync(vehicle.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(vehicle.Id, result.Value.Id);
        }

        [Fact]
        public async Task GetVehicleByIdAsync_ShouldReturnVehicle_FromCache_WhenExistsInCache()
        {
            // Arrange
            using var dbContext = new VehicleRegistrationDbContext(_dbContextOptions);
            var repository = new VehicleRepository(dbContext, Mock.Of<Microsoft.Extensions.Logging.ILogger<VehicleRepository>>(),
                            _cacheMock.Object);
            var owner = new VehicleRegistrationAPI.Entities.Customer {
                Id = Guid.NewGuid(),
                Name = "Cached Owner",
                PersonalIdentificationNumber = "9999999999",
                Email = "cached@example.com",
                PhoneNumber = "9999999999"
            };
            var vehicle = new Vehicle {
                Id = Guid.NewGuid(),
                RegistrationNumber = "CACHED123",
                OwnerId = owner.Id,
                Owner = owner,
                Color = "Green",
                Make = "Ford",
                Model = "Focus",
                Name = "CachedCar",
                Year = 2021,
                RegistrationDate = DateTime.UtcNow
            };
            // Setup cache hit
            _cacheMock.Setup(c => c.GetAsync<Vehicle>(It.IsAny<string>()))
                .ReturnsAsync(Result<Vehicle?>.Success(vehicle));

            // Act
            var result = await repository.GetVehicleByIdAsync(vehicle.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(vehicle.Id, result.Value.Id);
            Assert.Equal("CACHED123", result.Value.RegistrationNumber);
        }
    }
}
