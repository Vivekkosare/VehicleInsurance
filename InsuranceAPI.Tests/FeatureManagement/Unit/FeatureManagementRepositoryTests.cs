using System.Threading.Tasks;
using Xunit;
using Moq;
using InsuranceAPI.Features.FeatureManagement.Repositories;
using InsuranceAPI.Features.FeatureManagement.Entities;
using InsuranceAPI.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using VehicleInsurance.Shared.Services;
using VehicleInsurance.Shared.DTOs;

namespace InsuranceAPI.Tests.FeatureManagement.Unit
{
    public class FeatureManagementRepositoryTests
    {
        [Fact]
        public async Task AddFeatureToggleAsync_ShouldReturnSuccess_WhenToggleIsNew()
        {
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: "FeatureToggleTestDb")
                .Options;

            var dbContext = new InsuranceDbContext(options);

            var cacheMock = new Mock<ICacheService>();
            cacheMock.Setup(c => c.GetAsync<FeatureToggle?>(It.IsAny<string>()))
                .ReturnsAsync(Result<FeatureToggle?>.Success(null));
            cacheMock.Setup(c => c.GetAsync<bool?>(It.IsAny<string>()))
                .ReturnsAsync(Result<bool?>.Success(null));
            var loggerMock = new Mock<ILogger<FeatureManagementRepository>>();
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["TimeoutInMinutes"]).Returns("5");

            var repo = new FeatureManagementRepository(dbContext, cacheMock.Object, loggerMock.Object, configMock.Object);
            var toggle = new FeatureToggle { Id = Guid.NewGuid(), Name = "TestToggle", IsEnabled = true };

            var result = await repo.AddFeatureToggleAsync(toggle);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("TestToggle", result.Value.Name);
        }

        [Fact]
        public async Task PatchFeatureToggleAsync_ShouldUpdateFields_WhenValidPatch()
        {
            var options = new DbContextOptionsBuilder<InsuranceDbContext>()
                .UseInMemoryDatabase(databaseName: $"PatchFeatureToggleTestDb_{Guid.NewGuid()}")
                .Options;
            var dbContext = new InsuranceDbContext(options);
            var cacheMock = new Mock<ICacheService>();
            cacheMock.Setup(c => c.GetAsync<FeatureToggle?>(It.IsAny<string>()))
                .ReturnsAsync(Result<FeatureToggle?>.Success(null));
            var loggerMock = new Mock<ILogger<FeatureManagementRepository>>();
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["TimeoutInMinutes"]).Returns("5");

            var repo = new FeatureManagementRepository(dbContext, cacheMock.Object, loggerMock.Object, configMock.Object);

            var toggle = new FeatureToggle {
                Id = Guid.NewGuid(), 
                Name = "PatchToggle",
                Description = "Old Desc",
                IsEnabled = false,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            dbContext.FeatureToggles.Add(toggle);
            dbContext.SaveChanges();

            var patchDto = new InsuranceAPI.Features.FeatureManagement.DTOs.FeatureTogglePatchDto("New Desc", true);

            var result = await repo.PatchFeatureToggleAsync("PatchToggle", patchDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("New Desc", result.Value.Description);
            Assert.True(result.Value.IsEnabled);
        }
    }
}
