using Moq;
using InsuranceAPI.Features.FeatureManagement.Services;
using InsuranceAPI.Features.FeatureManagement.Repositories;
using InsuranceAPI.Features.FeatureManagement.Entities;
using Microsoft.Extensions.Logging;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Tests.FeatureManagement.Unit
{
    public class FeatureManagementServiceTests
    {
        [Fact]
        public async Task GetFeatureToggleByNameAsync_ShouldReturnToggle_WhenExists()
        {
            var repoMock = new Mock<IFeatureManagementRepository>();
            var loggerMock = new Mock<ILogger<FeatureManagementService>>();
            var toggle = new FeatureToggle { Id = Guid.NewGuid(), Name = "TestToggle", IsEnabled = true };
            repoMock.Setup(r => r.GetFeatureToggleByNameAsync("TestToggle"))
                .ReturnsAsync(Result<FeatureToggle>.Success(toggle));
            var service = new FeatureManagementService(repoMock.Object, loggerMock.Object);

            var result = await service.GetFeatureToggleByNameAsync("TestToggle");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("TestToggle", result.Value.Name);
        }
    }
}
