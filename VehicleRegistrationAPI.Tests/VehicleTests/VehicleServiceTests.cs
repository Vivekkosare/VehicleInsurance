using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VehicleRegistrationAPI.Features.Vehicles.DTOs;
using VehicleRegistrationAPI.Features.Vehicles.Repositories;
using VehicleRegistrationAPI.Features.Vehicles.Services;
using Xunit;
using Microsoft.Extensions.Logging;

namespace VehicleRegistrationAPI.Tests.VehicleTests
{
    public class VehicleServiceTests
    {
        [Fact]
        public async Task GetVehiclesByPersonalIdentificationNumberAsync_ShouldReturnVehicles_WhenFound()
        {
            // Arrange
            var pin = "1234567890";
            var vehicles = new List<VehicleRegistrationAPI.Entities.Vehicle> {
                new VehicleRegistrationAPI.Entities.Vehicle { Id = Guid.NewGuid(), RegistrationNumber = "ABC123", Owner = new VehicleRegistrationAPI.Entities.Customer { PersonalIdentificationNumber = pin } }
            };
            var repoMock = new Mock<IVehicleRepository>();
            repoMock.Setup(r => r.GetVehiclesByPersonalIdentificationNumberAsync(pin)).ReturnsAsync(VehicleInsurance.Shared.DTOs.Result<IEnumerable<VehicleRegistrationAPI.Entities.Vehicle>>.Success(vehicles));
            var loggerMock = new Mock<ILogger<VehicleService>>();
            var service = new VehicleService(repoMock.Object, loggerMock.Object);

            // Act
            var result = await service.GetVehiclesByPersonalIdentificationNumberAsync(pin);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal("ABC123", result.Value.First().RegistrationNumber);
        }
    }
}
